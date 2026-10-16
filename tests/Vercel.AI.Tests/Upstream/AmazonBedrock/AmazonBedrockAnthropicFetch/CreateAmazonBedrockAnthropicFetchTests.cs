// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Fetch = Vercel.AI.AmazonBedrock.AmazonBedrockAnthropicFetch;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockAnthropicFetch;

public sealed class CreateAmazonBedrockAnthropicFetchTests
{
    private const string Prefix = "packages/amazon-bedrock/src/anthropic/amazon-bedrock-anthropic-fetch.test.ts::createAmazonBedrockAnthropicFetch::";
    private const string EventStream = "application/vnd.amazon.eventstream";

    [Fact]
    [UpstreamTest(Prefix + "should pass through non-streaming responses unchanged", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_non_streaming_responses_unchanged()
    {
        var jsonBody = "{\"id\":\"msg_123\",\"content\":[]}";
        using var response = await Send(Response(HttpStatusCode.OK, "OK", jsonBody, "application/json"));

        Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(jsonBody, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    [UpstreamTest(Prefix + "should transform Bedrock event stream to SSE format", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_transform_Bedrock_event_stream_to_SSE_format()
    {
        var anthropicEvent = "{\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello\"}}";
        var chunkPayload = "{\"bytes\":\"" + Base64(anthropicEvent) + "\"}";
        var amazonBedrockEvent = EventStreamFrames.Event("chunk", chunkPayload);

        using var response = await Send(StreamResponse(EventStream, amazonBedrockEvent));

        Assert.Equal("text/event-stream", response.Content.Headers.ContentType!.ToString());
        var text = await ReadChunk(response);
        Assert.Equal("data: " + anthropicEvent + "\n\n", text);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle messageStop event and emit [DONE]", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_messageStop_event_and_emit_DONE()
    {
        var amazonBedrockEvent = EventStreamFrames.Event("messageStop", "{}");

        using var response = await Send(StreamResponse(EventStream, amazonBedrockEvent));

        Assert.Equal("data: [DONE]\n\n", await ReadChunk(response));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle exception messages", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_exception_messages()
    {
        var errorData = "{\"message\":\"Rate limit exceeded\"}";
        var amazonBedrockEvent = EventStreamFrames.Exception("ThrottlingException", errorData);

        using var response = await Send(StreamResponse(EventStream, amazonBedrockEvent));

        Assert.Equal(
            "data: {\"type\":\"error\",\"error\":{\"type\":\"ThrottlingException\",\"message\":\"Rate limit exceeded\",\"statusCode\":429,\"isRetryable\":true,\"data\":{\"message\":\"Rate limit exceeded\"}}}\n\n",
            await ReadChunk(response));
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle multiple events in sequence", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_multiple_events_in_sequence()
    {
        var event1 = "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_123\"}}";
        var event2 = "{\"type\":\"content_block_delta\",\"delta\":{\"text\":\"Hi\"}}";
        var chunk1 = EventStreamFrames.Event("chunk", "{\"bytes\":\"" + Base64(event1) + "\"}");
        var chunk2 = EventStreamFrames.Event("chunk", "{\"bytes\":\"" + Base64(event2) + "\"}");
        var stopEvent = EventStreamFrames.Event("messageStop", "{}");

        using var response = await Send(StreamResponse(EventStream, chunk1, chunk2, stopEvent));

        var fullText = await ReadAll(response);
        Assert.Contains("data: " + event1 + "\n\n", fullText);
        Assert.Contains("data: " + event2 + "\n\n", fullText);
        Assert.Contains("data: [DONE]\n\n", fullText);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle chunked event data spanning multiple network chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_chunked_event_data_spanning_multiple_network_chunks()
    {
        var anthropicEvent = "{\"type\":\"content_block_delta\",\"delta\":{\"text\":\"Hello World\"}}";
        var amazonBedrockEvent = EventStreamFrames.Event("chunk", "{\"bytes\":\"" + Base64(anthropicEvent) + "\"}");

        // Split the event into two chunks to simulate network fragmentation.
        var midpoint = amazonBedrockEvent.Length / 2;
        var firstHalf = amazonBedrockEvent.Take(midpoint).ToArray();
        var secondHalf = amazonBedrockEvent.Skip(midpoint).ToArray();

        using var response = await Send(StreamResponse(EventStream, firstHalf, secondHalf));

        Assert.Equal("data: " + anthropicEvent + "\n\n", await ReadAll(response));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve response status and statusText", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_response_status_and_statusText()
    {
        var amazonBedrockEvent = EventStreamFrames.Event("messageStop", "{}");
        var mockResponse = StreamResponse(EventStream, amazonBedrockEvent);
        mockResponse.StatusCode = HttpStatusCode.OK;
        mockResponse.ReasonPhrase = "OK";

        using var response = await Send(mockResponse);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("OK", response.ReasonPhrase);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle chunk events with missing bytes field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_chunk_events_with_missing_bytes_field()
    {
        // A chunk without a bytes field falls back to emitting the raw data.
        var chunkPayload = "{\"someOtherField\":\"value\"}";
        var amazonBedrockEvent = EventStreamFrames.Event("chunk", chunkPayload);

        using var response = await Send(StreamResponse(EventStream, amazonBedrockEvent));

        Assert.Equal("data: " + chunkPayload + "\n\n", await ReadChunk(response));
    }

    [Fact]
    [UpstreamTest(Prefix + "should transform Bedrock error with message into Anthropic error format", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_transform_Bedrock_error_with_message_into_Anthropic_error_format()
    {
        var errorBody = "{\"message\":\"cache_control: Extra inputs are not permitted\"}";

        using var response = await Send(Response(HttpStatusCode.BadRequest, "Bad Request", errorBody, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Bad Request", response.ReasonPhrase);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "{\"type\":\"error\",\"error\":{\"type\":\"error\",\"message\":\"cache_control: Extra inputs are not permitted\"}}",
            body.RootElement.GetRawText());
    }

    [Fact]
    [UpstreamTest(Prefix + "should transform Bedrock error with extra fields into Anthropic error format", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_transform_Bedrock_error_with_extra_fields_into_Anthropic_error_format()
    {
        var errorBody = "{\"message\":\"tools.0.custom.input_schema.type: Field required\",\"someOtherField\":\"value\"}";

        using var response = await Send(Response(HttpStatusCode.BadRequest, "Bad Request", errorBody, "application/json"));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            "tools.0.custom.input_schema.type: Field required",
            body.RootElement.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should use raw text as message when Bedrock error has no message field", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_raw_text_as_message_when_Bedrock_error_has_no_message_field()
    {
        var errorBody = "{\"code\":\"ValidationException\"}";

        using var response = await Send(Response(HttpStatusCode.BadRequest, "Bad Request", errorBody, "application/json"));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(errorBody, body.RootElement.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should use raw text as message when Bedrock error is not valid JSON", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_use_raw_text_as_message_when_Bedrock_error_is_not_valid_JSON()
    {
        var errorBody = "Internal Server Error";

        using var response = await Send(Response(HttpStatusCode.InternalServerError, "Internal Server Error", errorBody, "text/plain"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Internal Server Error", body.RootElement.GetProperty("error").GetProperty("message").GetString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should correctly decode multi-byte UTF-8 characters (emoji and non-Latin scripts)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_correctly_decode_multi_byte_UTF_8_characters_emoji_and_non_Latin_scripts()
    {
        var anthropicEvent = "{\"type\":\"content_block_delta\",\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello! 👋🌍 Привет 你好 مرحبا\"}}";
        var chunkPayload = "{\"bytes\":\"" + Base64(anthropicEvent) + "\"}";
        var amazonBedrockEvent = EventStreamFrames.Event("chunk", chunkPayload);

        using var response = await Send(StreamResponse(EventStream, amazonBedrockEvent));

        var text = await ReadChunk(response);
        Assert.Equal("data: " + anthropicEvent + "\n\n", text);
        Assert.Contains("👋", text);
        Assert.Contains("🌍", text);
        Assert.Contains("Привет", text);
        Assert.Contains("你好", text);
        Assert.Contains("مرحبا", text);
    }

    private static string Base64(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private static HttpResponseMessage Response(HttpStatusCode status, string reason, string body, string contentType)
    {
        return new HttpResponseMessage(status)
        {
            ReasonPhrase = reason,
            Content = new StringContent(body, Encoding.UTF8, contentType),
        };
    }

    private static HttpResponseMessage StreamResponse(string contentType, params byte[][] networkChunks)
    {
        var content = new StreamContent(new ChunkedStream(networkChunks));
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static async Task<HttpResponseMessage> Send(HttpResponseMessage mockResponse)
    {
        using var client = new HttpClient(Fetch.Create(new StubHandler(mockResponse)));
        return await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), HttpCompletionOption.ResponseHeadersRead);
    }

    private static async Task<string> ReadChunk(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        var buffer = new byte[8192];
        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, read);
    }

    private static async Task<string> ReadAll(HttpResponseMessage response)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        var chunks = new StringBuilder();
        var buffer = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            chunks.Append(Encoding.UTF8.GetString(buffer, 0, read));
        }

        return chunks.ToString();
    }

    private sealed class StubHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }

    /// <summary>Returns one network chunk per read.</summary>
    private sealed class ChunkedStream(byte[][] chunks) : Stream
    {
        private int _next;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_next >= chunks.Length)
            {
                return 0;
            }

            var chunk = chunks[_next++];
            Buffer.BlockCopy(chunk, 0, buffer, offset, chunk.Length);
            return chunk.Length;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
