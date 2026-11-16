// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; data part conversion &gt; in assistant messages</c>.</summary>
[Collection("LogWarnings")]
public sealed class DataPartConversionInAssistantMessagesTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > data part conversion > in assistant messages::";

    [UpstreamTest(Suite + "should not emit empty assistant message for persistent data written before model stream starts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Should_not_emit_empty_assistant_message_for_persistent_data_written_before_model_stream_starts()
    {
        var recordedMessage = await RecordAssistantMessageJsonFromChunks(
            """
            [
              { "type": "start", "messageId": "msg-123" },
              { "type": "data-weather", "id": "weather-1", "data": { "city": "San Francisco", "status": "loading" } },
              { "type": "start-step" },
              { "type": "text-start", "id": "text-1" },
              { "type": "text-delta", "id": "text-1", "delta": "It is sunny." },
              { "type": "text-end", "id": "text-1" },
              { "type": "finish-step" },
              { "type": "finish" }
            ]
            """);

        // "metadata" and "providerMetadata" are undefined upstream, so they are absent from the JSON form.
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(
                    """
                    {
                      "id": "msg-123",
                      "parts": [
                        { "data": { "city": "San Francisco", "status": "loading" }, "id": "weather-1", "type": "data-weather" },
                        { "type": "step-start" },
                        { "state": "done", "text": "It is sunny.", "type": "text" }
                      ],
                      "role": "assistant"
                    }
                    """),
                recordedMessage),
            recordedMessage.ToJsonString());

        var result = Convert(new[] { UIMessageJson.FromJson(recordedMessage) });

        AssertEqual(
            """
            [
              { "content": [ { "text": "It is sunny.", "type": "text" } ], "role": "assistant" }
            ]
            """,
            result);
    }
}
