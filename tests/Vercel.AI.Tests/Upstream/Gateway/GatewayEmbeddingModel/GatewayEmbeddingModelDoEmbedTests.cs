// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayEmbeddingModel;

/// <summary>Port of <c>gateway-embedding-model.test.ts</c> &gt; <c>GatewayEmbeddingModel &gt; doEmbed</c>.</summary>
public sealed class GatewayEmbeddingModelDoEmbedTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-embedding-model.test.ts::GatewayEmbeddingModel > doEmbed::";

    private const string DummyEmbeddings = "[[0.1,0.2,0.3],[0.4,0.5,0.6]]";

    private static readonly string[] TestValues =
    {
        "sunny day at the beach",
        "rainy afternoon in the city",
    };

    [Fact]
    [UpstreamTest(
        Prefix + "should forward dimensions alongside provider options for server-side resolution",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_dimensions_alongside_provider_options_for_server_side_resolution()
    {
        var handler = PrepareJsonResponse();

        await CreateTestModel(handler)
            .DoEmbedAsync(
                TestValues,
                JsonSerializer.Deserialize<JsonElement>("{\"openai\":{\"dimensions\":512}}"),
                null,
                CancellationToken.None,
                256
            );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"],\"dimensions\":256,\"providerOptions\":{\"openai\":{\"dimensions\":512}}}"
                ),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should forward dimensions without provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_forward_dimensions_without_provider_options()
    {
        var handler = PrepareJsonResponse();

        await ((IEmbeddingModel)CreateTestModel(handler)).DoEmbedAsync(
            TestValues,
            null,
            CancellationToken.None,
            256
        );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"],\"dimensions\":256}"
                ),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_correctly()
    {
        var handler = PrepareJsonResponse();

        await CreateTestModel(handler)
            .DoEmbedAsync(
                TestValues,
                null,
                new Dictionary<string, string?> { ["Custom-Header"] = "test-value" },
                CancellationToken.None
            );

        Assert.Equal("Bearer test-token", handler.Headers["Authorization"]);
        Assert.Equal("test-value", handler.Headers["Custom-Header"]);
        Assert.Equal("4", handler.Headers["ai-embedding-model-specification-version"]);
        Assert.Equal("openai/text-embedding-3-small", handler.Headers["ai-model-id"]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should include o11y headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_include_o11y_headers()
    {
        var handler = PrepareJsonResponse();
        var o11yHeaders = new Dictionary<string, string>
        {
            ["ai-o11y-deployment-id"] = "deployment-1",
            ["ai-o11y-environment"] = "production",
            ["ai-o11y-region"] = "iad1",
        };

        await CreateTestModel(handler, o11yHeaders)
            .DoEmbedAsync(TestValues, CancellationToken.None);

        foreach (var pair in o11yHeaders)
        {
            Assert.Equal(pair.Value, handler.Headers[pair.Key]);
        }
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract embeddings and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_embeddings_and_usage()
    {
        var handler = PrepareJsonResponse(usage: "{\"tokens\":42}");

        var result = await CreateTestModel(handler)
            .DoEmbedAsync(TestValues, CancellationToken.None);

        Assert.Equal(
            new[] { new[] { 0.1f, 0.2f, 0.3f }, new[] { 0.4f, 0.5f, 0.6f } },
            result.Embeddings
        );
        Assert.Equal(42, result.Tokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_warnings()
    {
        var handler = PrepareJsonResponse(
            warnings: "[{\"type\":\"deprecated\",\"setting\":\"dimensions\",\"message\":\"Use outputDimensions instead.\"}]"
        );

        var result = await CreateTestModel(handler)
            .DoEmbedAsync(TestValues, CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal("deprecated", warning.Type);
        Assert.Equal("dimensions", warning.Setting);
        Assert.Equal("Use outputDimensions instead.", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should default warnings to an empty array when absent",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_default_warnings_to_an_empty_array_when_absent()
    {
        var handler = PrepareJsonResponse();

        var result = await CreateTestModel(handler)
            .DoEmbedAsync(TestValues, CancellationToken.None);

        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send value as array", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_value_as_array()
    {
        var handler = PrepareJsonResponse();

        await CreateTestModel(handler).DoEmbedAsync(TestValues, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"]}"
                ),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerOptions into request body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_providerOptions_into_request_body()
    {
        var handler = PrepareJsonResponse();

        await CreateTestModel(handler)
            .DoEmbedAsync(
                TestValues,
                JsonSerializer.Deserialize<JsonElement>("{\"openai\":{\"dimensions\":64}}"),
                CancellationToken.None
            );

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"],\"providerOptions\":{\"openai\":{\"dimensions\":64}}}"
                ),
                JsonNode.Parse(handler.Body)
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include providerOptions when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_include_providerOptions_when_not_provided()
    {
        var handler = PrepareJsonResponse();

        await CreateTestModel(handler).DoEmbedAsync(TestValues, CancellationToken.None);

        var body = JsonNode.Parse(handler.Body)!.AsObject();
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    "{\"values\":[\"sunny day at the beach\",\"rainy afternoon in the city\"]}"
                ),
                body
            )
        );
        Assert.False(body.ContainsKey("providerOptions"));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should convert gateway error responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_convert_gateway_error_responses()
    {
        var invalid = await Assert.ThrowsAsync<GatewayInvalidRequestError>(() =>
            CreateTestModel(
                    new UpstreamRecordingHandler(
                        "{\"error\":{\"message\":\"Invalid input\",\"type\":\"invalid_request_error\"}}",
                        HttpStatusCode.BadRequest
                    )
                )
                .DoEmbedAsync(TestValues, CancellationToken.None)
        );
        Assert.Equal(400, invalid.StatusCode);

        var server = await Assert.ThrowsAsync<GatewayInternalServerError>(() =>
            CreateTestModel(
                    new UpstreamRecordingHandler(
                        "{\"error\":{\"message\":\"Server blew up\",\"type\":\"internal_server_error\"}}",
                        HttpStatusCode.InternalServerError
                    )
                )
                .DoEmbedAsync(TestValues, CancellationToken.None)
        );
        Assert.Equal(500, server.StatusCode);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include providerMetadata in response body",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_providerMetadata_in_response_body()
    {
        var handler = PrepareJsonResponse(
            usage: "{\"tokens\":5}",
            providerMetadata: "{\"gateway\":{\"routing\":{\"test\":true}}}"
        );

        var result = await CreateTestModel(handler)
            .DoEmbedAsync(TestValues, CancellationToken.None);

        var body = JsonNode.Parse(result.Response!.Body!)!;
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"gateway\":{\"routing\":{\"test\":true}}}"),
                body["providerMetadata"]
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract providerMetadata to top level",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_providerMetadata_to_top_level()
    {
        var handler = PrepareJsonResponse(
            usage: "{\"tokens\":5}",
            providerMetadata: "{\"gateway\":{\"routing\":{\"test\":true}}}"
        );

        var result = await CreateTestModel(handler)
            .DoEmbedAsync(TestValues, CancellationToken.None);

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("{\"gateway\":{\"routing\":{\"test\":true}}}"),
                JsonNode.Parse(result.ProviderMetadata!.Value.GetRawText())
            )
        );
    }

    /// <summary>Maps to <c>prepareJsonResponse</c>.</summary>
    private static UpstreamRecordingHandler PrepareJsonResponse(
        string usage = "{\"tokens\":8}",
        string? warnings = null,
        string? providerMetadata = null
    )
    {
        var body = "{\"embeddings\":" + DummyEmbeddings + ",\"usage\":" + usage;
        if (warnings is not null)
        {
            body += ",\"warnings\":" + warnings;
        }

        if (providerMetadata is not null)
        {
            body += ",\"providerMetadata\":" + providerMetadata;
        }

        return new UpstreamRecordingHandler(body + "}");
    }

    /// <summary>Maps to <c>createTestModel</c>.</summary>
    private static Vercel.AI.Gateway.GatewayEmbeddingModel CreateTestModel(
        HttpMessageHandler handler,
        IReadOnlyDictionary<string, string>? o11yHeaders = null
    )
    {
        var provider = GatewayProvider.Create(
            new GatewayOptions
            {
                ApiKey = "test-token",
                BaseUrl = "https://api.test.com",
                O11yHeaders = o11yHeaders ?? new Dictionary<string, string>(),
            },
            handler
        );
        return (Vercel.AI.Gateway.GatewayEmbeddingModel)
            provider.EmbeddingModel("openai/text-embedding-3-small");
    }
}
