// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ConvertToAmazonBedrockChatMessages;

public sealed class SystemMessagesTests
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
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should set isSystemCachePoint when system message has cache point", Coverage = UpstreamCoverage.Covered)]
    public void Should_set_isSystemCachePoint_when_system_message_has_cache_point()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new SystemModelMessage("Hello", CachePoint("{\"type\":\"default\"}")),
        }, out var system, out var messages);

        AssertJson("""[{"text":"Hello"},{"cachePoint":{"type":"default"}}]""", system);
        Assert.Empty(messages);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should add cache point with 5m TTL to system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_with_5m_TTL_to_system_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new SystemModelMessage("Hello", CachePoint("{\"type\":\"default\",\"ttl\":\"5m\"}")),
        }, out var system, out var messages);

        AssertJson("""[{"text":"Hello"},{"cachePoint":{"type":"default","ttl":"5m"}}]""", system);
        Assert.Empty(messages);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::should add cache point with 1h TTL to system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_with_1h_TTL_to_system_message()
    {
        AmazonBedrockMessages.Convert(ModelId, new ModelMessage[]
        {
            new SystemModelMessage("Hello", CachePoint("{\"type\":\"default\",\"ttl\":\"1h\"}")),
        }, out var system, out var messages);

        AssertJson("""[{"text":"Hello"},{"cachePoint":{"type":"default","ttl":"1h"}}]""", system);
        Assert.Empty(messages);
    }
}
