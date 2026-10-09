// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class PdfInputTests
{
    [SkippableTheory]
    [MemberData(nameof(AnthropicFeatureSuite.LanguageModels), MemberType = typeof(AnthropicFeatureSuite))]
    public async Task Should_generate_text_with_PDF_input(string modelId)
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var result = await AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat(modelId),
            Messages = new ModelMessage[]
            {
                new UserModelMessage(new UserContentPart[]
                {
                    new TextContentPart("Summarize the contents of this PDF."),
                    new FileContentPart("application/pdf", null, AnthropicFeatureSuite.Fixture("ai.pdf"), "ai.pdf"),
                }),
            },
        });

        Assert.False(string.IsNullOrEmpty(result.Text));
        Assert.Contains("embedding", result.Text.ToLowerInvariant());
        Assert.True(result.Usage.TotalTokens > 0);
    }
}
