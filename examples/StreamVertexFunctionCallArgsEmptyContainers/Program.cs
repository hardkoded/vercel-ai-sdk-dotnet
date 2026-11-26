// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI;
using Vercel.AI.Google;
using Vercel.AI.Provider;

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

// reproduction/google-vertex-stream-function-call-args-empty-containers.ts
// https://github.com/vercel/ai/issues/23760
#region stream-vertex-function-call-args-empty-containers
var vertex = GoogleVertexProvider.Create(
    new VertexOptions
    {
        Project = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_PROJECT")!,
        Region = Environment.GetEnvironmentVariable("GOOGLE_VERTEX_LOCATION") ?? "us-central1",
        GenerateAuthToken = _ =>
            Task.FromResult(Environment.GetEnvironmentVariable("GOOGLE_VERTEX_ACCESS_TOKEN")!),
    }
);

foreach (var streamFunctionCallArguments in new[] { true, false })
{
    var result = Ai.StreamTextAsync(
        new StreamTextOptions
        {
            Model = vertex.LanguageModel("gemini-3.1-pro-preview"),
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["vertex"] = JsonDocument
                    .Parse(
                        $$"""{ "streamFunctionCallArguments": {{(streamFunctionCallArguments ? "true" : "false")}} }"""
                    )
                    .RootElement.Clone(),
            },
            Tools = new[]
            {
                Tool.Function(
                    "t",
                    "",
                    """
                    {
                      "type": "object",
                      "properties": {
                        "id": { "type": "string" },
                        "emptyArr": { "type": "array", "items": { "type": "string" } },
                        "emptyObj": { "type": "object", "properties": { "a": { "type": "string" } } },
                        "emptyStr": { "type": "string" },
                        "nested": {
                          "type": "object",
                          "properties": { "tags": { "type": "array", "items": { "type": "string" } } },
                          "required": ["tags"]
                        },
                        "zero": { "type": "number" },
                        "no": { "type": "boolean" }
                      },
                      "required": ["id"]
                    }
                    """,
                    (arguments, cancellationToken) => Task.FromResult("""{ "ok": true }""")
                ),
            },
            ToolChoice = ToolChoice.Required,
            StopWhen = StopWhen.IsStepCount(1),
            Prompt =
                "Call t with exactly: id \"x\", emptyArr [], emptyObj {}, emptyStr \"\", nested {\"tags\": []}, zero 0, no false.",
            Include = new StreamTextInclude { RawChunks = true },
        }
    );

    await foreach (var part in result.Stream())
    {
        if (part is RawPart raw && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAW")))
        {
            Console.WriteLine("[raw] " + raw.RawValue.GetRawText());
        }

        if (part is ToolCallPart toolCall)
        {
            Console.WriteLine(
                $"streamFunctionCallArguments={streamFunctionCallArguments.ToString().ToLowerInvariant()} "
                    + toolCall.ToolCall.ArgumentsJson
            );
        }
    }
}
#endregion
