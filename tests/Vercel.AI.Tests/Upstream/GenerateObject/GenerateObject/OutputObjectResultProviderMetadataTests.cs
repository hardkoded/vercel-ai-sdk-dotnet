// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; result.providerMetadata</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultProviderMetadataTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > result.providerMetadata::";

    [Fact]
    [UpstreamTest(Id + "should contain provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_provider_metadata()
    {
        var metadata = JsonSerializer.SerializeToElement(
            new Dictionary<string, object> { ["exampleProvider"] = new { a = 10, b = 20 } }
        );
        var model = new GenerateObjectTestModel
        {
            Generate = _ => new LanguageModelGenerateResult(
                [new GeneratedText("{ \"content\": \"Hello, world!\" }")],
                FinishReason.Stop,
                new LanguageModelUsage(10, 20, 30),
                providerMetadata: metadata
            ),
        };

        var result = await Client().GenerateObjectAsync(ObjectOptions(model));

        Assert.Equal(metadata.GetRawText(), result.ProviderMetadata!.Value.GetRawText());
    }
}
