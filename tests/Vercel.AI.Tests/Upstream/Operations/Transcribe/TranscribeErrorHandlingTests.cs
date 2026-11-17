// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Transcribe.TranscribeSupport;
using StreamTranscribeOperation = Vercel.AI.Operations.StreamTranscribe;
using TranscribeOperation = Vercel.AI.Operations.Transcribe;

namespace Vercel.AI.Tests.Upstream.Operations.Transcribe;

/// <summary>Port of <c>transcribe.test.ts</c> &gt; <c>transcribe</c> &gt; <c>error handling</c>.</summary>
[Collection("LogWarnings")]
public sealed class TranscribeErrorHandlingTests
{
    private const string Error =
        "packages/ai/src/transcribe/transcribe.test.ts::transcribe > error handling::";

    [Fact]
    [UpstreamTest(
        Error + "should throw NoTranscriptGeneratedError when no transcript is returned",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Throws_when_no_transcript_is_returned()
    {
        var error = await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() =>
            TranscribeOperation.TranscribeAsync(
                new TranscribeRequest
                {
                    Model = new TranscriptFake { Text = null },
                    Audio = new byte[] { 1 },
                }
            )
        );
        Assert.Equal("No transcript generated.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Error + "should include response headers in error when no transcript generated",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Includes_response_headers_when_no_transcript_is_generated()
    {
        var model = new TranscriptFake
        {
            Text = null,
            Response = new ProviderResponse(
                new Dictionary<string, string> { ["x-request-id"] = "req" }
            ),
        };
        var error = await Assert.ThrowsAsync<NoTranscriptGeneratedException>(() =>
            TranscribeOperation.TranscribeAsync(
                new TranscribeRequest { Model = model, Audio = new byte[] { 1 } }
            )
        );
        Assert.Equal("req", error.Responses[0].Headers!["x-request-id"]);
    }
}
