// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralEmbeddingModel;

/// <summary>Port of <c>mistral-embedding-model.test.ts</c> &gt; <c>doEmbed</c>.</summary>
public sealed class DoEmbedTests
{
    private static readonly string[] TestValues = { "sunny day at the beach", "rainy day in the city" };

    private const string Response = "{\"id\":\"b322cfc2b9d34e2f8e14fc99874faee5\",\"object\":\"list\",\"data\":[{\"object\":\"embedding\",\"embedding\":[0.1,0.2,0.3,0.4,0.5],\"index\":0},{\"object\":\"embedding\",\"embedding\":[0.6,0.7,0.8,0.9,1.0],\"index\":1}],\"model\":\"mistral-embed\",\"usage\":{\"prompt_tokens\":8,\"total_tokens\":8}}";

    public static TheoryData<int?, int?, int?> Cases => EmbeddingDimensionCases.Cases;

    [Theory]
    [MemberData(nameof(Cases))]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::maps dimensions $dimensions with provider override $providerDimensions to $expected", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_dimensions_with_provider_override(int? dimensions, int? providerDimensions, int? expected)
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("codestral-embed").DoEmbedAsync(TestValues, EmbeddingDimensionCases.Options("mistral", "outputDimension", providerDimensions), CancellationToken.None, dimensions);

        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        EmbeddingDimensionCases.AssertDimension(body["output_dimension"], expected);
        Assert.Null(body["dimensions"]);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should pass the model and the values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_the_model_and_the_values()
    {
        var capture = new UpstreamCapture { ResponseBody = Response };
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);

        await provider.EmbeddingModel("mistral-embed").DoEmbedAsync(TestValues, null, CancellationToken.None);

        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"model\":\"mistral-embed\",\"input\":[\"sunny day at the beach\",\"rainy day in the city\"],\"encoding_format\":\"float\"}");
    }
}
