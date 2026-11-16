// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.DeepSeek;
using Vercel.AI.OpenAICompatible;

var apiKey = Environment.GetEnvironmentVariable(DeepSeekProvider.ApiKeyVariable);
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Set DEEPSEEK_API_KEY to run this example.");
    return;
}

var deepSeek = DeepSeekProvider.Create(new OpenAICompatibleOptions { ApiKey = apiKey });

// generate-text/deepseek/responses.ts
#region deepseek-responses-generate
var result = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = deepSeek.ResponsesModel("deepseek-flash"),
        Prompt = "Why is the sky blue? Answer in one sentence.",
        ProviderOptions = new DeepSeekLanguageModelResponsesOptions
        {
            ReasoningEffort = "low",
            UserId = "example_user",
        }.ToProviderOptions(),
    }
);

Console.WriteLine("Content: " + result.Text);
Console.WriteLine("Finish reason: " + result.FinishReason);
#endregion

// generate-text/deepseek/responses-web-search.ts
var search = await Ai.GenerateTextAsync(
    new GenerateTextOptions
    {
        Model = deepSeek.ResponsesModel("deepseek-v4-pro"),
        Prompt = "What happened in AI news this week? Cite your sources.",
        Tools = new[] { DeepSeekTools.WebSearch() },
    }
);

Console.WriteLine("Content: " + search.Text);
Console.WriteLine("Provider metadata: " + search.ProviderMetadata);

// stream-text/deepseek/responses.ts
var stream = Ai.StreamTextAsync(
    new StreamTextOptions
    {
        Model = deepSeek.ResponsesModel("deepseek-flash"),
        Prompt = "Why is the sky blue? Answer in one sentence.",
    }
);

await foreach (var delta in stream.TextStream())
{
    Console.Write(delta);
}

Console.WriteLine();

// stream-text/deepseek/responses-web-search.ts
var searchStream = Ai.StreamTextAsync(
    new StreamTextOptions
    {
        Model = deepSeek.ResponsesModel("deepseek-v4-pro"),
        Prompt = "What happened in AI news this week? Cite your sources.",
        Tools = new[] { DeepSeekTools.WebSearch() },
    }
);

await foreach (var delta in searchStream.TextStream())
{
    Console.Write(delta);
}

Console.WriteLine();
