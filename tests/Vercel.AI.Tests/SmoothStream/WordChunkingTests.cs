// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class WordChunkingTests
{
    private readonly List<object> _events = [];

    private static async IAsyncEnumerable<TextStreamPart> From(params TextStreamPart[] parts)
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    private async Task Run(TextStreamPart[] input, int? delayInMs = 10, bool recordDelay = true)
    {
        var options = new Vercel.AI.GenerateText.SmoothStreamOptions
        {
            DelayInMs = delayInMs,
            Delay = !recordDelay
                ? null
                : (ms, _) =>
                {
                    _events.Add($"delay {ms}");
                    return Task.CompletedTask;
                },
        };
        var transform = Vercel.AI.GenerateText.SmoothStream.Create(options)(null);
        await foreach (var part in transform(From(input), default))
        {
            _events.Add(part);
        }
    }

    private void Expect(params object[] expected)
    {
        Assert.Equal(expected.Length, _events.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            switch (expected[i])
            {
                case string s:
                    Assert.Equal(s, _events[i]);
                    break;
                case PingPart:
                    Assert.IsType<PingPart>(_events[i]);
                    break;
                case TextStartPart e:
                    Assert.Equal(e.Id, Assert.IsType<TextStartPart>(_events[i]).Id);
                    break;
                case TextEndPart e:
                    Assert.Equal(e.Id, Assert.IsType<TextEndPart>(_events[i]).Id);
                    break;
                case TextDeltaPart e:
                    var actual = Assert.IsType<TextDeltaPart>(_events[i]);
                    Assert.Equal(e.Id, actual.Id);
                    Assert.Equal(e.Text, actual.Text);
                    break;
                case ToolCallPart e:
                    var call = Assert.IsType<ToolCallPart>(_events[i]);
                    Assert.Equal(e.ToolCall.ToolCallId, call.ToolCall.ToolCallId);
                    Assert.Equal(e.ToolCall.ToolName, call.ToolCall.ToolName);
                    Assert.Equal(e.ToolCall.ArgumentsJson, call.ToolCall.ArgumentsJson);
                    break;
                case ToolInputStartPart e:
                    var start = Assert.IsType<ToolInputStartPart>(_events[i]);
                    Assert.Equal(e.Id, start.Id);
                    Assert.Equal(e.ToolName, start.ToolName);
                    break;
                case ToolInputDeltaPart e:
                    var delta = Assert.IsType<ToolInputDeltaPart>(_events[i]);
                    Assert.Equal(e.Id, delta.Id);
                    Assert.Equal(e.Delta, delta.Delta);
                    break;
                case ToolInputEndPart e:
                    Assert.Equal(e.Id, Assert.IsType<ToolInputEndPart>(_events[i]).Id);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected expectation.");
            }
        }
    }

    private static TextDeltaPart D(string text) => new("1", text);

    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    private static string Delay => "delay 10";

    [Fact]
    public async Task ForwardsPingsWithoutFlushingAPartialWord()
    {
        await Run(
            [Start, D("Hel"), new PingPart(), D("lo "), End],
            delayInMs: null,
            recordDelay: false
        );
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
        Expect(
            Start,
            D("Hello, "),
            Delay,
            D("World! "),
            Delay,
            D("This "),
            Delay,
            D("is "),
            Delay,
            D("an "),
            Delay,
            D("example "),
            Delay,
            D("text."),
            End
        );
    }

    [Fact]
    public async Task ShouldKeepLongerWhitespaceSequencesTogether()
    {
        await Run([
            Start,
            D("First line"),
            D(" \n\n"),
            D("  "),
            D("  Multiple spaces"),
            D("\n    Indented"),
            End,
        ]);
        Expect(
            Start,
            D("First "),
            Delay,
            D("line \n\n"),
            Delay,
            D("    Multiple "),
            Delay,
            D("spaces\n    "),
            Delay,
            D("Indented"),
            End
        );
    }

    [Fact]
    public async Task ShouldSendRemainingTextBufferBeforeToolCallStarts()
    {
        var call = new ToolCallPart(new GeneratedToolCall("1", "weather", "{\"city\":\"London\"}"));
        await Run([Start, D("I will check the"), D(" weather in Lon"), D("don."), call, End]);
        Expect(
            Start,
            D("I "),
            Delay,
            D("will "),
            Delay,
            D("check "),
            Delay,
            D("the "),
            Delay,
            D("weather "),
            Delay,
            D("in "),
            Delay,
            D("London."),
            call,
            End
        );
    }

    [Fact]
    public async Task ShouldSendRemainingTextBufferBeforeToolCallStartsAndToolCallStreamingIsEnabled()
    {
        var call = new ToolCallPart(new GeneratedToolCall("1", "weather", "{\"city\":\"London\"}"));
        var inputStart = new ToolInputStartPart("2", "weather", null, null, false);
        var inputDelta = new ToolInputDeltaPart("2", "{ city: \"London\" }", null);
        var inputEnd = new ToolInputEndPart("2", null);
        await Run([
            Start,
            D("I will check the"),
            D(" weather in Lon"),
            D("don."),
            inputStart,
            inputDelta,
            inputEnd,
            call,
            End,
        ]);
        Expect(
            Start,
            D("I "),
            Delay,
            D("will "),
            Delay,
            D("check "),
            Delay,
            D("the "),
            Delay,
            D("weather "),
            Delay,
            D("in "),
            Delay,
            D("London."),
            inputStart,
            inputDelta,
            inputEnd,
            call,
            End
        );
    }

    [Fact]
    public async Task DoesntReturnChunksWithJustSpaces()
    {
        await Run([Start, D(" "), D(" "), D(" "), D("foo"), End]);
        Expect(Start, D("   foo"), End);
    }
}
