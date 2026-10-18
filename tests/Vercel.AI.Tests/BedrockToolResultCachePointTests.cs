// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class BedrockToolResultCachePointTests
{
    private static IReadOnlyDictionary<string, JsonElement> CachePoint(string json)
    {
        using var document = JsonDocument.Parse("{\"cachePoint\":" + json + "}");
        return new Dictionary<string, JsonElement> { ["bedrock"] = document.RootElement.Clone() };
    }

    private static string Convert(AssistantToolResultPart result)
    {
        AmazonBedrockMessages.Convert("meta.llama3-70b-instruct-v1:0", new ModelMessage[]
        {
            new UserModelMessage("Add"),
            new AssistantModelMessage(new AssistantContentPart[] { result }),
            new UserModelMessage("Next"),
        }, out _, out var messages);
        return messages!.ToJsonString();
    }

    [Fact]
    public void ShouldEmitTheOutputCachePointAfterAProviderExecutedToolResult()
    {
        var output = new ToolModelMessage("call-1", "add", "{\"sum\":4}", false, "json") { ProviderOptions = CachePoint("{\"type\":\"default\"}") };
        var json = Convert(new AssistantToolResultPart("call-1", "add", output));

        Assert.Contains("{\"toolResult\":{\"toolUseId\":\"call-1\",\"content\":[{\"text\":\"{\\u0022sum\\u0022:4}\"}]}},{\"cachePoint\":{\"type\":\"default\"}},{\"text\":\"Next\"}", json);
    }

    [Fact]
    public void ShouldPreferThePartCachePointOverTheOutputCachePoint()
    {
        var output = new ToolModelMessage("call-1", "add", "{\"sum\":4}", false, "json") { ProviderOptions = CachePoint("{\"type\":\"default\"}") };
        var json = Convert(new AssistantToolResultPart("call-1", "add", output, CachePoint("{\"type\":\"default\",\"ttl\":\"1h\"}")));

        Assert.Contains("{\"cachePoint\":{\"type\":\"default\",\"ttl\":\"1h\"}}", json);
        Assert.Single(JsonNode.Parse(json)!.AsArray()[1]!["content"]!.AsArray(), b => b!.AsObject().ContainsKey("cachePoint"));
    }
}
