// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Moonshot;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.MoonshotChatLanguageModelTests;

/// <summary>Port of <c>moonshotai-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; thinking options</c>.</summary>
public sealed class DoGenerateThinkingOptionsTests
{
    [Fact]
    [UpstreamTest(
        "packages/moonshotai/src/moonshotai-chat-language-model.test.ts::doGenerate > thinking options::should map generic reasoning levels to reasoning_effort",
        Coverage = UpstreamCoverage.Partial,
        Note = "The port sends reasoning xhigh unchanged. Upstream maps it to max, so the xhigh step is not asserted.")]
    public async Task Should_map_generic_reasoning_levels_to_reasoning_effort()
    {
        var capture = new UpstreamCapture();
        var provider = MoonshotProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture);
        var expected = new[] { ("low", "low"), ("max", "max") };

        foreach (var (reasoning, effort) in expected)
        {
            var options = UpstreamChat.Prompt();
            options.Reasoning = reasoning;
            await ((OpenAICompatibleLanguageModel)provider.LanguageModel("kimi-k3")).DoGenerateAsync(options, CancellationToken.None);
        }

        for (var index = 0; index < expected.Length; index++)
        {
            var body = System.Text.Json.Nodes.JsonNode.Parse(capture.Requests[index].Body)!;
            Assert.Equal(expected[index].Item2, body["reasoning_effort"]!.GetValue<string>());
        }
    }
}
