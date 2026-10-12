// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Decoder = Vercel.AI.AmazonBedrock.AmazonBedrockEventStreamDecoder;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockEventStreamDecoder;

public sealed class CreateAmazonBedrockEventStreamDecoderTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-event-stream-decoder.test.ts::createAmazonBedrockEventStreamDecoder::";

    private static byte[] CreateEvent(string data) => EventStreamFrames.Event("chunk", data);

    private static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }

    [Fact]
    [UpstreamTest(Suite + "surfaces frame decode errors instead of stranding later frames", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_frame_decode_errors_instead_of_stranding_later_frames()
    {
        var corruptedFrame = CreateEvent("corrupted");
        corruptedFrame[corruptedFrame.Length - 1] ^= 0xff;
        var processed = 0;

        var result = ToList(Decoder.Create<string>(
            new ChunkedStream(corruptedFrame, CreateEvent("later")),
            (_, _) => processed++));

        await Assert.ThrowsAnyAsync<Exception>(() => result);
        Assert.Equal(0, processed);
    }

    [Fact]
    [UpstreamTest(Suite + "surfaces processEvent errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_processEvent_errors()
    {
        var processEventError = new InvalidOperationException("processEvent failed");

        var result = ToList(Decoder.Create<string>(
            new ChunkedStream(CreateEvent("data")),
            (_, _) => throw processEventError));

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => result);
        Assert.Same(processEventError, thrown);
    }

    [Fact]
    [UpstreamTest(Suite + "processes frames split across chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Processes_frames_split_across_chunks()
    {
        var frame = CreateEvent("data");
        var midpoint = frame.Length / 2;

        var result = await ToList(Decoder.Create<string>(
            new ChunkedStream(frame.Take(midpoint).ToArray(), frame.Skip(midpoint).ToArray()),
            (evt, enqueue) => enqueue(evt.Data)));

        Assert.Equal(new[] { "data" }, result);
    }

    [Fact]
    [UpstreamTest(Suite + "rejects when EOF leaves an incomplete frame", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_when_EOF_leaves_an_incomplete_frame()
    {
        var frame = CreateEvent("data");
        var incompleteFrame = frame.Take(frame.Length - 1).ToArray();

        var result = ToList(Decoder.Create<string>(
            new ChunkedStream(incompleteFrame),
            (_, _) => { }));

        var error = await Assert.ThrowsAnyAsync<Exception>(() => result);
        Assert.Contains(
            "Incomplete Amazon Bedrock event-stream frame: " + incompleteFrame.Length + " buffered bytes remain at end of stream.",
            error.Message);
    }

    [Fact]
    [UpstreamTest(Suite + "allows EOF at a complete frame boundary", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_EOF_at_a_complete_frame_boundary()
    {
        var result = await ToList(Decoder.Create<string>(
            new ChunkedStream(CreateEvent("data")),
            (evt, enqueue) => enqueue(evt.Data)));

        Assert.Equal(new[] { "data" }, result);
    }
}
