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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; result.request</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultRequestTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > result.request::";

    [Fact]
    [UpstreamTest(Id + "should contain request information", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_request_information()
    {
        var model = new GenerateObjectTestModel
        {
            Generate = _ => new LanguageModelGenerateResult(
                [new GeneratedText("{ \"content\": \"Hello, world!\" }")],
                FinishReason.Stop,
                new LanguageModelUsage(10, 20, 30),
                request: new LanguageModelStreamRequest(
                    JsonSerializer.SerializeToElement("test body")
                )
            ),
        };

        var result = await Client().GenerateObjectAsync(ObjectOptions(model));

        Assert.Equal("test body", result.Request.Body!.Value.GetString());
    }
}
