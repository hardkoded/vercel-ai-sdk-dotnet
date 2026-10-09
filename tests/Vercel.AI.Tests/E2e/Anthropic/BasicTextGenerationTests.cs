// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class BasicTextGenerationTests
{
    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_text(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Prompt = "Write a haiku about programming.",
        });

        Assert.False(string.IsNullOrEmpty(result.Text));
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_text_with_system_prompt(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Messages = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage(new UserContentPart[] { new TextContentPart("Write a haiku about programming.") }),
            },
        });

        Assert.False(string.IsNullOrEmpty(result.Text));
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_stream_text(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = AnthropicFeatureSuite.Client().StreamTextAsync(new StreamTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Prompt = "Count from 1 to 5 slowly.",
        });

        var chunks = new List<string>();
        await foreach (var chunk in result.TextStream())
        {
            chunks.Add(chunk);
        }

        Assert.True(chunks.Count > 0);
        Assert.True((await result.Usage).TotalTokens > 0);
    }
}
