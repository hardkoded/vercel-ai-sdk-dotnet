// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.E2e.Anthropic;

[Trait("Category", AnthropicFeatureSuite.Category)]
public sealed class ChatModelErrorHandlingTests
{
    private static void ErrorValidator(ApiException error)
    {
        Assert.Matches(new Regex("model:", RegexOptions.IgnoreCase), error.Message);
    }

    [SkippableFact]
    public async Task Should_throw_error_on_generate_text_attempt_with_invalid_model_ID()
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var error = await Assert.ThrowsAnyAsync<ApiException>(() => AnthropicFeatureSuite.Client().GenerateTextAsync(new GenerateTextOptions
        {
            Model = AnthropicFeatureSuite.Chat("no-such-model"),
            Prompt = "This should fail",
        }));

        ErrorValidator(error);
    }

    [SkippableFact]
    public async Task Should_throw_error_on_stream_text_attempt_with_invalid_model_ID()
    {
        AnthropicFeatureSuite.SkipWithoutApiKey();
        var error = await Assert.ThrowsAnyAsync<ApiException>(async () =>
        {
            var result = AnthropicFeatureSuite.Client().StreamTextAsync(new StreamTextOptions
            {
                Model = AnthropicFeatureSuite.Chat("no-such-model"),
                Prompt = "This should fail",
            });

            await foreach (var _ in result.TextStream())
            {
            }
        });

        ErrorValidator(error);
    }
}
