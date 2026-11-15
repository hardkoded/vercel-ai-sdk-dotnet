// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.ScaleDown;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SCALEDOWN_API_KEY")))
{
    Console.WriteLine("Set SCALEDOWN_API_KEY to run this example.");
    return;
}

#region decide-scaledown-basic
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = ScaleDownProvider.Default.DecisionModel("classify-1"),
    State = "I was charged twice for my subscription. Please refund the duplicate.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["category"] = new(
            "choice",
            "Which team should handle this request?",
            new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
            {
                ["billing"] = "Charges, invoices, and refunds",
                ["technical"] = "Bugs and outages",
                ["account"] = "Login and account settings",
            })),
    },
});
#endregion

foreach (var answer in result.Answers)
{
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} choice={answer.Value.Choice}");
}

Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
