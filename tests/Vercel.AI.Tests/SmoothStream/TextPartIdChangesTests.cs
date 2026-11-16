// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

public sealed class TextPartIdChangesTests : SmoothStreamTestBase
{
    [Fact]
    public async Task ShouldChangeTheIdWhenTheTextPartIdChanges()
    {
        await Run([
            new TextStartPart("1"),
            new TextStartPart("2"),
            D("I will check the", "1"),
            D(" weather in Lon", "1"),
            D("don.", "1"),
            D("I will check the", "2"),
            D(" weather in Lon", "2"),
            D("don.", "2"),
            new TextEndPart("1"),
            new TextEndPart("2"),
        ]);
        Expect(
            new TextStartPart("1"),
            new TextStartPart("2"),
            D("I ", "1"), Delay, D("will ", "1"), Delay, D("check ", "1"), Delay, D("the ", "1"), Delay, D("weather ", "1"), Delay, D("in ", "1"), Delay, D("London.", "1"),
            D("I ", "2"), Delay, D("will ", "2"), Delay, D("check ", "2"), Delay, D("the ", "2"), Delay, D("weather ", "2"), Delay, D("in ", "2"), Delay, D("London.", "2"),
            new TextEndPart("1"),
            new TextEndPart("2"));
    }
}
