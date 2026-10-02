// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests;

public sealed class GenAiFormatTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map known providers to well-known values",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_known_providers()
    {
        Assert.Equal("anthropic", GenAiTelemetry.MapProviderName("anthropic.messages"));
        Assert.Equal("openai", GenAiTelemetry.MapProviderName("openai.chat"));
        Assert.Equal("gcp.gemini", GenAiTelemetry.MapProviderName("google.generative-ai"));
        Assert.Equal("mistral_ai", GenAiTelemetry.MapProviderName("mistral.chat"));
        Assert.Equal("groq", GenAiTelemetry.MapProviderName("groq.chat"));
        Assert.Equal("deepseek", GenAiTelemetry.MapProviderName("deepseek.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map google vertex provider strings",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_google_vertex_providers()
    {
        Assert.Equal("gcp.vertex_ai", GenAiTelemetry.MapProviderName("google.vertex.chat"));
        Assert.Equal("gcp.vertex_ai", GenAiTelemetry.MapProviderName("google.vertex.embedding"));
        Assert.Equal("gcp.vertex_ai", GenAiTelemetry.MapProviderName("google.vertex.image"));
        Assert.Equal("gcp.vertex_ai", GenAiTelemetry.MapProviderName("google-vertex"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map bare google prefix to gcp.gemini",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_bare_google_to_gemini()
    {
        Assert.Equal("gcp.gemini", GenAiTelemetry.MapProviderName("google.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map bedrock provider",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_bedrock_providers()
    {
        Assert.Equal("aws.bedrock", GenAiTelemetry.MapProviderName("amazon-bedrock.chat"));
        Assert.Equal("aws.bedrock", GenAiTelemetry.MapProviderName("bedrock.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should map azure providers",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_azure_providers()
    {
        Assert.Equal("azure.ai.inference", GenAiTelemetry.MapProviderName("azure.chat"));
        Assert.Equal("azure.ai.openai", GenAiTelemetry.MapProviderName("azure-openai.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapProviderName::should return the original string for unknown providers",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_unknown_provider_strings()
    {
        Assert.Equal("custom-provider.chat", GenAiTelemetry.MapProviderName("custom-provider.chat"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map generateText/streamText to invoke_agent",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_text_operations_to_invoke_agent()
    {
        Assert.Equal("invoke_agent", GenAiTelemetry.MapOperationName("ai.generateText"));
        Assert.Equal("invoke_agent", GenAiTelemetry.MapOperationName("ai.streamText"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map generateObject/streamObject to invoke_agent",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_object_operations_to_invoke_agent()
    {
        Assert.Equal("invoke_agent", GenAiTelemetry.MapOperationName("ai.generateObject"));
        Assert.Equal("invoke_agent", GenAiTelemetry.MapOperationName("ai.streamObject"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map embed/embedMany to embeddings",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_embed_operations_to_embeddings()
    {
        Assert.Equal("embeddings", GenAiTelemetry.MapOperationName("ai.embed"));
        Assert.Equal("embeddings", GenAiTelemetry.MapOperationName("ai.embedMany"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should map rerank to rerank",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_rerank_to_rerank()
    {
        Assert.Equal("rerank", GenAiTelemetry.MapOperationName("ai.rerank"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::mapOperationName::should return the original string for unknown operations",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_unknown_operation_strings()
    {
        Assert.Equal("ai.unknown", GenAiTelemetry.MapOperationName("ai.unknown"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatSystemInstructions::should format a system string into SemConv system instructions",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_a_system_string()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatSystemInstructions(JsonValue.Create("You are a helpful assistant.")),
            "[{\"type\":\"text\",\"content\":\"You are a helpful assistant.\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::extractSystemFromPrompt::should extract system message from prompt",
        Coverage = UpstreamCoverage.Covered)]
    public void Extracts_the_system_message()
    {
        Assert.Equal(
            "Be helpful",
            GenAiTelemetry.ExtractSystemFromPrompt(Prompt("[{\"role\":\"system\",\"content\":\"Be helpful\"},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]")));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::extractSystemFromPrompt::should return undefined when no system message",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_null_when_the_prompt_has_no_system_message()
    {
        Assert.Null(GenAiTelemetry.ExtractSystemFromPrompt(Prompt("[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]")));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert user text messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_user_text_messages()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"What is the weather?\"}]}]")),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"What is the weather?\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should preserve system messages in prompt order",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_system_messages_in_prompt_order()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"First\"}]},{\"role\":\"system\",\"content\":\"Be helpful\"},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Second\"}]}]")),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"First\"}]},{\"role\":\"system\",\"parts\":[{\"type\":\"text\",\"content\":\"Be helpful\"}]},{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Second\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert assistant messages with tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_assistant_tool_calls()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"call_123\",\"toolName\":\"get_weather\",\"input\":{\"city\":\"Paris\"}}]}]")),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"tool_call\",\"id\":\"call_123\",\"name\":\"get_weather\",\"arguments\":{\"city\":\"Paris\"}}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert tool result messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_tool_result_messages()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"call_123\",\"toolName\":\"get_weather\",\"output\":{\"type\":\"text\",\"value\":\"Sunny, 72°F\"}}]}]")),
            "[{\"role\":\"tool\",\"parts\":[{\"type\":\"tool_call_response\",\"id\":\"call_123\",\"response\":\"Sunny, 72°F\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert file parts to blob parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_file_parts_to_blobs()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":{\"type\":\"data\",\"data\":\"base64data\"},\"mediaType\":\"image/png\"}]}]")),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"blob\",\"modality\":\"image\",\"mime_type\":\"image/png\",\"content\":\"base64data\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert URL file parts to uri parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_url_file_parts_to_uri_parts()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"user\",\"content\":[{\"type\":\"file\",\"data\":{\"type\":\"url\",\"url\":\"https://example.com/image.png\"},\"mediaType\":\"image/png\"}]}]")),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"uri\",\"modality\":\"image\",\"mime_type\":\"image/png\",\"uri\":\"https://example.com/image.png\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatInputMessages::should convert reasoning parts",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_reasoning_parts()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatInputMessages(Prompt("[{\"role\":\"assistant\",\"content\":[{\"type\":\"reasoning\",\"text\":\"Let me think about this...\"}]}]")),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"reasoning\",\"content\":\"Let me think about this...\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format text-only output",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_text_only_output()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatOutputMessages(new GenAiOutput { Text = "Hello world", FinishReason = "stop" }),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello world\"}],\"finish_reason\":\"stop\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with reasoning",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_output_with_reasoning()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatOutputMessages(new GenAiOutput
            {
                Text = "The answer is 42",
                Reasoning = new[] { "Let me think..." },
                FinishReason = "stop",
            }),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"reasoning\",\"content\":\"Let me think...\"},{\"type\":\"text\",\"content\":\"The answer is 42\"}],\"finish_reason\":\"stop\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with tool calls",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_output_with_tool_calls()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatOutputMessages(new GenAiOutput
            {
                ToolCalls = new[] { new GenAiToolCall("call_abc", "get_weather", JsonNode.Parse("{\"city\":\"Paris\"}")) },
                FinishReason = "tool-calls",
            }),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"tool_call\",\"id\":\"call_abc\",\"name\":\"get_weather\",\"arguments\":{\"city\":\"Paris\"}}],\"finish_reason\":\"tool_call\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with tool results",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_output_with_tool_results()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatOutputMessages(new GenAiOutput
            {
                ToolResults = new[] { new GenAiToolResult("call_abc", JsonNode.Parse("{\"temperature\":21}")) },
                FinishReason = "stop",
            }),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"tool_call_response\",\"id\":\"call_abc\",\"response\":{\"temperature\":21}}],\"finish_reason\":\"stop\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should format output with files",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_output_with_files()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatOutputMessages(new GenAiOutput
            {
                Files = new[] { new GenAiOutputFile("image/png", "abc123") },
                FinishReason = "stop",
            }),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"blob\",\"modality\":\"image\",\"mime_type\":\"image/png\",\"content\":\"abc123\"}],\"finish_reason\":\"stop\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should combine reasoning, text, tool calls, and files",
        Coverage = UpstreamCoverage.Covered)]
    public void Combines_reasoning_text_tool_calls_and_files()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatOutputMessages(new GenAiOutput
            {
                Text = "Here is the result",
                Reasoning = new[] { "Thinking..." },
                ToolCalls = new[] { new GenAiToolCall("tc1", "search", JsonNode.Parse("{\"q\":\"test\"}")) },
                Files = new[] { new GenAiOutputFile("image/jpeg", "data") },
                FinishReason = "stop",
            }),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"reasoning\",\"content\":\"Thinking...\"},{\"type\":\"text\",\"content\":\"Here is the result\"},{\"type\":\"tool_call\",\"id\":\"tc1\",\"name\":\"search\",\"arguments\":{\"q\":\"test\"}},{\"type\":\"blob\",\"modality\":\"image\",\"mime_type\":\"image/jpeg\",\"content\":\"data\"}],\"finish_reason\":\"stop\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputMessages::should map finish reasons correctly",
        Coverage = UpstreamCoverage.Covered)]
    public void Maps_finish_reasons()
    {
        Assert.Equal("stop", Finish("stop"));
        Assert.Equal("length", Finish("length"));
        Assert.Equal("tool_call", Finish("tool-calls"));
        Assert.Equal("content_filter", Finish("content-filter"));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatObjectOutputMessages::should format object output as text content",
        Coverage = UpstreamCoverage.Covered)]
    public void Formats_object_output_as_text()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatObjectOutputMessages("{\"name\":\"test\"}", "stop"),
            "[{\"role\":\"assistant\",\"parts\":[{\"type\":\"text\",\"content\":\"{\\\"name\\\":\\\"test\\\"}\"}],\"finish_reason\":\"stop\"}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should convert a prompt string to a user message",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_prompt_string_to_a_user_message()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatModelMessages(JsonValue.Create("Hello"), null),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hello\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should convert ModelMessage array from prompt",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_a_model_message_array_from_the_prompt()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatModelMessages(
                Prompt("[{\"role\":\"user\",\"content\":\"Hi there\"},{\"role\":\"assistant\",\"content\":[{\"type\":\"tool-call\",\"toolCallId\":\"tc1\",\"toolName\":\"weather\",\"input\":{\"city\":\"NYC\"}}]}]"),
                null),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Hi there\"}]},{\"role\":\"assistant\",\"parts\":[{\"type\":\"tool_call\",\"id\":\"tc1\",\"name\":\"weather\",\"arguments\":{\"city\":\"NYC\"}}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should preserve system messages in message order",
        Coverage = UpstreamCoverage.Covered)]
    public void Preserves_system_messages_in_message_order()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatModelMessages(
                null,
                Prompt("[{\"role\":\"user\",\"content\":\"First\"},{\"role\":\"system\",\"content\":\"Be helpful\"},{\"role\":\"user\",\"content\":\"Second\"}]")),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"First\"}]},{\"role\":\"system\",\"parts\":[{\"type\":\"text\",\"content\":\"Be helpful\"}]},{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Second\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should convert tool messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Converts_tool_messages()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatModelMessages(
                null,
                Prompt("[{\"role\":\"tool\",\"content\":[{\"type\":\"tool-result\",\"toolCallId\":\"tc1\",\"toolName\":\"weather\",\"output\":{\"type\":\"text\",\"value\":\"Sunny\"}}]}]")),
            "[{\"role\":\"tool\",\"parts\":[{\"type\":\"tool_call_response\",\"id\":\"tc1\",\"response\":\"Sunny\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should combine prompt and messages",
        Coverage = UpstreamCoverage.Covered)]
    public void Combines_prompt_and_messages()
    {
        ParityAssert.JsonEqual(
            GenAiTelemetry.FormatModelMessages(JsonValue.Create("First message"), Prompt("[{\"role\":\"user\",\"content\":\"Second message\"}]")),
            "[{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"First message\"}]},{\"role\":\"user\",\"parts\":[{\"type\":\"text\",\"content\":\"Second message\"}]}]");
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatModelMessages::should return empty array when both prompt and messages are undefined",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_empty_array_when_prompt_and_messages_are_null()
    {
        ParityAssert.JsonEqual(GenAiTelemetry.FormatModelMessages(null, null), "[]");
    }

    private static string Finish(string reason)
    {
        var messages = GenAiTelemetry.FormatOutputMessages(new GenAiOutput { FinishReason = reason });
        if (messages[0]!["finish_reason"] is not JsonValue value)
        {
            throw new InvalidOperationException("Missing finish reason.");
        }

        return value.GetValue<string>();
    }

    private static JsonArray Prompt(string json)
    {
        return JsonNode.Parse(json)!.AsArray();
    }
}
