// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>
/// Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>timeout</c>.
/// The port has no <c>executeToolCall</c> function, so each test runs one tool call through <c>GenerateTextAsync</c>.
/// A tool receives a <see cref="CancellationToken"/>, so "an abort signal" is a token that can be cancelled, and "no abort signal" is the token the caller passed.
/// </summary>
public sealed class ExecuteToolCallTimeoutTests
{
    private const string Prefix = "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > timeout::";

    private static Tool TestTool(Func<string, CancellationToken, string> execute)
    {
        return Tool.Function("testTool", null, ValueSchema, (input, token) => Task.FromResult(JsonSerializer.Serialize(execute(input.GetProperty("value").GetString()!, token))));
    }

    private static async Task<GenerateTextResult> Run(Tool tool, TimeoutConfiguration? timeout, CancellationToken token = default)
    {
        var model = new DelegateModel
        {
            Generate = (_, _) => Task.FromResult(new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedToolCall("call-1", "testTool", "{ \"value\": \"test\" }") },
                FinishReason.ToolCalls,
                OptionsTimeoutSupport.Usage,
                "tool-calls")),
        };

        return await Client().GenerateTextAsync(
            new GenerateTextOptions { Model = model, Prompt = "test-input", Tools = new[] { tool }, Timeout = timeout },
            token);
    }

    [Fact]
    [UpstreamTest(Prefix + "should return tool-result when tool completes before timeout", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_tool_result_when_tool_completes_before_timeout()
    {
        var result = await Run(TestTool((value, _) => value + "-result"), new TimeoutConfiguration { ToolMs = 5000 });

        var toolResult = Assert.Single(result.ToolResults);
        Assert.False(toolResult.IsError);
        Assert.Equal("\"test-result\"", toolResult.OutputJson);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass an abort signal to tool when toolMs is set", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_an_abort_signal_to_tool_when_toolMs_is_set()
    {
        CancellationToken? received = null;

        await Run(TestTool((value, token) =>
        {
            received = token;
            return value + "-result";
        }), new TimeoutConfiguration { ToolMs = 5000 });

        Assert.True(received?.CanBeCanceled);
        Assert.False(received!.Value.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not create abort signal when no timeout", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_create_abort_signal_when_no_timeout()
    {
        CancellationToken? received = null;

        await Run(TestTool((value, token) =>
        {
            received = token;
            return value + "-result";
        }), null);

        Assert.NotNull(received);
        Assert.False(received.Value.CanBeCanceled);
    }

    [Fact]
    [UpstreamTest(Prefix + "should merge toolMs with existing abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_merge_toolMs_with_existing_abort_signal()
    {
        using var controller = new CancellationTokenSource();
        CancellationToken? received = null;

        await Run(
            TestTool((value, token) =>
            {
                received = token;
                return value + "-result";
            }),
            new TimeoutConfiguration { ToolMs = 5000 },
            controller.Token);

        Assert.True(received?.CanBeCanceled);
        Assert.NotEqual(controller.Token, received!.Value);
        Assert.False(received.Value.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should use per-tool timeout over generic toolMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_per_tool_timeout_over_generic_toolMs()
    {
        CancellationToken? received = null;

        await Run(TestTool((value, token) =>
        {
            received = token;
            return value + "-result";
        }), new TimeoutConfiguration { ToolMs = 10000, Tools = new Dictionary<string, int> { ["testToolMs"] = 2000 } });

        Assert.True(received?.CanBeCanceled);
        Assert.False(received!.Value.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should fall back to toolMs when tool not in tools", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_fall_back_to_toolMs_when_tool_not_in_tools()
    {
        CancellationToken? received = null;

        await Run(TestTool((value, token) =>
        {
            received = token;
            return value + "-result";
        }), new TimeoutConfiguration { ToolMs = 5000, Tools = new Dictionary<string, int> { ["otherToolMs"] = 2000 } });

        Assert.True(received?.CanBeCanceled);
        Assert.False(received!.Value.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not create abort signal when tool not in tools and no toolMs", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_create_abort_signal_when_tool_not_in_tools_and_no_toolMs()
    {
        CancellationToken? received = null;

        await Run(TestTool((value, token) =>
        {
            received = token;
            return value + "-result";
        }), new TimeoutConfiguration { Tools = new Dictionary<string, int> { ["otherToolMs"] = 2000 } });

        Assert.NotNull(received);
        Assert.False(received.Value.CanBeCanceled);
    }
}
