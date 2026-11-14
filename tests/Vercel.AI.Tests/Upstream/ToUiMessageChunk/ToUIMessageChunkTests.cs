// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;

namespace Vercel.AI.Tests.Upstream.ToUiMessageChunk;

/// <summary>Port of <c>to-ui-message-chunk.test.ts</c> &gt; <c>toUIMessageChunk</c>.</summary>
public sealed class ToUIMessageChunkTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/to-ui-message-chunk.test.ts::toUIMessageChunk::";
    private const string Commit = "ceeee29f9a7b7ca8c12a9270a542581bd760bdc7";

    [Fact]
    [UpstreamTest(Prefix + "masks error parts by default", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Masks_error_parts_by_default()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(new ErrorPart("boom"));

        Assert.Equal("""{"type":"error","errorText":"An error occurred."}""", chunk!.ToJsonString());
    }

    [Fact]
    [UpstreamTest(Prefix + "masks error parts when redactToolErrors is false", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Masks_error_parts_when_redactToolErrors_is_false()
    {
        var chunk = UIMessageStreamConverter.ToUIMessageChunk(new ErrorPart("boom"), new ToUIMessageChunkOptions { RedactToolErrors = false });

        Assert.Equal("""{"type":"error","errorText":"An error occurred."}""", chunk!.ToJsonString());
    }
}
