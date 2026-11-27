// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.UiMessages;

/// <summary>Port of <c>ui-messages.test.ts</c> &gt; <c>isDataUIPart</c>.</summary>
public sealed class IsDataUIPartTests
{
    private const string Prefix = "packages/ai/src/ui/ui-messages.test.ts::";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [UpstreamTest(
        Prefix + "isDataUIPart::should return true if the part is a data part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsDataUIPart_should_return_true_if_the_part_is_a_data_part()
    {
        var part = new DataUIPart("someDataPart", Json("\"some data\""));

        Assert.Equal("data-someDataPart", part.Type);
        Assert.True(UIMessageParts.IsDataUIPart(part));
    }

    [UpstreamTest(
        Prefix + "isDataUIPart::should return false if the part is not a data part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsDataUIPart_should_return_false_if_the_part_is_not_a_data_part()
    {
        Assert.False(UIMessageParts.IsDataUIPart(new TextUIPart("some text")));
    }
}
