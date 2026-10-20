// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Alibaba.AlibabaChatLanguageModelTests;

/// <summary>Port of <c>alibaba-chat-language-model.test.ts</c> &gt; <c>doGenerate &gt; reasoning</c>.</summary>
public sealed class DoGenerateReasoningTests
{
    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-chat-language-model.test.ts::doGenerate > reasoning::should extract reasoning content", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_extract_reasoning_content()
    {
        var body = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "alibaba-reasoning.json"));
        var message = JsonNode.Parse(body)!["choices"]![0]!["message"]!;
        var capture = new UpstreamCapture { ResponseBody = body };
        var model = (OpenAICompatibleLanguageModel)AlibabaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture).LanguageModel("qwen-plus");

        var result = await model.DoGenerateAsync(UpstreamChat.Prompt(), CancellationToken.None);

        Assert.Collection(
            result.Content,
            part => Assert.Equal(message["reasoning_content"]!.GetValue<string>(), Assert.IsType<GeneratedReasoning>(part).Text),
            part => Assert.Equal(message["content"]!.GetValue<string>(), Assert.IsType<GeneratedText>(part).Text));
        Assert.Equal(FinishReason.Stop, result.FinishReason);
    }
}
