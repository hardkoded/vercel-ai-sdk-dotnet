// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary><c>GoogleInteractionsLanguageModel agent polling</c>: only the background model call is ported. The model has no agent calls.</summary>
public sealed class GoogleInteractionsAgentPollingUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/google/src/interactions/google-interactions-language-model.test.ts::GoogleInteractionsLanguageModel agent polling::polls doGenerate until terminal for model calls with background: true", Coverage = UpstreamCoverage.Covered)]
    public async Task Polls_doGenerate_until_terminal_for_model_calls_with_background_true()
    {
        var handler = new SequencedHandler()
            .Json("{\"id\":\"v1_poll-test\",\"status\":\"in_progress\"}")
            .Json("{\"id\":\"v1_poll-test\",\"status\":\"completed\",\"steps\":[{\"type\":\"model_output\",\"content\":[{\"type\":\"text\",\"text\":\"long answer\"}]}],\"usage\":{\"total_input_tokens\":5,\"total_output_tokens\":100000,\"total_tokens\":100005}}");
        var provider = GoogleProvider.Create(new GoogleOptions { ApiKey = "test-api-key" }, handler);
        var options = GoogleUpstream.Hello();
        options.MaxOutputTokens = 200_000;
        options.ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"background\":true}}");

        var result = await provider.Interactions("gemini-4-argon").DoGenerateAsync(options, CancellationToken.None);

        var body = JsonNode.Parse(handler.Bodies[0])!;
        Assert.Equal("gemini-4-argon", (string?)body["model"]);
        Assert.True((bool?)body["background"]);
        Assert.Equal(200_000, (int?)body["generation_config"]!["max_output_tokens"]);
        Assert.Equal("GET", handler.Methods[1]);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("long answer", Assert.IsAssignableFrom<GeneratedText>(Assert.Single(result.Content)).Text);
        Assert.Equal(100_000, result.Usage.OutputTokens);
    }
}
