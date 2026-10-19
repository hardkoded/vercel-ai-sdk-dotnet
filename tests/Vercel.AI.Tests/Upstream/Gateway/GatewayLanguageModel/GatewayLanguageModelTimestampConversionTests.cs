// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; timestamp conversion</c>.</summary>
public sealed class GatewayLanguageModelTimestampConversionTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > timestamp conversion::";

    [Fact]
    [UpstreamTest(Prefix + "should convert timestamp strings to Date objects in response-metadata chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_timestamp_strings_to_Date_objects_in_response_metadata_chunks()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"response-metadata\",\"id\":\"test-id\",\"modelId\":\"test-model\",\"timestamp\":\"2023-12-07T10:30:00.000Z\"}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Equal(4, chunks.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(chunks[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(chunks[1]);
        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Equal(new DateTimeOffset(2023, 12, 7, 10, 30, 0, TimeSpan.Zero), metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not modify timestamp if it is already a Date object", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_modify_timestamp_if_it_is_already_a_Date_object()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"response-metadata\",\"id\":\"test-id\",\"modelId\":\"test-model\",\"timestamp\":\"2023-12-07T10:30:00.000Z\"}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Equal(4, chunks.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(chunks[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(chunks[1]);
        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Equal(new DateTimeOffset(2023, 12, 7, 10, 30, 0, TimeSpan.Zero), metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not modify response-metadata chunks without timestamp", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_modify_response_metadata_chunks_without_timestamp()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"response-metadata\",\"id\":\"test-id\",\"modelId\":\"test-model\"}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Equal(4, chunks.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(chunks[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(chunks[1]);
        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Null(metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle null timestamp values gracefully", Coverage = UpstreamCoverage.Covered, Note = "The part type does not tell a null timestamp from a missing one, so both are null.")]
    public async Task Should_handle_null_timestamp_values_gracefully()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"response-metadata\",\"id\":\"test-id\",\"modelId\":\"test-model\",\"timestamp\":null}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5}}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Equal(4, chunks.Count);
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(chunks[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(chunks[1]);
        Assert.Equal("test-id", metadata.Id);
        Assert.Equal("test-model", metadata.ModelId);
        Assert.Null(metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should only convert timestamps for response-metadata chunks, not other chunk types",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_convert_timestamps_for_response_metadata_chunks_not_other_chunk_types()
    {
        var server = StreamChunks(
            "{\"type\":\"stream-start\",\"warnings\":[]}",
            "{\"type\":\"text-delta\",\"textDelta\":\"Hello\",\"timestamp\":\"2023-12-07T10:30:00.000Z\"}",
            "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":5},\"timestamp\":\"2023-12-07T10:30:00.000Z\"}");

        var result = await CreateTestModel(server).DoStreamAsync(TestPrompt, CancellationToken.None);
        var chunks = await ReadAll(result);

        Assert.Collection(
            chunks,
            part => Assert.Empty(Assert.IsType<StreamStartStreamPart>(part).Warnings),
            part =>
            {
                Assert.Equal("Hello", Assert.IsType<TextDeltaStreamPart>(part).Delta);
                JsonAssert.Equal(part.Chunk!.Value, "{\"textDelta\":\"Hello\",\"timestamp\":\"2023-12-07T10:30:00.000Z\",\"type\":\"text-delta\"}");
            },
            part =>
            {
                Assert.Equal(FinishReason.Stop, Assert.IsType<FinishStreamPart>(part).FinishReason);
                JsonAssert.Equal(part.Chunk!.Value, "{\"finishReason\":\"stop\",\"timestamp\":\"2023-12-07T10:30:00.000Z\",\"type\":\"finish\",\"usage\":{\"completion_tokens\":5,\"prompt_tokens\":10}}");
            });
    }
}
