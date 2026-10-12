// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.BedrockStreamSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockChatLanguageModelEventStream;

public sealed class AmazonBedrockChatLanguageModelDoStreamTests
{
    private const string Suite = "packages/amazon-bedrock/src/amazon-bedrock-chat-language-model-event-stream.test.ts::AmazonBedrockChatLanguageModel doStream::";

    private static readonly LanguageModelCallOptions Options = new()
    {
        Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
    };

    private static UpstreamEventStreamHandler Frames(params byte[][] frames) => new(frames);

    [Fact]
    [UpstreamTest(Suite + "surfaces event stream decoding failures", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_event_stream_decoding_failures()
    {
        var corruptedFrame = EventStreamFrames.Event("contentBlockDelta", "corrupted");
        corruptedFrame[corruptedFrame.Length - 1] ^= 0xff;
        var handler = Frames(
            corruptedFrame,
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":0,"delta":{"text":"later"}}"""));

        var error = await Assert.ThrowsAnyAsync<Exception>(() => Stream(handler, Options));
        Assert.Contains("The message checksum", error.Message);
    }

    [Fact]
    [UpstreamTest(Suite + "rejects a truncated event stream frame at EOF", Coverage = UpstreamCoverage.Covered)]
    public async Task Rejects_a_truncated_event_stream_frame_at_EOF()
    {
        var textFrame = EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":0,"delta":{"text":"partial result"}}""");
        var messageStopFrame = EventStreamFrames.Event("messageStop", """{"stopReason":"end_turn"}""");
        var handler = Frames(textFrame, messageStopFrame.Take(messageStopFrame.Length - 1).ToArray());

        var error = await Assert.ThrowsAnyAsync<Exception>(() => Stream(handler, Options));
        Assert.Contains("Incomplete Amazon Bedrock event-stream frame", error.Message);
    }

    [Fact]
    [UpstreamTest(Suite + "allows EOF at a complete frame boundary without messageStop", Coverage = UpstreamCoverage.Covered)]
    public async Task Allows_EOF_at_a_complete_frame_boundary_without_messageStop()
    {
        var handler = Frames(EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":0,"delta":{"text":"complete frame"}}"""));

        var parts = await Stream(handler, Options);

        Assert.Contains("text-delta 0 \"complete frame\"", Render(parts));
        var finish = parts.OfType<FinishStreamPart>().Single();
        Assert.Equal(FinishReason.Other, finish.FinishReason);
        Assert.Null(finish.RawFinishReason);
    }

    [Theory]
    [InlineData("internalServerException", 500, true)]
    [InlineData("modelStreamErrorException", 424, true)]
    [InlineData("serviceUnavailableException", 503, true)]
    [InlineData("throttlingException", 429, true)]
    [InlineData("validationException", 400, false)]
    [UpstreamTest(Suite + "surfaces $exceptionType frames as stream errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Surfaces_exceptionType_frames_as_stream_errors(string exceptionType, int statusCode, bool isRetryable)
    {
        var message = "Modeled exception: " + exceptionType;
        var handler = Frames(EventStreamFrames.Exception(exceptionType, "{\"message\":\"" + message + "\"}"));

        var parts = await Stream(handler, Options);

        var errorPart = Assert.IsType<ErrorStreamPart>(parts[^2]);
        var error = Assert.IsType<ProviderStreamError>(errorPart.ProviderError);
        Assert.Equal(message, error.Message);
        Assert.Equal(exceptionType, error.Type);
        Assert.Equal(statusCode, error.StatusCode);
        Assert.Equal(isRetryable, error.IsRetryable);
        Assert.True(JsonEquals("{\"" + exceptionType + "\":{\"message\":\"" + message + "\"}}", Assert.IsType<JsonElement>(error.Data)));
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.Error, finish.FinishReason);
    }

    [Fact]
    [UpstreamTest(Suite + "streams reasoning redacted as `redactedContent` for replay", Coverage = UpstreamCoverage.Covered)]
    public async Task Streams_reasoning_redacted_as_redactedContent_for_replay()
    {
        // Deltas are accumulated and attached once via reasoning-end, because the merged provider
        // metadata of a reasoning part is last-write-wins.
        var handler = Frames(
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":0,"delta":{"reasoningContent":{"redactedContent":"encrypted-reasoning-"}}}"""),
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":0,"delta":{"reasoningContent":{"redactedContent":"payload"}}}"""),
            EventStreamFrames.Event("contentBlockStop", """{"contentBlockIndex":0}"""),
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":1,"delta":{"text":"The answer is 42."}}"""),
            EventStreamFrames.Event("contentBlockStop", """{"contentBlockIndex":1}"""),
            EventStreamFrames.Event("messageStop", """{"stopReason":"end_turn"}"""));

        var parts = await Stream(handler, Options, "us.openai.gpt-5.6-luna");

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var rendered = Render(parts);
        Assert.Contains("reasoning-start 0", rendered);
        Assert.Contains(
            "reasoning-end 0 meta={\"amazonBedrock\":{\"redactedContent\":\"encrypted-reasoning-payload\"},\"bedrock\":{\"redactedContent\":\"encrypted-reasoning-payload\"}}",
            rendered);
        Assert.Contains("text-delta 1 \"The answer is 42.\"", rendered);
    }

    [Fact]
    [UpstreamTest(Suite + "keeps multiple redacted reasoning blocks separate", Coverage = UpstreamCoverage.Covered)]
    public async Task Keeps_multiple_redacted_reasoning_blocks_separate()
    {
        var handler = Frames(
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":0,"delta":{"reasoningContent":{"redactedContent":"first-payload"}}}"""),
            EventStreamFrames.Event("contentBlockStop", """{"contentBlockIndex":0}"""),
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":1,"delta":{"reasoningContent":{"redactedContent":"second-payload"}}}"""),
            EventStreamFrames.Event("contentBlockStop", """{"contentBlockIndex":1}"""),
            EventStreamFrames.Event("contentBlockDelta", """{"contentBlockIndex":2,"delta":{"text":"The answer is 42."}}"""),
            EventStreamFrames.Event("contentBlockStop", """{"contentBlockIndex":2}"""),
            EventStreamFrames.Event("messageStop", """{"stopReason":"end_turn"}"""));

        var parts = await Stream(handler, Options, "us.openai.gpt-5.6-luna");

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        Assert.Equal(
            new[]
            {
                "reasoning-end 0 meta={\"amazonBedrock\":{\"redactedContent\":\"first-payload\"},\"bedrock\":{\"redactedContent\":\"first-payload\"}}",
                "reasoning-end 1 meta={\"amazonBedrock\":{\"redactedContent\":\"second-payload\"},\"bedrock\":{\"redactedContent\":\"second-payload\"}}",
            },
            Render(parts.OfType<ReasoningEndStreamPart>()));
    }
}
