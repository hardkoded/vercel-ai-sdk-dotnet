// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY to run this example.");
    return;
}

#region agent
var weatherAgent = new Agent(new AgentOptions
{
    ModelId = "openai/gpt-6-astra",
    Instructions = "You are an agent that answers questions about the weather.",
    Tools = new[]
    {
        Tool.Function(
            "weather",
            "Returns the weather in a city.",
            """{ "type": "object", "properties": { "city": { "type": "string" } }, "required": ["city"] }""",
            (arguments, cancellationToken) =>
            {
                var city = arguments.GetProperty("city").GetString();
                return Task.FromResult(JsonSerializer.Serialize(new { city, forecast = "sunny", temperature = 22 }));
            }),
    },
});

var result = await weatherAgent.GenerateAsync("What is the weather in Lisbon?");
Console.WriteLine(result.Text);
#endregion
