// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Gateway;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY to run this example.");
    return;
}

var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(
    new GenerateTextOptions
    {
        ModelId = "openai/gpt-6-luna",
        Prompt = "Fetch https://example.com and summarize its page content.",
        Tools = new[] { GatewayDeepSeekFetch.Create() },
    }
);

Console.WriteLine("Text: " + result.Text);
foreach (var toolResult in result.ToolResults)
{
    if (toolResult.IsError)
    {
        Console.Error.WriteLine("Tool error: " + toolResult.OutputJson);
    }
}

Console.WriteLine(
    "Tool results: " + string.Join(", ", result.ToolResults.Select(r => r.OutputJson))
);
Console.WriteLine("Token usage: " + result.Usage);
Console.WriteLine("Finish reason: " + result.FinishReason);
