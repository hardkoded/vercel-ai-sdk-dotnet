// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Builds Google and Vertex models over a scripted SSE body for the streamed tool order tests.</summary>
internal static class GoogleStreamOrderSupport
{
    public const string Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5";

    public static readonly string[] Providers = ["google.generative-ai", "google.vertex.chat"];

    public static string MetadataKey(string provider) =>
        provider == "google.vertex.chat" ? "vertex" : "google";

    public static ILanguageModel Model(string provider, RecordingHandler handler)
    {
        if (provider == "google.vertex.chat")
        {
            return GoogleVertexProvider
                .Create(
                    new VertexOptions
                    {
                        Project = "test-project",
                        Region = "us-central1",
                        ApiKey = "test-api-key",
                        GenerateId = () => "generated-call-id",
                    },
                    handler
                )
                .LanguageModel("gemini-2.5-flash");
        }

        return GoogleProvider
            .Create(
                new GoogleOptions
                {
                    ApiKey = "test-api-key",
                    GenerateId = () => "generated-call-id",
                },
                handler
            )
            .LanguageModel("gemini-2.5-flash");
    }

    /// <summary>One SSE event per group of parts. Only the last one carries the finish reason.</summary>
    public static string Sse(IReadOnlyList<JsonArray> groups) =>
        string.Concat(
            groups.Select(
                (parts, index) =>
                {
                    var candidate = new JsonObject
                    {
                        ["content"] = new JsonObject { ["role"] = "model", ["parts"] = parts },
                        ["index"] = 0,
                    };
                    if (index == groups.Count - 1)
                    {
                        candidate["finishReason"] = "STOP";
                    }

                    return "data: "
                        + new JsonObject
                        {
                            ["candidates"] = new JsonArray(candidate),
                        }.ToJsonString()
                        + "\n\n";
                }
            )
        );

    public static JsonArray Parts(params string[] parts) =>
        new(parts.Select(part => JsonNode.Parse(part)).ToArray());

    public static RecordingHandler Stream(IReadOnlyList<JsonArray> groups) =>
        new() { ResponseText = Sse(groups), MediaType = "text/event-stream" };
}
