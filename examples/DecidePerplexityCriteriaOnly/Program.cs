// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Perplexity;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY")))
{
    Console.WriteLine("Set PERPLEXITY_API_KEY to run this example.");
    return;
}

#region decide-perplexity-criteria-only
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = PerplexityProvider.Create().DecisionModel("pplx-decider-latest"),
    State = "I was charged twice. Please refund the extra payment.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        // Instructions are omitted. The criteria describe the decision.
        ["team"] = new(
            "choice",
            null,
            new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
            {
                ["billing"] = DecisionInput.Parse("""{ "includes": ["Charges", "Refunds"] }"""),
                ["support"] = DecisionInput.Parse("""["Technical problems"]"""),
                ["other"] = null,
            })),
        // Instructions are an explicit null. Without instructions, at least one boolean criterion must be non-null.
        ["refund"] = new DecisionQuestion(
            "boolean",
            null,
            new DecisionBooleanCriteria
            {
                True = "The customer asks for money back",
                False = "The customer only asks for information",
            })
        { HasInstructions = true },
    },
});
#endregion

Console.WriteLine("Team: " + result.Answers["team"].Choice);
Console.WriteLine("Team probabilities: " + JsonSerializer.Serialize(result.Answers["team"].Probabilities));
Console.WriteLine("Refund probability: " + result.Answers["refund"].Probability);
Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
