// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateText.OptionsTimeoutSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.abortSignal</c>. The <see cref="CancellationToken"/> stands in for <c>abortSignal</c>.</summary>
public sealed class OptionsAbortSignalTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/generate-text.test.ts::generateText > options.abortSignal::should forward abort signal to tool execution", Coverage = UpstreamCoverage.Partial, Note = "The token is not cancelled, because a cancelled token stops the call before the tool runs, while upstream's mock ignores the abort. A tool delegate gets the input and the token, not toolCallId, messages, or context.")]
    public async Task Should_forward_abort_signal_to_tool_execution()
    {
        using var abort = new CancellationTokenSource();
        var toolCalls = new List<(string Input, CancellationToken Token)>();
        var model = new DelegateModel { Generate = (_, _) => Task.FromResult(ToolCallResult("{ \"value\": \"value\" }")) };

        await Client().GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = model,
                Prompt = "test-input",
                Tools = new[]
                {
                    ToolOne((input, token) =>
                    {
                        toolCalls.Add((input.GetProperty("value").GetString()!, token));
                        return Task.FromResult("\"tool result\"");
                    }),
                },
            },
            abort.Token);

        var call = Assert.Single(toolCalls);
        Assert.Equal("value", call.Input);
        Assert.Equal(abort.Token, call.Token);
    }
}
