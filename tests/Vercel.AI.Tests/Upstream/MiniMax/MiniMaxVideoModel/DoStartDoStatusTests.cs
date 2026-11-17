// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.MiniMax;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
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

    private static Task<Vercel.AI.Operations.VideoStatusResult> Status(
        ParityHandler handler,
        MiniMaxCall? call = null,
        string taskId = TaskId
    )
    {
        return CreateModel(handler)
            .DoStatusAsync(
                (call ?? new MiniMaxCall()).StatusCall(StatusOperation(taskId)),
                CancellationToken.None
            );
    }

    private static ParityHandler PollReturns(
        string body,
        params (string Name, string Value)[] headers
    )
    {
        return new ParityHandler(_ => ParityHandler.Json(body, headers));
    }

    [Theory]
    [InlineData("queued")]
    [InlineData("running")]
    [InlineData("unknown")]
    [InlineData(null)]
    [InlineData("undefined")]
    [UpstreamTest(
        Prefix + "should return pending after one GET for status %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_pending_after_one_GET_for_status(string? status)
    {
        var body = status switch
        {
            null => "{\"task\":{\"status\":null}}",
            "undefined" => "{\"task\":{}}",
            _ => "{\"task\":{\"status\":\"" + status + "\"}}",
        };
        var handler = PollReturns(body);

        var result = await Status(handler);

        Assert.Equal("pending", result.Status);
        Assert.Equal("MiniMax-H3", result.Response.ModelId);
        Assert.NotEqual(default, result.Response.Timestamp);
        Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Get, handler.Calls[0].Method);
    }

    [Theory]
    [InlineData("failed", "MiniMax video generation failed")]
    [InlineData("cancelled", "MiniMax video generation was cancelled")]
    [InlineData("expired", "MiniMax video generation request expired")]
    [UpstreamTest(
        Prefix + "should return an error result for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_an_error_result_for_status(string status, string message)
    {
        var handler = PollReturns(
            "{\"task\":{\"status\":\"" + status + "\"}}",
            ("x-minimax-request-id", "status-request")
        );

        var result = await Status(handler);

        Assert.Equal("error", result.Status);
        Assert.Equal(message + ". Task ID: " + TaskId, result.Error);
        Assert.Equal("status-request", result.Response.Headers!["x-minimax-request-id"]);
        Assert.Single(handler.Calls);
    }

    [Theory]
    [InlineData("1027")]
    [InlineData("\"content_policy\"")]
    [UpstreamTest(
        Prefix + "should include task error code %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_task_error_code(string code)
    {
        var handler = PollReturns(
            "{\"task\":{\"status\":\"failed\",\"error\":{\"code\":"
                + code
                + ",\"message\":\"Rejected\"}}}"
        );

        var result = await Status(handler);

        Assert.Equal("error", result.Status);
        Assert.Equal(
            "MiniMax video generation failed: Rejected ("
                + code.Trim('"')
                + "). Task ID: "
                + TaskId,
            result.Error
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(",\"content\":{}")]
    [InlineData(",\"content\":{\"url\":\"\"}")]
    [UpstreamTest(
        Prefix + "should reject a succeeded task without a video URL (%j)",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_a_succeeded_task_without_a_video_URL(string content)
    {
        var handler = PollReturns("{\"task\":{\"status\":\"succeeded\"" + content + "}}");

        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            Status(handler)
        );

        Assert.Equal("MINIMAX_VIDEO_GENERATION_ERROR", error.Name);
        Assert.Equal(
            "MiniMax video generation completed but no video URL was returned. Task ID: " + TaskId,
            error.Message
        );
        Assert.Single(handler.Calls);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"task_id\":null}")]
    [InlineData("{\"task_id\":\"\"}")]
    [UpstreamTest(
        Prefix + "should reject missing task ID %s without polling",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_missing_task_ID_without_polling(string body)
    {
        var handler = Server(createBody: body);

        var error = await Assert.ThrowsAsync<MiniMaxVideoGenerationException>(() =>
            CreateModel(handler).DoStartAsync(new MiniMaxCall().ToCall(), CancellationToken.None)
        );

        Assert.Equal("MINIMAX_VIDEO_GENERATION_ERROR", error.Name);
        Assert.Single(handler.Calls);
    }

    [Theory]
    [InlineData("start")]
    [InlineData("status")]
    [UpstreamTest(
        Prefix + "should surface HTTP errors from %s without retrying",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_surface_HTTP_errors_from_method_without_retrying(string method)
    {
        var url = method == "start" ? CreateUrl : PollUrl;
        var handler = new ParityHandler(_ =>
            ErrorResponse(429, ErrorEnvelope("Rate limited", 429))
        );
        var model = CreateModel(handler);

        var error = await Assert.ThrowsAsync<RateLimitException>(() =>
            method == "start"
                ? model.DoStartAsync(new MiniMaxCall().ToCall(), CancellationToken.None)
                : model.DoStatusAsync(
                    new MiniMaxCall().StatusCall(StatusOperation()),
                    CancellationToken.None
                )
        );

        Assert.Equal(429, error.StatusCode);
        Assert.Equal("Rate limited", error.Message);
        Assert.Equal(url, error.Url);
        Assert.Single(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge and override auth headers separately on start and status",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_and_override_auth_headers_separately_on_start_and_status()
    {
        var handler = Server();
        var model = CreateModel(handler);

        await model.DoStartAsync(
            new MiniMaxCall
            {
                Headers = new Dictionary<string, string> { ["x-request"] = "start" },
            }.ToCall(),
            CancellationToken.None
        );
        await model.DoStatusAsync(
            new MiniMaxCall
            {
                Headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer override",
                    ["x-request"] = "status",
                },
            }.StatusCall(StatusOperation()),
            CancellationToken.None
        );

        Assert.Equal("Bearer test-key", handler.Calls[0].Header("authorization"));
        Assert.Equal("start", handler.Calls[0].Header("x-request"));
        Assert.Equal("Bearer override", handler.Calls[1].Header("authorization"));
        Assert.Equal("status", handler.Calls[1].Header("x-request"));
    }

    [Theory]
    [InlineData(".", "%252E")]
    [InlineData("..", "%252E%252E")]
    [UpstreamTest(
        Prefix + "should preserve the $taskId task ID as a URL path segment",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_taskId_task_ID_as_a_URL_path_segment(
        string taskId,
        string encodedTaskId
    )
    {
        var handler = new ParityHandler(_ => ParityHandler.Json(SucceededStatusResponse));

        await Status(handler, taskId: taskId);

        Assert.Equal(
            new[] { "/v2/query/video_generation/" + encodedTaskId },
            handler.Calls.Select(call => call.Uri.AbsolutePath)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should encode the task ID and forward each abort signal to the custom fetch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_the_task_ID_and_forward_each_abort_signal_to_the_custom_fetch()
    {
        const string taskId = "task/with?query=value#fragment";
        using var startSource = new CancellationTokenSource();
        using var statusSource = new CancellationTokenSource();
        var forwarded = new List<bool>();
        var handler = new ParityHandler(call =>
        {
            // The call's token is linked to the caller's token; a never-cancellable token would mean it was dropped.
            forwarded.Add(call.CancellationToken.CanBeCanceled);
            return ParityHandler.Json(
                call.Method == HttpMethod.Post
                    ? "{\"task_id\":\"" + taskId + "\"}"
                    : SucceededStatusResponse
            );
        });
        var model = CreateModel(handler);

        var start = await model.DoStartAsync(
            new MiniMaxCall { CancellationToken = startSource.Token }.ToCall(),
            CancellationToken.None
        );
        Assert.Single(handler.Calls);
        await model.DoStatusAsync(
            new MiniMaxCall { CancellationToken = statusSource.Token }.StatusCall(
                (JsonElement)start.Operation!
            ),
            statusSource.Token
        );

        Assert.Equal(2, handler.Calls.Count);
        Assert.Equal(new[] { true, true }, forwarded);
        Assert.Equal(
            BaseUrl + "/v2/query/video_generation/" + Uri.EscapeDataString(taskId),
            handler.Calls[1].Uri.AbsoluteUri
        );
    }

    [Theory]
    [InlineData("start")]
    [InlineData("status")]
    [UpstreamTest(
        Prefix + "should propagate abort errors from %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_propagate_abort_errors_from_method(string method)
    {
        using var source = new CancellationTokenSource();
        var handler = new ParityHandler(call =>
        {
            source.Cancel();
            call.CancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Expected abort");
        });
        var model = CreateModel(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            method == "start"
                ? model.DoStartAsync(
                    new MiniMaxCall { CancellationToken = source.Token }.ToCall(),
                    source.Token
                )
                : model.DoStatusAsync(
                    new MiniMaxCall { CancellationToken = source.Token }.StatusCall(
                        StatusOperation()
                    ),
                    source.Token
                )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should strip caller headers on cross-origin status redirects",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_strip_caller_headers_on_cross_origin_status_redirects()
    {
        var handler = new ParityHandler(call =>
        {
            if (call.Uri.Host == "api.example.com")
            {
                var redirect = new System.Net.Http.HttpResponseMessage(
                    System.Net.HttpStatusCode.Found
                );
                redirect.Headers.Location = new Uri("https://cdn.example.com/status");
                return redirect;
            }

            return ParityHandler.Json(SucceededStatusResponse);
        });

        await Status(
            handler,
            new MiniMaxCall
            {
                Headers = new Dictionary<string, string> { ["x-secret"] = "private" },
            }
        );

        Assert.Equal(2, handler.Calls.Count);
        Assert.Equal("Bearer test-key", handler.Calls[0].Header("authorization"));
        Assert.Null(handler.Calls[1].Header("authorization"));
        Assert.Null(handler.Calls[1].Header("x-secret"));
    }
}
