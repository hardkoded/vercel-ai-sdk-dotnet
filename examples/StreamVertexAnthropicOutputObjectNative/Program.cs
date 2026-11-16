// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Google;

if (
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT"))
    || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_ACCESS_TOKEN"))
)
{
    Console.WriteLine(
        "Set GOOGLE_VERTEX_PROJECT and GOOGLE_VERTEX_ACCESS_TOKEN to run this example."
    );
    return;
}

// stream-text/google/vertex-anthropic-output-object-native.ts
#region stream-vertex-anthropic-output-object-native
// Configure Google Cloud's structured_outputs policy for this model first.
var vertexAnthropic = GoogleVertexAnthropicProvider.Create(
    new GoogleVertexAnthropicOptions
    {
        Project = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT"),
        Location = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_LOCATION") ?? "global",
        GenerateAuthToken = _ =>
            Task.FromResult(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_ACCESS_TOKEN")!),
        SupportsNativeStructuredOutput = true,
    }
);

var result = Ai.StreamTextAsync(
    new StreamTextOptions
    {
        Model = vertexAnthropic.LanguageModel("claude-sonnet-4-6"),
        Output = OutputSpec.Object(
            """
            {
              "type": "object",
              "properties": {
                "name": { "type": "string" },
                "ingredients": { "type": "array", "items": { "type": "string" } },
                "steps": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["name", "ingredients", "steps"],
              "additionalProperties": false
            }
            """
        ),
        Prompt = "Create a vegetarian lasagna recipe.",
    }
);

// Upstream logs result.partialOutputStream here. The port has no partial output stream yet, so this prints the JSON text deltas.
await foreach (var delta in result.TextStream())
{
    Console.Write(delta);
}

Console.WriteLine();
Console.WriteLine("Recipe: " + await result.Output);
#endregion
