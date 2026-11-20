// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; output = "object"</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > output = \"object\"::";
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"content\":{\"type\":\"string\"}},\"required\":[\"content\"],\"additionalProperties\":false}";

    private sealed class ContentResult
    {
        public string Content { get; set; } = string.Empty;
    }

    private static async Task<List<LogWarningsOptions>> Spy(CallWarning[] warnings)
    {
        var calls = new List<LogWarningsOptions>();
        var previous = LogWarnings.Observer;
        LogWarnings.Observer = calls.Add;
        try
        {
            await new AiClient(
                Vercel.AI.Gateway.GatewayProvider.Create(
                    new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
                )
            ).GenerateObjectAsync(
                new GenerateObjectOptions<ContentResult>
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => new LanguageModelGenerateResult(
                            [new GeneratedText("{ \"content\": \"Hello, world!\" }")],
                            FinishReason.Stop,
                            new LanguageModelUsage(3, 10, 13),
                            warnings: warnings
                        ),
                    },
                    Schema = Schema,
                    Prompt = "prompt",
                    SerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    },
                }
            );
        }
        finally
        {
            LogWarnings.Observer = previous;
        }

        return calls;
    }

    [Fact]
    [UpstreamTest(
        Id + "should call logWarnings with the correct warnings",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_the_correct_warnings()
    {
        CallWarning[] expected =
        [
            new("other", "Setting is not supported"),
            new("unsupported", "temperature", "Temperature parameter not supported"),
        ];

        var calls = await Spy(expected);

        Assert.Equal(2, calls.Count);
        Assert.Equal("test", calls[1].Provider);
        Assert.Equal("test", calls[1].Model);
        Assert.Equal(2, calls[1].Warnings.Count);
        Assert.Equal(
            "Setting is not supported",
            Assert.IsType<OtherWarning>(calls[1].Warnings[0]).Message
        );
        var unsupported = Assert.IsType<UnsupportedWarning>(calls[1].Warnings[1]);
        Assert.Equal("temperature", unsupported.Feature);
        Assert.Equal("Temperature parameter not supported", unsupported.Details);
    }

    [Fact]
    [UpstreamTest(
        Id + "should call logWarnings with empty array when no warnings are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_empty_array_when_no_warnings_are_present()
    {
        var calls = await Spy([]);

        Assert.Equal(2, calls.Count);
        Assert.Equal("test", calls[1].Provider);
        Assert.Equal("test", calls[1].Model);
        Assert.Empty(calls[1].Warnings);
    }
}
