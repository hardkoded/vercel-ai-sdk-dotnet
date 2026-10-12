// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Anthropic;
using Vercel.AI.Gateway;

var gatewayKey = Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY");
var anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
if (string.IsNullOrEmpty(gatewayKey) && string.IsNullOrEmpty(anthropicKey))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY, ANTHROPIC_API_KEY, or both to run this example.");
    return;
}

if (!string.IsNullOrEmpty(gatewayKey))
{
    #region gateway-model
    var client = new AiClient(GatewayProvider.Create());

    var result = await client.GenerateTextAsync(new GenerateTextOptions
    {
        ModelId = "anthropic/claude-opus-5.5", // or "openai/gpt-6-astra", "google/gemini-3.8-flash", etc.
        Prompt = "Hello!",
    });
    #endregion

    Console.WriteLine("AI Gateway: " + result.Text);
}

if (!string.IsNullOrEmpty(anthropicKey))
{
    #region provider-model
    var client = new AiClient(GatewayProvider.Create());
    var anthropic = AnthropicProvider.Create(); // or OpenAIProvider.Create(), GoogleProvider.Create(), etc.

    var result = await client.GenerateTextAsync(new GenerateTextOptions
    {
        Model = anthropic.LanguageModel("claude-opus-5-5"),
        Prompt = "Hello!",
    });
    #endregion

    Console.WriteLine("Anthropic: " + result.Text);
}
