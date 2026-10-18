// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ConvertToAmazonBedrockChatMessages;

public sealed class AssistantMessagesTests
{
    private const string ModelId = "meta.llama3-70b-instruct-v1:0";

    private static IReadOnlyDictionary<string, JsonElement> CachePoint(string cachePointJson)
    {
        using var document = JsonDocument.Parse("{\"cachePoint\":" + cachePointJson + "}");
        return new Dictionary<string, JsonElement> { ["bedrock"] = document.RootElement.Clone() };
    }

    private static IReadOnlyDictionary<string, JsonElement> Options(string provider, string json)
    {
        using var document = JsonDocument.Parse(json);
        return new Dictionary<string, JsonElement> { [provider] = document.RootElement.Clone() };
    }

    private static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should add cache point to assistant message content when specified", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_to_assistant_message_content_when_specified()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello") }) { ProviderOptions = CachePoint("{\"type\":\"default\"}") },
        }, out var system, out var messages);

        AssertJson("""[{"role":"assistant","content":[{"text":"Hello"},{"cachePoint":{"type":"default"}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should add cache point with 5m TTL to assistant message", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_with_5m_TTL_to_assistant_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello") }) { ProviderOptions = CachePoint("{\"type\":\"default\",\"ttl\":\"5m\"}") },
        }, out var system, out var messages);

        AssertJson("""[{"role":"assistant","content":[{"text":"Hello"},{"cachePoint":{"type":"default","ttl":"5m"}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should add cache point with 1h TTL to assistant message", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_with_1h_TTL_to_assistant_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello") }) { ProviderOptions = CachePoint("{\"type\":\"default\",\"ttl\":\"1h\"}") },
        }, out var system, out var messages);

        AssertJson("""[{"role":"assistant","content":[{"text":"Hello"},{"cachePoint":{"type":"default","ttl":"1h"}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should add cache point to assistant content part when specified", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_to_assistant_content_part_when_specified()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantTextPart("Hello"),
                new AssistantTextPart("cached", CachePoint("{\"type\":\"default\",\"ttl\":\"1h\"}")),
                new AssistantTextPart("World"),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"assistant","content":[{"text":"Hello"},{"text":"cached"},{"cachePoint":{"type":"default","ttl":"1h"}},{"text":"World"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should properly convert reasoning content type", Coverage = UpstreamCoverage.Covered)]
    public void should_properly_convert_reasoning_content_type()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("This is my step-by-step reasoning process", Options("bedrock", """{"signature":"test-signature"}""")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"reasoningContent":{"reasoningText":{"text":"This is my step-by-step reasoning process","signature":"test-signature"}}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should properly convert redacted-reasoning content type", Coverage = UpstreamCoverage.Covered)]
    public void should_properly_convert_redacted_reasoning_content_type()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("", Options("bedrock", """{"redactedData":"Redacted reasoning information"}""")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"reasoningContent":{"redactedReasoning":{"data":"Redacted reasoning information"}}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should replay reasoning redacted as `redactedContent`", Coverage = UpstreamCoverage.Covered)]
    public void should_replay_reasoning_redacted_as_redactedContent()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("", Options("bedrock", """{"redactedContent":"encrypted-reasoning-payload"}""")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"reasoningContent":{"redactedContent":"encrypted-reasoning-payload"}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit assistant message reasoning parts signed by a foreign provider", Coverage = UpstreamCoverage.Covered)]
    public void should_omit_assistant_message_reasoning_parts_signed_by_a_foreign_provider()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Anthropic-signed reasoning replayed to Bedrock", Options("anthropic", """{"signature":"anthropic-signature"}""")),
                new AssistantTextPart("final answer"),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"text":"final answer"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should preserve assistant message reasoning parts with amazonBedrock providerOptions", Coverage = UpstreamCoverage.Covered)]
    public void should_preserve_assistant_message_reasoning_parts_with_amazonBedrock_providerOptions()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Bedrock-signed reasoning round-tripped to Bedrock", Options("amazonBedrock", """{"signature":"bedrock-signature"}""")),
                new AssistantTextPart("final answer"),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"reasoningContent":{"reasoningText":{"text":"Bedrock-signed reasoning round-tripped to Bedrock","signature":"bedrock-signature"}}},{"text":"final answer"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should not trim reasoning text when a signature is present", Coverage = UpstreamCoverage.Covered)]
    public void should_not_trim_reasoning_text_when_a_signature_is_present()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("This is my reasoning with trailing space    ", Options("bedrock", """{"signature":"test-signature"}""")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"reasoningContent":{"reasoningText":{"text":"This is my reasoning with trailing space    ","signature":"test-signature"}}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit reasoning content without signature", Coverage = UpstreamCoverage.Covered)]
    public void should_omit_reasoning_content_without_signature()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("This is my reasoning with trailing space    "),
                new AssistantTextPart("final answer"),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"text":"final answer"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit an assistant message when unsigned reasoning is its only content", Coverage = UpstreamCoverage.Covered)]
    public void should_omit_an_assistant_message_when_unsigned_reasoning_is_its_only_content()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Think hard then answer"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Let me consider the options"),
            }),
            new UserModelMessage("Hello?"),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Think hard then answer"}]},{"role":"user","content":[{"text":"Hello?"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit an assistant message when only a cache point remains after filtering unsigned reasoning", Coverage = UpstreamCoverage.Covered)]
    public void should_omit_an_assistant_message_when_only_a_cache_point_remains_after_filtering_unsigned_reasoning()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Think hard then answer"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Let me consider the options"),
            }) { ProviderOptions = Options("bedrock", """{"cachePoint":{"type":"default"}}""") },
            new UserModelMessage("Hello?"),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Think hard then answer"}]},{"role":"user","content":[{"text":"Hello?"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit multiple reasoning parts without signatures", Coverage = UpstreamCoverage.Covered)]
    public void should_omit_multiple_reasoning_parts_without_signatures()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("First reasoning with trailing space    "),
                new AssistantReasoningPart("Second reasoning with trailing space    "),
                new AssistantTextPart("final answer"),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"text":"final answer"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should omit unsigned reasoning while preserving tool calls in multi-turn tool use", Coverage = UpstreamCoverage.Covered)]
    public void should_omit_unsigned_reasoning_while_preserving_tool_calls_in_multi_turn_tool_use()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("What is the weather?"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("I should call the weather tool."),
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "getWeather", """{"city":"SF"}""")),
            }),
            new ToolModelMessage("call-1", "getWeather", "Sunny, 72F", false),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"What is the weather?"}]},{"role":"assistant","content":[{"toolUse":{"toolUseId":"call-1","name":"getWeather","input":{"city":"SF"}}}]},{"role":"user","content":[{"toolResult":{"toolUseId":"call-1","content":[{"text":"Sunny, 72F"}]}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should preserve reasoning text with signature in multi-turn tool use", Coverage = UpstreamCoverage.Covered)]
    public void should_preserve_reasoning_text_with_signature_in_multi_turn_tool_use()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("What is the weather?"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("Let me check the weather API.\n", Options("bedrock", """{"signature":"sig-abc123"}""")),
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "getWeather", """{"city":"SF"}""")),
            }),
            new ToolModelMessage("call-1", "getWeather", "Sunny, 72F", false),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("The weather is sunny and warm.\n", Options("bedrock", """{"signature":"sig-def456"}""")),
                new AssistantTextPart("It is sunny and 72F in SF."),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"What is the weather?"}]},{"role":"assistant","content":[{"reasoningContent":{"reasoningText":{"text":"Let me check the weather API.\n","signature":"sig-abc123"}}},{"toolUse":{"toolUseId":"call-1","name":"getWeather","input":{"city":"SF"}}}]},{"role":"user","content":[{"toolResult":{"toolUseId":"call-1","content":[{"text":"Sunny, 72F"}]}}]},{"role":"assistant","content":[{"reasoningContent":{"reasoningText":{"text":"The weather is sunny and warm.\n","signature":"sig-def456"}}},{"text":"It is sunny and 72F in SF."}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should handle a mix of text and reasoning content types", Coverage = UpstreamCoverage.Covered)]
    public void should_handle_a_mix_of_text_and_reasoning_content_types()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Explain your reasoning"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantTextPart("My answer is 42."),
                new AssistantReasoningPart("I calculated this by analyzing the meaning of life", Options("bedrock", """{"signature":"reasoning-process"}""")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Explain your reasoning"}]},{"role":"assistant","content":[{"text":"My answer is 42."},{"reasoningContent":{"reasoningText":{"text":"I calculated this by analyzing the meaning of life","signature":"reasoning-process"}}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should filter out empty text blocks in assistant messages", Coverage = UpstreamCoverage.Covered)]
    public void should_filter_out_empty_text_blocks_in_assistant_messages()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantTextPart("\n\n"),
                new AssistantToolCallPart(new GeneratedToolCall("call-123", "test", "{}")),
                new AssistantTextPart("  "),
                new AssistantTextPart("actual content"),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Hello"}]},{"role":"assistant","content":[{"toolUse":{"toolUseId":"call-123","name":"test","input":{}}},{"text":"actual content"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should preserve empty text blocks when reasoning blocks are present", Coverage = UpstreamCoverage.Covered)]
    public void should_preserve_empty_text_blocks_when_reasoning_blocks_are_present()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Hello"),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantReasoningPart("thinking...", Options("bedrock", """{"signature":"sig-1"}""")),
                new AssistantTextPart(""),
                new AssistantReasoningPart("more thinking...", Options("bedrock", """{"signature":"sig-2"}""")),
                new AssistantTextPart("response text"),
                new AssistantToolCallPart(new GeneratedToolCall("call-123", "test", "{}")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Hello"}]},{"role":"assistant","content":[{"reasoningContent":{"reasoningText":{"text":"thinking...","signature":"sig-1"}}},{"text":""},{"reasoningContent":{"reasoningText":{"text":"more thinking...","signature":"sig-2"}}},{"text":"response text"},{"toolUse":{"toolUseId":"call-123","name":"test","input":{}}}]}]""", messages);
        Assert.Null(system);
    }
}
