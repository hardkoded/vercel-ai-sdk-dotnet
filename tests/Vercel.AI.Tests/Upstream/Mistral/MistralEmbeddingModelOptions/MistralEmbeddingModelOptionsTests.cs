// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Mistral;
using Vercel.AI.Operations;

namespace Vercel.AI.Tests.Upstream.Mistral.MistralEmbeddingModelOptions;

/// <summary>Port of <c>mistral-embedding-model-options.test.ts</c> &gt; <c>mistralEmbeddingModelOptions</c>.</summary>
public sealed class MistralEmbeddingModelOptionsTests
{
    private const string Id =
        "packages/mistral/src/mistral-embedding-model-options.test.ts::mistralEmbeddingModelOptions::";

    private static Dictionary<string, JsonElement> Options(string json) =>
        new() { ["mistral"] = JsonDocument.Parse(json).RootElement.Clone() };

    [Fact]
    [UpstreamTest(
        Id + "accepts supported embedding provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Accepts_supported_embedding_provider_options()
    {
        var options = Vercel.AI.Mistral.MistralEmbeddingModelOptions.Parse(
            Options(
                "{\"metadata\":{\"source\":\"test\"},\"outputDimension\":1024,\"outputDtype\":\"float\"}"
            )
        );

        Assert.NotNull(options);
        Assert.Equal("test", options!.Metadata!["source"]!.GetValue<string>());
        Assert.Equal(1024, options.OutputDimension);
        Assert.Equal("float", options.OutputDtype);
    }

    [Fact]
    [UpstreamTest(Id + "accepts all supported output dtypes", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_all_supported_output_dtypes()
    {
        foreach (var outputDtype in new[] { "float", "int8", "uint8", "binary", "ubinary" })
        {
            var options = Vercel.AI.Mistral.MistralEmbeddingModelOptions.Parse(
                Options("{\"outputDtype\":\"" + outputDtype + "\"}")
            );

            Assert.Equal(outputDtype, options!.OutputDtype);
        }
    }

    [Fact]
    [UpstreamTest(Id + "rejects invalid output dtype values", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_invalid_output_dtype_values()
    {
        Assert.Throws<InvalidArgumentException>(() =>
            Vercel.AI.Mistral.MistralEmbeddingModelOptions.Parse(
                Options("{\"outputDtype\":\"base64\"}")
            )
        );
    }
}
