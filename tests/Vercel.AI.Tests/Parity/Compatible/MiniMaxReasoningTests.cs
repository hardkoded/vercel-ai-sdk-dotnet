// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.MiniMax;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class MiniMaxReasoningTests
{
    [Fact]
    [UpstreamTest("packages/minimax/src/minimax-reasoning.test.ts::MiniMax reasoning::sends thinking via the minimax provider option and returns a reasoning part", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_adaptive_thinking_and_returns_reasoning_before_text()
    {
        var handler = new CaptureHandler
        {
            ResponseBody = "{\"id\":\"msg_minimax_reasoning\",\"content\":[{\"type\":\"thinking\",\"thinking\":\"Counting the letters...\",\"signature\":\"sig_123\"},{\"type\":\"text\",\"text\":\"There are 3 \\\"r\\\"s.\"}],\"stop_reason\":\"end_turn\",\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}",
        };
        var provider = MiniMaxProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["minimax"] = Parse("{\"thinking\":{\"type\":\"adaptive\"}}"),
            },
        };
        var result = await provider.LanguageModel("minimax-m3").DoGenerateAsync(options, CancellationToken.None);
        var body = JsonDocument.Parse(handler.Body).RootElement;
        Assert.Equal("minimax-m3", body.GetProperty("model").GetString());
        Assert.Equal("adaptive", body.GetProperty("thinking").GetProperty("type").GetString());
        Assert.Contains("/anthropic/v1/messages", handler.Uri!.AbsoluteUri, StringComparison.Ordinal);
        Assert.Equal("2023-06-01", handler.RequestHeaders["anthropic-version"]);
        Assert.Equal("test-api-key", handler.RequestHeaders["x-api-key"]);
        Assert.StartsWith("Bearer ", handler.RequestHeaders["Authorization"], StringComparison.Ordinal);
        Assert.IsType<GeneratedReasoning>(result.Content[0]);
        Assert.Equal("Counting the letters...", ((GeneratedReasoning)result.Content[0]).Text);
        Assert.Equal("There are 3 \"r\"s.", ((GeneratedText)result.Content[1]).Text);
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
