// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using static Vercel.AI.Tests.Upstream.Xai.XaiImageModel.XaiImageTestSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiImageModel;

/// <summary>Port of <c>xai-image-model.test.ts</c> &gt; <c>XaiImageModel</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Id =
        "packages/xai/src/xai-image-model.test.ts::XaiImageModel > doGenerate::";

    private const string Head =
        "{\"model\":\"grok-imagine-image\",\"n\":1,\"response_format\":\"b64_json\",";

    [Fact]
    [UpstreamTest(
        Id + "should send correct parameters for generation",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_parameters_for_generation()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(Call(aspectRatio: "16:9"), CancellationToken.None);

        var call = Assert.Single(handler.Calls);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal(GenerationsUrl, call.Uri.AbsoluteUri);
        AssertJson(
            Head + "\"prompt\":\"A cute baby sea otter\",\"aspect_ratio\":\"16:9\"}",
            Body(call)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send correct parameters for editing",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_correct_parameters_for_editing()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call("Turn the cat into a dog", files: [PngFile()]),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal(EditsUrl, call.Uri.AbsoluteUri);
        AssertJson(
            Head
                + "\"prompt\":\"Turn the cat into a dog\",\"image\":{\"url\":\"data:image/png;base64,iVBORw==\",\"type\":\"image_url\"}}",
            Body(call)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send URL-based file as image_url",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_URL_based_file_as_image_url()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(
                    "Edit this image",
                    files: [ImageModelFile.FromUrl("https://example.com/input.png")]
                ),
                CancellationToken.None
            );

        AssertJson(
            Head
                + "\"prompt\":\"Edit this image\",\"image\":{\"url\":\"https://example.com/input.png\",\"type\":\"image_url\"}}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send base64 file as data URI",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_base64_file_as_data_URI()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(
                    "Edit this image",
                    files: [ImageModelFile.FromBase64("iVBORw0KGgoAAAANSUhEUgAAAAE=", "image/png")]
                ),
                CancellationToken.None
            );

        AssertJson(
            Head
                + "\"prompt\":\"Edit this image\",\"image\":{\"url\":\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAE=\",\"type\":\"image_url\"}}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send multiple files as images array",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_multiple_files_as_images_array()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(
                    "Combine these images",
                    files: [PngFile(), ImageModelFile.FromFile([255, 216, 255, 224], "image/jpeg")]
                ),
                CancellationToken.None
            );

        var call = Assert.Single(handler.Calls);
        Assert.Equal(EditsUrl, call.Uri.AbsoluteUri);
        AssertJson(
            Head
                + "\"prompt\":\"Combine these images\",\"images\":[{\"url\":\"data:image/png;base64,iVBORw==\",\"type\":\"image_url\"},{\"url\":\"data:image/jpeg;base64,/9j/4A==\",\"type\":\"image_url\"}]}",
            Body(call)
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should send mixed file types as images array",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_send_mixed_file_types_as_images_array()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(
                    "Combine these images",
                    files: [ImageModelFile.FromUrl("https://example.com/input.png"), PngFile()]
                ),
                CancellationToken.None
            );

        AssertJson(
            Head
                + "\"prompt\":\"Combine these images\",\"images\":[{\"url\":\"https://example.com/input.png\",\"type\":\"image_url\"},{\"url\":\"data:image/png;base64,iVBORw==\",\"type\":\"image_url\"}]}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should return base64 images from b64_json response",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_return_base64_images_from_b64_json_response()
    {
        var result = await CreateModel(Server()).DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Single(result.Images);
        Assert.Equal("dGVzdA==", result.Images[0]);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw when an image is blocked by moderation",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_throw_when_an_image_is_blocked_by_moderation()
    {
        var handler = Server(
            "{\"data\":[{\"url\":null,\"b64_json\":null,\"respect_moderation\":false}]}"
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None)
        );

        Assert.Contains(
            "Image generation was blocked due to a content policy violation.",
            error.Message
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass headers",
        Coverage = UpstreamCoverage.Partial,
        Commit = Commit,
        Note = "The port's provider always adds its user-agent header, so the exact header set is checked without user-agent. HttpClient adds content-length, which is also left out."
    )]
    public async Task Should_pass_headers()
    {
        var handler = Server();
        var model = CreateModel(
            handler,
            new Dictionary<string, string> { ["Custom-Provider-Header"] = "provider-header-value" },
            apiKey: null
        );

        await model.DoGenerateAsync(
            Call(
                headers: new Dictionary<string, string>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                }
            ),
            CancellationToken.None
        );

        var call = Assert.Single(handler.Calls);
        var actual = call
            .Headers.Where(pair =>
                !string.Equals(pair.Key, "user-agent", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(pair.Key, "content-length", StringComparison.OrdinalIgnoreCase)
            )
            .ToDictionary(
                pair => pair.Key.ToLowerInvariant(),
                pair =>
                    pair.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase)
                        ? pair.Value.Split(';')[0]
                        : pair.Value
            );
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["content-type"] = "application/json",
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
            },
            actual
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass provider options",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_provider_options()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(xai: "{\"output_format\":\"jpeg\",\"sync_mode\":true}"),
                CancellationToken.None
            );

        AssertJson(
            Head
                + "\"prompt\":\"A cute baby sea otter\",\"output_format\":\"jpeg\",\"sync_mode\":true}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Theory]
    [InlineData("1k")]
    [InlineData("1.5k")]
    [InlineData("2k")]
    [UpstreamTest(
        Id + "should pass %s resolution provider option",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_resolution_provider_option(string resolution)
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(xai: "{\"resolution\":\"" + resolution + "\"}"),
                CancellationToken.None
            );

        AssertJson(
            Head + "\"prompt\":\"A cute baby sea otter\",\"resolution\":\"" + resolution + "\"}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Theory]
    [InlineData("high")]
    [InlineData("auto")]
    [UpstreamTest(
        Id + "should pass %s quality provider option",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_quality_provider_option(string quality)
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(
                Call(xai: "{\"quality\":\"" + quality + "\"}"),
                CancellationToken.None
            );

        AssertJson(
            Head + "\"prompt\":\"A cute baby sea otter\",\"quality\":\"" + quality + "\"}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass user provider option",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_pass_user_provider_option()
    {
        var handler = Server();

        await CreateModel(handler)
            .DoGenerateAsync(Call(xai: "{\"user\":\"example-user-123\"}"), CancellationToken.None);

        AssertJson(
            Head + "\"prompt\":\"A cute baby sea otter\",\"user\":\"example-user-123\"}",
            Body(Assert.Single(handler.Calls))
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should include revised_prompt in providerMetadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_include_revised_prompt_in_providerMetadata()
    {
        var handler = Server(
            "{\"data\":[{\"b64_json\":\"dGVzdA==\",\"revised_prompt\":\"A revised prompt\"}]}"
        );

        var result = await CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None);

        AssertJson(
            "{\"xai\":{\"images\":[{\"revisedPrompt\":\"A revised prompt\"}]}}",
            System.Text.Json.Nodes.JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle API errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_handle_API_errors()
    {
        var handler = new ParityHandler(_ =>
            Error(
                HttpStatusCode.BadRequest,
                "{\"error\":{\"message\":\"Invalid prompt\",\"type\":\"invalid_request_error\"}}"
            )
        );

        var error = await Assert.ThrowsAnyAsync<Vercel.AI.Provider.ApiException>(() =>
            CreateModel(handler).DoGenerateAsync(Call(), CancellationToken.None)
        );

        Assert.Equal("Invalid prompt", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Id + "should respect the abort signal",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_respect_the_abort_signal()
    {
        var handler = Server();
        handler.RespondAsync = async call =>
        {
            await Task.Delay(Timeout.Infinite, call.CancellationToken);
            return ParityHandler.Json(DefaultResponse);
        };
        using var source = new CancellationTokenSource();

        var generate = CreateModel(handler)
            .DoGenerateAsync(Call(cancellationToken: source.Token), source.Token);
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generate);
    }
}
