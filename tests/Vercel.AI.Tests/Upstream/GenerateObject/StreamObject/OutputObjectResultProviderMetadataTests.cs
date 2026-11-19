// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.providerMetadata</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultProviderMetadataTests
{
    private const string Id = Prefix + "output = \"object\" > result.providerMetadata::";

    [Fact]
    [UpstreamTest(
        Id + "should resolve with provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_resolve_with_provider_metadata()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    CreateTestModel([
                        new TextStartStreamPart("1"),
                        Delta("{ \"content\": \"Hello, world!\" }"),
                        new TextEndStreamPart("1"),
                        Finish(
                            JsonSerializer.SerializeToElement(
                                new { testProvider = new { testKey = "testValue" } }
                            )
                        ),
                    ])
                )
            );

        // consume stream (runs in parallel)
        _ = Collect(result.PartialObjectStream());

        Assert.Equal(
            "{\"testProvider\":{\"testKey\":\"testValue\"}}",
            (await result.ProviderMetadata)!.Value.GetRawText()
        );
    }
}
