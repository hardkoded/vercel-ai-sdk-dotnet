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

#region decide-scaledown-mixed-questions
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = ScaleDownProvider.Default.DecisionModel("classify-1"),
    State = "The export button crashes in Safari. It works in Chrome.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["category"] = new(
            "choice",
            null,
            new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
            {
                ["bug"] = "Broken functionality",
                ["feedback"] = "Product suggestions",
            })),
        ["hasWorkaround"] = new("boolean", "Can the user work around this issue?"),
        ["severity"] = new(
            "score",
            "How severe is the reported issue?",
            new DecisionScoreCriteria(new DecisionInput?[] { "Cosmetic", "Degraded, with a workaround", "Blocking" })),
    },
});
#endregion

foreach (var answer in result.Answers)
{
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} choice={answer.Value.Choice} score={answer.Value.Score} probability={answer.Value.Probability}");
}

Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
Console.WriteLine("Provider metadata: " + result.ProviderMetadata);
