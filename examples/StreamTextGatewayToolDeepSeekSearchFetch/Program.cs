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
        Prompt =
            "Search for DeepSeek API documentation, fetch one relevant page, and summarize it.",
        Tools = new[]
        {
            GatewayDeepSeekSearch.Create(
                new GatewayDeepSeekSearchConfig { Count = 3, NeedContent = true }
            ),
            GatewayDeepSeekFetch.Create(),
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
