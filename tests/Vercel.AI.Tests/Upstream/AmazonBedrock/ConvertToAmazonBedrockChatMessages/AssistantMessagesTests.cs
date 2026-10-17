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
