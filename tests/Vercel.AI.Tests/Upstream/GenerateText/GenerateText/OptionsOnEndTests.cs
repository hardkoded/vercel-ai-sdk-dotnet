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

    [Fact]
    [UpstreamTest(Prefix + "should send correct information", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_correct_information()
    {
        GenerateTextResult? result = null;

        await Client()
            .GenerateTextAsync(
                new GenerateTextOptions
                {
                    Model = new TestLanguageModel("mock-model-id")
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
                            "stop",
                            responseId: "id-0",
                            responseModelId: "mock-model-id",
                            responseTimestamp: DateTimeOffset.FromUnixTimeMilliseconds(0),
                            responseHeaders: new Dictionary<string, string> { ["call"] = "2" }
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
                    GenerateCallId = () => "test-telemetry-call-id",
                    NowMs = () => 0,
                    OnEnd = (e, _) =>
                    {
                        result = e;
                        return Task.CompletedTask;
                    },
                    Prompt = "irrelevant",
                }
            );

        Assert.NotNull(result);
        Assert.Equal("test-telemetry-call-id", result!.CallId);
        Assert.Equal(0, result.StepNumber);
        Assert.Equal("mock-model-id", result.ModelId);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("stop", result.RawFinishReason);
        Assert.Equal("Hello, World!", result.Text);
        Assert.Null(result.ReasoningText);
        Assert.Empty(result.Reasoning);
        Assert.Equal("test", result.Model.Provider);
        Assert.Equal("mock-model-id", result.Model.ModelId);
        Assert.Empty(result.Files);
        Assert.Empty(result.Sources);
        Assert.Empty(result.Warnings);
        Assert.Null(result.ProviderMetadata);
        Assert.Empty(result.RuntimeContext);
        Assert.Empty(result.ToolsContext);
        Assert.Equal(TestUsage.TotalTokens, result.Usage.TotalTokens);
        Assert.Equal(TestUsage.TotalTokens, result.TotalUsage.TotalTokens);

        Assert.Equal(3, result.Content.Count);
        Assert.Equal("Hello, World!", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        Assert.Equal("call-1", Assert.IsType<GeneratedToolCall>(result.Content[1]).ToolCallId);

        var call = Assert.Single(result.ToolCalls);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.False(call.Dynamic);
        Assert.Equal(call, Assert.Single(result.StaticToolCalls));
        Assert.Empty(result.DynamicToolCalls);

        var toolResult = Assert.Single(result.ToolResults);
        Assert.Equal("call-1", toolResult.ToolCallId);
        Assert.Equal("value-result", toolResult.OutputJson);
        Assert.Equal(toolResult, Assert.Single(result.StaticToolResults));
        Assert.Empty(result.DynamicToolResults);

        Assert.Equal("id-0", result.Response.Id);
        Assert.Equal("mock-model-id", result.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), result.Response.Timestamp);
        Assert.Equal("2", result.Response.Headers!["call"]);
        Assert.Equal(2, result.ResponseMessages.Count);
        Assert.Equal(2, result.Response.Messages.Count);

        var step = Assert.Single(result.Steps);
        Assert.Same(step, result.FinalStep);
        Assert.Equal("test-telemetry-call-id", step.CallId);
        Assert.Equal(0, step.StepNumber);
        Assert.Equal("test", step.Provider);
        Assert.Equal("mock-model-id", step.ModelId);
        var performance = step.Performance;
        Assert.Equal(0, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, performance.EffectiveTotalTokensPerSecond);
        Assert.Null(performance.InputTokensPerSecond);
        Assert.Null(performance.OutputTokensPerSecond);
        Assert.Equal(0, performance.ResponseTimeMs);
        Assert.Equal(0, performance.StepTimeMs);
        Assert.Null(performance.TimeToFirstOutputMs);
        Assert.Equal(0, Assert.Single(performance.ToolExecutionMs).Value);
        Assert.Equal("call-1", Assert.Single(performance.ToolExecutionMs).Key);
        Assert.Equal(3, step.Content.Count);
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
