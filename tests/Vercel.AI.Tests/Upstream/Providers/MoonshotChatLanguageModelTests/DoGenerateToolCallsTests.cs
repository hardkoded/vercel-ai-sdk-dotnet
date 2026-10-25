// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; tool calls</c>.</summary>
public sealed class DoGenerateToolCallsTests
{
    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > tool calls::should send root unions of objects with type object on the wire",
        Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_root_unions_of_objects_with_type_object_on_the_wire()
    {
        const string city = """{"type":"object","properties":{"city":{"type":"string"}},"required":["city"]}""";
        const string coords = """{"properties":{"lat":{"type":"number"}},"required":["lat"]}""";
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        var options = UpstreamChat.Prompt();
        options.Tools = new[]
        {
            new LanguageModelTool("untyped", null, UpstreamChat.Json("{\"anyOf\":[" + city + "," + coords + "]}")),
            new LanguageModelTool("typed", null, UpstreamChat.Json("{\"type\":\"object\",\"anyOf\":[" + city + "," + coords + "]}")),
        };

        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("kimi-k3")).DoGenerateAsync(options, CancellationToken.None);

        var tools = JsonNode.Parse(capture.Requests[0].Body)!["tools"]!.AsArray();
        Assert.Equal(2, tools.Count);
        foreach (var tool in tools)
        {
            Assert.True(JsonNode.DeepEquals(
                JsonNode.Parse("{\"type\":\"object\",\"anyOf\":[" + city + "," + coords + "]}"),
                tool!["function"]!["parameters"]));
        }
    }
}
