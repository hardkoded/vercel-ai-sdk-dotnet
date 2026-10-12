// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.AmazonBedrock;
using ResponseHandler = Vercel.AI.AmazonBedrock.AmazonBedrockEventStreamResponseHandler;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockEventStreamResponseHandler;

public sealed class CreateEventSourceResponseHandlerTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-event-stream-response-handler.test.ts::createEventSourceResponseHandler::";

    // The schema upstream's tests use: { chunk: { content: string } }.
    private static string? TestSchema(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("chunk", out var chunk)
            && chunk.ValueKind == JsonValueKind.Object
            && chunk.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.String
                ? null
                : "Type validation failed: expected { chunk: { content: string } }.";
    }

    private static HttpResponseMessage ResponseOf(params byte[][] chunks)
    {
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StreamContent(new ChunkedStream(chunks)) };
    }

    private static async Task<AmazonBedrockStreamChunk> FirstChunk(HttpResponseMessage response)
    {
        var (_, stream) = await ResponseHandler.HandleAsync(response, TestSchema);
        await using var enumerator = stream.GetAsyncEnumerator();
        Assert.True(await enumerator.MoveNextAsync());
        return enumerator.Current;
    }

    [Fact]
    [UpstreamTest(Suite + "throws EmptyResponseBodyError when response body is null", Coverage = UpstreamCoverage.Covered)]
    public async Task Throws_EmptyResponseBodyError_when_response_body_is_null()
    {
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = null };

        var error = await Assert.ThrowsAsync<ApiException>(() =>
            ResponseHandler.HandleAsync(response, TestSchema));
        Assert.Equal("Response body is empty", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Suite + "successfully processes valid event stream data",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream mocks EventStreamCodec; this test decodes a real frame.")]
    public async Task Successfully_processes_valid_event_stream_data()
    {
        using var response = ResponseOf(EventStreamFrames.Event("chunk", "{\"content\":\"test message\"}"));

        var chunk = await FirstChunk(response);

        Assert.True(chunk.Success);
        Assert.True(BedrockStreamSupport.JsonEquals("""{"chunk":{"content":"test message"}}""", chunk.Value));
        Assert.True(BedrockStreamSupport.JsonEquals("""{"chunk":{"content":"test message"}}""", chunk.RawValue));
    }

    [Fact]
    [UpstreamTest(
        Suite + "handles invalid JSON data",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream mocks EventStreamCodec; this test decodes a real frame.")]
    public async Task Handles_invalid_JSON_data()
    {
        using var response = ResponseOf(EventStreamFrames.Event("chunk", "invalid json"));

        var chunk = await FirstChunk(response);

        Assert.False(chunk.Success);
        Assert.NotNull(chunk.Error);
    }

    [Fact]
    [UpstreamTest(
        Suite + "handles schema validation failures",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream mocks EventStreamCodec; this test decodes a real frame.")]
    public async Task Handles_schema_validation_failures()
    {
        using var response = ResponseOf(EventStreamFrames.Event("chunk", "{\"invalid\":\"data\"}"));

        var chunk = await FirstChunk(response);

        Assert.False(chunk.Success);
        Assert.NotNull(chunk.Error);
    }

    [Fact]
    [UpstreamTest(
        Suite + "surfaces event stream decoding failures",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream mocks EventStreamCodec to throw; this test sends a frame whose checksum is wrong.")]
    public async Task Surfaces_event_stream_decoding_failures()
    {
        var frame = EventStreamFrames.Event("chunk", "{\"content\":\"test message\"}");
        frame[frame.Length - 1] ^= 0xff;
        using var response = ResponseOf(frame);

        var (_, stream) = await ResponseHandler.HandleAsync(response, TestSchema);
        await using var enumerator = stream.GetAsyncEnumerator();

        await Assert.ThrowsAnyAsync<Exception>(async () => await enumerator.MoveNextAsync());
    }
}
