// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages &gt; assistant message</c>.</summary>
[Collection("LogWarnings")]
public sealed class AssistantMessageTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages > assistant message::";

    [UpstreamTest(Suite + "should convert an assistant message with file parts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_convert_an_assistant_message_with_file_parts()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" }
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should include filename for assistant file parts when provided", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_include_filename_for_assistant_file_parts_when_provided()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==") { Filename = "test.png" }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" },
                    "filename": "test.png"
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should handle assistant message file parts with provider metadata", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_handle_assistant_message_file_parts_with_provider_metadata()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("image/png", "data:image/png;base64,dGVzdA==")
            {
                ProviderMetadata = new Dictionary<string, JsonElement>
                {
                    ["testProvider"] = Json("{\"signature\":\"test-signature\"}"),
                },
            }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "image/png",
                    "data": { "type": "url", "url": "data:image/png;base64,dGVzdA==" },
                    "providerOptions": { "testProvider": { "signature": "test-signature" } }
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should use providerReference as data for assistant file parts", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Should_use_providerReference_as_data_for_assistant_file_parts()
    {
        var result = Convert(new[]
        {
            Assistant(new FileUIPart("application/pdf", "data:application/pdf;base64,xyz")
            {
                Filename = "doc.pdf",
                ProviderReference = new Dictionary<string, string> { ["anthropic"] = "file-xyz789" },
            }),
        });

        AssertEqual(
            """
            [
              {
                "role": "assistant",
                "content": [
                  {
                    "type": "file",
                    "mediaType": "application/pdf",
                    "filename": "doc.pdf",
                    "data": { "type": "reference", "reference": { "anthropic": "file-xyz789" } }
                  }
                ]
              }
            ]
            """,
            result);
    }

    [UpstreamTest(Suite + "should preserve an Anthropic compaction signature through UI message persistence", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Should_preserve_an_Anthropic_compaction_signature_through_UI_message_persistence()
    {
        var recordedMessage = await RecordAssistantMessageJsonFromChunks(
            """
            [
              { "type": "start", "messageId": "msg-123" },
              { "type": "start-step" },
              {
                "type": "text-start",
                "id": "compaction",
                "providerMetadata": { "anthropic": { "type": "compaction", "signature": "compaction-signature" } }
              },
              { "type": "text-delta", "id": "compaction", "delta": "Summary of the conversation." },
              { "type": "text-end", "id": "compaction" },
              { "type": "finish-step" },
              { "type": "finish" }
            ]
            """);

        var expectedPart = JsonNode.Parse(
            """
            {
              "type": "text",
              "text": "Summary of the conversation.",
              "state": "done",
              "providerMetadata": { "anthropic": { "type": "compaction", "signature": "compaction-signature" } }
            }
            """)!;
        Assert.Contains(((JsonArray)recordedMessage["parts"]!), part => JsonNode.DeepEquals(part, expectedPart));

        var result = Convert(new[] { UIMessageJson.FromJson(recordedMessage) });

        Assert.Contains(Render(result).AsArray(), message => JsonNode.DeepEquals(
            message,
            JsonNode.Parse(
                """
                {
                  "role": "assistant",
                  "content": [
                    {
                      "type": "text",
                      "text": "Summary of the conversation.",
                      "providerOptions": { "anthropic": { "type": "compaction", "signature": "compaction-signature" } }
                    }
                  ]
                }
                """)));
    }

    [UpstreamTest(Suite + "should omit persisted output when a static tool is no longer available", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Should_omit_persisted_output_when_a_static_tool_is_no_longer_available()
    {
        var historicalTool = new Tool(
            "search",
            null,
            Json("""{"type":"object","properties":{"query":{"type":"string"}},"required":["query"]}"""),
            null)
        {
            OutputSchema = Json("""{"type":"object","properties":{"summary":{"type":"string"},"privateMetadata":{"type":"string"}},"required":["summary","privateMetadata"]}"""),
            ToModelOutput = call => new TextToolModelOutput(JsonDocument.Parse((string)call.Output!).RootElement.GetProperty("summary").GetString()!),
        };
        var tools = new Dictionary<string, Tool> { ["search"] = historicalTool };

        var history = new[]
        {
            new UIMessage(
                UIMessageRole.Assistant,
                new UIMessagePart[]
                {
                    new ToolUIPart("search", "call-1", ToolUIPartState.OutputAvailable)
                    {
                        Input = Json("""{"query":"weather"}"""),
                        Output = Json("""{"summary":"sunny","privateMetadata":"must-not-reach-the-model"}"""),
                    },
                })
            { Id = "assistant-1" },
        };

        var before = Convert(
            await Ai.ValidateUIMessagesAsync(new ValidateUIMessagesOptions { Messages = history, Tools = tools }),
            new ConvertToModelMessagesOptions { Tools = tools });

        var validatedWithoutTool = await Ai.ValidateUIMessagesAsync(
            new ValidateUIMessagesOptions { Messages = history, Tools = new Dictionary<string, Tool>() });

        var unavailablePart = Assert.IsType<ToolUIPart>(validatedWithoutTool[0].Parts[0]);
        Assert.True(unavailablePart.IsDynamic);
        Assert.False(unavailablePart.Dynamic);

        foreach (var messages in new[] { Reload(validatedWithoutTool), Reload(validatedWithoutTool) })
        {
            var revalidatedMessages = await Ai.ValidateUIMessagesAsync(
                new ValidateUIMessagesOptions { Messages = messages, Tools = new Dictionary<string, Tool>() });

            AssertEqual(
                """
                [
                  {
                    "role": "assistant",
                    "content": [
                      { "type": "tool-call", "toolCallId": "call-1", "toolName": "search", "input": { "query": "weather" } }
                    ]
                  },
                  {
                    "role": "tool",
                    "content": [
                      {
                        "type": "tool-result",
                        "toolCallId": "call-1",
                        "toolName": "search",
                        "output": { "type": "text", "value": "Tool output omitted because the tool is no longer available." }
                      }
                    ]
                  }
                ]
                """,
                Convert(revalidatedMessages, new ConvertToModelMessagesOptions { Tools = new Dictionary<string, Tool>() }));

            Assert.True(JsonNode.DeepEquals(
                Render(before),
                Render(Convert(revalidatedMessages, new ConvertToModelMessagesOptions { Tools = tools }))));
        }
    }
}
