// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; doGenerate</c>.</summary>
public sealed class GatewayLanguageModelDoGenerateTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > doGenerate::";

    [Fact]
    [UpstreamTest(
        Prefix + "should include o11y headers in the request",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_o11y_headers_in_the_request()
    {
        var server = new GatewayLanguageModelServer
        {
            ContentType = "application/json",
            ResponseBody =
                "{\"id\":\"test-id\",\"created\":1711115037,\"model\":\"test-model\",\"content\":[{\"type\":\"text\",\"text\":\"Hello, World!\"}],\"finish_reason\":\"stop\",\"usage\":{\"prompt_tokens\":4,\"completion_tokens\":30}}",
        };
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "test-deployment",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };

        await CreateTestModel(server, o11yHeaders)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        foreach (var header in o11yHeaders)
        {
            Assert.Equal(header.Value, server.RequestHeaders[header.Key]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should remove abortSignal from the request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_remove_abortSignal_from_the_request_body()
    {
        var server = PrepareJsonResponse("Test response");
        using var source = new CancellationTokenSource();

        await CreateTestModel(server).DoGenerateAsync(TestPrompt, source.Token);

        Assert.Null(server.RequestJson["abortSignal"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass abortSignal to fetch when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_abortSignal_to_fetch_when_provided()
    {
        var server = PrepareJsonResponse("Test response");
        var tokens = new List<CancellationToken>();
        using var source = new CancellationTokenSource();

        await CreateTestModel(Recording(server, tokens)).DoGenerateAsync(TestPrompt, source.Token);

        Assert.Equal(source.Token, Assert.Single(tokens));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not pass abortSignal to fetch when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_pass_abortSignal_to_fetch_when_not_provided()
    {
        var server = PrepareJsonResponse("Test response");
        var tokens = new List<CancellationToken>();

        await CreateTestModel(Recording(server, tokens))
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.False(Assert.Single(tokens).CanBeCanceled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var server = PrepareJsonResponse("Hello, World!");

        await CreateTestModel(server)
            .DoGenerateAsync(
                new LanguageModelCallOptions
                {
                    Prompt = TestPrompt.Prompt,
                    Headers = new Dictionary<string, string?> { ["Custom-Header"] = "test-value" },
                },
                CancellationToken.None
            );

        var headers = server.RequestHeaders;
        Assert.Equal("Bearer test-token", headers["authorization"]);
        Assert.Equal("test-value", headers["custom-header"]);
        Assert.Equal("4", headers["ai-language-model-specification-version"]);
        Assert.Equal("test-model", headers["ai-language-model-id"]);
        Assert.Equal("false", headers["ai-language-model-streaming"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract text response",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port reads typed content, so the response carries the content array and the result is a list with one text part."
    )]
    public async Task Should_extract_text_response()
    {
        var server = PrepareJsonResponse("Hello, World!");

        var result = await CreateTestModel(server)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.Equal(
            "Hello, World!",
            Assert.IsType<GeneratedText>(Assert.Single(result.Content)).Text
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract usage information",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port reads typed usage, so the response carries the V4 token counts instead of prompt_tokens and completion_tokens."
    )]
    public async Task Should_extract_usage_information()
    {
        var server = PrepareJsonResponse(
            "Test",
            usage: "{\"inputTokens\":{\"total\":10},\"outputTokens\":{\"total\":20}}"
        );

        var result = await CreateTestModel(server)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.Equal(10, result.Usage.InputTokens);
        Assert.Equal(20, result.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward warnings returned by the gateway",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_warnings_returned_by_the_gateway()
    {
        var server = PrepareJsonResponse(
            "Hello, World!",
            warnings: "[{\"type\":\"compatibility\",\"feature\":\"maxOutputTokens\",\"details\":\"lowered to 38000\"},{\"type\":\"other\",\"message\":\"from provider\"}]"
        );

        var result = await CreateTestModel(server)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.Collection(
            result.Warnings,
            warning =>
            {
                Assert.Equal("compatibility", warning.Type);
                Assert.Equal("maxOutputTokens", warning.Feature);
                Assert.Equal("lowered to 38000", warning.Message);
            },
            warning =>
            {
                Assert.Equal("other", warning.Type);
                Assert.Equal("from provider", warning.Message);
            }
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default warnings to an empty array when absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_warnings_to_an_empty_array_when_absent()
    {
        var server = PrepareJsonResponse("Hello");

        var result = await CreateTestModel(server)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert API call errors to Gateway errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_API_call_errors_to_Gateway_errors()
    {
        var server = ErrorResponse(
            401,
            "{\"error\":{\"message\":\"Invalid API key provided\",\"type\":\"authentication_error\"}}"
        );

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() =>
            CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None)
        );

        Assert.Contains("Invalid API key", error.Message);
        Assert.Contains("vercel.com/d?to=", error.Message);
        Assert.Equal(401, error.StatusCode);
        Assert.Equal("authentication_error", error.Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle malformed error responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_malformed_error_responses()
    {
        var server = ErrorResponse(500, "Not JSON");

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() =>
            CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None)
        );

        Assert.Equal(500, error.StatusCode);
        Assert.Equal("response_error", error.Type);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle rate limit errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_rate_limit_errors()
    {
        var server = ErrorResponse(
            429,
            "{\"error\":{\"message\":\"Rate limit exceeded. Try again later.\",\"type\":\"rate_limit_exceeded\"}}"
        );

        var error = await Assert.ThrowsAsync<GatewayRateLimitError>(() =>
            CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None)
        );

        Assert.Equal("Rate limit exceeded. Try again later.", error.Message);
        Assert.Equal(429, error.StatusCode);
        Assert.Equal("rate_limit_exceeded", error.Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle invalid request errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_invalid_request_errors()
    {
        var server = ErrorResponse(
            400,
            "{\"error\":{\"message\":\"Invalid prompt format\",\"type\":\"invalid_request_error\"}}"
        );

        var error = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() =>
            CreateTestModel(server).DoGenerateAsync(TestPrompt, CancellationToken.None)
        );

        Assert.Equal("Invalid prompt format", error.Message);
        Assert.Equal(400, error.StatusCode);
        Assert.Equal("invalid_request_error", error.Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle various error types with proper conversion",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_various_error_types_with_proper_conversion()
    {
        var server = new GatewayLanguageModelServer { ContentType = "application/json" };
        var model = CreateTestModel(server);

        server.Status = System.Net.HttpStatusCode.BadRequest;
        server.ResponseBody =
            "{\"error\":{\"message\":\"Invalid request format\",\"type\":\"invalid_request_error\"}}";
        var invalidError = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() =>
            model.DoGenerateAsync(TestPrompt, CancellationToken.None)
        );
        Assert.Equal("Invalid request format", invalidError.Message);
        Assert.Equal(400, invalidError.StatusCode);
        Assert.Equal("invalid_request_error", invalidError.Type);

        // Test model not found error
        server.Status = System.Net.HttpStatusCode.NotFound;
        server.ResponseBody =
            "{\"error\":{\"message\":\"Model xyz not found\",\"type\":\"model_not_found\",\"param\":{\"modelId\":\"xyz\"}}}";
        var modelError = await Assert.ThrowsAsync<GatewayModelNotFoundError>(() =>
            model.DoGenerateAsync(TestPrompt, CancellationToken.None)
        );
        Assert.Equal("Model xyz not found", modelError.Message);
        Assert.Equal(404, modelError.StatusCode);
        Assert.Equal("model_not_found", modelError.Type);
        Assert.Equal("xyz", modelError.ModelId);

        // Test internal server error
        server.Status = System.Net.HttpStatusCode.InternalServerError;
        server.ResponseBody =
            "{\"error\":{\"message\":\"Database connection failed\",\"type\":\"internal_server_error\"}}";
        var serverError = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            model.DoGenerateAsync(TestPrompt, CancellationToken.None)
        );
        Assert.Equal("Database connection failed", serverError.Message);
        Assert.Equal(500, serverError.StatusCode);
        Assert.Equal("internal_server_error", serverError.Type);
        Assert.IsAssignableFrom<ApiException>(serverError.Cause);
        Assert.Contains("Database connection failed", serverError.Cause!.Message);
    }
}
