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

#region decide-perplexity-web-grounding
var result = await Decide.DecideAsync(new DecideRequest
{
    Model = PerplexityProvider.Create().DecisionModel("pplx-decider-latest"),
    State = DecisionState.FromJson(JsonDocument.Parse("\"https://en.wikipedia.org/wiki/James_Webb_Space_Telescope\"").RootElement),
    Questions = new Dictionary<string, DecisionQuestion>
    {
        ["launched"] = new("boolean", "Did Webb launch in December 2021?"),
    },
    ProviderOptions = JsonDocument.Parse("""{ "perplexity": { "mode": "web_grounding", "maxStaleness": 86400 } }""").RootElement.Clone(),
});
#endregion

foreach (var answer in result.Answers)
{
    Console.WriteLine($"{answer.Key}: probability={answer.Value.Probability}");
}

var metadata = result.ProviderMetadata!.Value.GetProperty("perplexity");
Console.WriteLine("Search results: " + (metadata.TryGetProperty("searchResults", out var search) ? search : "none"));
Console.WriteLine("Fetched pages: " + (metadata.TryGetProperty("fetchedResults", out var fetched) ? fetched : "none"));
Console.WriteLine("Tool calls: " + (metadata.TryGetProperty("toolCalls", out var calls) ? calls : "none"));
