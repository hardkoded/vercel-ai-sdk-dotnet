// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using static Vercel.AI.Tests.MiniMaxVideoModel.MiniMaxVideoModelHarness;

namespace Vercel.AI.Tests.MiniMaxVideoModel;

/// <summary>Port of the request-side tests of <c>minimax-video-model.test.ts</c> &gt; <c>MiniMaxVideoModel &gt; doStart / doStatus</c>.</summary>
public sealed class DoStartDoStatusTests
{
    private const string Prefix =
        "packages/minimax/src/minimax-video-model.test.ts::MiniMaxVideoModel > doStart / doStatus::";

    private const string Operation =
        "{\"taskId\":\"task-123\",\"resolvedInputs\":{\"imageCount\":0,\"referenceVideoIndices\":[]}}";

    [Fact]
    [UpstreamTest(
        Prefix + "should submit once with callback_url without polling",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_submit_once_with_callback_url_without_polling()
    {
        var testDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var handler = new ParityHandler(_ =>
            ParityHandler.Json(
                "{\"task_id\":\"task-123\"}",
                ("x-minimax-request-id", "start-request")
            )
        );
        var model = CreateModel(handler, currentDate: () => testDate);

        var result = await model.DoStartAsync(
            new MiniMaxCall
            {
                ProviderOptions = OperationJson.Parse("{}"),
                WebhookUrl = "https://example.com/callback",
            }.ToCall(),
            CancellationToken.None
        );

        Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        JsonAssert.Equal(
            RequestBody(handler),
            "{\"callback_url\":\"https://example.com/callback\",\"model\":\"MiniMax-H3\",\"content\":[{\"type\":\"text\",\"text\":\""
                + PromptText
                + "\"}],\"resolution\":\"2K\",\"duration\":5,\"ratio\":\"16:9\"}"
        );
        JsonAssert.Equal((JsonElement)result.Operation!, Operation);
        Assert.Empty(result.Warnings);
        Assert.Equal(testDate, result.Response.Timestamp);
        Assert.Equal("MiniMax-H3", result.Response.ModelId);
        Assert.Equal("start-request", result.Response.Headers!["x-minimax-request-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit callback_url when webhookUrl is not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_callback_url_when_webhookUrl_is_not_provided()
    {
        var handler = Server();

        var result = await CreateModel(handler)
            .DoStartAsync(new MiniMaxCall().ToCall(), CancellationToken.None);

        Assert.Single(handler.Calls);
        Assert.Null(RequestBody(handler)!["callback_url"]);
        JsonAssert.Equal((JsonElement)result.Operation!, Operation);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve normalized inputs and metadata across JSON and a fresh model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_normalized_inputs_and_metadata_across_JSON_and_a_fresh_model()
    {
        var handler = Server();
        var references = Enumerable
            .Range(0, 10)
            .Select(_ => ImageUrlFile)
            .Concat(
                Enumerable
                    .Range(0, 4)
                    .Select(i =>
                        VideoModelFile.FromUrl($"https://cdn.example.com/clip-{i}.mp4", "video/mp4")
                    )
            )
            .ToList();
        var options = new MiniMaxCall { Duration = 20, InputReferences = references };

        var start = await CreateModel(handler)
            .DoStartAsync(options.ToCall(), CancellationToken.None);
        var restoredOperation = OperationJson.Parse(
            JsonSerializer.Serialize((JsonElement)start.Operation!)
        );
        JsonAssert.Equal(
            restoredOperation,
            "{\"taskId\":\"task-123\",\"resolvedInputs\":{\"imageCount\":9,\"referenceVideoIndices\":[10,11,12]}}"
        );
        var testDate = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var status = await CreateModel(handler, currentDate: () => testDate)
            .DoStatusAsync(options.StatusCall(restoredOperation), CancellationToken.None);
        Assert.Equal(2, handler.Calls.Count);
        Assert.Equal(HttpMethod.Get, handler.Calls[1].Method);
        Assert.Equal("completed", status.Status);
        Assert.Equal(testDate, status.Response.Timestamp);
        Assert.Empty(status.Warnings);
        JsonAssert.Equal(
            Resolve(status.ProviderMetadata, "resolvedInputs"),
            restoredOperation.GetProperty("resolvedInputs").GetRawText()
        );

        var generated = await CreateModel(handler)
            .DoGenerateAsync(options.ToCall(), CancellationToken.None);
        Assert.Equal(
            RequestBody(handler, 0)!.ToJsonString(),
            RequestBody(handler, 2)!.ToJsonString()
        );
        Assert.Equal(
            start.Warnings.Select(w => (w.Type, w.Feature, w.Details)),
            generated.Warnings.Select(w => (w.Type, w.Feature, w.Details))
        );
        Assert.Equal(3, start.Warnings.Count);
        Assert.Equal(
            status.Videos.Select(v => (v.Type, v.Url, v.MediaType)),
            generated.Videos.Select(v => (v.Type, v.Url, v.MediaType))
        );
        var expected = JsonNode.Parse(status.ProviderMetadata!.Value.GetRawText())!.AsObject();
        expected["minimax"]!["resolvedInputs"] = JsonNode.Parse(
            "{\"imageCount\":9,\"referenceVideoUrls\":[\"https://cdn.example.com/clip-0.mp4\",\"https://cdn.example.com/clip-1.mp4\",\"https://cdn.example.com/clip-2.mp4\"]}"
        );
        JsonAssert.Equal(generated.ProviderMetadata!.Value, expected.ToJsonString());
        Assert.Equal(4, handler.Calls.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should retain mixed input indices, preserving duplicates and counting inline videos toward the cap",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_retain_mixed_input_indices_preserving_duplicates_and_counting_inline_videos_toward_the_cap()
    {
        var handler = Server();
        var options = new MiniMaxCall
        {
            InputReferences = new[]
            {
                VideoModelFile.FromUrl("https://example.com/audio.mp3", "audio/mpeg"),
                ImageUrlFile,
                VideoUrlFile,
                VideoModelFile.FromBase64("aW5saW5lLXZpZGVv", "video/mp4"),
                VideoUrlFile,
                VideoModelFile.FromUrl("https://example.com/capped.mp4", "video/mp4"),
                VideoModelFile.FromUrl("https://example.com/untyped.png"),
            },
        };

        var start = await CreateModel(handler)
            .DoStartAsync(options.ToCall(), CancellationToken.None);
        var serialized = JsonSerializer.Serialize((JsonElement)start.Operation!);
        JsonAssert.Equal(
            OperationJson.Parse(serialized),
            "{\"taskId\":\"task-123\",\"resolvedInputs\":{\"imageCount\":2,\"referenceVideoIndices\":[2,4]}}"
        );
        Assert.DoesNotContain("https://", serialized);
        Assert.DoesNotContain(PromptText, serialized);
        Assert.DoesNotContain("aW5saW5lLXZpZGVv", serialized);
        JsonAssert.Equal(
            RequestBody(handler)!["content"],
            "[{\"type\":\"text\",\"text\":\""
                + PromptText
                + "\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\""
                + ImageUrl
                + "\"},\"role\":\"reference_image\"},{\"type\":\"image_url\",\"image_url\":{\"url\":\"https://example.com/untyped.png\"},\"role\":\"reference_image\"},{\"type\":\"video_url\",\"video_url\":{\"url\":\""
                + VideoUrl
                + "\"},\"role\":\"reference_video\"},{\"type\":\"video_url\",\"video_url\":{\"url\":\"data:video/mp4;base64,aW5saW5lLXZpZGVv\"},\"role\":\"reference_video\"},{\"type\":\"video_url\",\"video_url\":{\"url\":\""
                + VideoUrl
                + "\"},\"role\":\"reference_video\"}]"
        );

        var restored = OperationJson.Parse(serialized);
        var status = await CreateModel(handler)
            .DoStatusAsync(options.StatusCall(restored), CancellationToken.None);
        Assert.Equal("completed", status.Status);
        JsonAssert.Equal(
            Resolve(status.ProviderMetadata, "resolvedInputs"),
            restored.GetProperty("resolvedInputs").GetRawText()
        );
        var generated = await CreateModel(handler)
            .DoGenerateAsync(options.ToCall(), CancellationToken.None);
        JsonAssert.Equal(
            Resolve(generated.ProviderMetadata, "resolvedInputs"),
            "{\"imageCount\":2,\"referenceVideoUrls\":[\"" + VideoUrl + "\",\"" + VideoUrl + "\"]}"
        );
        Assert.Equal(
            start.Warnings.Select(w => (w.Type, w.Feature, w.Details)),
            generated.Warnings.Select(w => (w.Type, w.Feature, w.Details))
        );
    }

    [Theory]
    [InlineData("standalone frame", "MiniMax-H3", "image", 1)]
    [InlineData("first and last frames", "MiniMax-H3", "frames", 2)]
    [InlineData("H3-Max references", "MiniMax-H3-Max", "none", 0)]
    [InlineData("H3-Max frame", "MiniMax-H3-Max", "image", 1)]
    [UpstreamTest(
        Prefix + "should omit ignored video indices for $name",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_ignored_video_indices_for_name(
        string name,
        string modelId,
        string kind,
        int imageCount
    )
    {
        Assert.NotEmpty(name);
        var handler = Server();
        var options = new MiniMaxCall { InputReferences = new[] { ImageUrlFile, VideoUrlFile } };
        if (kind == "image")
        {
            options.Image = ImageUrlFile;
        }
        else if (kind == "frames")
        {
            options.FrameImages = new[]
            {
                Frame(ImageUrlFile, "first_frame"),
                Frame(ImageUrlFile, "last_frame"),
            };
        }

        var start = await CreateModel(handler, modelId)
            .DoStartAsync(options.ToCall(), CancellationToken.None);
        var restored = OperationJson.Parse(JsonSerializer.Serialize((JsonElement)start.Operation!));
        JsonAssert.Equal(
            restored,
            "{\"taskId\":\"task-123\",\"resolvedInputs\":{\"imageCount\":"
                + imageCount
                + ",\"referenceVideoIndices\":[]}}"
        );
        var status = await CreateModel(handler, modelId)
            .DoStatusAsync(options.StatusCall(restored), CancellationToken.None);
        JsonAssert.Equal(
            Resolve(status.ProviderMetadata, "resolvedInputs"),
            restored.GetProperty("resolvedInputs").GetRawText()
        );
        var generated = await CreateModel(handler, modelId)
            .DoGenerateAsync(options.ToCall(), CancellationToken.None);
        JsonAssert.Equal(
            Resolve(generated.ProviderMetadata, "resolvedInputs"),
            "{\"imageCount\":" + imageCount + ",\"referenceVideoUrls\":[]}"
        );
        Assert.Equal(
            start.Warnings.Select(w => (w.Type, w.Feature, w.Details)),
            generated.Warnings.Select(w => (w.Type, w.Feature, w.Details))
        );
    }
}
