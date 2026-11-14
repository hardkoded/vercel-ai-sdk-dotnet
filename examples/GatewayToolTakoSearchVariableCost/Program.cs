// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI;
using Vercel.AI.Gateway;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY to run this example.");
    return;
}

var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-5.4-nano",
    Prompt = "How much monthly web traffic does vercel.com get?",
    Tools = new[]
    {
        GatewayTakoSearch.Create(new GatewayTakoSearchConfig
        {
            // Keep the data index in every search so variable-cost cards can appear.
            Sources = new TakoSearchSourcesConfig
            {
                Data = new TakoDataSourceConfig { Count = 5 },
                Web = new TakoWebSourceConfig { Count = 3 },
            },
            MaxVariableCostUsd = 0.1,
        }),
    },
    StopWhen = StopWhen.IsStepCount(4),
});

Console.WriteLine("Text: " + result.Text);
Console.WriteLine();

foreach (var toolResult in result.ToolResults)
{
    if (toolResult.IsError || toolResult.ToolName != "tako_search")
    {
        continue;
    }

    var output = JsonSerializer.Deserialize<TakoSearchResponse>(toolResult.OutputJson);
    if (output == null)
    {
        continue;
    }

    Console.WriteLine("Total cost (USD): " + output.Usage?.TotalCostUsd);
    Console.WriteLine("Variable cost (USD): " + output.Usage?.VariableCost?.CostUsd);

    foreach (var card in output.Cards ?? [])
    {
        if (card.Placeholder is { } placeholder)
        {
            Console.WriteLine($"[placeholder:{placeholder.Reason ?? "not_served"}] {card.Title} ({placeholder.WithheldSource})");
            continue;
        }

        if (card.Content?.ExportPricing?.VariableCostMaxUsd is { } maxCost)
        {
            Console.WriteLine($"{card.Title}: variable cost up to ${maxCost}");
        }
    }
}

Console.WriteLine();
Console.WriteLine("Token usage: " + result.Usage);
Console.WriteLine("Finish reason: " + result.FinishReason);
