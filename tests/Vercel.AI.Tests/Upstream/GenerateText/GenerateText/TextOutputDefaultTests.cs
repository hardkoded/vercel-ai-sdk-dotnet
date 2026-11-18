// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Testing;

namespace Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

/// <summary>Port of <c>generate-text.test.ts</c> &gt; <c>generateText &gt; options.output &gt; text output (default)</c>.</summary>
public sealed class TextOutputDefaultTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/generate-text/generate-text.test.ts::generateText > options.output > text output (default)::should throw error when accessing output",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_error_when_accessing_output()
    {
        var result = await new AiClient(
            Vercel.AI.Gateway.GatewayProvider.Create(
                new Vercel.AI.Gateway.GatewayOptions { ApiKey = "test" }
            )
        ).GenerateTextAsync(
            new GenerateTextOptions
            {
                Model = new TestLanguageModel
                {
                    OnGenerate = _ => new LanguageModelGenerateResult(
                        new GeneratedContent[] { new GeneratedText("Hello, world!") },
                        FinishReason.Stop,
                        new LanguageModelUsage(3, 10, 13)
                    ),
                },
                Prompt = "prompt",
            }
        );

        Assert.Equal("Hello, world!", result.Output!.Value.GetString());
    }
}
