// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>image file parts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputImageFilePartsTests
{
    private const string Prefix = ConvertToGoogleInteractionsInputSupport.Id + "image file parts::";

    [Fact]
    [UpstreamTest(
        Prefix + "maps a base64 / Uint8Array data part to an inline image block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_base64_Uint8Array_data_part_to_an_inline_image_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Describe this"),
                    new FileContentPart("image/png", null, new byte[] { 1, 2, 3, 4 }, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            ConvertToGoogleInteractionsInputSupport.Snapshot(result),
            "{\"input\":[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Describe this\"},{\"type\":\"image\",\"data\":\"AQIDBA==\",\"mime_type\":\"image/png\"}]}],\"warnings\":[]}"
        );
        Assert.Null(result.SystemInstruction);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes a base64-string data part through untouched",
        Coverage = UpstreamCoverage.Covered
    )]
    public void passes_a_base64_string_data_part_through_untouched()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[] { new FileDataContentPart("SGVsbG8=", "image/jpeg") }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"image\",\"data\":\"SGVsbG8=\",\"mime_type\":\"image/jpeg\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps a url data part to an image block with `uri` (URL passthrough)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_url_data_part_to_an_image_block_with_uri_URL_passthrough()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Describe this"),
                    new FileContentPart("image/png", "https://example.com/cat.png", null, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            ConvertToGoogleInteractionsInputSupport.Snapshot(result),
            "{\"input\":[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Describe this\"},{\"type\":\"image\",\"mime_type\":\"image/png\",\"uri\":\"https://example.com/cat.png\"}]}],\"warnings\":[]}"
        );
        Assert.Null(result.SystemInstruction);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits mime_type for url parts when only a top-level mediaType is given",
        Coverage = UpstreamCoverage.Covered
    )]
    public void omits_mime_type_for_url_parts_when_only_a_top_level_mediaType_is_given()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("image", "https://example.com/cat.png", null, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"image\",\"uri\":\"https://example.com/cat.png\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps a reference data part to an image block with the resolved Files API URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_reference_data_part_to_an_image_block_with_the_resolved_Files_API_URI()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Describe this"),
                    FileContentPart.FromProviderReference(
                        "image/png",
                        new Dictionary<string, string>
                        {
                            ["google"] =
                                "https://generativelanguage.googleapis.com/v1beta/files/abc123",
                        }
                    ),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            ConvertToGoogleInteractionsInputSupport.Snapshot(result),
            "{\"input\":[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Describe this\"},{\"type\":\"image\",\"mime_type\":\"image/png\",\"uri\":\"https://generativelanguage.googleapis.com/v1beta/files/abc123\"}]}],\"warnings\":[]}"
        );
        Assert.Null(result.SystemInstruction);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "throws when a reference does not contain a `google` entry",
        Coverage = UpstreamCoverage.Covered
    )]
    public void throws_when_a_reference_does_not_contain_a_google_entry()
    {
        Assert.Throws<NoSuchProviderReferenceError>(() =>
            ConvertToGoogleInteractionsInputSupport.Convert(
                new UserModelMessage(
                    new UserContentPart[]
                    {
                        FileContentPart.FromProviderReference(
                            "image/png",
                            new Dictionary<string, string> { ["openai"] = "file-abc" }
                        ),
                    }
                )
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "threads mediaResolution onto inline image blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public void threads_mediaResolution_onto_inline_image_blocks()
    {
        var result = GoogleInteractionsInput.Convert(
            new ModelMessage[]
            {
                new UserModelMessage(
                    new UserContentPart[] { new FileDataContentPart("SGVsbG8=", "image/png") }
                ),
            },
            mediaResolution: "high"
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"image\",\"data\":\"SGVsbG8=\",\"mime_type\":\"image/png\",\"resolution\":\"high\"}]}]"
        );
    }
}
