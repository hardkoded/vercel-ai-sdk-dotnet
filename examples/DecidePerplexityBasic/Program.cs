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

#region decide-perplexity-basic
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = PerplexityProvider.Create().DecisionModel("pplx-decider-latest"),
    State = DecisionState.FromJson(JsonDocument.Parse("""{ "review": "Great sound, but the battery stopped charging." }""").RootElement),
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["defect"] = new("boolean", "Does the review report a product defect?"),
        ["sentiment"] = new(
            "choice",
            "What is the overall sentiment?",
            new DecisionChoiceCriteria(new Dictionary<string, DecisionInput?>
            {
                ["positive"] = "Mostly satisfied",
                ["mixed"] = "Praise and complaints",
                ["negative"] = "Mostly dissatisfied",
            })),
        ["severity"] = new(
            "score",
            "How severe is the problem?",
            new DecisionScoreCriteria(new DecisionInput?[] { "Cosmetic", "Inconvenient", "Product unusable" })),
    },
});
#endregion

foreach (var answer in result.Answers)
{
    Console.WriteLine($"{answer.Key}: {answer.Value.Type} choice={answer.Value.Choice} score={answer.Value.Score} probability={answer.Value.Probability}");
}

Console.WriteLine("Usage: " + result.Usage.InputTokens + " in, " + result.Usage.OutputTokens + " out");
Console.WriteLine("Model: " + result.Response.ModelId);
Console.WriteLine("Provider metadata: " + result.ProviderMetadata);
