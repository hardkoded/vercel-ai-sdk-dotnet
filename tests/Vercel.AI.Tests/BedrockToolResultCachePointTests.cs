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

    private static async Task<JsonNode> SendAsync(AssistantToolResultPart result)
    {
        var handler = new ScriptedHandler();
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        var model = provider.LanguageModel("anthropic.claude-3-haiku");
        await model.DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Add"),
                new AssistantModelMessage(new AssistantContentPart[] { result }),
                new UserModelMessage("Next"),
            },
        }, CancellationToken.None);
        Assert.Contains("/converse", handler.Uri);
        return JsonNode.Parse(handler.Body)!["messages"]!;
    }

    [Fact]
    public async Task ShouldEmitTheOutputCachePointAfterAProviderExecutedToolResult()
    {
        var output = new ToolModelMessage("call-1", "add", "{\"sum\":4}", false, "json") { ProviderOptions = CachePoint("{\"type\":\"default\"}") };
        var messages = await SendAsync(new AssistantToolResultPart("call-1", "add", output));

        var content = messages.AsArray()[1]!["content"]!.AsArray();
        Assert.Equal(3, content.Count);
        Assert.NotNull(content[0]!["toolResult"]);
        Assert.Equal("default", content[1]!["cachePoint"]!["type"]!.GetValue<string>());
        Assert.Equal("Next", content[2]!["text"]!.GetValue<string>());
    }

    [Fact]
    public async Task ShouldPreferThePartCachePointOverTheOutputCachePoint()
    {
        var output = new ToolModelMessage("call-1", "add", "{\"sum\":4}", false, "json") { ProviderOptions = CachePoint("{\"type\":\"default\"}") };
        var messages = await SendAsync(new AssistantToolResultPart("call-1", "add", output, CachePoint("{\"type\":\"default\",\"ttl\":\"1h\"}")));

        var content = messages.AsArray()[1]!["content"]!.AsArray();
        Assert.Equal(3, content.Count);
        Assert.NotNull(content[0]!["toolResult"]);
        Assert.Equal("1h", content[1]!["cachePoint"]!["ttl"]!.GetValue<string>());
        Assert.Equal("Next", content[2]!["text"]!.GetValue<string>());
    }
}
