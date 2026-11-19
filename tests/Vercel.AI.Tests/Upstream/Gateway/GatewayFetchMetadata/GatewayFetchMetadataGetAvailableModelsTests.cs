// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayFetchMetadata;

/// <summary>Port of <c>gateway-fetch-metadata.test.ts</c> &gt; <c>GatewayFetchMetadata &gt; getAvailableModels</c>.</summary>
public sealed class GatewayFetchMetadataGetAvailableModelsTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-fetch-metadata.test.ts::GatewayFetchMetadata > getAvailableModels::";

    private static JsonObject Spec(string provider, string modelId) =>
        new()
        {
            ["specificationVersion"] = "v4",
            ["provider"] = provider,
            ["modelId"] = modelId,
        };

    private static JsonObject MockModelEntry() =>
        new()
        {
            ["id"] = "model-1",
            ["name"] = "Model One",
            ["description"] = "A test model",
            ["pricing"] = new JsonObject { ["input"] = "0.000001", ["output"] = "0.000002" },
            ["specification"] = Spec("test-provider", "model-1"),
        };

    private static JsonObject MockModelEntryWithoutPricing() =>
        new()
        {
            ["id"] = "model-2",
            ["name"] = "Model Two",
            ["specification"] = Spec("test-provider", "model-2"),
        };

    private static string Models(params JsonObject[] models) =>
        new JsonObject
        {
            ["models"] = new JsonArray(models.Select(m => (JsonNode)m).ToArray()),
        }.ToJsonString();

    private static GatewayTestServer CreateServer(params JsonObject[] models) =>
        new(Models(models.Length == 0 ? [MockModelEntry()] : models));

    private static Vercel.AI.Gateway.GatewayFetchMetadata CreateFetcher(
        GatewayTestServer server,
        Func<IReadOnlyDictionary<string, string?>>? headers = null
    ) => new(GatewayTestServer.CreateConfig(server, headers));

    private static void AssertEntry(JsonObject expected, GatewayLanguageModelEntry actual)
    {
        Assert.Equal(expected["id"]!.GetValue<string>(), actual.Id);
        Assert.Equal(expected["name"]!.GetValue<string>(), actual.Name);
        Assert.Equal(expected["description"]?.GetValue<string>(), actual.Description);
        var spec = expected["specification"]!;
        Assert.Equal(
            spec["specificationVersion"]!.GetValue<string>(),
            actual.Specification.SpecificationVersion
        );
        Assert.Equal(spec["provider"]!.GetValue<string>(), actual.Specification.Provider);
        Assert.Equal(spec["modelId"]!.GetValue<string>(), actual.Specification.ModelId);
        if (expected["pricing"] is JsonObject pricing)
        {
            Assert.Equal(pricing["input"]!.GetValue<string>(), actual.Pricing!.Input);
            Assert.Equal(pricing["output"]!.GetValue<string>(), actual.Pricing.Output);
        }
        else
        {
            Assert.Null(actual.Pricing);
        }
    }

    private static async Task<T> Capture<T>(Func<Task> action)
        where T : Exception
    {
        return await Assert.ThrowsAsync<T>(action);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should fetch available models from the correct endpoint",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fetch_available_models_from_the_correct_endpoint()
    {
        var server = CreateServer();

        var result = await CreateFetcher(server).GetAvailableModelsAsync();

        Assert.Equal(HttpMethod.Get, server.Calls[0].Method);
        Assert.Equal("https://api.example.com/config", server.Calls[0].RequestUri!.ToString());
        Assert.Single(result.Models);
        AssertEntry(MockModelEntry(), result.Models[0]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle models with pricing information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_models_with_pricing_information()
    {
        var result = await CreateFetcher(CreateServer(MockModelEntry())).GetAvailableModelsAsync();

        AssertEntry(MockModelEntry(), result.Models[0]);
        Assert.Equal("0.000001", result.Models[0].Pricing!.Input);
        Assert.Equal("0.000002", result.Models[0].Pricing!.Output);
        Assert.Null(result.Models[0].Pricing!.CachedInputTokens);
        Assert.Null(result.Models[0].Pricing!.CacheCreationInputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map cache pricing fields to SDK names when present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_map_cache_pricing_fields_to_SDK_names_when_present()
    {
        var entry = MockModelEntry();
        entry["pricing"] = new JsonObject
        {
            ["input"] = "0.000003",
            ["output"] = "0.000015",
            ["input_cache_read"] = "0.0000003",
            ["input_cache_write"] = "0.00000375",
        };

        var result = await CreateFetcher(CreateServer(entry)).GetAvailableModelsAsync();

        var pricing = result.Models[0].Pricing!;
        Assert.Equal("0.000003", pricing.Input);
        Assert.Equal("0.000015", pricing.Output);
        Assert.Equal("0.0000003", pricing.CachedInputTokens);
        Assert.Equal("0.00000375", pricing.CacheCreationInputTokens);
        Assert.DoesNotContain(typeof(GatewayModelPricing).GetProperties(), p =>
            p.Name is "input_cache_read" or "input_cache_write"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle models without pricing information",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_models_without_pricing_information()
    {
        var result = await CreateFetcher(CreateServer(MockModelEntryWithoutPricing()))
            .GetAvailableModelsAsync();

        AssertEntry(MockModelEntryWithoutPricing(), result.Models[0]);
        Assert.Null(result.Models[0].Pricing);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle mixed models with and without pricing",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_mixed_models_with_and_without_pricing()
    {
        var result = await CreateFetcher(
                CreateServer(MockModelEntry(), MockModelEntryWithoutPricing())
            )
            .GetAvailableModelsAsync();

        Assert.Equal(2, result.Models.Count);
        Assert.Equal("0.000001", result.Models[0].Pricing!.Input);
        Assert.Equal("0.000002", result.Models[0].Pricing!.Output);
        Assert.Null(result.Models[1].Pricing);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle models with description",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_models_with_description()
    {
        var entry = MockModelEntry();
        entry["description"] = "A powerful language model";

        var result = await CreateFetcher(CreateServer(entry)).GetAvailableModelsAsync();

        Assert.Equal("A powerful language model", result.Models[0].Description);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept top-level modelType when present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_top_level_modelType_when_present()
    {
        var entry = MockModelEntry();
        entry["modelType"] = "language";

        var result = await CreateFetcher(CreateServer(entry)).GetAvailableModelsAsync();

        Assert.Equal("language", result.Models[0].ModelType);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should filter out models with unknown modelType values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_filter_out_models_with_unknown_modelType_values()
    {
        var server = CreateServer(
            new JsonObject
            {
                ["id"] = "model-unknown-type",
                ["name"] = "Unknown Type Model",
                ["specification"] = Spec("test-provider", "model-unknown-type"),
                ["modelType"] = "some-future-type",
            }
        );

        var result = await CreateFetcher(server).GetAvailableModelsAsync();

        Assert.Empty(result.Models);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve all known modelType values",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_all_known_modelType_values()
    {
        string[] knownTypes =
        [
            "embedding",
            "decision",
            "evaluation",
            "extraction",
            "image",
            "language",
            "realtime",
            "reranking",
            "speech",
            "transcription",
            "video",
        ];
        var server = CreateServer(
            knownTypes
                .Select(type => new JsonObject
                {
                    ["id"] = "model-" + type,
                    ["name"] = "Model " + type,
                    ["specification"] = Spec("test-provider", "model-" + type),
                    ["modelType"] = type,
                })
                .ToArray()
        );

        var result = await CreateFetcher(server).GetAvailableModelsAsync();

        Assert.Equal(knownTypes.Length, result.Models.Count);
        for (var i = 0; i < knownTypes.Length; i++)
        {
            Assert.Equal(knownTypes[i], result.Models[i].ModelType);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep known models and filter unknown from mixed response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_keep_known_models_and_filter_unknown_from_mixed_response()
    {
        var known = MockModelEntry();
        known["modelType"] = "language";
        var server = CreateServer(
            known,
            new JsonObject
            {
                ["id"] = "model-future",
                ["name"] = "Future Model",
                ["specification"] = Spec("test-provider", "model-future"),
                ["modelType"] = "hologram",
            }
        );

        var result = await CreateFetcher(server).GetAvailableModelsAsync();

        Assert.Single(result.Models);
        Assert.Equal("language", result.Models[0].ModelType);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var server = CreateServer();
        var metadata = CreateFetcher(server, () =>
            new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer custom-token",
                ["Custom-Header"] = "custom-value",
            }
        );

        await metadata.GetAvailableModelsAsync();

        var headers = server.RequestHeaders();
        Assert.Equal(2, headers.Count);
        Assert.Equal("Bearer custom-token", headers["authorization"]);
        Assert.Equal("custom-value", headers["custom-header"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_API_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Unauthorized\",\"type\":\"authentication_error\"}}",
            HttpStatusCode.Unauthorized
        );

        var error = await Capture<GatewayAuthenticationError>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.Contains("No authentication provided", error.Message);
        Assert.Equal("authentication_error", error.Type);
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert API call errors to Gateway errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_API_call_errors_to_Gateway_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Forbidden access\",\"type\":\"authentication_error\"}}",
            HttpStatusCode.Forbidden
        );

        var error = await Capture<GatewayAuthenticationError>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.Contains("No authentication provided", error.Message);
        Assert.Equal("authentication_error", error.Type);
        Assert.Equal(403, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle malformed JSON error responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_malformed_JSON_error_responses()
    {
        var server = new GatewayTestServer("{ invalid json", HttpStatusCode.InternalServerError);

        var error = await Capture<GatewayResponseError>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );

        Assert.True(GatewayResponseError.IsInstance(error));
        Assert.Equal(500, error.StatusCode);
        Assert.Equal("response_error", error.Type);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle malformed response data",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_malformed_response_data()
    {
        var server = new GatewayTestServer("{\"invalid\":\"response\"}");

        await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject models with invalid pricing format",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_models_with_invalid_pricing_format()
    {
        var entry = new JsonObject
        {
            ["id"] = "model-1",
            ["name"] = "Model One",
            ["pricing"] = new JsonObject { ["input"] = 123, ["output"] = "0.000002" }, // Should be string, not number
            ["specification"] = Spec("test-provider", "model-1"),
        };

        await Assert.ThrowsAnyAsync<Exception>(() =>
            CreateFetcher(CreateServer(entry)).GetAvailableModelsAsync()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not double-wrap existing Gateway errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_double_wrap_existing_Gateway_errors()
    {
        var existingError = new GatewayAuthenticationError("Already wrapped", 401);

        try
        {
            throw existingError;
        }
        catch (Exception error) when (GatewayError.IsInstance(error))
        {
            Assert.Same(existingError, GatewayErrors.AsGatewayError(error)); // Should be the same instance
            Assert.Equal("Already wrapped", error.Message);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle various server error types",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_various_server_error_types()
    {
        // Test rate limit error
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Rate limit exceeded\",\"type\":\"rate_limit_exceeded\"}}",
            (HttpStatusCode)429
        );

        var error = await Capture<GatewayRateLimitError>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );

        Assert.True(GatewayRateLimitError.IsInstance(error));
        Assert.Equal("Rate limit exceeded", error.Message);
        Assert.Equal("rate_limit_exceeded", error.Type);
        Assert.Equal(429, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle internal server errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_internal_server_errors()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Database connection failed\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );

        var error = await Capture<GatewayInternalServerError>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );

        Assert.True(GatewayInternalServerError.IsInstance(error));
        Assert.Equal("Database connection failed", error.Message);
        Assert.Equal("internal_server_error", error.Type);
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve error cause chain",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_error_cause_chain()
    {
        var server = new GatewayTestServer(
            "{\"error\":{\"message\":\"Token expired\",\"type\":\"authentication_error\"}}",
            HttpStatusCode.Unauthorized
        );

        var error = await Capture<GatewayAuthenticationError>(() =>
            CreateFetcher(server).GetAvailableModelsAsync()
        );

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.NotNull(error.Cause);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom fetch function when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_fetch_function_when_provided()
    {
        var customModelEntry = new JsonObject
        {
            ["id"] = "custom-model-1",
            ["name"] = "Custom Model One",
            ["description"] = "Custom model description",
            ["pricing"] = new JsonObject { ["input"] = "0.000005", ["output"] = "0.000010" },
            ["specification"] = Spec("custom-provider", "custom-model-1"),
        };
        var mockFetch = CreateServer(customModelEntry);

        var result = await CreateFetcher(mockFetch).GetAvailableModelsAsync();

        Assert.NotEmpty(mockFetch.Calls);
        Assert.Single(result.Models);
        AssertEntry(customModelEntry, result.Models[0]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle empty response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_empty_response()
    {
        var server = new GatewayTestServer("{\"models\":[]}");

        var result = await CreateFetcher(server).GetAvailableModelsAsync();

        Assert.Empty(result.Models);
    }
}
