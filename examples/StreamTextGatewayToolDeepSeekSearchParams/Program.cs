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

var stream = client.StreamTextAsync(
    new StreamTextOptions
    {
        ModelId = "openai/gpt-6-luna",
        Prompt = "Search the web for DeepSeek API documentation and summarize the results.",
        Tools = new[]
        {
            GatewayDeepSeekSearch.Create(
                new GatewayDeepSeekSearchConfig
                {
                    Query = "DeepSeek API documentation site:api-docs.deepseek.com",
                    Count = 3,
                    NeedContent = true,
                    OriQuestion = "How do DeepSeek Search and Fetch work?",
                }
            ),
        },
    }
);

await foreach (var part in stream.Stream())
{
    switch (part)
    {
        case TextDeltaPart delta:
            Console.Write(delta.Text);
            break;
        case ToolCallPart call:
            Console.WriteLine($"Tool call: {call.ToolCall.ToolName} {call.ToolCall.ArgumentsJson}");
            break;
        case ToolResultPart result:
            Console.WriteLine("Tool result: " + result.Result.OutputJson);
            break;
        case ToolErrorPart error:
            Console.Error.WriteLine("Tool error: " + error.Result.OutputJson);
            break;
    }
}

Console.WriteLine();
Console.WriteLine("Token usage: " + await stream.Usage);
Console.WriteLine("Finish reason: " + await stream.FinishReason);
