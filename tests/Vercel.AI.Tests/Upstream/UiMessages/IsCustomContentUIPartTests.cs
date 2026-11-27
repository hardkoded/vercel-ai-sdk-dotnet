// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.UiMessages;

/// <summary>Port of <c>ui-messages.test.ts</c> &gt; <c>isCustomContentUIPart</c>.</summary>
public sealed class IsCustomContentUIPartTests
{
    private const string Prefix = "packages/ai/src/ui/ui-messages.test.ts::";

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [UpstreamTest(
        Prefix + "isCustomContentUIPart::should return true for a custom part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsCustomContentUIPart_should_return_true_for_a_custom_part()
    {
        var part = new CustomContentUIPart("test-provider.compaction")
        {
            ProviderMetadata = new Dictionary<string, JsonElement>
            {
                ["openai"] = Json("{\"itemId\":\"cmp_123\"}"),
            },
        };

        Assert.True(UIMessageParts.IsCustomContentUIPart(part));
    }

    [UpstreamTest(
        Prefix
            + "isCustomContentUIPart::should return true for a custom part without providerMetadata",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsCustomContentUIPart_should_return_true_for_a_custom_part_without_providerMetadata()
    {
        Assert.True(
            UIMessageParts.IsCustomContentUIPart(new CustomContentUIPart("openai.compaction"))
        );
    }

    [UpstreamTest(
        Prefix + "isCustomContentUIPart::should return false for a text part",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void IsCustomContentUIPart_should_return_false_for_a_text_part()
    {
        Assert.False(UIMessageParts.IsCustomContentUIPart(new TextUIPart("some text")));
    }
}
