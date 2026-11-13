// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Azure;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.OpenAIResponsesTextFormatUpdate;

/// <summary>Shared request helpers for the port of <c>openai-responses-text-format-update.test.ts</c>.</summary>
internal static class TextFormatUpdateSupport
{
    internal const string Schema = "{\"type\":\"object\",\"properties\":{\"total\":{\"type\":\"integer\"}},\"required\":[\"total\"],\"additionalProperties\":false}";

    internal const string Format = "{\"type\":\"json_schema\",\"name\":\"count\",\"schema\":" + Schema + ",\"strict\":true}";

    internal static UserModelMessage User() => new("Count the crates.");

    /// <summary>A system message that carries message-level OpenAI options, like upstream's <c>update</c>.</summary>
    internal static SystemModelMessage Update(string options, string content = "") =>
        new(content, new Dictionary<string, JsonElement> { ["openai"] = OpenAIUpstream.Json(options) });

    internal static async Task<(JsonNode Body, IReadOnlyList<CallWarning> Warnings)> Request(
        string method,
        LanguageModelCallOptions options,
        OpenAICapture capture,
        string modelId = "gpt-6-sol",
        string provider = "openai.responses")
    {
        const string response = "{\"id\":\"resp_test\",\"created_at\":0,\"model\":\"m\",\"output\":[]}";
        if (method == "generate")
        {
            capture.ResponseJson = response;
        }
        else
        {
            capture.ServerSentEvents = "data: {\"type\":\"response.completed\",\"response\":" + response + "}\n\ndata: [DONE]\n\n";
        }

        var model = provider == "azure.responses"
            ? AzureOpenAIProvider.Create(new AzureOpenAIOptions { ApiKey = "test-api-key", ResourceName = "test-resource" }, capture).ResponsesModel(modelId)
            : OpenAIUpstream.Provider(capture).ResponsesModel(modelId);
        IReadOnlyList<CallWarning> warnings;
        if (method == "generate")
        {
            warnings = (await model.DoGenerateAsync(options, CancellationToken.None)).Warnings;
        }
        else
        {
            var parts = new List<LanguageModelStreamPart>();
            await foreach (var part in (await model.DoStreamAsync(options, CancellationToken.None)))
            {
                parts.Add(part);
            }

            Assert.Empty(parts.OfType<ErrorStreamPart>());
            warnings = parts.OfType<StreamStartStreamPart>().First().Warnings;
        }

        return (JsonNode.Parse(capture.Body)!, warnings);
    }

    internal static LanguageModelCallOptions Options(
        IReadOnlyList<ModelMessage> prompt,
        string? openai = null,
        string provider = "openai",
        string? responseSchema = null,
        double? temperature = null,
        double? topP = null) =>
        new()
        {
            Prompt = prompt,
            Temperature = temperature,
            TopP = topP,
            JsonSchema = responseSchema == null ? null : OpenAIUpstream.Json(responseSchema),
            ProviderOptions = openai == null
                ? null
                : new Dictionary<string, JsonElement> { [provider] = OpenAIUpstream.Json(openai) },
        };
}
