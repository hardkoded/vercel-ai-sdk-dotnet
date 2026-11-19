// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Liquid;
using Vercel.AI.Operations;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LIQUID_API_KEY")))
{
    Console.WriteLine("Set LIQUID_API_KEY to run this example.");
    return;
}

#region decide-liquid-basic
var result = await Decide.DecideAsync(
    new DecideRequest
    {
        Model = LiquidProvider.Create().DecisionModel("d1.2"),
        State =
            "Our production API is returning 500 errors on every request. All customers are affected and we are losing revenue.",
        Questions = new Dictionary<string, DecisionQuestion>
        {
            ["team"] = new(
                "choice",
                "Which team should handle this ticket?",
                new DecisionChoiceCriteria(
                    new Dictionary<string, DecisionInput?>
                    {
                        ["billing"] = "Charges, invoices, refunds, or payment methods",
                        ["engineering"] = "Bugs, outages, and technical malfunctions",
                        ["account"] = "Sign-in, permissions, or plan changes",
                    }
                )
            ),
            ["outage"] = new("boolean", "Is the customer reporting a service outage?"),
            ["urgency"] = new(
                "score",
                "How urgent is this ticket?",
                new DecisionScoreCriteria([
                    "Can wait: no immediate business impact",
                    "Should handle today: minor or non-blocking issue",
                    "Time-sensitive: noticeable customer impact",
                    "Blocking revenue: critical failure affecting customers",
                ])
            ),
        },
    }
);
#endregion

Console.WriteLine("Answers: " + JsonSerializer.Serialize(result.Answers));
Console.WriteLine(
    "Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out"
);
Console.WriteLine("Model: " + result.Response?.ModelId);
Console.WriteLine("Provider metadata: " + result.ProviderMetadata);
