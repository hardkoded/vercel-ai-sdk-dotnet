// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.request</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultRequestTests
{
    private const string Id = Prefix + "output = \"object\" > result.request::";

    [Fact]
    [UpstreamTest(Id + "should contain request information", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_request_information()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    Model(
                        [
                            ResponseMetadata(),
                            new TextStartStreamPart("1"),
                            Delta("{\"content\": \"Hello, world!\"}"),
                            new TextEndStreamPart("1"),
                            Finish(),
                        ],
                        new LanguageModelStreamRequest(
                            JsonSerializer.SerializeToElement("test body")
                        )
                    )
                )
            );

        // consume stream (runs in parallel)
        await Collect(result.PartialObjectStream());

        Assert.Equal("\"test body\"", (await result.Request).Body!.Value.GetRawText());
    }
}
