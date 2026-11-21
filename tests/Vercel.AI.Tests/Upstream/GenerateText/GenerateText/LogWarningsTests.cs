// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; logWarnings</c>.</summary>
[Collection("LogWarnings")]
public sealed class LogWarningsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > logWarnings::";
    private const string Schema =
        "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}";

    private static async Task<List<LogWarningsOptions>> Spy(Func<Task> body)
    {
        var calls = new List<LogWarningsOptions>();
        var previous = LogWarnings.Observer;
        LogWarnings.Observer = calls.Add;
        try
        {
            await body();
        }
        finally
        {
            LogWarnings.Observer = previous;
        }

        return calls;
    }

    private static LanguageModelGenerateResult Generated(
        GeneratedContent content,
        FinishReason finishReason,
        CallWarning[] warnings
    ) => new([content], finishReason, new LanguageModelUsage(3, 10, 13), warnings: warnings);

    [Fact]
    [UpstreamTest(
        Prefix + "should call logWarnings with warnings from a single step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_warnings_from_a_single_step()
    {
        CallWarning[] expected =
        [
            new("other", "Setting is not supported"),
            new("unsupported", "temperature", "Temperature parameter not supported"),
        ];

        var calls = await Spy(() =>
            new AiClient(
                GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
            ).GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                            Generated(
                                new GeneratedText("Hello, world!"),
                                FinishReason.Stop,
                                expected
                            ),
                    },
                    Prompt = "Hello",
                }
            )
        );

        var call = Assert.Single(calls);
        Assert.Equal("test", call.Provider);
        Assert.Equal("test", call.Model);
        Assert.Equal(2, call.Warnings.Count);
        Assert.Equal(
            "Setting is not supported",
            Assert.IsType<OtherWarning>(call.Warnings[0]).Message
        );
        var unsupported = Assert.IsType<UnsupportedWarning>(call.Warnings[1]);
        Assert.Equal("temperature", unsupported.Feature);
        Assert.Equal("Temperature parameter not supported", unsupported.Details);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call logWarnings once for each step with warnings from that step",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_once_for_each_step_with_warnings_from_that_step()
    {
        var callCount = 0;

        var calls = await Spy(() =>
            new AiClient(
                GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
            ).GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                            callCount++ switch
                            {
                                0 => Generated(
                                    new GeneratedToolCall(
                                        "call-1",
                                        "testTool",
                                        "{ \"value\": \"test\" }"
                                    ),
                                    FinishReason.ToolCalls,
                                    [new("other", "Warning from step 1")]
                                ),
                                1 => Generated(
                                    new GeneratedText("Final response"),
                                    FinishReason.Stop,
                                    [new("other", "Warning from step 2")]
                                ),
                                _ => throw new InvalidOperationException("Unexpected call"),
                            },
                    },
                    Prompt = "Hello",
                    Tools =
                    [
                        Tool.Function("testTool", null, Schema, (_, _) =>
                            Task.FromResult("\"result\"")
                        ),
                    ],
                    StopWhen = StopWhen.IsStepCount(3),
                }
            )
        );

        Assert.Equal(2, calls.Count);
        Assert.Equal(
            "Warning from step 1",
            Assert.IsType<OtherWarning>(Assert.Single(calls[0].Warnings)).Message
        );
        Assert.Equal(
            "Warning from step 2",
            Assert.IsType<OtherWarning>(Assert.Single(calls[1].Warnings)).Message
        );
        Assert.All(calls, call =>
        {
            Assert.Equal("test", call.Provider);
            Assert.Equal("test", call.Model);
        });
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should call logWarnings with empty array when no warnings are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_logWarnings_with_empty_array_when_no_warnings_are_present()
    {
        var calls = await Spy(() =>
            new AiClient(
                GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })
            ).GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ =>
                            Generated(new GeneratedText("Hello, world!"), FinishReason.Stop, []),
                    },
                    Prompt = "Hello",
                }
            )
        );

        var call = Assert.Single(calls);
        Assert.Empty(call.Warnings);
        Assert.Equal("test", call.Provider);
        Assert.Equal("test", call.Model);
    }
}
