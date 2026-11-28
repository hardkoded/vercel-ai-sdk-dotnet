// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>text file parts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputTextFilePartsTests
{
    private const string Prefix = ConvertToGoogleInteractionsInputSupport.Id + "text file parts::";

    [Fact]
    [UpstreamTest(
        Prefix + "collapses a text-data file part into an inline text block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void collapses_a_text_data_file_part_into_an_inline_text_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    FileContentPart.FromText("text/plain", "inline text content"),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"inline text content\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "merges an inline text-data file part with an adjacent text part",
        Coverage = UpstreamCoverage.Covered
    )]
    public void merges_an_inline_text_data_file_part_with_an_adjacent_text_part()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Please review:"),
                    FileContentPart.FromText("text/plain", "inline body"),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Please review:\\n\\ninline body\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not merge a text part across a non-text part",
        Coverage = UpstreamCoverage.Covered
    )]
    public void does_not_merge_a_text_part_across_a_non_text_part()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("before"),
                    new FileDataContentPart("AQID", "image/png"),
                    FileContentPart.FromText("text/plain", "after"),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"before\"},{\"type\":\"image\",\"data\":\"AQID\",\"mime_type\":\"image/png\"},{\"type\":\"text\",\"text\":\"after\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "merges three adjacent text-derived parts into one",
        Coverage = UpstreamCoverage.Covered
    )]
    public void merges_three_adjacent_text_derived_parts_into_one()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("a"),
                    new TextContentPart("b"),
                    FileContentPart.FromText("text/markdown", "c"),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"a\\n\\nb\\n\\nc\"}]}]"
        );
    }
}
