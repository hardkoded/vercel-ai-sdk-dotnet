// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Streaming;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; timestamp conversion</c>.</summary>
public sealed class GatewayLanguageModelTimestampConversionTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > timestamp conversion::";
    private const string Stamp = "2023-12-07T10:30:00.000Z";

    private static readonly LanguageModelCallOptions TestPrompt = new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    private static async Task<List<LanguageModelStreamPart>> Stream(params string[] chunks)
    {
        var handler = new GatedStreamHandler();
        foreach (var chunk in chunks)
        {
            handler.Body.Write($"data: {chunk}\n\n");
        }

        handler.Body.Complete();
        var model = GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler).LanguageModel("test-model");
        var result = await model.DoStreamAsync(TestPrompt, CancellationToken.None);
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result)
        {
            parts.Add(part);
        }

        return parts;
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert timestamp strings to Date objects in response-metadata chunks", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_convert_timestamp_strings_to_Date_objects_in_response_metadata_chunks()
    {
        var parts = await Stream($"{{\"type\":\"response-metadata\",\"id\":\"test-id\",\"timestamp\":\"{Stamp}\"}}");

        var metadata = Assert.IsType<ResponseMetadataStreamPart>(Assert.Single(parts));
        Assert.Equal(DateTimeOffset.Parse(Stamp, System.Globalization.CultureInfo.InvariantCulture), metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not modify timestamp if it is already a Date object", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_modify_timestamp_if_it_is_already_a_Date_object()
    {
        var parts = await Stream($"{{\"type\":\"response-metadata\",\"id\":\"test-id\",\"timestamp\":\"{Stamp}\"}}");

        var metadata = Assert.IsType<ResponseMetadataStreamPart>(Assert.Single(parts));
        Assert.Equal(new DateTimeOffset(2023, 12, 7, 10, 30, 0, TimeSpan.Zero), metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should not modify response-metadata chunks without timestamp", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_modify_response_metadata_chunks_without_timestamp()
    {
        var parts = await Stream("{\"type\":\"response-metadata\",\"id\":\"test-id\"}");

        var metadata = Assert.IsType<ResponseMetadataStreamPart>(Assert.Single(parts));
        Assert.Equal("test-id", metadata.Id);
        Assert.Null(metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle null timestamp values gracefully", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_null_timestamp_values_gracefully()
    {
        var parts = await Stream("{\"type\":\"response-metadata\",\"id\":\"test-id\",\"timestamp\":null}");

        var metadata = Assert.IsType<ResponseMetadataStreamPart>(Assert.Single(parts));
        Assert.Null(metadata.Timestamp);
    }

    [Fact]
    [UpstreamTest(Prefix + "should only convert timestamps for response-metadata chunks, not other chunk types", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_only_convert_timestamps_for_response_metadata_chunks_not_other_chunk_types()
    {
        var parts = await Stream(
            $"{{\"type\":\"text-delta\",\"id\":\"1\",\"delta\":\"Hello\",\"timestamp\":\"{Stamp}\"}}",
            $"{{\"type\":\"response-metadata\",\"id\":\"test-id\",\"timestamp\":\"{Stamp}\"}}",
            $"{{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{{\"inputTokens\":1,\"outputTokens\":2}},\"timestamp\":\"{Stamp}\"}}");

        Assert.Equal(3, parts.Count);
        Assert.Equal(Stamp, Assert.IsType<TextDeltaStreamPart>(parts[0]).RawValue!.Value.GetProperty("timestamp").GetString());
        Assert.Equal(new DateTimeOffset(2023, 12, 7, 10, 30, 0, TimeSpan.Zero), Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Timestamp);
        Assert.Equal(Stamp, Assert.IsType<FinishStreamPart>(parts[2]).RawValue!.Value.GetProperty("timestamp").GetString());
    }
}
