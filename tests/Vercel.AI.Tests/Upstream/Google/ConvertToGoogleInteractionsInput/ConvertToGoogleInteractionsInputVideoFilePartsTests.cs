// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>video file parts</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputVideoFilePartsTests
{
    private const string Prefix = ConvertToGoogleInteractionsInputSupport.Id + "video file parts::";

    [Fact]
    [UpstreamTest(
        Prefix + "passes a YouTube URL through as a video block with `uri`",
        Coverage = UpstreamCoverage.Covered
    )]
    public void passes_a_YouTube_URL_through_as_a_video_block_with_uri()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Summarize this video"),
                    new FileContentPart(
                        "video/*",
                        "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                        null,
                        null
                    ),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            ConvertToGoogleInteractionsInputSupport.Snapshot(result),
            "{\"input\":[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Summarize this video\"},{\"type\":\"video\",\"uri\":\"https://www.youtube.com/watch?v=dQw4w9WgXcQ\"}]}],\"warnings\":[]}"
        );
        Assert.Null(result.SystemInstruction);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "passes a youtu.be short URL through as a video block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void passes_a_youtu_be_short_URL_through_as_a_video_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("video/mp4", "https://youtu.be/abc123", null, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"video\",\"mime_type\":\"video/mp4\",\"uri\":\"https://youtu.be/abc123\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps a video data part to an inline video block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_video_data_part_to_an_inline_video_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("video/mp4", null, new byte[] { 5, 6, 7 }, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"video\",\"data\":\"BQYH\",\"mime_type\":\"video/mp4\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps a video reference part to a video block with the resolved Files API URI",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_a_video_reference_part_to_a_video_block_with_the_resolved_Files_API_URI()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    FileContentPart.FromProviderReference(
                        "video/mp4",
                        new Dictionary<string, string>
                        {
                            ["google"] =
                                "https://generativelanguage.googleapis.com/v1beta/files/vid-xyz",
                        }
                    ),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"video\",\"mime_type\":\"video/mp4\",\"uri\":\"https://generativelanguage.googleapis.com/v1beta/files/vid-xyz\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps per-file agentic processing onto a video block",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_per_file_agentic_processing_onto_a_video_block()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart(
                        "video",
                        "https://www.youtube.com/watch?v=abc123",
                        null,
                        null
                    )
                    {
                        ProviderOptions = GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"processing\":\"agentic\"}}"
                        ),
                    },
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"video\",\"uri\":\"https://www.youtube.com/watch?v=abc123\",\"processing\":\"agentic\"}]}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "maps static processing offsets to Google duration strings",
        Coverage = UpstreamCoverage.Covered
    )]
    public void maps_static_processing_offsets_to_Google_duration_strings()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new FileContentPart("video/mp4", null, new byte[] { 0, 0, 0 }, null)
                    {
                        ProviderOptions = GoogleUpstream.ProviderOptions(
                            "{\"google\":{\"processing\":{\"type\":\"static\",\"startOffset\":0,\"endOffset\":10.5,\"fps\":0.5}}}"
                        ),
                    },
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"video\",\"data\":\"AAAA\",\"mime_type\":\"video/mp4\",\"processing\":{\"type\":\"static\",\"start_offset\":\"0s\",\"end_offset\":\"10.5s\",\"fps\":0.5}}]}]"
        );
    }
}
