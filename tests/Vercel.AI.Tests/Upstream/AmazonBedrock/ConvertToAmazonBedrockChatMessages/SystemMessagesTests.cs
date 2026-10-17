// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.AmazonBedrock.BedrockStreamSupport;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ConvertToAmazonBedrockChatMessages;

public sealed class SystemMessagesTests
{
    private const string Suite = "packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::system messages::";

    [Fact]
    [UpstreamTest(Suite + "should set isSystemCachePoint when system message has cache point", Coverage = UpstreamCoverage.Covered)]
    public void Should_set_isSystemCachePoint_when_system_message_has_cache_point()
    {
        AssertSystem("""{"bedrock":{"cachePoint":{"type":"default"}}}""", """[{"text":"Hello"},{"cachePoint":{"type":"default"}}]""");
    }

    [Fact]
    [UpstreamTest(Suite + "should add cache point with 5m TTL to system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_with_5m_TTL_to_system_message()
    {
        AssertSystem("""{"bedrock":{"cachePoint":{"type":"default","ttl":"5m"}}}""", """[{"text":"Hello"},{"cachePoint":{"type":"default","ttl":"5m"}}]""");
    }

    [Fact]
    [UpstreamTest(Suite + "should add cache point with 1h TTL to system message", Coverage = UpstreamCoverage.Covered)]
    public void Should_add_cache_point_with_1h_TTL_to_system_message()
    {
        AssertSystem("""{"bedrock":{"cachePoint":{"type":"default","ttl":"1h"}}}""", """[{"text":"Hello"},{"cachePoint":{"type":"default","ttl":"1h"}}]""");
    }

    private static void AssertSystem(string providerOptions, string expectedSystem)
    {
        AmazonBedrockMessages.Convert(
            ModelId,
            new ModelMessage[] { new SystemModelMessage("Hello", Options(providerOptions)) },
            out var system,
            out var messages);

        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expectedSystem), system));
        Assert.Empty(messages);
    }
}
