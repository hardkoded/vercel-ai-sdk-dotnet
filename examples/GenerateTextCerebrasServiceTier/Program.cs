// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI;
using Vercel.AI.Cerebras;
using Vercel.AI.OpenAICompatible;

// Service tiers require a preview-enabled dedicated Cerebras endpoint.
var baseUrl = Environment.GetEnvironmentVariable("CEREBRAS_BASE_URL");
if (
    string.IsNullOrEmpty(baseUrl)
    || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CEREBRAS_API_KEY"))
)
{
    Console.WriteLine(
        "Set CEREBRAS_API_KEY and CEREBRAS_BASE_URL (your preview-enabled dedicated endpoint) to run this example."
    );
    return;
}

var cerebras = CerebrasProvider.Create(new OpenAICompatibleOptions { BaseUrl = baseUrl });

// generate-text/cerebras/service-tier.ts
var result = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = cerebras.LanguageModel(
            Environment.GetEnvironmentVariable("CEREBRAS_MODEL") ?? "gpt-oss-120b"
        ),
        Prompt = "What color is the sky in one word?",
        // Start at standard priority and fall back to flex when needed.
        // Reject the request if the expected flex queue time exceeds 200ms.
        ProviderOptions = new Dictionary<string, JsonElement>
        {
            ["cerebras"] = JsonDocument
                .Parse("""{"serviceTier":"auto","queueThreshold":200}""")
                .RootElement.Clone(),
        },
    }
);

Console.WriteLine(result.Text);
Console.WriteLine();
Console.WriteLine(
    "serviceTier: " + result.ProviderMetadata?.GetProperty("cerebras").GetProperty("serviceTier")
);
