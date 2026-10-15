// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Tests.Upstream.AmazonBedrock;

namespace Vercel.AI.Tests.Streaming;

/// <summary>The Gateway, Bedrock Anthropic, and Bedrock Mantle models stream while the response is open.</summary>
public sealed class ProviderStreamingTests
{
    private static LanguageModelCallOptions Prompt() => new() { Prompt = new ModelMessage[] { new UserModelMessage("Hello") } };

    // Gateway

    private static ILanguageModel Gateway(GatedStreamHandler handler)
        => GatewayProvider.Create(new GatewayOptions { ApiKey = "test-token", BaseUrl = "https://api.test.com" }, handler).LanguageModel("test-model");

    private static string GatewayEvent(string json) => "data: " + json + "\n\n";

    private const string GatewayHello = "{\"type\":\"text-delta\",\"textDelta\":\"Hello\"}";

    private const string GatewayFinish = "{\"type\":\"finish\",\"finishReason\":\"stop\",\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":20}}";

    [Fact]
    public async Task Gateway_yields_the_first_part_before_the_stream_ends()
    {
        var handler = new GatedStreamHandler();
        var model = Gateway(handler);
        await LiveStreamAssertions.FirstPartArrivesBeforeTheStreamEndsAsync(
            handler.Body,
            () => model.DoStreamAsync(Prompt(), CancellationToken.None),
            () => handler.Body.Write(GatewayEvent(GatewayHello)),
            delta => Assert.Equal("Hello", delta),
            () =>
            {
                handler.Body.Write(GatewayEvent(GatewayFinish));
                handler.Body.Complete();
            });
    }

    [Fact]
    public async Task Gateway_stops_reading_when_the_consumer_stops()
    {
        var handler = new GatedStreamHandler();
        var model = Gateway(handler);
        await LiveStreamAssertions.StoppingTheConsumerStopsTheReadAsync(
            handler.Body,
            () => model.DoStreamAsync(Prompt(), CancellationToken.None),
            () => handler.Body.Write(GatewayEvent(GatewayHello)),
            () => handler.Body.Write(GatewayEvent(GatewayFinish)));
    }

    [Fact]
    public async Task Gateway_stops_reading_when_the_token_is_cancelled()
    {
        var handler = new GatedStreamHandler();
        var model = Gateway(handler);
        await LiveStreamAssertions.CancellingTheTokenStopsTheReadAsync(
            handler.Body,
            token => model.DoStreamAsync(Prompt(), token),
            () => handler.Body.Write(GatewayEvent(GatewayHello)));
    }

    [Fact]
    public async Task Gateway_fails_the_stream_when_a_chunk_is_not_json()
    {
        var handler = new GatedStreamHandler();
        var model = Gateway(handler);
        handler.Body.Write(GatewayEvent(GatewayHello) + GatewayEvent("{not json"));
        handler.Body.Write(GatewayEvent(GatewayFinish));
        handler.Body.Complete();

        var result = await model.DoStreamAsync(Prompt(), CancellationToken.None);
        await using var parts = result.GetAsyncEnumerator();
        await LiveStreamAssertions.NextAsync(parts, part => part is TextDeltaStreamPart);
        await Assert.ThrowsAsync<JsonParseException>(async () => await parts.MoveNextAsync());
    }

    // Bedrock Anthropic

    private static ILanguageModel BedrockAnthropic(GatedStreamHandler handler)
        => AmazonBedrockAnthropicProvider.Create(new AmazonBedrockAnthropicOptions { ApiKey = "test-key" }, handler).LanguageModel("anthropic.claude-test");

    private static byte[] AnthropicChunk(string anthropicEvent)
        => EventStreamFrames.Event("chunk", new JsonObject { ["bytes"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(anthropicEvent)) }.ToJsonString());

    private static readonly byte[] AnthropicStart = Concat(
        AnthropicChunk("{\"type\":\"message_start\",\"message\":{\"id\":\"msg_1\",\"model\":\"claude\",\"usage\":{\"input_tokens\":3,\"output_tokens\":1}}}"),
        AnthropicChunk("{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"text\",\"text\":\"\"}}"),
        AnthropicChunk("{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"text_delta\",\"text\":\"Hello\"}}"));

    private static readonly byte[] AnthropicEnd = Concat(
        AnthropicChunk("{\"type\":\"content_block_stop\",\"index\":0}"),
        AnthropicChunk("{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"end_turn\"},\"usage\":{\"output_tokens\":2}}"),
        AnthropicChunk("{\"type\":\"message_stop\"}"),
        EventStreamFrames.Event("messageStop", "{}"));

    [Fact]
    public async Task Bedrock_anthropic_yields_the_first_part_before_the_stream_ends()
    {
        var handler = new GatedStreamHandler("application/vnd.amazon.eventstream");
        var model = BedrockAnthropic(handler);
        await LiveStreamAssertions.FirstPartArrivesBeforeTheStreamEndsAsync(
            handler.Body,
            () => model.DoStreamAsync(Prompt(), CancellationToken.None),
            () => handler.Body.Write(AnthropicStart),
            delta => Assert.Equal("Hello", delta),
            () =>
            {
                handler.Body.Write(AnthropicEnd);
                handler.Body.Complete();
            });
    }

    [Fact]
    public async Task Bedrock_anthropic_stops_reading_when_the_consumer_stops()
    {
        var handler = new GatedStreamHandler("application/vnd.amazon.eventstream");
        var model = BedrockAnthropic(handler);
        await LiveStreamAssertions.StoppingTheConsumerStopsTheReadAsync(
            handler.Body,
            () => model.DoStreamAsync(Prompt(), CancellationToken.None),
            () => handler.Body.Write(AnthropicStart),
            () => handler.Body.Write(AnthropicEnd));
    }

    [Fact]
    public async Task Bedrock_anthropic_stops_reading_when_the_token_is_cancelled()
    {
        var handler = new GatedStreamHandler("application/vnd.amazon.eventstream");
        var model = BedrockAnthropic(handler);
        await LiveStreamAssertions.CancellingTheTokenStopsTheReadAsync(
            handler.Body,
            token => model.DoStreamAsync(Prompt(), token),
            () => handler.Body.Write(AnthropicStart));
    }

    [Fact]
    public async Task Bedrock_anthropic_streams_the_event_stream_the_service_sends()
    {
        var handler = new GatedStreamHandler("application/vnd.amazon.eventstream");
        var model = BedrockAnthropic(handler);
        handler.Body.Write(AnthropicStart);
        handler.Body.Write(AnthropicEnd);
        handler.Body.Complete();

        var result = await model.DoStreamAsync(Prompt(), CancellationToken.None);
        await using var parts = result.GetAsyncEnumerator();
        var all = await LiveStreamAssertions.DrainAsync(parts);

        Assert.Equal("Hello", string.Concat(all.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
        var finish = Assert.IsType<FinishStreamPart>(all[^1]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Contains("invoke-with-response-stream", handler.RequestUri);
    }

    // Bedrock Mantle

    private static ILanguageModel Mantle(GatedStreamHandler handler)
        => BedrockMantleProvider.Create(new BedrockMantleOptions { ApiKey = "test-key" }, handler).LanguageModel("openai.gpt-oss-120b");

    private static string MantleEvent(string json) => "data: " + json + "\n\n";

    private const string MantleHello = "{\"choices\":[{\"delta\":{\"content\":\"Hello\"}}]}";

    private const string MantleEnd = "{\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":3,\"completion_tokens\":2}}";

    [Fact]
    public async Task Mantle_yields_the_first_part_before_the_stream_ends()
    {
        var handler = new GatedStreamHandler();
        var model = Mantle(handler);
        await LiveStreamAssertions.FirstPartArrivesBeforeTheStreamEndsAsync(
            handler.Body,
            () => model.DoStreamAsync(Prompt(), CancellationToken.None),
            () => handler.Body.Write(MantleEvent(MantleHello)),
            delta => Assert.Equal("Hello", delta),
            () =>
            {
                handler.Body.Write(MantleEvent(MantleEnd) + "data: [DONE]\n\n");
                handler.Body.Complete();
            });
        Assert.Contains("\"stream\":true", handler.RequestBody);
    }

    [Fact]
    public async Task Mantle_stops_reading_when_the_consumer_stops()
    {
        var handler = new GatedStreamHandler();
        var model = Mantle(handler);
        await LiveStreamAssertions.StoppingTheConsumerStopsTheReadAsync(
            handler.Body,
            () => model.DoStreamAsync(Prompt(), CancellationToken.None),
            () => handler.Body.Write(MantleEvent(MantleHello)),
            () => handler.Body.Write(MantleEvent(MantleEnd)));
    }

    [Fact]
    public async Task Mantle_stops_reading_when_the_token_is_cancelled()
    {
        var handler = new GatedStreamHandler();
        var model = Mantle(handler);
        await LiveStreamAssertions.CancellingTheTokenStopsTheReadAsync(
            handler.Body,
            token => model.DoStreamAsync(Prompt(), token),
            () => handler.Body.Write(MantleEvent(MantleHello)));
    }

    [Fact]
    public async Task Mantle_responses_streams_output_text_deltas()
    {
        var handler = new GatedStreamHandler();
        var model = BedrockMantleProvider.Create(new BedrockMantleOptions { ApiKey = "test-key" }, handler).Responses("openai.gpt-oss-120b");
        handler.Body.Write(MantleEvent("{\"type\":\"response.output_text.delta\",\"delta\":\"Hi\"}"));
        handler.Body.Write(MantleEvent("{\"type\":\"response.completed\",\"response\":{\"usage\":{\"input_tokens\":4,\"output_tokens\":1}}}"));
        handler.Body.Complete();

        var result = await model.DoStreamAsync(Prompt(), CancellationToken.None);
        await using var parts = result.GetAsyncEnumerator();
        var all = await LiveStreamAssertions.DrainAsync(parts);

        Assert.Equal("Hi", string.Concat(all.OfType<TextDeltaStreamPart>().Select(part => part.Delta)));
        var finish = Assert.IsType<FinishStreamPart>(all[^1]);
        Assert.Equal(4, finish.Usage.InputTokens);
    }

    private static byte[] Concat(params byte[][] parts) => parts.SelectMany(part => part).ToArray();
}
