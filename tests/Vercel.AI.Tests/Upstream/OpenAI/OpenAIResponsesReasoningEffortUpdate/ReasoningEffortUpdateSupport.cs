// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesReasoningEffortUpdate;

/// <summary>Shared request helpers for the ports of <c>openai-responses-reasoning-effort-update.test.ts</c>.</summary>
internal static class ReasoningEffortUpdateSupport
{
    internal const string WireUser = "{\"role\":\"user\",\"content\":[{\"type\":\"input_text\",\"text\":\"Question\"}]}";

    internal static string WireUpdate(string effort) =>
        "{\"type\":\"configuration_update\",\"reasoning\":{\"effort\":\"" + effort + "\"}}";

    internal static UserModelMessage User() => new("Question");

    internal static SystemModelMessage Update(string effort, string content = "") =>
        new(content, new Dictionary<string, JsonElement>
        {
            ["openai"] = OpenAIUpstream.Json("{\"reasoningEffortUpdate\":\"" + effort + "\"}"),
        });

    internal static List<ModelMessage> Build(IEnumerable<string> kinds)
    {
        var prompt = new List<ModelMessage>();
        foreach (var kind in kinds)
        {
            prompt.Add(kind == "user" ? User() : Update(kind));
        }

        return prompt;
    }

    internal static async Task<(JsonNode Body, IReadOnlyList<CallWarning> Warnings)> Request(
        string method,
        IReadOnlyList<ModelMessage> prompt,
        JsonObject options,
        string modelId,
        double? temperature = null,
        double? topP = null,
        OpenAICapture? capture = null)
    {
        const string response = "{\"id\":\"resp_test\",\"created_at\":0,\"model\":\"m\",\"output\":[]}";
        capture ??= new OpenAICapture();
        if (method == "generate")
        {
            capture.ResponseJson = response;
        }
        else
        {
            capture.ServerSentEvents = "data: {\"type\":\"response.completed\",\"response\":" + response + "}\n\ndata: [DONE]\n\n";
        }

        var callOptions = new LanguageModelCallOptions
        {
            Prompt = prompt,
            Temperature = temperature,
            TopP = topP,
            ProviderOptions = new Dictionary<string, JsonElement> { ["openai"] = OpenAIUpstream.Json(options.ToJsonString()) },
        };
        var model = OpenAIUpstream.Provider(capture).ResponsesModel(modelId);
        IReadOnlyList<CallWarning> warnings;
        if (method == "generate")
        {
            warnings = (await model.DoGenerateAsync(callOptions, CancellationToken.None)).Warnings;
        }
        else
        {
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in model.DoStreamAsync(callOptions, CancellationToken.None))
            {
                parts.Add(part);
            }

            Assert.Empty(parts.OfType<ErrorStreamPart>());
            warnings = parts.OfType<StreamStartStreamPart>().First().Warnings;
        }

        var body = JsonNode.Parse(capture.Body)!;
        Assert.Equal(method == "stream" ? true : (bool?)null, body["stream"]?.GetValue<bool>());
        return (body, warnings);
    }
}
