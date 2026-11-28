// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.BuildGoogleInteractionsStreamTransform;

/// <summary>Port of <c>build-google-interactions-stream-transform.test.ts</c> &gt; <c>buildGoogleInteractionsStreamTransform — video deltas</c>.</summary>
public sealed class VideoDeltasTests
{
    private const string Prefix =
        "packages/google/src/interactions/build-google-interactions-stream-transform.test.ts::buildGoogleInteractionsStreamTransform — video deltas::";

    [Fact]
    [UpstreamTest(
        Prefix + "emits a file stream part for a video delta carrying inline data",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_file_stream_part_for_a_video_delta_carrying_inline_data()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            """{"event_type":"interaction.created","interaction":{"id":"v1_video-stream","status":"in_progress"}}""",
            """{"event_type":"step.start","index":0,"step":{"type":"model_output"}}""",
            """{"event_type":"step.delta","index":0,"delta":{"type":"video","data":"AAAAIGZ0eXBpc29t","mime_type":"video/mp4"}}""",
            """{"event_type":"step.stop","index":0}""",
            """{"event_type":"interaction.completed","interaction":{"id":"v1_video-stream","status":"completed"}}"""
        );

        var file = Assert.IsType<FileStreamPart>(
            Assert.Single(parts, part => part is FileStreamPart)
        );
        Assert.Equal("file", file.Type);
        Assert.Equal("video/mp4", file.MediaType);
        Assert.Null(file.Url);
        Assert.Equal("AAAAIGZ0eXBpc29t", System.Convert.ToBase64String(file.Data!));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "emits a file stream part for a video delta carrying a uri",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_file_stream_part_for_a_video_delta_carrying_a_uri()
    {
        var parts = GoogleInteractionsStreamSupport.Run(
            """{"event_type":"interaction.created","interaction":{"id":"v1_video-stream","status":"in_progress"}}""",
            """{"event_type":"step.start","index":0,"step":{"type":"model_output"}}""",
            """{"event_type":"step.delta","index":0,"delta":{"type":"video","uri":"https://example.test/clip.mp4"}}""",
            """{"event_type":"step.stop","index":0}""",
            """{"event_type":"interaction.completed","interaction":{"id":"v1_video-stream","status":"completed"}}"""
        );

        var file = Assert.IsType<FileStreamPart>(
            Assert.Single(parts, part => part is FileStreamPart)
        );
        Assert.Equal("file", file.Type);
        Assert.Equal("video/mp4", file.MediaType);
        Assert.Equal(new Uri("https://example.test/clip.mp4"), file.Url);
    }
}
