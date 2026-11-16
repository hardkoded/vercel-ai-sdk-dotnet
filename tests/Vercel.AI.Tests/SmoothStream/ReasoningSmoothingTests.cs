// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class ReasoningSmoothingTests : SmoothStreamTestBase
{
    private static ReasoningStartPart Start => new("1");

    private static ReasoningEndPart End => new("1");

    [Fact]
    public async Task ShouldCombinePartialReasoningWords()
    {
        await Run([Start, R("Let"), R(" me "), R("think..."), End]);
        Expect(Start, R("Let "), Delay, R("me "), Delay, R("think..."), End);
    }

    [Fact]
    public async Task ShouldSplitLargerReasoningChunks()
    {
        await Run([Start, R("First I need to analyze the problem. Then I will solve it."), End]);
        Expect(
            Start,
            R("First "), Delay, R("I "), Delay, R("need "), Delay, R("to "), Delay, R("analyze "), Delay, R("the "), Delay,
            R("problem. "), Delay, R("Then "), Delay, R("I "), Delay, R("will "), Delay, R("solve "), Delay, R("it."),
            End);
    }

    [Fact]
    public async Task ShouldFlushReasoningBufferBeforeToolCall()
    {
        var call = new ToolCallPart(new GeneratedToolCall("1", "weather", "{\"city\":\"London\"}"));
        await Run([Start, R("I should check the"), R(" weather"), call, End]);
        Expect(Start, R("I "), Delay, R("should "), Delay, R("check "), Delay, R("the "), Delay, R("weather"), call, End);
    }

    [Fact]
    public async Task ShouldUseLineChunkingForReasoning()
    {
        await Run(
            [Start, R("Step 1: Analyze\nStep 2: Solve\n"), End],
            new SmoothStreamOptions { DelayInMs = 10, Chunking = SmoothStreamChunking.Line });
        Expect(Start, R("Step 1: Analyze\n"), Delay, R("Step 2: Solve\n"), Delay, End);
    }
}
