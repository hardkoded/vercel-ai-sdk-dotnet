// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Parallel;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PARALLEL_API_KEY")))
{
    Console.WriteLine("Set PARALLEL_API_KEY to run this example.");
    return;
}

#region decide-parallel-basic
var result = await Decide.DecideAsync(
    new DecideRequest
    {
        Model = ParallelProvider.Default.DecisionModel("fast"),
        State = DecisionState.FromJson(
            JsonDocument
                .Parse("""{"company":"Parallel","website":"https://parallel.ai"}""")
                .RootElement
        ),
        Questions = new Dictionary<string, DecisionQuestion>
        {
            ["hasSearch"] = new("boolean", "Does this company offer a web search API?"),
        },
    }
);
#endregion

foreach (var answer in result.Answers)
{
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} probability={answer.Value.Probability}");
}

Console.WriteLine(result.ProviderMetadata);
Console.WriteLine(
    "Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out"
);
