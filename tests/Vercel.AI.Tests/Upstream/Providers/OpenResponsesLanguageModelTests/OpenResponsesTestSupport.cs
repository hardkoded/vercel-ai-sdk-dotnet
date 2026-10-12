// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenResponses;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

internal static class OpenResponsesTestSupport
{
    public static OpenResponsesLanguageModel CreateModel(UpstreamCapture capture, string modelId = "gemma-7b-it")
    {
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(new OpenResponsesOptions { ApiKey = "test-key", BaseUrl = "https://example.test/v1" }, capture).LanguageModel(modelId);
    }

    public static string Fixture(string name)
    {
        return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name + ".json"));
    }

    public static string OutputResponse(string outputJson)
    {
        return "{\"id\":\"resp_1\",\"object\":\"response\",\"created_at\":0,\"model\":\"test-model\",\"status\":\"completed\",\"output\":" + outputJson + ",\"usage\":{\"input_tokens\":0,\"output_tokens\":0,\"total_tokens\":0}}";
    }

    public static LanguageModelCallOptions Replay(IReadOnlyList<GeneratedContent> content)
    {
        var parts = new List<AssistantContentPart>();
        foreach (var part in content)
        {
            switch (part)
            {
                case GeneratedReasoning reasoning:
                    parts.Add(new AssistantReasoningPart(reasoning.Text, Options(reasoning.ProviderMetadata)));
                    break;
                case GeneratedText text:
                    parts.Add(new AssistantTextPart(text.Text, Options(text.ProviderMetadata)));
                    break;
                case GeneratedToolCall call:
                    parts.Add(new AssistantToolCallPart(call));
                    break;
            }
        }

        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new AssistantModelMessage(parts) } };
    }

    public static JsonNode? RequestInput(UpstreamCapture capture, int index)
    {
        return JsonNode.Parse(capture.Requests[index].Body)!["input"];
    }

    public static bool JsonEquals(string expectedJson, JsonNode? actual)
    {
        return JsonNode.DeepEquals(JsonNode.Parse(expectedJson), actual);
    }

    private static IReadOnlyDictionary<string, JsonElement>? Options(JsonElement? metadata)
    {
        if (metadata is not { ValueKind: JsonValueKind.Object } root)
        {
            return null;
        }

        var options = new Dictionary<string, JsonElement>();
        foreach (var provider in root.EnumerateObject())
        {
            options[provider.Name] = provider.Value.Clone();
        }

        return options;
    }
}
