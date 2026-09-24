// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.OpenAI;

namespace Vercel.AI.Tests;

public sealed class IntegrationTests
{
    [SkippableFact]
    public async Task Gateway_generates_text_when_a_key_is_configured()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")));
        var client = new AiClient(GatewayProvider.Create());
        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            ModelId = "openai/gpt-4.1-mini",
            Prompt = "Reply with the single word pong.",
            MaxOutputTokens = 16,
        });
        Assert.False(string.IsNullOrWhiteSpace(result.Text));
    }

    [SkippableFact]
    public async Task OpenAI_generates_text_when_a_key_is_configured()
    {
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
        var model = OpenAIProvider.Create().LanguageModel("gpt-4.1-mini");
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "unused" }));
        var result = await client.GenerateTextAsync(new GenerateTextOptions
        {
            Model = model,
            Prompt = "Reply with the single word pong.",
            MaxOutputTokens = 16,
        });
        Assert.False(string.IsNullOrWhiteSpace(result.Text));
    }
}
