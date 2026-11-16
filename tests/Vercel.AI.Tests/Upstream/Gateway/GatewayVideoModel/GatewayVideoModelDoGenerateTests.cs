// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel.GatewayVideoModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayVideoModel;

/// <summary>Port of <c>gateway-video-model.test.ts</c> &gt; <c>GatewayVideoModel &gt; doGenerate</c>.</summary>
public sealed class GatewayVideoModelDoGenerateTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-video-model.test.ts::GatewayVideoModel > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_request_headers()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(
            CreateTestModel(handler),
            Call(prompt: "A beautiful sunset over mountains")
        );

        Assert.Equal("https://api.test.com/video-model", handler.Uri);
        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("4", handler.Headers["ai-video-model-specification-version"]);
        Assert.Equal(ModelId, handler.Headers["ai-model-id"]);
        Assert.Equal("text/event-stream", handler.Headers["accept"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should send correct request body with all parameters",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_request_body_with_all_parameters()
    {
        var handler = PrepareJsonResponse(
            "[{\"type\":\"base64\",\"data\":\"base64-1\",\"mediaType\":\"video/mp4\"}]"
        );

        await DoGenerate(
            CreateTestModel(handler),
            Call(
                prompt: "A cat playing piano",
                aspectRatio: "16:9",
                resolution: "1920x1080",
                duration: 5,
                fps: 24,
                seed: 42,
                generateAudio: true,
                providerOptions: "{\"fal\":{\"motionStrength\":0.8}}"
            )
        );

        Assert.True(
            Equal(
                "{\"prompt\":\"A cat playing piano\",\"n\":1,\"aspectRatio\":\"16:9\",\"resolution\":\"1920x1080\",\"duration\":5,\"fps\":24,\"seed\":42,\"generateAudio\":true,\"providerOptions\":{\"fal\":{\"motionStrength\":0.8}}}",
                Body(handler)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass seed when it is 0",
        Coverage = UpstreamCoverage.Covered,
        Commit = "ef2dfb5e9fb7b72287b226eba8b743f809414172"
    )]
    public async Task Should_pass_seed_when_it_is_0()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), Call(prompt: "A simple prompt", seed: 0));

        var requestBody = Body(handler)!.AsObject();
        Assert.True(requestBody.ContainsKey("seed"));
        Assert.Equal(0, (int)requestBody["seed"]!);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit optional parameters when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_optional_parameters_when_not_provided()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), Call(prompt: "A simple prompt"));

        var requestBody = Body(handler)!.AsObject();
        Assert.True(
            Equal("{\"prompt\":\"A simple prompt\",\"n\":1,\"providerOptions\":{}}", requestBody)
        );
        Assert.False(requestBody.ContainsKey("aspectRatio"));
        Assert.False(requestBody.ContainsKey("resolution"));
        Assert.False(requestBody.ContainsKey("duration"));
        Assert.False(requestBody.ContainsKey("fps"));
        Assert.False(requestBody.ContainsKey("seed"));
        Assert.False(requestBody.ContainsKey("generateAudio"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return videos array correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_videos_array_correctly()
    {
        var handler = PrepareJsonResponse(
            "[{\"type\":\"base64\",\"data\":\"base64-video-1\",\"mediaType\":\"video/mp4\"},{\"type\":\"base64\",\"data\":\"base64-video-2\",\"mediaType\":\"video/webm\"}]"
        );

        var result = await DoGenerate(CreateTestModel(handler), Call(n: 2));

        Assert.Equal(
            new[]
            {
                ("base64", (string?)null, (string?)"base64-video-1", (string?)"video/mp4"),
                ("base64", null, "base64-video-2", "video/webm"),
            },
            result.Videos.Select(Video).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return URL-type videos correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_URL_type_videos_correctly()
    {
        var handler = PrepareJsonResponse(
            "[{\"type\":\"url\",\"url\":\"https://example.com/video.mp4\",\"mediaType\":\"video/mp4\"}]"
        );

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(
            new[]
            {
                (
                    "url",
                    (string?)"https://example.com/video.mp4",
                    (string?)null,
                    (string?)"video/mp4"
                ),
            },
            result.Videos.Select(Video).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "accepts uploaded video outcomes",
        Coverage = UpstreamCoverage.Covered,
        Commit = "2e4414481225ed70af7c39326a5f0ef5052a90ca"
    )]
    public async Task Accepts_uploaded_video_outcomes()
    {
        var handler = PrepareJsonResponse("[{\"type\":\"uploaded\",\"mediaType\":\"video/mp4\"}]");

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(
            new[] { ("uploaded", (string?)null, (string?)null, (string?)"video/mp4") },
            result.Videos.Select(Video).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return provider metadata correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_provider_metadata_correctly()
    {
        const string metadata =
            "{\"fal\":{\"videos\":[{\"duration\":5.0,\"fps\":24,\"width\":1280,\"height\":720}]},\"gateway\":{\"routing\":{\"provider\":\"fal\"},\"cost\":\"0.15\"}}";
        var handler = PrepareJsonResponse(providerMetadata: metadata);

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.True(Equal(metadata, Node(result.ProviderMetadata)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle provider metadata without videos field",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_provider_metadata_without_videos_field()
    {
        const string metadata =
            "{\"gateway\":{\"routing\":{\"provider\":\"google\"},\"cost\":\"0.10\"}}";
        var handler = PrepareJsonResponse(providerMetadata: metadata);

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.True(Equal(metadata, Node(result.ProviderMetadata)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle empty provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_provider_metadata()
    {
        var handler = PrepareJsonResponse(providerMetadata: "{}");

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.True(Equal("{}", Node(result.ProviderMetadata)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle undefined provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_undefined_provider_metadata()
    {
        var handler = PrepareJsonResponse();

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return warnings when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_warnings_when_provided()
    {
        var handler = PrepareJsonResponse(
            warnings: "[{\"type\":\"other\",\"message\":\"Duration exceeds maximum\"}]"
        );

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(
            new[] { ("other", (string?)null, (string?)null, (string?)"Duration exceeds maximum") },
            result.Warnings.Select(Warning).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return unsupported warnings correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_unsupported_warnings_correctly()
    {
        const string details =
            "KlingAI image-to-video does not support aspectRatio. The output dimensions are determined by the input image.";
        var handler = PrepareJsonResponse(
            warnings: "[{\"type\":\"unsupported\",\"feature\":\"aspectRatio\",\"details\":\""
                + details
                + "\"}]"
        );

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(
            new[] { ("unsupported", (string?)"aspectRatio", (string?)details, (string?)null) },
            result.Warnings.Select(Warning).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return compatibility warnings correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_compatibility_warnings_correctly()
    {
        const string details = "Resolution was adjusted to nearest supported value.";
        var handler = PrepareJsonResponse(
            warnings: "[{\"type\":\"compatibility\",\"feature\":\"resolution\",\"details\":\""
                + details
                + "\"}]"
        );

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(
            new[] { ("compatibility", (string?)"resolution", (string?)details, (string?)null) },
            result.Warnings.Select(Warning).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should return empty warnings array when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_return_empty_warnings_array_when_not_provided()
    {
        var handler = PrepareJsonResponse();

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_response_metadata()
    {
        var handler = PrepareJsonResponse();

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(ModelId, result.Response.ModelId);
        Assert.NotNull(result.Response.Timestamp);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should merge custom headers with config headers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_merge_custom_headers_with_config_headers()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(
            CreateTestModel(handler),
            Call(headers: new Dictionary<string, string> { ["X-Custom-Header"] = "custom-value" })
        );

        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("custom-value", handler.Headers["x-custom-header"]);
        Assert.Equal("4", handler.Headers["ai-video-model-specification-version"]);
        Assert.Equal(ModelId, handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(
            CreateTestModel(
                handler,
                o11yHeaders: new Dictionary<string, string>
                {
                    ["ai-o11y-deployment-id"] = "dpl_123",
                    ["ai-o11y-environment"] = "production",
                }
            )
        );

        Assert.Equal("dpl_123", handler.Headers["ai-o11y-deployment-id"]);
        Assert.Equal("production", handler.Headers["ai-o11y-environment"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass abort signal to fetch",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_abort_signal_to_fetch()
    {
        var handler = PrepareJsonResponse();

        using var cancellation = new CancellationTokenSource();
        await DoGenerate(CreateTestModel(handler), Call(cancellationToken: cancellation.Token));

        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle API errors correctly",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_API_errors_correctly()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"error\":{\"message\":\"Invalid request\",\"code\":\"invalid_request\"}}",
            HttpStatusCode.BadRequest
        );

        await Assert.ThrowsAnyAsync<Exception>(() => DoGenerate(CreateTestModel(handler)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle authentication errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_authentication_errors()
    {
        var handler = new UpstreamRecordingHandler(
            "{\"error\":{\"message\":\"Unauthorized\",\"code\":\"unauthorized\"}}",
            HttpStatusCode.Unauthorized
        );

        await Assert.ThrowsAnyAsync<Exception>(() => DoGenerate(CreateTestModel(handler)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw on SSE error event with correct message and status",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_on_SSE_error_event_with_correct_message_and_status()
    {
        var handler = new UpstreamRecordingHandler(
            "data: {\"type\":\"error\",\"message\":\"Rate limit exceeded\",\"errorType\":\"rate_limit_exceeded\",\"statusCode\":429,\"param\":null}\n\n"
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            DoGenerate(CreateTestModel(handler))
        );

        Assert.Contains("Rate limit exceeded", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should throw on SSE error event with provider routing failure",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_on_SSE_error_event_with_provider_routing_failure()
    {
        var handler = new UpstreamRecordingHandler(
            "data: {\"type\":\"error\",\"message\":\"All providers failed\",\"errorType\":\"internal_server_error\",\"statusCode\":500,\"param\":null}\n\n"
        );

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            DoGenerate(CreateTestModel(handler))
        );

        Assert.Contains("All providers failed", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should throw on empty SSE stream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_on_empty_SSE_stream()
    {
        var handler = new UpstreamRecordingHandler(string.Empty);

        await Assert.ThrowsAnyAsync<Exception>(() => DoGenerate(CreateTestModel(handler)));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore SSE heartbeat comments and parse data event",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_ignore_SSE_heartbeat_comments_and_parse_data_event()
    {
        var handler = new UpstreamRecordingHandler(
            ":\n\n:\n\ndata: {\"type\":\"result\",\"videos\":" + DefaultVideos + "}\n\n"
        );

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.Equal(
            new[] { ("base64", (string?)null, (string?)"base64-video-1", (string?)"video/mp4") },
            result.Videos.Select(Video).ToArray()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include providerOptions object in request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_providerOptions_object_in_request_body()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(
            CreateTestModel(handler),
            Call(
                prompt: "Test prompt",
                providerOptions: "{\"fal\":{\"motionStrength\":0.8,\"loop\":true},\"google\":{\"enhancePrompt\":true}}"
            )
        );

        Assert.True(
            Equal(
                "{\"prompt\":\"Test prompt\",\"n\":1,\"providerOptions\":{\"fal\":{\"motionStrength\":0.8,\"loop\":true},\"google\":{\"enhancePrompt\":true}}}",
                Body(handler)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle empty provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_empty_provider_options()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler), Call(prompt: "Test prompt"));

        Assert.True(
            Equal("{\"prompt\":\"Test prompt\",\"n\":1,\"providerOptions\":{}}", Body(handler))
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle different model IDs",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_different_model_IDs()
    {
        var handler = PrepareJsonResponse();

        await DoGenerate(CreateTestModel(handler, "fal/luma-ray-2"));

        Assert.Equal("fal/luma-ray-2", handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle complex provider metadata with multiple providers",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_complex_provider_metadata_with_multiple_providers()
    {
        const string metadata =
            "{\"fal\":{\"videos\":[{\"duration\":5.0,\"fps\":24,\"width\":1920,\"height\":1080}],\"usage\":{\"computeUnits\":10}},\"gateway\":{\"routing\":{\"provider\":\"fal\",\"attempts\":[{\"provider\":\"google\",\"success\":false},{\"provider\":\"fal\",\"success\":true}]},\"cost\":\"0.20\",\"marketCost\":\"0.30\",\"generationId\":\"gen-xyz-789\"}}";
        var handler = PrepareJsonResponse(providerMetadata: metadata);

        var result = await DoGenerate(CreateTestModel(handler));

        Assert.True(Equal(metadata, Node(result.ProviderMetadata)));
    }
}
