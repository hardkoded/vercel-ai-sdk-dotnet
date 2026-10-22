// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText;

/// <summary>
/// Port of <c>generate-text.test.ts</c> &gt; <c>generateText</c>, the interrupted finish reason. Not ported: <c>should stop an interrupted response with a deferred provider tool result</c>,
/// because the port has no provider tool that defers its result.
/// </summary>
public sealed class GenerateTextInterruptedTests
{
    private const string Prefix = "packages/ai/src/generate-text/generate-text.test.ts::generateText::";

    public static IEnumerable<object[]> ToolChoices() =>
    [
        [ToolChoice.Required],
        [ToolChoice.Tool("tool1")],
    ];

    [Theory]
    [MemberData(nameof(ToolChoices))]
    [UpstreamTest(Prefix + "should preserve interrupted output without enforcing tool choice %j", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_preserve_interrupted_output_without_enforcing_tool_choice(ToolChoice toolChoice)
    {
        var model = new TestLanguageModel
        {
            OnGenerate = _ => new LanguageModelGenerateResult(
                new GeneratedContent[] { new GeneratedText("Let me check") },
                FinishReason.Interrupted,
                new LanguageModelUsage(3, 10, 13),
                null),
        };

        var result = await new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "test-input",
            Tools = new[] { Tool.Function("tool1", null, "{\"type\":\"object\",\"properties\":{}}", null) },
            ToolChoice = toolChoice,
        });

        Assert.Equal("Let me check", result.Text);
        Assert.Equal(FinishReason.Interrupted, result.FinishReason);
        Assert.Null(result.FinalStep.RawFinishReason);
    }
}
