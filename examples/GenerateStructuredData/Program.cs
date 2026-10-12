// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI;
using Vercel.AI.Gateway;

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI_GATEWAY_API_KEY")))
{
    Console.WriteLine("Set AI_GATEWAY_API_KEY to run this example.");
    return;
}

#region generate-structured-data
var client = new AiClient(GatewayProvider.Create());

var result = await client.GenerateTextAsync(new GenerateTextOptions
{
    ModelId = "openai/gpt-6-astra",
    Output = OutputSpec.Object("""
        {
          "type": "object",
          "properties": {
            "recipe": {
              "type": "object",
              "properties": {
                "name": { "type": "string" },
                "ingredients": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" },
                      "amount": { "type": "string" }
                    },
                    "required": ["name", "amount"],
                    "additionalProperties": false
                  }
                },
                "steps": { "type": "array", "items": { "type": "string" } }
              },
              "required": ["name", "ingredients", "steps"],
              "additionalProperties": false
            }
          },
          "required": ["recipe"],
          "additionalProperties": false
        }
        """),
    Prompt = "Generate a lasagna recipe.",
});

Console.WriteLine(result.Output);
#endregion
