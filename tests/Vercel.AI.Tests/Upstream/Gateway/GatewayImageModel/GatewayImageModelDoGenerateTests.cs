// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayImageModel;

/// <summary>Port of <c>gateway-image-model.test.ts</c> &gt; <c>GatewayImageModel &gt; doGenerate</c>.</summary>
public sealed class GatewayImageModelDoGenerateTests
{
    private const string Prefix = "packages/gateway/src/gateway-image-model.test.ts::GatewayImageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Prefix + "should send correct request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_headers()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), prompt: "A beautiful sunset over mountains");

        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("4", handler.Headers["ai-image-model-specification-version"]);
        Assert.Equal(GatewayImageModelSupport.ModelId, handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send correct request body with all parameters", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_request_body_with_all_parameters()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\",\"base64-2\"]");

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            prompt: "A cat playing piano",
            n: 2,
            size: "1024x1024",
            aspectRatio: "16:9",
            seed: 42,
            providerOptions: "{\"vertex\":{\"safetySettings\":\"block_none\"}}");

        Assert.True(GatewayImageModelSupport.Equal(
            "{\"prompt\":\"A cat playing piano\",\"n\":2,\"size\":\"1024x1024\",\"aspectRatio\":\"16:9\",\"seed\":42,\"providerOptions\":{\"vertex\":{\"safetySettings\":\"block_none\"}}}",
            GatewayImageModelSupport.Body(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should omit optional parameters when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_omit_optional_parameters_when_not_provided()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), prompt: "A simple prompt");

        var requestBody = GatewayImageModelSupport.Body(handler)!.AsObject();
        Assert.True(GatewayImageModelSupport.Equal("{\"prompt\":\"A simple prompt\",\"n\":1,\"providerOptions\":{}}", requestBody));
        Assert.False(requestBody.ContainsKey("size"));
        Assert.False(requestBody.ContainsKey("aspectRatio"));
        Assert.False(requestBody.ContainsKey("seed"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should return images array correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_images_array_correctly()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-image-1\",\"base64-image-2\"]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), n: 2);

        Assert.Equal(new object?[] { "base64-image-1", "base64-image-2" }, result.Images);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve response retryability", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_response_retryability()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[]", "\"isRetryable\":false");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.False(result.IsRetryable);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return provider metadata correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_provider_metadata_correctly()
    {
        const string metadata = "{\"vertex\":{\"images\":[{\"revisedPrompt\":\"Revised prompt 1\"},{\"revisedPrompt\":\"Revised prompt 2\"}]},\"gateway\":{\"routing\":{\"provider\":\"vertex\"},\"cost\":\"0.08\"}}";
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\",\"base64-2\"]", "\"providerMetadata\":" + metadata);

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), n: 2);

        Assert.True(GatewayImageModelSupport.Equal(metadata, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle provider metadata without images field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_provider_metadata_without_images_field()
    {
        const string metadata = "{\"gateway\":{\"routing\":{\"provider\":\"vertex\"},\"cost\":\"0.04\"}}";
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"providerMetadata\":" + metadata);

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), n: 1);

        Assert.True(GatewayImageModelSupport.Equal(metadata, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_provider_metadata()
    {
        const string metadata = "{}";
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"providerMetadata\":" + metadata);

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), n: 1);

        Assert.True(GatewayImageModelSupport.Equal(metadata, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle undefined provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_undefined_provider_metadata()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return warnings when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_warnings_when_provided()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"warnings\":[{\"type\":\"other\",\"message\":\"Setting not supported\"}]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Single(result.Warnings);
        Assert.Equal("other", result.Warnings[0].Type);
        Assert.Equal("Setting not supported", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return deprecated warnings correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_deprecated_warnings_correctly()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"warnings\":[{\"type\":\"deprecated\",\"setting\":\"size\",\"message\":\"Use `aspectRatio` instead.\"}]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Single(result.Warnings);
        Assert.Equal("deprecated", result.Warnings[0].Type);
        Assert.Equal("size", result.Warnings[0].Setting);
        Assert.Equal("Use `aspectRatio` instead.", result.Warnings[0].Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return unsupported warnings correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_unsupported_warnings_correctly()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"warnings\":[{\"type\":\"unsupported\",\"feature\":\"size\",\"details\":\"This model does not support the `size` option. Use `aspectRatio` instead.\"}]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), size: "1024x1024");

        Assert.Single(result.Warnings);
        Assert.Equal("unsupported", result.Warnings[0].Type);
        Assert.Equal("size", result.Warnings[0].Feature);
        Assert.Equal("This model does not support the `size` option. Use `aspectRatio` instead.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return compatibility warnings correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_compatibility_warnings_correctly()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"warnings\":[{\"type\":\"compatibility\",\"feature\":\"seed\",\"details\":\"Seed support is approximate for this model.\"}]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), seed: 42);

        Assert.Single(result.Warnings);
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Equal("seed", result.Warnings[0].Feature);
        Assert.Equal("Seed support is approximate for this model.", result.Warnings[0].Details);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle mixed warning types", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_mixed_warning_types()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"warnings\":[{\"type\":\"unsupported\",\"feature\":\"size\"},{\"type\":\"compatibility\",\"feature\":\"seed\",\"details\":\"Approximate seed support.\"},{\"type\":\"other\",\"message\":\"Rate limit approaching.\"}]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), size: "1024x1024", seed: 42);

        Assert.Equal(3, result.Warnings.Count);
        Assert.Equal("unsupported", result.Warnings[0].Type);
        Assert.Equal("size", result.Warnings[0].Feature);
        Assert.Null(result.Warnings[0].Details);
        Assert.Equal("compatibility", result.Warnings[1].Type);
        Assert.Equal("seed", result.Warnings[1].Feature);
        Assert.Equal("Approximate seed support.", result.Warnings[1].Details);
        Assert.Equal("other", result.Warnings[2].Type);
        Assert.Equal("Rate limit approaching.", result.Warnings[2].Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return empty warnings array when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_empty_warnings_array_when_not_provided()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_response_metadata()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Equal(GatewayImageModelSupport.ModelId, result.Response.ModelId);
        Assert.NotNull(result.Response.Timestamp);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return usage when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_usage_when_provided()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"usage\":{\"inputTokens\":27,\"outputTokens\":6240,\"totalTokens\":6267}");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Equal(27, result.Usage!.InputTokens);
        Assert.Equal(6240, result.Usage.OutputTokens);
        Assert.Equal(6267, result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return usage with partial token counts", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_usage_with_partial_token_counts()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]", "\"usage\":{\"inputTokens\":10}");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Equal(10, result.Usage!.InputTokens);
        Assert.Null(result.Usage.OutputTokens);
        Assert.Null(result.Usage.TotalTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not include usage when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_include_usage_when_not_provided()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\"]");

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.Null(result.Usage);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge custom headers with config headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_custom_headers_with_config_headers()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            headers: new Dictionary<string, string> { ["X-Custom-Header"] = "custom-value" });

        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("custom-value", handler.Headers["x-custom-header"]);
        Assert.Equal("4", handler.Headers["ai-image-model-specification-version"]);
        Assert.Equal(GatewayImageModelSupport.ModelId, handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(
            handler,
            o11yHeaders: new Dictionary<string, string> { ["ai-o11y-deployment-id"] = "dpl_123", ["ai-o11y-environment"] = "production" }));

        Assert.Equal("dpl_123", handler.Headers["ai-o11y-deployment-id"]);
        Assert.Equal("production", handler.Headers["ai-o11y-environment"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass abort signal to fetch", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_abort_signal_to_fetch()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        using var abortController = new CancellationTokenSource();
        await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), cancellationToken: abortController.Token);

        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle API errors correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_API_errors_correctly()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Invalid request\",\"code\":\"invalid_request\"}}", HttpStatusCode.BadRequest);

        await Assert.ThrowsAnyAsync<GatewayError>(() => GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle authentication errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_authentication_errors()
    {
        var handler = new UpstreamRecordingHandler("{\"error\":{\"message\":\"Unauthorized\",\"code\":\"unauthorized\"}}", HttpStatusCode.Unauthorized);

        await Assert.ThrowsAnyAsync<GatewayError>(() => GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should include providerOptions object in request body", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_providerOptions_object_in_request_body()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(
            GatewayImageModelSupport.CreateTestModel(handler),
            providerOptions: "{\"vertex\":{\"safetySettings\":\"block_none\"},\"openai\":{\"style\":\"vivid\"}}");

        Assert.True(GatewayImageModelSupport.Equal(
            "{\"prompt\":\"Test prompt\",\"n\":1,\"providerOptions\":{\"vertex\":{\"safetySettings\":\"block_none\"},\"openai\":{\"style\":\"vivid\"}}}",
            GatewayImageModelSupport.Body(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_provider_options()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler));

        Assert.True(GatewayImageModelSupport.Equal("{\"prompt\":\"Test prompt\",\"n\":1,\"providerOptions\":{}}", GatewayImageModelSupport.Body(handler)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle different model IDs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_different_model_IDs()
    {
        var handler = GatewayImageModelSupport.PrepareJsonResponse();

        await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler, "openai/dall-e-3"));

        Assert.Equal("openai/dall-e-3", handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle complex provider metadata with multiple providers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_complex_provider_metadata_with_multiple_providers()
    {
        const string metadata = "{\"vertex\":{\"images\":[{\"revisedPrompt\":\"Revised 1\"},{\"revisedPrompt\":\"Revised 2\"}],\"usage\":{\"tokens\":150}},\"gateway\":{\"routing\":{\"provider\":\"vertex\",\"attempts\":[{\"provider\":\"openai\",\"success\":false},{\"provider\":\"vertex\",\"success\":true}]},\"cost\":\"0.08\",\"marketCost\":\"0.12\",\"generationId\":\"gen-xyz-789\"}}";
        var handler = GatewayImageModelSupport.PrepareJsonResponse("[\"base64-1\",\"base64-2\"]", "\"providerMetadata\":" + metadata);

        var result = await GatewayImageModelSupport.DoGenerate(GatewayImageModelSupport.CreateTestModel(handler), n: 2);

        Assert.True(GatewayImageModelSupport.Equal(metadata, JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())));
    }
}
