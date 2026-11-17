// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class DelayTests : SmoothStreamTestBase
{
    private static TextStartPart Start => new("1");

    private static TextEndPart End => new("1");

    [Fact]
    public async Task ShouldDefaultTo10ms()
    {
        await Run([Start, D("Hello, world!"), End]);
        Expect(Start, D("Hello, "), "delay 10", D("world!"), End);
    }

    [Fact]
    public async Task ShouldSupportDifferentNumberOfMillisecondsDelay()
    {
        await Run([Start, D("Hello, world!"), End], new SmoothStreamOptions { DelayInMs = 20 });
        Expect(Start, D("Hello, "), "delay 20", D("world!"), End);
    }

    [Fact]
    public async Task ShouldSupportNullDelay()
    {
        await Run([Start, D("Hello, world!"), End], new SmoothStreamOptions { DelayInMs = null });
        Expect(Start, D("Hello, "), "delay null", D("world!"), End);
    }
}
