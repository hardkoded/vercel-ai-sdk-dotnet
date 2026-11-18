// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayGenerationInfo;

/// <summary>Port of <c>gateway-generation-info.test.ts</c> &gt; <c>GatewayGenerationInfoFetcher &gt; getGenerationInfo</c>.</summary>
public sealed class GatewayGenerationInfoFetcherGetGenerationInfoTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-generation-info.test.ts::GatewayGenerationInfoFetcher > getGenerationInfo::";
    private const string Commit = "32acf43";
    private const string Id = "gen_01ARZ3NDEKTSV4RRFFQ69G5FAV";

    private static JsonObject MockData() =>
        JsonNode
            .Parse(
                "{\"id\":\"gen_01ARZ3NDEKTSV4RRFFQ69G5FAV\",\"total_cost\":0.00123,\"market_cost\":0.0011,\"surcharge_cost\":0.00013,\"gateway_cost\":0.00123,\"upstream_inference_cost\":0.0011,\"usage\":0.00123,\"created_at\":\"2024-01-01T00:00:00.000Z\",\"model\":\"gpt-4\",\"is_byok\":false,\"provider_name\":\"openai\",\"streamed\":true,\"finish_reason\":\"stop\",\"latency\":200,\"generation_time\":1500,\"native_tokens_prompt\":100,\"native_tokens_completion\":50,\"native_tokens_reasoning\":0,\"native_tokens_cached\":0,\"native_tokens_cache_creation\":0,\"billable_web_search_calls\":0}"
            )!
            .AsObject();

    private static string Wrap(JsonObject data) =>
        new JsonObject { ["data"] = data }.ToJsonString();

    [Fact]
    [UpstreamTest(
        Prefix + "should fetch from the correct endpoint with generation ID",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fetch_from_the_correct_endpoint_with_generation_ID()
    {
        var server = new Server(Wrap(MockData()));

        await CreateFetcher(server)
            .GetGenerationInfoAsync(new GatewayGenerationInfoParams { Id = Id });

        Assert.Equal(HttpMethod.Get, server.Calls[0].Method);
        var url = server.Calls[0].RequestUri!;
        Assert.Equal("/v1/generation", url.AbsolutePath);
        Assert.Equal("?id=" + Id, url.Query);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should transform snake_case response fields to camelCase",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_transform_snake_case_response_fields_to_camelCase()
    {
        var result = await Fetch(new Server(Wrap(MockData())));

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"id\":\"gen_01ARZ3NDEKTSV4RRFFQ69G5FAV\",\"totalCost\":0.00123,\"marketCost\":0.0011,\"surchargeCost\":0.00013,\"gatewayCost\":0.00123,\"upstreamInferenceCost\":0.0011,\"usage\":0.00123,\"createdAt\":\"2024-01-01T00:00:00.000Z\",\"model\":\"gpt-4\",\"isByok\":false,\"providerName\":\"openai\",\"streamed\":true,\"finishReason\":\"stop\",\"latency\":200,\"generationTime\":1500,\"promptTokens\":100,\"completionTokens\":50,\"reasoningTokens\":0,\"cachedTokens\":0,\"cacheCreationTokens\":0,\"billableWebSearchCalls\":0}"
                ),
                JsonSerializer.SerializeToNode(result)
            )
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should unwrap the data envelope", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_unwrap_the_data_envelope()
    {
        var result = await Fetch(new Server(Wrap(MockData())));

        // Result should be the data object directly, not { data: ... }
        Assert.False(JsonSerializer.SerializeToNode(result)!.AsObject().ContainsKey("data"));
        Assert.Equal(Id, result.Id);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not have snake_case fields in result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_have_snake_case_fields_in_result()
    {
        var result = await Fetch(new Server(Wrap(MockData())));
        var json = JsonSerializer.SerializeToNode(result)!.AsObject();

        foreach (
            var key in new[]
            {
                "total_cost",
                "market_cost",
                "surcharge_cost",
                "gateway_cost",
                "is_byok",
                "provider_name",
                "created_at",
                "generation_time",
                "finish_reason",
            }
        )
        {
            Assert.False(json.ContainsKey(key), key);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var server = new Server(Wrap(MockData()));
        var fetcher = CreateFetcher(server, () =>
            new Dictionary<string, string?>
            {
                ["Authorization"] = "Bearer custom-token",
                ["Custom-Header"] = "custom-value",
            }
        );

        await fetcher.GetGenerationInfoAsync(new GatewayGenerationInfoParams { Id = Id });

        var headers = server
            .Calls[0]
            .Headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => string.Join(",", h.Value));
        Assert.Equal("Bearer custom-token", headers["authorization"]);
        Assert.Equal("custom-value", headers["custom-header"]);
        Assert.DoesNotContain("ai-gateway-protocol-version", headers.Keys);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle 401 authentication errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_401_authentication_errors()
    {
        var server = new Server(
            "{\"error\":{\"message\":\"Unauthorized\",\"type\":\"authentication_error\"}}",
            HttpStatusCode.Unauthorized
        );

        var error = await Assert.ThrowsAsync<GatewayAuthenticationError>(() => Fetch(server));

        Assert.True(GatewayAuthenticationError.IsInstance(error));
        Assert.Equal(401, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle 500 internal server errors",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_500_internal_server_errors()
    {
        var server = new Server(
            "{\"error\":{\"message\":\"Failed to retrieve usage data\",\"type\":\"internal_server_error\"}}",
            HttpStatusCode.InternalServerError
        );

        await Assert.ThrowsAsync<GatewayInternalServerError>(() => Fetch(server));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle malformed JSON error responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_malformed_JSON_error_responses()
    {
        var server = new Server("{ invalid json", HttpStatusCode.InternalServerError);

        var error = await Assert.ThrowsAsync<GatewayResponseError>(() => Fetch(server));

        Assert.True(GatewayResponseError.IsInstance(error));
        Assert.Equal(500, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should use custom fetch function when provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_use_custom_fetch_function_when_provided()
    {
        var server = new Server(Wrap(MockData()));

        var result = await Fetch(server);

        Assert.NotEmpty(server.Calls);
        Assert.Equal(0.00123, result.TotalCost);
        Assert.Equal("gpt-4", result.Model);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should encode special characters in generation ID",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_encode_special_characters_in_generation_ID()
    {
        var server = new Server(Wrap(MockData()));

        await CreateFetcher(server)
            .GetGenerationInfoAsync(new GatewayGenerationInfoParams { Id = Id });

        Assert.Equal("?id=" + Id, server.Calls[0].RequestUri!.Query);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should handle BYOK generation response",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_handle_BYOK_generation_response()
    {
        var data = MockData();
        data["is_byok"] = true;
        data["upstream_inference_cost"] = 0.0009;
        data["provider_name"] = "anthropic";
        data["model"] = "claude-sonnet-4";

        var result = await Fetch(new Server(Wrap(data)));

        Assert.True(result.IsByok);
        Assert.Equal(0.0009, result.UpstreamInferenceCost);
        Assert.Equal("anthropic", result.ProviderName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "should accept an unavailable cost breakdown (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_accept_an_unavailable_cost_breakdown(bool isNull)
    {
        var data = MockData();
        foreach (var key in new[] { "market_cost", "surcharge_cost", "gateway_cost" })
        {
            if (isNull)
            {
                data[key] = null;
            }
            else
            {
                data.Remove(key);
            }
        }

        var result = await Fetch(new Server(Wrap(data)));
        var json = JsonSerializer.SerializeToNode(result)!.AsObject();

        Assert.Equal(0.00123, result.TotalCost);
        Assert.False(json.ContainsKey("marketCost"));
        Assert.False(json.ContainsKey("surchargeCost"));
        Assert.False(json.ContainsKey("gatewayCost"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve zero cost values",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_preserve_zero_cost_values()
    {
        var data = MockData();
        data["market_cost"] = 0;
        data["surcharge_cost"] = 0;
        data["gateway_cost"] = 0;

        var result = await Fetch(new Server(Wrap(data)));

        Assert.Equal(0, result.MarketCost);
        Assert.Equal(0, result.SurchargeCost);
        Assert.Equal(0, result.GatewayCost);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [UpstreamTest(
        Prefix + "should omit unavailable marketCost (%s)",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_omit_unavailable_marketCost(bool isNull)
    {
        var data = MockData();
        if (isNull)
        {
            data["market_cost"] = null;
        }
        else
        {
            data.Remove("market_cost");
        }

        var result = await Fetch(new Server(Wrap(data)));

        Assert.False(JsonSerializer.SerializeToNode(result)!.AsObject().ContainsKey("marketCost"));
        Assert.Equal(0.00013, result.SurchargeCost);
        Assert.Equal(0.00123, result.GatewayCost);
    }

    private static Task<Vercel.AI.Gateway.GatewayGenerationInfo> Fetch(Server server) =>
        CreateFetcher(server).GetGenerationInfoAsync(new GatewayGenerationInfoParams { Id = Id });

    private static GatewayGenerationInfoFetcher CreateFetcher(
        Server server,
        Func<IReadOnlyDictionary<string, string?>>? headers = null
    )
    {
        return new GatewayGenerationInfoFetcher(
            new GatewayConfig
            {
                BaseUrl = "https://api.example.com",
                Headers = _ =>
                    Task.FromResult(
                        headers?.Invoke()
                            ?? new Dictionary<string, string?>
                            {
                                ["Authorization"] = "Bearer test-token",
                            }
                    ),
                Http = new ProviderHttp(
                    new HttpClient(server, disposeHandler: false),
                    new RetryPolicy { MaxRetries = 0 }
                ),
            }
        );
    }

    private sealed class Server(string body, HttpStatusCode status = HttpStatusCode.OK)
        : HttpMessageHandler
    {
        public List<HttpRequestMessage> Calls { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Calls.Add(request);
            return Task.FromResult(
                new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                }
            );
        }
    }
}
