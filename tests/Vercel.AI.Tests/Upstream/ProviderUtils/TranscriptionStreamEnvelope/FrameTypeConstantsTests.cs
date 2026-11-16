// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.TranscriptionStreamEnvelope;

/// <summary>Port of <c>transcription-stream-envelope.test.ts</c> &gt; <c>frame type constants</c>.</summary>
public sealed class FrameTypeConstantsTests
{
    [Fact]
    [UpstreamTest("packages/provider-utils/src/transcription-stream-envelope.test.ts::frame type constants::should use the transcription-stream namespace", Coverage = UpstreamCoverage.Covered)]
    public void Should_use_the_transcription_stream_namespace()
    {
        Assert.Equal("transcription-stream.start", Vercel.AI.ProviderUtils.TranscriptionStreamEnvelope.TranscriptionStreamStartFrameType);
        Assert.Equal("transcription-stream.audio-done", Vercel.AI.ProviderUtils.TranscriptionStreamEnvelope.TranscriptionStreamAudioDoneFrameType);
    }
}
