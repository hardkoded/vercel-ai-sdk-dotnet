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

#region decide-perplexity-single-level-score
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = PerplexityProvider.Create().DecisionModel("pplx-decider-latest"),
    State = "The product stopped working.",
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["severity"] = new("score", null, new DecisionScoreCriteria(new DecisionInput?[] { "Product unusable" })),
    },
});
#endregion

// A single level always has index 0 and probability 1.
// Use two or more levels to distinguish outcomes.
Console.WriteLine("Score: " + result.Answers["severity"].Score); // 0
Console.WriteLine("Probabilities: " + JsonSerializer.Serialize(result.Answers["severity"].Probabilities)); // {"0":1}
Console.WriteLine("Legend: " + result.ProviderMetadata?.GetProperty("perplexity").GetProperty("legend"));
Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
