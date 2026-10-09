// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class ImageInputTests
{
    private const string CatUrl = "https://github.com/vercel/ai/blob/main/examples/ai-functions/data/comic-cat.png?raw=true";

    private static ModelMessage[] Messages(FileContentPart file) => new ModelMessage[]
    {
        new UserModelMessage(new UserContentPart[] { new TextContentPart("Describe the image in detail."), file }),
    };

    private static FileContentPart UrlImage() => new FileContentPart("image/png", CatUrl, null, null);

    private static FileContentPart DataImage() => new FileContentPart("image/png", null, AnthropicFeatureSuite.Fixture("comic-cat.png"), "comic-cat.png");

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_text_with_image_URL_input(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Messages = Messages(UrlImage()),
        });

        Assert.False(string.IsNullOrEmpty(result.Text));
        Assert.Contains("cat", result.Text.ToLowerInvariant());
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_text_with_image_input(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Messages = Messages(DataImage()),
        });

        Assert.Contains("cat", result.Text.ToLowerInvariant());
        Assert.True(result.Usage.TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_stream_text_with_image_URL_input(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = AnthropicFeatureSuite.Client().StreamTextAsync(new StreamTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Messages = Messages(UrlImage()),
        });

        var chunks = new List<string>();
        await foreach (var chunk in result.TextStream())
        {
            chunks.Add(chunk);
        }

        Assert.True(chunks.Count > 0);
        Assert.Contains("cat", string.Concat(chunks).ToLowerInvariant());
        Assert.True((await result.Usage).TotalTokens > 0);
    }

    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_stream_text_with_image_input(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = AnthropicFeatureSuite.Client().StreamTextAsync(new StreamTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Messages = Messages(DataImage()),
        });

        var chunks = new List<string>();
        await foreach (var chunk in result.TextStream())
        {
            chunks.Add(chunk);
        }

        Assert.Contains("cat", string.Concat(chunks).ToLowerInvariant());
        Assert.True(chunks.Count > 0);
        Assert.True((await result.Usage).TotalTokens > 0);
    }
}
