// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class InterleavedTextAndReasoningTests : SmoothStreamTestBase
{
    [Fact]
    public async Task ShouldFlushTextBufferWhenSwitchingToReasoning()
    {
        await Run([
            new TextStartPart("1"),
            new ReasoningStartPart("2"),
            D("Hello ", "1"),
            D("world", "1"),
            R("Let me", "2"),
            R(" think", "2"),
            new TextEndPart("1"),
            new ReasoningEndPart("2"),
        ]);
        Expect(
            new TextStartPart("1"),
            new ReasoningStartPart("2"),
            D("Hello ", "1"),
            Delay,
            D("world", "1"),
            R("Let ", "2"),
            Delay,
            R("me ", "2"),
            Delay,
            R("think", "2"),
            new TextEndPart("1"),
            new ReasoningEndPart("2")
        );
    }

    [Fact]
    public async Task ShouldFlushReasoningBufferWhenSwitchingToText()
    {
        await Run([
            new ReasoningStartPart("1"),
            new TextStartPart("2"),
            R("Thinking ", "1"),
            R("hard", "1"),
            D("The answer", "2"),
            D(" is 42", "2"),
            new ReasoningEndPart("1"),
            new TextEndPart("2"),
        ]);
        Expect(
            new ReasoningStartPart("1"),
            new TextStartPart("2"),
            R("Thinking ", "1"),
            Delay,
            R("hard", "1"),
            D("The ", "2"),
            Delay,
            D("answer ", "2"),
            Delay,
            D("is ", "2"),
            Delay,
            D("42", "2"),
            new ReasoningEndPart("1"),
            new TextEndPart("2")
        );
    }

    [Fact]
    public async Task ShouldHandleMultipleSwitchesBetweenTextAndReasoning()
    {
        await Run([
            new ReasoningStartPart("r1"),
            new TextStartPart("t1"),
            R("Think ", "r1"),
            D("Hello ", "t1"),
            R("more ", "r1"),
            D("world ", "t1"),
            new ReasoningEndPart("r1"),
            new TextEndPart("t1"),
        ]);
        Expect(
            new ReasoningStartPart("r1"),
            new TextStartPart("t1"),
            R("Think ", "r1"),
            Delay,
            D("Hello ", "t1"),
            Delay,
            R("more ", "r1"),
            Delay,
            D("world ", "t1"),
            Delay,
            new ReasoningEndPart("r1"),
            new TextEndPart("t1")
        );
    }
}
