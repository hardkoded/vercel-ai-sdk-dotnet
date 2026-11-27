// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.Operations;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using AlibabaProviderType = Vercel.AI.Alibaba.AlibabaProvider;

namespace Vercel.AI.Tests.AlibabaEmbeddingModel;

/// <summary>Port of <c>alibaba-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private const string Prefix = "packages/alibaba/src/alibaba-embedding-model.test.ts::doEmbed::";

    private const string Url =
        "https://dashscope-intl.aliyuncs.com/api/v1/services/embeddings/text-embedding/text-embedding";

    private static readonly string[] TestValues =
    {
        "sunny day at the beach",
        "rainy day in the city",
    };

    private static readonly double[][] DummyEmbeddings =
    {
        new[] { 0.1, 0.2, 0.3 },
        new[] { 0.4, 0.5, 0.6 },
    };

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest(
        Prefix
            + "maps dimensions $dimensions with provider override $providerDimensions to $expected",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Maps_dimensions_with_provider_override(
        int? dimensions,
        int? providerDimensions,
        int? expected
    )
    {
        var server = PrepareJsonResponse();

        await Model(server)
            .DoEmbedAsync(
                TestValues,
                EmbeddingDimensionCases.Options("alibaba", "dimension", providerDimensions),
                CancellationToken.None,
                dimensions
            );

        var body = JsonNode.Parse(server.Calls[0].Text)!;
        EmbeddingDimensionCases.AssertDimension(body["parameters"]!["dimension"], expected);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract embeddings in input order",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_embeddings_in_input_order()
    {
        var server = PrepareJsonResponse(reverseOrder: true);

        var result = await Model(server).DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal(2, result.Embeddings.Count);
        Assert.Equal(new[] { 0.1f, 0.2f, 0.3f }, result.Embeddings[0]);
        Assert.Equal(new[] { 0.4f, 0.5f, 0.6f }, result.Embeddings[1]);
    }

    [Fact]
    [UpstreamTest(Prefix + "should extract usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_usage()
    {
        var server = PrepareJsonResponse(usage: "{\"total_tokens\":20}");

        var result = await Model(server).DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Equal(20, result.Tokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should omit usage when the response does not include usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_usage_when_the_response_does_not_include_usage()
    {
        var server = PrepareJsonResponse(usage: null);

        var result = await Model(server).DoEmbedAsync(TestValues, null, CancellationToken.None);

        Assert.Null(result.Tokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "should expose the raw response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_expose_the_raw_response()
    {
        var server = PrepareJsonResponse(headers: [("test-header", "test-value")]);

        var result = await Model(server).DoEmbedAsync(TestValues, null, CancellationToken.None);

        var headers = new Dictionary<string, string>(
            result.Response!.Headers,
            StringComparer.OrdinalIgnoreCase
        );
        Assert.Equal("application/json", headers["content-type"]);
        Assert.Equal("test-value", headers["test-header"]);
        JsonAssert.Equal(
            JsonNode.Parse(result.Response.Body!),
            "{\"output\":{\"embeddings\":[{\"embedding\":[0.1,0.2,0.3],\"text_index\":0},{\"embedding\":[0.4,0.5,0.6],\"text_index\":1}]},\"usage\":{\"total_tokens\":8}}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass the model and values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_model_and_values()
    {
        var server = PrepareJsonResponse();

        await Model(server).DoEmbedAsync(TestValues, null, CancellationToken.None);

        JsonAssert.Equal(
            JsonNode.Parse(server.Calls[0].Text),
            "{\"model\":\"text-embedding-v4\",\"input\":{\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]},\"parameters\":{}}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_provider_options()
    {
        var server = PrepareJsonResponse();

        await Model(server)
            .DoEmbedAsync(
                TestValues,
                AlibabaOptions(
                    "{\"textType\":\"query\",\"dimension\":768,\"outputType\":\"dense&sparse\"}"
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            JsonNode.Parse(server.Calls[0].Text),
            "{\"model\":\"text-embedding-v4\",\"input\":{\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]},\"parameters\":{\"text_type\":\"query\",\"dimension\":768,\"output_type\":\"dense&sparse\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should extract sparse embeddings into provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_extract_sparse_embeddings_into_provider_metadata()
    {
        var server = PrepareJsonResponse(sparse: true);

        var result = await ((IEmbeddingCaller)Model(server)).DoEmbedAsync(
            new EmbeddingModelCall(
                TestValues,
                new Dictionary<string, string>(),
                JsonSerializer.SerializeToElement(
                    AlibabaOptions("{\"outputType\":\"dense&sparse\"}")
                ),
                CancellationToken.None
            ),
            CancellationToken.None
        );

        JsonAssert.Equal(
            result.ProviderMetadata!.Value,
            "{\"alibaba\":{\"sparseEmbeddings\":[{\"textIndex\":0,\"sparseEmbedding\":[{\"index\":100,\"value\":0.8,\"token\":\"token-0\"}]},{\"textIndex\":1,\"sparseEmbedding\":[{\"index\":101,\"value\":0.8,\"token\":\"token-1\"}]}]}}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject sparse-only output", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_sparse_only_output()
    {
        var server = PrepareJsonResponse();

        await Assert.ThrowsAsync<UnsupportedFunctionalityException>(() =>
            Model(server)
                .DoEmbedAsync(
                    TestValues,
                    AlibabaOptions("{\"outputType\":\"sparse\"}"),
                    CancellationToken.None
                )
        );

        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject too many values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reject_too_many_values()
    {
        var server = PrepareJsonResponse();
        var values = Enumerable.Range(0, 11).Select(index => "value-" + index).ToArray();

        await Assert.ThrowsAsync<TooManyEmbeddingValuesForCallException>(() =>
            Model(server).DoEmbedAsync(values, null, CancellationToken.None)
        );

        Assert.Empty(server.Calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass headers",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port's user agent is ai-sdk/alibaba/{version}, not ai-sdk-alibaba/{version}. HttpClient adds Content-Length, which the comparison leaves out like the user agent."
    )]
    public async Task Should_pass_headers()
    {
        var server = PrepareJsonResponse();
        var options = new AlibabaProviderSettings { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = AlibabaProviderType.Create(options, server);

        await provider
            .Embedding("text-embedding-v4")
            .DoEmbedAsync(
                TestValues,
                null,
                CancellationToken.None,
                headers: new Dictionary<string, string?>
                {
                    ["Custom-Request-Header"] = "request-header-value",
                }
            );

        var call = server.Calls[0];
        var headers = call
            .Headers.Where(pair =>
                !string.Equals(pair.Key, "User-Agent", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(pair.Key, "Content-Length", StringComparison.OrdinalIgnoreCase)
            )
            .ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["authorization"] = "Bearer test-api-key",
                ["content-type"] = "application/json",
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
            },
            headers
        );
        Assert.Contains(
            "ai-sdk/alibaba/" + AiSdkVersion.Version,
            call.Header("User-Agent") ?? string.Empty,
            StringComparison.Ordinal
        );
    }

    /// <summary>The upstream <c>prepareJsonResponse</c>: a server that answers the embedding route.</summary>
    private static ParityHandler PrepareJsonResponse(
        string? usage = "{\"total_tokens\":8}",
        bool reverseOrder = false,
        bool sparse = false,
        (string Name, string Value)[]? headers = null
    )
    {
        var data = new List<JsonNode>();
        for (var textIndex = 0; textIndex < DummyEmbeddings.Length; textIndex++)
        {
            var item = new JsonObject
            {
                ["embedding"] = new JsonArray(
                    DummyEmbeddings[textIndex].Select(value => (JsonNode?)value).ToArray()
                ),
                ["text_index"] = textIndex,
            };
            if (sparse)
            {
                item["sparse_embedding"] = new JsonArray(
                    new JsonObject
                    {
                        ["index"] = textIndex + 100,
                        ["value"] = 0.8,
                        ["token"] = "token-" + textIndex,
                    }
                );
            }

            data.Add(item);
        }

        if (reverseOrder)
        {
            data.Reverse();
        }

        var body = new JsonObject
        {
            ["output"] = new JsonObject { ["embeddings"] = new JsonArray(data.ToArray()) },
        };
        if (usage != null)
        {
            body["usage"] = JsonNode.Parse(usage);
        }

        var json = body.ToJsonString();
        return new ParityHandler(call =>
            call.Uri.AbsoluteUri == Url
                ? ParityHandler.Json(json, headers ?? [])
                : throw new InvalidOperationException("Unexpected URL " + call.Uri)
        );
    }

    private static Vercel.AI.Alibaba.AlibabaEmbeddingModel Model(ParityHandler server)
    {
        return AlibabaProviderType
            .Create(new AlibabaProviderSettings { ApiKey = "test-api-key" }, server)
            .Embedding("text-embedding-v4");
    }

    private static Dictionary<string, JsonElement> AlibabaOptions(string json)
    {
        return new Dictionary<string, JsonElement>
        {
            ["alibaba"] = JsonSerializer.Deserialize<JsonElement>(json),
        };
    }
}
