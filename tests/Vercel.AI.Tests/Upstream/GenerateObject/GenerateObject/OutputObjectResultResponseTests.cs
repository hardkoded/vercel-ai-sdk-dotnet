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

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object" &gt; result.response</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultResponseTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\" > result.response::";

    [Fact]
    [UpstreamTest(Id + "should contain response information", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_contain_response_information()
    {
        var model = new GenerateObjectTestModel
        {
            Generate = _ => new LanguageModelGenerateResult(
                [new GeneratedText("{ \"content\": \"Hello, world!\" }")],
                FinishReason.Stop,
                new LanguageModelUsage(10, 20, 30),
                responseId: "test-id-from-model",
                responseModelId: "test-response-model-id",
                responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(10000),
                responseHeaders: new Dictionary<string, string>
                {
                    ["custom-response-header"] = "response-header-value",
                    ["user-agent"] = AiSdkVersion.UserAgent,
                },
                rawResponse: "\"test body\""
            ),
        };

        var result = await Client().GenerateObjectAsync(ObjectOptions(model));

        Assert.Equal("test-id-from-model", result.Response.Id);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(10000), result.Response.Timestamp);
        Assert.Equal("test-response-model-id", result.Response.ModelId);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["custom-response-header"] = "response-header-value",
                ["user-agent"] = AiSdkVersion.UserAgent,
            },
            result.Response.Headers
        );
        Assert.Equal("test body", result.Response.Body!.Value.GetString());
    }
}
