// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

/// <summary>Maps to <c>describe('document visibility')</c> of upstream's <c>smooth-stream.test.ts</c>.</summary>
public sealed class DocumentVisibilityTests : SmoothStreamTestBase
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/smooth-stream.test.ts::smoothStream > document visibility::should skip delays while the document is hidden"
    )]
    public async Task ShouldSkipDelaysWhileTheDocumentIsHidden()
    {
        await Run(
            [
                new TextStartPart("1"),
                D("Hello, World! This is an example text."),
                new TextEndPart("1"),
            ],
            new SmoothStreamOptions { DelayInMs = 10, IsDocumentHidden = () => true }
        );

        Assert.Equal(Enumerable.Repeat("delay null", 6), Delays());
        Assert.Equal(new TextEndPart("1").Type, Parts[^1].Type);
        Assert.Equal("1", ((TextEndPart)Parts[^1]).Id);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/smooth-stream.test.ts::smoothStream > document visibility::should resume without delays when the document becomes hidden"
    )]
    public async Task ShouldResumeWithoutDelaysWhenTheDocumentBecomesHidden()
    {
        var hidden = false;
        var events = new List<string>();
        await Run(
            [new TextStartPart("1"), D("one two three "), new TextEndPart("1")],
            new SmoothStreamOptions
            {
                DelayInMs = 10,
                IsDocumentHidden = () => hidden,
                Delay = (ms, _) =>
                {
                    events.Add(DelayOf(ms));
                    hidden = true;
                    return Task.CompletedTask;
                },
            }
        );

        Assert.Equal(["delay 10", "delay null", "delay null"], events);
        Assert.Equal("1", ((TextEndPart)Parts[^1]).Id);
    }
}
