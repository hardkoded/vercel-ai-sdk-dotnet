// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Perplexity;

namespace Vercel.AI.Tests.PerplexityLanguageModelTests;

/// <summary>Port of <c>perplexity-language-model.test.ts</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string AgentResponse =
        "{\"id\":\"resp-123\",\"created_at\":1784292159,\"model\":\"openai/gpt-5.1\",\"object\":\"response\",\"output\":[{\"id\":\"msg-123\",\"type\":\"message\",\"content\":[{\"type\":\"output_text\",\"text\":\"Hello from Perplexity.\",\"annotations\":[]}]}],\"status\":\"completed\",\"usage\":{\"input_tokens\":1,\"output_tokens\":1,\"total_tokens\":2}}";

    [Fact]
    [UpstreamTest("packages/perplexity/src/perplexity-language-model.test.ts::doGenerate::coerces top-level max reasoning to xhigh with a warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Coerces_top_level_max_reasoning_to_xhigh_with_a_warning()
    {
        var capture = new UpstreamCapture { ResponseBody = AgentResponse };
        var model = PerplexityProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("low");
        var options = UpstreamChat.Prompt();
        options.Reasoning = "max";

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        Assert.Equal("xhigh", UpstreamChat.Body(capture)["reasoning"]!["effort"]!.GetValue<string>());
        Assert.Contains(result.Warnings, warning => warning.Type == "compatibility" && warning.Message == "reasoning \"max\" is not directly supported by this model. mapped to effort \"xhigh\".");
    }
}
