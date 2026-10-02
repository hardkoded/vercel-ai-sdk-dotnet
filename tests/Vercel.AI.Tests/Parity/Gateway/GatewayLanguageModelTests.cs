// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Parity.Gateway;

/// <summary>Ports Gateway language-model request, response, and error-mapping tests.</summary>
public sealed class GatewayLanguageModelTests
{
    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > constructor::should set basic properties", Coverage = UpstreamCoverage.Covered)]
    public void SetsModelIdProviderAndSpecificationVersion()
    {
        var model = Model(new RecordingHandler());

        Assert.Equal("test-model", model.ModelId);
        Assert.Equal("gateway", model.Provider);
        Assert.Equal("V4", model.SpecificationVersion);
    }

    [Fact]
    public async Task SendsV4ProtocolHeadersAndTheDefaultLanguageRoute()
    {
        var custom = new RecordingHandler();
        var options = Prompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Header"] = "test-value" };
        await Model(custom).DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("https://api.test.com/language-model", custom.Uri);
        Assert.Equal(HttpMethod.Post, custom.Method);
        Assert.Equal("Bearer test-token", custom.Headers["Authorization"]);
        Assert.Equal("test-value", custom.Headers["Custom-Header"]);
        Assert.Equal("4", custom.Headers["ai-language-model-specification-version"]);
        Assert.Equal("test-model", custom.Headers["ai-language-model-id"]);
        Assert.Equal("false", custom.Headers["ai-language-model-streaming"]);
        Assert.Equal("0.0.1", custom.Headers["ai-gateway-protocol-version"]);
        Assert.Equal("api-key", custom.Headers["ai-gateway-auth-method"]);
        Assert.Null(JsonNode.Parse(custom.Body)!["abortSignal"]);

        var defaults = new RecordingHandler();
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "secret" }, defaults);
        await provider.LanguageModel("openai/gpt-4.1-mini").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal("https://ai-gateway.vercel.sh/v4/ai/language-model", defaults.Uri);
        Assert.Equal("Bearer secret", defaults.Headers["Authorization"]);
        Assert.Equal("4", defaults.Headers["ai-language-model-specification-version"]);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should extract text response", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsTextFromAnObjectContentPart()
    {
        var handler = new RecordingHandler { ResponseHeaders = { ["x-gateway-test"] = "1" } };
        var result = await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);

        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello, World!", text.Text);
        Assert.Equal("Hello, World!", result.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("test-id", result.ResponseId);
        Assert.Equal("test-model", result.ResponseModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1711115037), result.ResponseTimestamp);
        Assert.Equal("1", result.ResponseHeaders["x-gateway-test"]);
        Assert.Contains("Hello, World!", result.RawResponse);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should extract usage information", Coverage = UpstreamCoverage.Covered)]
    public async Task ExtractsPromptAndCompletionTokens()
    {
        var handler = new RecordingHandler { ResponseBody = GenerateBody("Test", promptTokens: 10, completionTokens: 20) };
        var result = await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(20, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should forward warnings returned by the gateway", Coverage = UpstreamCoverage.Covered)]
    public async Task ForwardsGatewayWarnings()
    {
        var handler = new RecordingHandler
        {
            ResponseBody = GenerateBody(
                "Hello, World!",
                "[{\"type\":\"compatibility\",\"feature\":\"maxOutputTokens\",\"details\":\"lowered to 38000\"},{\"type\":\"other\",\"message\":\"from provider\"}]"),
        };
        var result = await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Equal(2, result.Warnings.Count);
        Assert.Equal("compatibility", result.Warnings[0].Type);
        Assert.Contains("maxOutputTokens", result.Warnings[0].Message);
        Assert.Contains("lowered to 38000", result.Warnings[0].Message);
        Assert.Equal("other", result.Warnings[1].Type);
        Assert.Equal("from provider", result.Warnings[1].Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should default warnings to an empty array when absent", Coverage = UpstreamCoverage.Covered)]
    public async Task DefaultsWarningsToEmpty()
    {
        var result = await Model(new RecordingHandler()).DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should remove abortSignal from the request body", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsTheCancellationTokenFromTheGenerateBody()
    {
        var handler = new RecordingHandler();
        using var source = new CancellationTokenSource();
        await Model(handler).DoGenerateAsync(Prompt(), source.Token);

        Assert.Null(JsonNode.Parse(handler.Body)!["abortSignal"]);
        Assert.Contains("\"prompt\"", handler.Body);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should pass abortSignal to fetch when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateObservesTheCallerCancellationToken()
    {
        var handler = new RecordingHandler { WaitForCancel = true };
        using var source = new CancellationTokenSource();
        var call = Model(handler).DoGenerateAsync(Prompt(), source.Token);
        await WaitForStart(handler);
        source.Cancel();

        await Assert.ThrowsAnyAsync<Exception>(() => call);
        Assert.True(handler.ObservedCancel);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should not pass abortSignal to fetch when not provided", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateCompletesWhenNoCancellationTokenIsPassed()
    {
        var handler = new RecordingHandler();
        var result = await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.False(handler.ObservedCancel);
        Assert.Equal("Hello, World!", result.Text);
        Assert.Null(JsonNode.Parse(handler.Body)!["abortSignal"]);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should include o11y headers in the request", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsObservabilityHeaders()
    {
        var handler = new RecordingHandler();
        await Model(handler, options => options.ObservabilityHeaders = O11y()).DoGenerateAsync(Prompt(), CancellationToken.None);

        AssertO11y(handler);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should convert API call errors to Gateway errors", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateMapsAuthenticationErrors()
    {
        var error = await GenerateError<GatewayAuthenticationError>(401, "{\"error\":{\"message\":\"Invalid API key provided\",\"type\":\"authentication_error\"}}");

        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("authentication_error", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should handle malformed error responses", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateMapsANonJsonErrorBody()
    {
        var error = await GenerateError<GatewayResponseError>(500, "Not JSON");

        Assert.Equal(500, error.StatusCode);
        Assert.Equal("response_error", error.Type);
        Assert.Equal("Not JSON", error.Response);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should handle rate limit errors", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateMapsRateLimitErrors()
    {
        var error = await GenerateError<GatewayRateLimitError>(429, "{\"error\":{\"message\":\"Rate limit exceeded. Try again later.\",\"type\":\"rate_limit_exceeded\"}}");

        Assert.Equal("Rate limit exceeded. Try again later.", error.Message);
        Assert.Equal(429, error.StatusCode);
        Assert.Equal("rate_limit_exceeded", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should handle invalid request errors", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateMapsInvalidRequestErrors()
    {
        var error = await GenerateError<GatewayInvalidRequestError>(400, "{\"error\":{\"message\":\"Invalid prompt format\",\"type\":\"invalid_request_error\"}}");

        Assert.Equal("Invalid prompt format", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("invalid_request_error", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Image part encoding::should not modify prompt without image parts", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateLeavesATextPromptUnchanged()
    {
        var handler = new RecordingHandler();
        await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);
        var content = JsonNode.Parse(handler.Body)!["prompt"]![0]!["content"]!;

        Assert.Equal("text", content[0]!["type"]!.GetValue<string>());
        Assert.Equal("Hello", content[0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Image part encoding::should encode Uint8Array image part to inline base64 data with default mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateEncodesInlineImageBytes()
    {
        var handler = new RecordingHandler();
        var bytes = new byte[] { 1, 2, 3, 4 };
        await Model(handler).DoGenerateAsync(ImagePrompt("Describe this image:", "image/jpeg", bytes, null), CancellationToken.None);

        AssertFilePart(handler, 1, "image/jpeg", "data", Convert.ToBase64String(bytes));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Image part encoding::should encode Uint8Array image part to inline base64 data with specified mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateEncodesPngBytes()
    {
        var handler = new RecordingHandler();
        var bytes = new byte[] { 5, 6, 7, 8 };
        await Model(handler).DoGenerateAsync(FileOnlyPrompt("image/png", bytes, null), CancellationToken.None);

        AssertFilePart(handler, 0, "image/png", "data", Convert.ToBase64String(bytes));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Image part encoding::should not modify image part with URL", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateLeavesImageUrlsInline()
    {
        var handler = new RecordingHandler();
        await Model(handler).DoGenerateAsync(ImagePrompt("Image URL:", "image/jpeg", null, "https://example.com/image.jpg"), CancellationToken.None);

        AssertFilePart(handler, 1, "image/jpeg", "url", "https://example.com/image.jpg");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Image part encoding::should handle mixed content types correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateEncodesMixedFileParts()
    {
        var handler = new RecordingHandler();
        var bytes = new byte[] { 1, 2, 3, 4 };
        await Model(handler).DoGenerateAsync(MixedPrompt(bytes), CancellationToken.None);

        AssertMixed(handler, bytes);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::should handle various error types with proper conversion", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateMapsInvalidModelAndServerErrors()
    {
        var invalid = await GenerateError<GatewayInvalidRequestError>(400, "{\"error\":{\"message\":\"Invalid request format\",\"type\":\"invalid_request_error\"}}");
        Assert.Equal("Invalid request format", invalid.Message);
        Assert.Equal(400, invalid.StatusCode);
        Assert.Equal("invalid_request_error", invalid.Type);

        var missing = await GenerateError<GatewayModelNotFoundError>(404, "{\"error\":{\"message\":\"Model xyz not found\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"xyz\"}}}");
        Assert.Equal("Model xyz not found", missing.Message);
        Assert.Equal(404, missing.StatusCode);
        Assert.Equal("model_not_found", missing.Type);
        Assert.Equal("xyz", missing.ModelId);

        var server = await GenerateError<GatewayInternalServerError>(500, "{\"error\":{\"message\":\"Database connection failed\",\"type\":\"internal_server_error\"}}");
        Assert.Equal("Database connection failed", server.Message);
        Assert.Equal(500, server.StatusCode);
        Assert.Equal("internal_server_error", server.Type);
        var cause = Assert.IsAssignableFrom<Exception>(server.Cause);
        Assert.Contains("Database connection failed", cause.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Gateway error handling for malformed responses::should include actual response body when APICallError has no data", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateKeepsAMalformedErrorObject()
    {
        var error = await GenerateError<GatewayResponseError>(404, "{\"ferror\":{\"message\":\"Model not found\",\"type\":\"model_not_found\"}}");
        var body = Assert.IsAssignableFrom<JsonNode>(error.Response);

        Assert.Equal("Model not found", body["ferror"]!["message"]!.GetValue<string>());
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate > Gateway error handling for malformed responses::should use raw response body when JSON parsing fails", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateKeepsAnUnparseableErrorBody()
    {
        var error = await GenerateError<GatewayResponseError>(500, "invalid json response");

        Assert.Equal("invalid json response", error.Response);
        Assert.NotNull(error.ValidationError);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should stream text deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamsTextDeltasAndFinishUsage()
    {
        var parts = await Stream(Sse(
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"text-delta\",\"textDelta\":\", \"}",
            "{\"type\":\"text-delta\",\"textDelta\":\"World!\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}"));

        Assert.Equal(new[] { "Hello", ", ", "World!" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        var finish = Assert.Single(parts.OfType<FinishStreamPart>());
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Equal(10, finish.Usage.InputTokens);
        Assert.Equal(20, finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should forward server stream-start warnings exactly once", Coverage = UpstreamCoverage.Covered)]
    public async Task ForwardsStreamStartWarningsOnce()
    {
        var parts = await Stream(Sse(
            "{\"type\":\"stream-start\",\"warnings\":[{\"type\":\"other\",\"message\":\"from provider\"}]}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}"));
        var starts = parts.OfType<StreamStartStreamPart>().ToArray();

        var start = Assert.Single(starts);
        var warning = Assert.Single(start.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("from provider", warning.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should preserve explicit mid-stream provider error metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesProviderErrorMetadataOnTheStream()
    {
        var parts = await Stream(Sse(
            "{\"type\":\"text-delta\",\"textDelta\":\"Partial output\"}",
            "{\"type\":\"error\",\"error\":{\"message\":\"Upstream provider overloaded\",\"type\":\"provider_overloaded\",\"statusCode\":503,\"isRetryable\":true}}"));
        var error = Assert.IsType<ErrorStreamPart>(parts[1]);

        Assert.Equal("Partial output", Assert.IsType<TextDeltaStreamPart>(parts[0]).Delta);
        Assert.Contains("Upstream provider overloaded", error.Message);
        Assert.Contains("provider_overloaded", error.Message);
        Assert.Contains("503", error.Message);
        Assert.Contains("\"isRetryable\":true", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should pass streaming headers", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsTheStreamingHeader()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("Test") };
        await Read(Model(handler), Prompt());

        Assert.Equal("https://api.test.com/language-model", handler.Uri);
        Assert.Equal("4", handler.Headers["ai-language-model-specification-version"]);
        Assert.Equal("test-model", handler.Headers["ai-language-model-id"]);
        Assert.Equal("true", handler.Headers["ai-language-model-streaming"]);
        Assert.Equal("0.0.1", handler.Headers["ai-gateway-protocol-version"]);
        Assert.Equal("api-key", handler.Headers["ai-gateway-auth-method"]);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should remove abortSignal from the streaming request body", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsTheCancellationTokenFromTheStreamBody()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("Test content") };
        using var source = new CancellationTokenSource();
        await Read(Model(handler), Prompt(), source.Token);

        Assert.Null(JsonNode.Parse(handler.Body)!["abortSignal"]);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should pass abortSignal to fetch when provided for streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamObservesTheCallerCancellationToken()
    {
        var handler = new RecordingHandler { WaitForCancel = true, ResponseBody = TextStream("Test content") };
        using var source = new CancellationTokenSource();
        var call = Read(Model(handler), Prompt(), source.Token);
        await WaitForStart(handler);
        source.Cancel();

        await Assert.ThrowsAnyAsync<Exception>(() => call);
        Assert.True(handler.ObservedCancel);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should not pass abortSignal to fetch when not provided for streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamCompletesWhenNoCancellationTokenIsPassed()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("Test content") };
        var parts = await Read(Model(handler), Prompt(), CancellationToken.None);

        Assert.False(handler.ObservedCancel);
        Assert.NotEmpty(parts);
        Assert.Null(JsonNode.Parse(handler.Body)!["abortSignal"]);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should include o11y headers in the streaming request", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsObservabilityHeaders()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("Test content") };
        await Read(Model(handler, options => options.ObservabilityHeaders = O11y()), Prompt());

        AssertO11y(handler);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should convert API call errors to Gateway errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamMapsRateLimitErrors()
    {
        var error = await StreamError<GatewayRateLimitError>(429, "{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"}}");

        Assert.Equal("Rate limit exceeded", error.Message);
        Assert.Equal(429, error.StatusCode);
        Assert.Equal("rate_limit_exceeded", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should handle authentication errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamMapsAuthenticationErrors()
    {
        var error = await StreamError<GatewayAuthenticationError>(401, "{\"error\":{\"message\":\"Authentication failed for streaming\",\"type\":\"authentication_error\"}}");

        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("authentication_error", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should handle invalid request errors in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamMapsInvalidRequestErrors()
    {
        var error = await StreamError<GatewayInvalidRequestError>(400, "{\"error\":{\"message\":\"Invalid streaming request\",\"type\":\"invalid_request_error\"}}");

        Assert.Equal("Invalid streaming request", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("invalid_request_error", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream::should handle malformed error responses in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamMapsANonJsonErrorBody()
    {
        var error = await StreamError<GatewayResponseError>(500, "Invalid JSON for streaming");

        Assert.Equal(500, error.StatusCode);
        Assert.Equal("response_error", error.Type);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Image part encoding::should not modify prompt without image parts", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamLeavesATextPromptUnchanged()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("response") };
        await Read(Model(handler), Prompt());
        var content = JsonNode.Parse(handler.Body)!["prompt"]![0]!["content"]!;

        Assert.Equal("text", content[0]!["type"]!.GetValue<string>());
        Assert.Equal("Hello", content[0]!["text"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Image part encoding::should encode Uint8Array image part to inline base64 data with default mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamEncodesInlineImageBytes()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("response") };
        var bytes = new byte[] { 1, 2, 3, 4 };
        await Read(Model(handler), ImagePrompt("Describe:", "image/jpeg", bytes, null));

        AssertFilePart(handler, 1, "image/jpeg", "data", Convert.ToBase64String(bytes));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Image part encoding::should encode Uint8Array image part to inline base64 data with specified mime type", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamEncodesPngBytes()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("response") };
        var bytes = new byte[] { 5, 6, 7, 8 };
        await Read(Model(handler), ImagePrompt("Describe:", "image/png", bytes, null));

        AssertFilePart(handler, 1, "image/png", "data", Convert.ToBase64String(bytes));
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Image part encoding::should not modify image part with URL", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamLeavesImageUrlsInline()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("response") };
        await Read(Model(handler), ImagePrompt("URL:", "image/jpeg", null, "https://example.com/image.jpg"));

        AssertFilePart(handler, 1, "image/jpeg", "url", "https://example.com/image.jpg");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Image part encoding::should handle mixed content types correctly for streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamEncodesMixedFileParts()
    {
        var handler = new RecordingHandler { ResponseBody = TextStream("response") };
        var bytes = new byte[] { 1, 2, 3, 4 };
        await Read(Model(handler), MixedPrompt(bytes));

        AssertMixed(handler, bytes);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Error handling::should not double-wrap existing Gateway errors", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotWrapAnExistingGatewayError()
    {
        var existing = new GatewayAuthenticationError("Already a Gateway error", 401);
        var handler = new RecordingHandler { Throw = existing };

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None));

        Assert.Same(existing, error);
        Assert.Equal("Already a Gateway error", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Error handling::should handle network errors gracefully", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateWrapsNetworkErrors()
    {
        var network = new InvalidOperationException("Network connection failed");
        var handler = new RecordingHandler { Throw = network };
        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None));

        Assert.Contains("Gateway request failed: Network connection failed", error.Message);
        Assert.Same(network, error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Error handling::should handle network errors gracefully in streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamWrapsNetworkErrors()
    {
        var network = new InvalidOperationException("Network connection failed");
        var handler = new RecordingHandler { Throw = network };
        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => Read(Model(handler), Prompt()));

        Assert.Contains("Gateway request failed: Network connection failed", error.Message);
        Assert.Same(network, error.Cause);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doStream > Error handling::should preserve error cause chain", Coverage = UpstreamCoverage.Covered)]
    public async Task AuthenticationErrorKeepsItsCause()
    {
        var error = await GenerateError<GatewayAuthenticationError>(401, "{\"error\":{\"message\":\"Token expired\",\"type\":\"authentication_error\"}}");

        Assert.NotNull(error.Cause);
        Assert.Contains("Token expired", Assert.IsAssignableFrom<Exception>(error.Cause).Message);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > raw chunks filtering::should filter raw chunks based on includeRawChunks option", Coverage = UpstreamCoverage.Covered)]
    public async Task FiltersRawChunksUnlessRequested()
    {
        var options = Prompt();
        options.IncludeRawChunks = false;
        var parts = await Stream(RawStream(), options);

        Assert.DoesNotContain(parts, part => part is RawStreamPart);
        Assert.Equal(new[] { "Hello", " world" }, parts.OfType<TextDeltaStreamPart>().Select(part => part.Delta).ToArray());
        Assert.Single(parts.OfType<StreamStartStreamPart>());
        Assert.Single(parts.OfType<FinishStreamPart>());
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > raw chunks filtering::should include raw chunks when includeRawChunks is true", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesRawChunksWhenRequested()
    {
        var options = Prompt();
        options.IncludeRawChunks = true;
        var parts = await Stream(Sse(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"raw\",\"rawValue\":{\"id\":\"test-chunk\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}"), options);
        var raw = Assert.Single(parts.OfType<RawStreamPart>());

        Assert.Contains("test-chunk", raw.RawJson);
        Assert.Contains("Hello", raw.RawJson);
        Assert.Equal("Hello", Assert.Single(parts.OfType<TextDeltaStreamPart>()).Delta);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > timestamp conversion::should convert timestamp strings to Date objects in response-metadata chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task ConvertsResponseMetadataTimestamps()
    {
        var parts = await MetadataStream("\"2023-12-07T10:30:00.000Z\"");
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        var expected = DateTimeOffset.Parse("2023-12-07T10:30:00.000Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        Assert.Equal(4, parts.Count);
        Assert.IsType<StreamStartStreamPart>(parts[0]);
        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Equal(expected, metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > timestamp conversion::should not modify timestamp if it is already a Date object", Coverage = UpstreamCoverage.Covered)]
    public async Task ParsesAnIsoTimestampIntoAnOffset()
    {
        var parts = await MetadataStream("\"2023-12-07T10:30:00.000Z\"");
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);

        Assert.Equal(DateTimeOffset.Parse("2023-12-07T10:30:00.000Z", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind), metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > timestamp conversion::should not modify response-metadata chunks without timestamp", Coverage = UpstreamCoverage.Covered)]
    public async Task LeavesAMissingTimestampUnset()
    {
        var parts = await Stream(Sse(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"response-metadata\",\"id\":\"test-id\",\"modelId\":\"test-model\"}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}"));
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);

        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Null(metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > timestamp conversion::should handle null timestamp values gracefully", Coverage = UpstreamCoverage.Covered)]
    public async Task LeavesANullTimestampUnset()
    {
        var parts = await MetadataStream("null");
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);

        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Null(metadata.Timestamp);
        Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(parts[2]).Delta);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass provider routing order for doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsProviderOrder()
    {
        await AssertGatewayOption(false, "{\"order\":[\"bedrock\",\"anthropic\"]}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass single provider in order array", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsASingleProviderOrder()
    {
        await AssertGatewayOption(false, "{\"order\":[\"openai\"]}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should work without provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateOmitsProviderOptionsWhenUnset()
    {
        var handler = new RecordingHandler { ResponseBody = GenerateBody("Test response") };
        var result = await Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None);

        Assert.Null(JsonNode.Parse(handler.Body)!["providerOptions"]);
        Assert.Equal("Test response", result.Text);
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass provider routing order for doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsProviderOrder()
    {
        await AssertGatewayOption(true, "{\"order\":[\"groq\",\"openai\"]}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should validate provider options against schema", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsAValidatedProviderOrder()
    {
        await AssertGatewayOption(false, "{\"order\":[\"anthropic\",\"bedrock\",\"openai\"]}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass providerTimeouts for doGenerate", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsProviderTimeouts()
    {
        await AssertGatewayOption(false, "{\"providerTimeouts\":{\"byok\":{\"openai\":5000,\"anthropic\":2000}}}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass providerTimeouts for doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task StreamSendsProviderTimeouts()
    {
        await AssertGatewayOption(true, "{\"providerTimeouts\":{\"byok\":{\"anthropic\":3000}}}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass zeroDataRetention option", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsZeroDataRetention()
    {
        await AssertGatewayOption(false, "{\"zeroDataRetention\":true}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass disallowPromptTraining option", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsDisallowPromptTraining()
    {
        await AssertGatewayOption(false, "{\"disallowPromptTraining\":true}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass quotaEntityId option", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsQuotaEntityId()
    {
        await AssertGatewayOption(false, "{\"quotaEntityId\":\"entity-123\"}");
    }

    [Fact]
    [UpstreamTest("packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::should pass quotaEntityId with other options", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateSendsQuotaEntityIdWithUser()
    {
        await AssertGatewayOption(false, "{\"quotaEntityId\":\"entity-123\",\"user\":\"user-456\"}");
    }

    private static async Task AssertGatewayOption(bool stream, string gatewayJson)
    {
        var handler = new RecordingHandler { ResponseBody = stream ? TextStream("Test response") : GenerateBody("Test response") };
        var options = Prompt();
        using var document = JsonDocument.Parse(gatewayJson);
        options.ProviderOptions = new Dictionary<string, JsonElement> { ["gateway"] = document.RootElement.Clone() };
        if (stream)
        {
            await Read(Model(handler), options);
        }
        else
        {
            await Model(handler).DoGenerateAsync(options, CancellationToken.None);
        }

        Assert.Equal(gatewayJson, JsonNode.Parse(handler.Body)!["providerOptions"]!["gateway"]!.ToJsonString());
    }

    private static async Task<T> GenerateError<T>(int status, string body)
        where T : Exception
    {
        var handler = new RecordingHandler { Status = (HttpStatusCode)status, ResponseBody = body };
        return await Assert.ThrowsAsync<T>(() => Model(handler).DoGenerateAsync(Prompt(), CancellationToken.None));
    }

    private static async Task<T> StreamError<T>(int status, string body)
        where T : Exception
    {
        var handler = new RecordingHandler { Status = (HttpStatusCode)status, ResponseBody = body };
        return await Assert.ThrowsAsync<T>(() => Read(Model(handler), Prompt()));
    }

    private static async Task<List<LanguageModelStreamPart>> Stream(string body, LanguageModelCallOptions? options = null)
    {
        var handler = new RecordingHandler { ResponseBody = body };
        return await Read(Model(handler), options ?? Prompt());
    }

    private static async Task<List<LanguageModelStreamPart>> MetadataStream(string timestampJson)
    {
        return await Stream(Sse(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"response-metadata\",\"id\":\"test-id\",\"modelId\":\"test-model\",\"timestamp\":" + timestampJson + "}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}"));
    }

    private static async Task<List<LanguageModelStreamPart>> Read(ILanguageModel model, LanguageModelCallOptions options, CancellationToken cancellationToken = default)
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in model.DoStreamAsync(options, cancellationToken))
        {
            parts.Add(part);
        }

        return parts;
    }

    private static async Task WaitForStart(RecordingHandler handler)
    {
        var completed = await Task.WhenAny(handler.Started, Task.Delay(5000));
        Assert.Same(handler.Started, completed);
    }

    private static ILanguageModel Model(RecordingHandler handler, Action<GatewayOptions>? configure = null)
    {
        var options = new GatewayOptions
        {
            ApiKey = "test-token",
            BaseUrl = "https://api.test.com",
        };
        configure?.Invoke(options);
        return GatewayProvider.Create(options, handler).LanguageModel("test-model");
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
        };
    }

    private static LanguageModelCallOptions ImagePrompt(string text, string mediaType, byte[]? bytes, string? url)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart(text),
                    new FileContentPart(mediaType, url, bytes, null),
                }),
            },
        };
    }

    private static LanguageModelCallOptions FileOnlyPrompt(string mediaType, byte[]? bytes, string? url)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[] { new FileContentPart(mediaType, url, bytes, null) }),
            },
        };
    }

    private static LanguageModelCallOptions MixedPrompt(byte[] bytes)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("First text."),
                    new FileContentPart("image/gif", null, bytes, null),
                    new TextContentPart("Second text."),
                    new FileContentPart("image/png", "https://example.com/image2.png", null, null),
                }),
            },
        };
    }

    private static void AssertFilePart(RecordingHandler handler, int index, string mediaType, string dataType, string dataValue)
    {
        var part = JsonNode.Parse(handler.Body)!["prompt"]![0]!["content"]![index]!;
        Assert.Equal("file", part["type"]!.GetValue<string>());
        Assert.Equal(mediaType, part["mediaType"]!.GetValue<string>());
        Assert.Equal(dataType, part["data"]!["type"]!.GetValue<string>());
        Assert.Equal(dataValue, part["data"]![dataType == "url" ? "url" : "data"]!.GetValue<string>());
    }

    private static void AssertMixed(RecordingHandler handler, byte[] bytes)
    {
        var content = JsonNode.Parse(handler.Body)!["prompt"]![0]!["content"]!;
        Assert.Equal("First text.", content[0]!["text"]!.GetValue<string>());
        Assert.Equal("file", content[1]!["type"]!.GetValue<string>());
        Assert.Equal(Convert.ToBase64String(bytes), content[1]!["data"]!["data"]!.GetValue<string>());
        Assert.Equal("image/gif", content[1]!["mediaType"]!.GetValue<string>());
        Assert.Equal("Second text.", content[2]!["text"]!.GetValue<string>());
        Assert.Equal("https://example.com/image2.png", content[3]!["data"]!["url"]!.GetValue<string>());
        Assert.Equal("image/png", content[3]!["mediaType"]!.GetValue<string>());
    }

    private static void AssertO11y(RecordingHandler handler)
    {
        Assert.Equal("test-deployment", handler.Headers["ai-o11y-deployment-id"]);
        Assert.Equal("production", handler.Headers["ai-o11y-environment"]);
        Assert.Equal("iad1", handler.Headers["ai-o11y-region"]);
    }

    private static Dictionary<string, string> O11y()
    {
        return new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "test-deployment",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };
    }

    private static string GenerateBody(string text, string? warningsJson = null, int promptTokens = 4, int completionTokens = 30)
    {
        var warnings = warningsJson == null ? string.Empty : ",\"warnings\":" + warningsJson;
        return "{\"id\":\"test-id\",\"created\":1711115037,\"model\":\"test-model\",\"content\":{\"type\":\"text\",\"text\":" + JsonSerializer.Serialize(text) + "},\"finish_reason\":\"stop\",\"usage\":{\"prompt_tokens\":" + promptTokens.ToString(CultureInfo.InvariantCulture) + ",\"completion_tokens\":" + completionTokens.ToString(CultureInfo.InvariantCulture) + "}" + warnings + "}";
    }

    private static string TextStream(string text)
    {
        return Sse(
            "{\"type\":\"text-delta\",\"textDelta\":" + JsonSerializer.Serialize(text) + "}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}");
    }

    private static string RawStream()
    {
        return Sse(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"raw\",\"rawValue\":{\"id\":\"test-chunk\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"raw\",\"rawValue\":{\"id\":\"test-chunk-2\",\"object\":\"chat.completion.chunk\",\"choices\":[{\"delta\":{\"content\":\" world\"}}]}}",
            "{\"type\":\"text-delta\",\"textDelta\":\" world\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");
    }

    private static string Sse(params string[] events)
    {
        var builder = new StringBuilder();
        foreach (var json in events)
        {
            builder.Append("data: ").Append(json).Append("\n\n");
        }

        return builder.ToString();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource<bool> _started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string? ResponseBody { get; set; }

        public Dictionary<string, string> ResponseHeaders { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Exception? Throw { get; set; }

        public bool WaitForCancel { get; set; }

        public bool ObservedCancel { get; private set; }

        public string Uri { get; private set; } = string.Empty;

        public string Body { get; private set; } = string.Empty;

        public HttpMethod? Method { get; private set; }

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Task Started => _started.Task;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Uri = request.RequestUri == null ? string.Empty : request.RequestUri.AbsoluteUri;
            Body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            Headers.Clear();
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = string.Join(",", header.Value);
            }

            if (request.Headers.Authorization != null)
            {
                Headers["Authorization"] = request.Headers.Authorization.ToString();
            }

            _started.TrySetResult(true);
            if (Throw != null)
            {
                throw Throw;
            }

            if (WaitForCancel)
            {
                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    ObservedCancel = true;
                    throw;
                }
            }

            var message = new HttpResponseMessage(Status)
            {
                Content = new StringContent(ResponseBody ?? GenerateBody("Hello, World!"), Encoding.UTF8, "application/json"),
            };
            foreach (var header in ResponseHeaders)
            {
                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return message;
        }
    }
}
