// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;
using static Vercel.AI.Tests.Upstream.GenerateText.ResultToolCallsTests;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.onEnd</c>.</summary>
public sealed class OptionsOnEndTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.onEnd::";

    private static readonly LanguageModelUsage TestUsage = new(3, 10, 13);

    private static TestLanguageModel Model() =>
        new()
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Hello, World!") },
                FinishReason.Stop,
                TestUsage,
                "stop"
            ),
        };

    // Partial: the upstream inline snapshot of the whole event also covers fields the port's
    // GenerateTextResult does not carry (callId, performance, content aggregated across steps,
    // static/dynamic tool call splits). See the PR report.
    [Fact]
    [UpstreamTest(Prefix + "should send correct information", Coverage = UpstreamCoverage.Partial)]
    public async Task Should_send_correct_information()
    {
        GenerateTextResult? result = null;

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel
                    {
                        OnGenerate = _ => new LanguageModelGenerateResult(
                            new GeneratedContent[]
                            {
                                new GeneratedText("Hello, World!"),
                                new GeneratedToolCall(
                                    "call-1",
                                    "tool1",
                                    "{ \"value\": \"value\" }"
                                ),
                            },
                            FinishReason.Stop,
                            TestUsage,
                            "stop"
                        ),
                    },
                    Tools = new[]
                    {
                        Tool.Function(
                            "tool1",
                            null,
                            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"]}",
                            (_, _) => Task.FromResult("value-result")
                        ),
                    },
                    OnEnd = (e, _) =>
                    {
                        result = e;
                        return Task.CompletedTask;
                    },
                    Prompt = "irrelevant",
                }
            );

        Assert.NotNull(result);
        Assert.Equal("Hello, World!", result!.Text);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal(TestUsage.TotalTokens, result.Usage.TotalTokens);
        Assert.Single(result.Steps);
        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.Equal(2, result.ResponseMessages.Count);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should support onFinish as a deprecated alias",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_support_onFinish_as_a_deprecated_alias()
    {
        var calls = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model(),
                    Prompt = "irrelevant",
#pragma warning disable CS0618
                    OnFinish = (_, _) =>
                    {
                        calls.Add("onFinish");
                        return Task.CompletedTask;
                    },
#pragma warning restore CS0618
                }
            );

        Assert.Equal(new[] { "onFinish" }, calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should prefer onEnd over onFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_prefer_onEnd_over_onFinish()
    {
        var calls = new List<string>();

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = Model(),
                    Prompt = "irrelevant",
                    OnEnd = (_, _) =>
                    {
                        calls.Add("onEnd");
                        return Task.CompletedTask;
                    },
#pragma warning disable CS0618
                    OnFinish = (_, _) =>
                    {
                        calls.Add("onFinish");
                        return Task.CompletedTask;
                    },
#pragma warning restore CS0618
                }
            );

        Assert.Equal(new[] { "onEnd" }, calls);
    }
}
