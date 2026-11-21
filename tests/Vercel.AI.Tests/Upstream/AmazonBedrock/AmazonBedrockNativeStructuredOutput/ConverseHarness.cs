// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.AmazonBedrockNativeStructuredOutput;

internal static class ConverseHarness
{
    internal const string Schema =
        "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    internal const string OpenObject = "{\"type\":\"object\",\"additionalProperties\":true}";

    internal const string RecursiveReference =
        "{\"type\":\"object\",\"properties\":{\"child\":{\"$ref\":\"#\"}},\"required\":[\"child\"],\"additionalProperties\":false}";

    internal const string Response =
        "{\"output\":{\"message\":{\"role\":\"assistant\",\"content\":[{\"text\":\"{\\\"answer\\\":\\\"42\\\"}\"}]}},\"usage\":{\"inputTokens\":10,\"outputTokens\":20,\"totalTokens\":30},\"stopReason\":\"end_turn\"}";

    internal static string ManyOptionalProperties()
    {
        var properties = string.Join(
            ",",
            Enumerable.Range(0, 11).Select(i => $"\"f{i}\":{{\"type\":\"string\"}}")
        );
        return "{\"type\":\"object\",\"properties\":{"
            + properties
            + "},\"additionalProperties\":false}";
    }

    internal static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    internal static LanguageModelCallOptions Call(string schema)
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            JsonSchema = Json(schema),
        };
    }

    internal static async Task<JsonNode> Send(
        string modelId,
        LanguageModelCallOptions options,
        string method = "doGenerate"
    )
    {
        var handler = new UpstreamRecordingHandler(method == "doStream" ? "\0" : Response);
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(
            new AmazonBedrockOptions
            {
                AccessKeyId = "test-access-key",
                SecretAccessKey = "test-secret",
                UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            },
            handler
        );
        var model = provider.LanguageModel(modelId);
        if (method == "doStream")
        {
            var stream = await model.DoStreamAsync(options, CancellationToken.None);
            await using var enumerator = stream.Stream.GetAsyncEnumerator();
        }
        else
        {
            await model.DoGenerateAsync(options, CancellationToken.None);
        }

        return JsonNode.Parse(handler.Body)!;
    }
}
