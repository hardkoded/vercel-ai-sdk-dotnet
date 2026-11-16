// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Google;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT")) || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_ACCESS_TOKEN")))
{
    Console.WriteLine("Set GOOGLE_VERTEX_PROJECT and GOOGLE_VERTEX_ACCESS_TOKEN to run this example.");
    return;
}

// generate-text/google/vertex-anthropic-output-object-native.ts
#region generate-vertex-anthropic-output-object-native
// Configure Google Cloud's structured_outputs policy for this model first.
var vertexAnthropic = GoogleVertexAnthropicProvider.Create(new GoogleVertexAnthropicOptions
{
    Project = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT"),
    Location = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_LOCATION") ?? "global",
    GenerateAuthToken = _ => Task.FromResult(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_ACCESS_TOKEN")!),
    SupportsNativeStructuredOutput = true,
});

var result = await Ai.GenerateTextAsync(new GenerateTextOptions
{
    Model = vertexAnthropic.LanguageModel("claude-sonnet-4-6"),
    Output = OutputSpec.Object("""
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
        """),
    Prompt = "Create a vegetarian lasagna recipe.",
});

Console.WriteLine("Recipe: " + result.Output);
Console.WriteLine("Token usage: " + result.Usage);
#endregion
