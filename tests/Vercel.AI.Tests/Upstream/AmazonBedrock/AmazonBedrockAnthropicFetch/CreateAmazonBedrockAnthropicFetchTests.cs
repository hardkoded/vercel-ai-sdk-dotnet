// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Tests.Streaming;
using Fetch = Vercel.AI.AmazonBedrock.AmazonBedrockAnthropicFetch;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockAnthropicFetch;

/// <summary>
/// Port of <c>amazon-bedrock-anthropic-fetch.test.ts</c> &gt; <c>createAmazonBedrockAnthropicFetch</c>.
/// The port has no <c>fetch</c> wrapper, so these tests call the event-stream transform that the model reads.
/// Each yielded value is the data of one SSE event, so a test adds the <c>data: </c> prefix and the blank line that upstream reads.
/// </summary>
public sealed class CreateAmazonBedrockAnthropicFetchTests
{
    private const string Prefix = "packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-fetch.test.ts::createAmazonBedrockAnthropicFetch::";

    private static byte[] Chunk(string anthropicEvent)
        => EventStreamFrames.Event("chunk", new JsonObject { ["bytes"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(anthropicEvent)) }.ToJsonString());

    private static async Task<string> Sse(params byte[][] frames)
    {
        using var body = new MemoryStream(frames.SelectMany(frame => frame).ToArray());
        var text = new StringBuilder();
        await foreach (var data in Fetch.TransformEventStreamToSse(body))
        {
            text.Append("data: ").Append(data).Append("\n\n");
        }

        return text.ToString();
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should transform Bedrock event stream to SSE format",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the SSE text. It does not check the text/event-stream content type, because the port has no response wrapper. See #465.")]
    public async Task Should_transform_Bedrock_event_stream_to_SSE_format()
    {
        const string anthropicEvent = "{\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello\"}}";

        Assert.Equal($"data: {anthropicEvent}\n\n", await Sse(Chunk(anthropicEvent)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle messageStop event and emit [DONE]", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_messageStop_event_and_emit_DONE()
    {
        Assert.Equal("data: [DONE]\n\n", await Sse(EventStreamFrames.Event("messageStop", "{}")));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle exception messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_exception_messages()
    {
        var text = await Sse(EventStreamFrames.Exception("ThrottlingException", "{\"message\":\"Rate limit exceeded\"}"));

        Assert.Equal(
            "data: {\"type\":\"error\",\"error\":{\"type\":\"ThrottlingException\",\"message\":\"Rate limit exceeded\",\"statusCode\":429,\"isRetryable\":true,\"data\":{\"message\":\"Rate limit exceeded\"}}}\n\n",
            text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple events in sequence", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_multiple_events_in_sequence()
    {
        const string event1 = "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_123\"}}";
        const string event2 = "{\"type\":\"content_block_delta\",\"delta\":{\"text\":\"Hi\"}}";

        var text = await Sse(Chunk(event1), Chunk(event2), EventStreamFrames.Event("messageStop", "{}"));

        Assert.Contains($"data: {event1}\n\n", text);
        Assert.Contains($"data: {event2}\n\n", text);
        Assert.Contains("data: [DONE]\n\n", text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle chunked event data spanning multiple network chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_chunked_event_data_spanning_multiple_network_chunks()
    {
        const string anthropicEvent = "{\"type\":\"content_block_delta\",\"delta\":{\"text\":\"Hello World\"}}";
        var frame = Chunk(anthropicEvent);
        var midpoint = frame.Length / 2;
        var body = new GatedStream();
        body.Write(frame[..midpoint]);
        body.Write(frame[midpoint..]);
        body.Complete();

        var text = new StringBuilder();
        await foreach (var data in Fetch.TransformEventStreamToSse(body))
        {
            text.Append("data: ").Append(data).Append("\n\n");
        }

        Assert.Equal($"data: {anthropicEvent}\n\n", text.ToString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle chunk events with missing bytes field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_chunk_events_with_missing_bytes_field()
    {
        const string chunkPayload = "{\"someOtherField\":\"value\"}";

        Assert.Equal($"data: {chunkPayload}\n\n", await Sse(EventStreamFrames.Event("chunk", chunkPayload)));
    }

    [Fact]
    [UpstreamTest(Prefix + "should correctly decode multi-byte UTF-8 characters (emoji and non-Latin scripts)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_correctly_decode_multi_byte_UTF_8_characters_emoji_and_non_Latin_scripts()
    {
        var anthropicEvent = "{\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello! \U0001F44B\U0001F30D Привет 你好 مرحبا\"}}";

        var text = await Sse(Chunk(anthropicEvent));

        Assert.Equal($"data: {anthropicEvent}\n\n", text);
        Assert.Contains("\U0001F44B", text);
        Assert.Contains("\U0001F30D", text);
        Assert.Contains("Привет", text);
        Assert.Contains("你好", text);
        Assert.Contains("مرحبا", text);
    }
}
