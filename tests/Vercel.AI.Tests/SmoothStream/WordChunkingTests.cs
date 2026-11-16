// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class WordChunkingTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    [Fact]
    public async Task ForwardsPingsWithoutFlushingAPartialWord()
    {
        await Run([Start, D("Hel"), new PingPart(), D("lo "), End], new SmoothStreamOptions { DelayInMs = null, Delay = (_, _) => Task.CompletedTask });
        Expect(Start, new PingPart(), D("Hello "), End);
    }

    [Fact]
    public async Task ShouldCombinePartialWords()
    {
        await Run([Start, D("Hello"), D(", "), D("world!"), End]);
        Expect(Start, D("Hello, "), Delay, D("world!"), End);
    }

    [Fact]
    public async Task ShouldSplitLargerTextChunks()
    {
        await Run([Start, D("Hello, World! This is an example text."), End]);
        Expect(Start, D("Hello, "), Delay, D("World! "), Delay, D("This "), Delay, D("is "), Delay, D("an "), Delay, D("example "), Delay, D("text."), End);
    }

    [Fact]
    public async Task ShouldKeepLongerWhitespaceSequencesTogether()
    {
        await Run([Start, D("First line"), D(" \n\n"), D("  "), D("  Multiple spaces"), D("\n    Indented"), End]);
        Expect(Start, D("First "), Delay, D("line \n\n"), Delay, D("    Multiple "), Delay, D("spaces\n    "), Delay, D("Indented"), End);
    }

    [Fact]
    public async Task ShouldSendRemainingTextBufferBeforeToolCallStarts()
    {
        var call = new ToolCallPart(new GeneratedToolCall("1", "weather", "{\"city\":\"London\"}"));
        await Run([Start, D("I will check the"), D(" weather in Lon"), D("don."), call, End]);
        Expect(Start, D("I "), Delay, D("will "), Delay, D("check "), Delay, D("the "), Delay, D("weather "), Delay, D("in "), Delay, D("London."), call, End);
    }

    [Fact]
    public async Task ShouldSendRemainingTextBufferBeforeToolCallStartsAndToolCallStreamingIsEnabled()
    {
        var call = new ToolCallPart(new GeneratedToolCall("1", "weather", "{\"city\":\"London\"}"));
        var inputStart = new ToolInputStartPart("2", "weather", null, null, false);
        var inputDelta = new ToolInputDeltaPart("2", "{ city: \"London\" }", null);
        var inputEnd = new ToolInputEndPart("2", null);
        await Run([Start, D("I will check the"), D(" weather in Lon"), D("don."), inputStart, inputDelta, inputEnd, call, End]);
        Expect(Start, D("I "), Delay, D("will "), Delay, D("check "), Delay, D("the "), Delay, D("weather "), Delay, D("in "), Delay, D("London."), inputStart, inputDelta, inputEnd, call, End);
    }

    [Fact]
    public async Task DoesntReturnChunksWithJustSpaces()
    {
        await Run([Start, D(" "), D(" "), D(" "), D("foo"), End]);
        Expect(Start, D("   foo"), End);
    }
}
