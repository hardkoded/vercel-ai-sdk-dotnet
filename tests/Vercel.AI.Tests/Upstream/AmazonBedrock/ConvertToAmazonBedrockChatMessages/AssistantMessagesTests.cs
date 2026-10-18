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

    private static void AssertJson(string expected, JsonNode? actual)
    {
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should preserve the order of provider-executed tool calls and results", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_the_order_of_provider_executed_tool_calls_and_results()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Add 2 and 2, then add 3 and 3."),
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantTextPart("Running the additions."),
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "add", "{\"a\":2,\"b\":2}")),
                new AssistantToolResultPart("call-1", "add", new ToolModelMessage("call-1", "add", "{\"sum\":4}", false, "json")),
                new AssistantToolCallPart(new GeneratedToolCall("call-2", "add", "{\"a\":3,\"b\":3}")),
                new AssistantToolResultPart("call-2", "add", new ToolModelMessage("call-2", "add", "{\"sum\":6}", false, "json")),
                new AssistantTextPart("The sums are 4 and 6."),
            }),
            new UserModelMessage("Now add those sums together."),
        }, out var system, out var messages);

        AssertJson("""
            [
              {"role":"user","content":[{"text":"Add 2 and 2, then add 3 and 3."}]},
              {"role":"assistant","content":[{"text":"Running the additions."},{"toolUse":{"toolUseId":"call-1","name":"add","input":{"a":2,"b":2}}}]},
              {"role":"user","content":[{"toolResult":{"toolUseId":"call-1","content":[{"text":"{\"sum\":4}"}]}}]},
              {"role":"assistant","content":[{"toolUse":{"toolUseId":"call-2","name":"add","input":{"a":3,"b":3}}}]},
              {"role":"user","content":[{"toolResult":{"toolUseId":"call-2","content":[{"text":"{\"sum\":6}"}]}}]},
              {"role":"assistant","content":[{"text":"The sums are 4 and 6."}]},
              {"role":"user","content":[{"text":"Now add those sums together."}]}
            ]
            """, messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should combine a trailing provider-executed tool result with the next user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_combine_a_trailing_provider_executed_tool_result_with_the_next_user_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "add", "{\"a\":2,\"b\":2}")),
                new AssistantToolResultPart("call-1", "add", new ToolModelMessage("call-1", "add", "{\"sum\":4}", false, "json")),
            }),
            new UserModelMessage("Now double that."),
        }, out var system, out var messages);

        AssertJson("""
            [
              {"role":"assistant","content":[{"toolUse":{"toolUseId":"call-1","name":"add","input":{"a":2,"b":2}}}]},
              {"role":"user","content":[{"toolResult":{"toolUseId":"call-1","content":[{"text":"{\"sum\":4}"}]}},{"text":"Now double that."}]}
            ]
            """, messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should remove trailing whitespace from last assistant message when there is no further user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_remove_trailing_whitespace_from_last_assistant_message_when_there_is_no_further_user_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("user content"),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("assistant content  ") }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"user content"}]},{"role":"assistant","content":[{"text":"assistant content"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should remove trailing whitespace from last assistant message with multi-part content when there is no further user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_remove_trailing_whitespace_from_last_assistant_message_with_multi_part_content_when_there_is_no_further_user_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("user content"),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("assistant "), new AssistantTextPart("content  ") }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"user content"}]},{"role":"assistant","content":[{"text":"assistant "},{"text":"content"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should keep trailing whitespace from assistant message when there is a further user message", Coverage = UpstreamCoverage.Covered)]
    public void Should_keep_trailing_whitespace_from_assistant_message_when_there_is_a_further_user_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("user content"),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("assistant content  ") }),
            new UserModelMessage("user content 2"),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"user content"}]},{"role":"assistant","content":[{"text":"assistant content  "}]},{"role":"user","content":[{"text":"user content 2"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should combine multiple sequential assistant messages into a single message", Coverage = UpstreamCoverage.Covered)]
    public void Should_combine_multiple_sequential_assistant_messages_into_a_single_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new UserModelMessage("Hi!"),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("Hello") }),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("World") }),
            new AssistantModelMessage(new AssistantContentPart[] { new AssistantTextPart("!") }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"user","content":[{"text":"Hi!"}]},{"role":"assistant","content":[{"text":"Hello"},{"text":"World"},{"text":"!"}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should wrap non-object (invalid) tool call input in an object", Coverage = UpstreamCoverage.Covered)]
    public void Should_wrap_non_object_invalid_tool_call_input_in_an_object()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                // malformed JSON the model produced, kept as a raw string
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "cityAttractions", "{ \"city\": \"San Francisco\", }")),
            }),
        }, out var system, out var messages);

        AssertJson("""[{"role":"assistant","content":[{"toolUse":{"toolUseId":"call-1","name":"cityAttractions","input":{"rawInvalidInput":"{ \"city\": \"San Francisco\", }"}}}]}]""", messages);
        Assert.Null(system);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::assistant messages::should strip invalid characters from tool call names", Coverage = UpstreamCoverage.Covered)]
    public void Should_strip_invalid_characters_from_tool_call_names()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall("call-1", "$READFILE", "{}")),
                new AssistantToolCallPart(new GeneratedToolCall("call-2", "exchange_delivered_order_items<|channel|>", "{}")),
                new AssistantToolCallPart(new GeneratedToolCall("call-3", "$", "{}")),
            }),
        }, out _, out var messages);

        AssertJson("""
            [{"role":"assistant","content":[
              {"toolUse":{"toolUseId":"call-1","name":"READFILE","input":{}}},
              {"toolUse":{"toolUseId":"call-2","name":"exchange_delivered_order_itemschannel","input":{}}},
              {"toolUse":{"toolUseId":"call-3","name":"_","input":{}}}
            ]}]
            """, messages);
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
}
