// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.QuiverAI;
using Vercel.AI.Tests.MoreProviders;

namespace Vercel.AI.Tests.QuiverAILanguageModel;

/// <summary>Shared fixtures of the QuiverAI language model tests. Maps to the helpers of <c>quiverai-language-model.test.ts</c>.</summary>
internal static class QuiverAILanguageModelSupport
{
    public const string Url = "https://api.quiver.ai/v1/responses";
    public const string CustomUrl = "https://example.com/quiverai/responses";

    /// <summary>Maps to <c>prompt</c>.</summary>
    public static LanguageModelCallOptions Call() =>
        new() { Prompt = [new UserModelMessage("Create an icon.")] };

    /// <summary>Maps to <c>createResponse</c>.</summary>
    public static string CreateResponse(
        string output = "[]",
        string status = "completed",
        string? error = null
    ) =>
        "{\"id\":\"resp_1\",\"object\":\"response\",\"created_at\":1700000000,\"model\":\"arrow-2\",\"status\":\""
        + status
        + "\","
        + (error is null ? string.Empty : "\"error\":" + error + ",")
        + "\"output\":"
        + output
        + ",\"usage\":{\"input_tokens\":20,\"input_tokens_details\":{\"cached_tokens\":5,\"cache_write_tokens\":3},\"output_tokens\":12,\"output_tokens_details\":{\"reasoning_tokens\":4},\"total_tokens\":32}}";

    public static QuiverAIProvider CreateQuiverAI(
        ParityHandler handler,
        string? baseUrl = null,
        Action<OpenAICompatibleOptions>? configure = null
    )
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        if (baseUrl is not null)
        {
            options.BaseUrl = baseUrl;
        }

        configure?.Invoke(options);
        return QuiverAIProvider.Create(options, handler);
    }

    public static ParityHandler Json(string body, int status = 200)
    {
        return new ParityHandler(_ =>
        {
            var response = ParityHandler.Json(body);
            response.StatusCode = (HttpStatusCode)status;
            return response;
        });
    }

    /// <summary>Maps to a <c>stream-chunks</c> response.</summary>
    public static ParityHandler Sse(params string[] events)
    {
        return new ParityHandler(_ =>
        {
            var body = new StringBuilder();
            foreach (var @event in events)
            {
                body.Append("data: ").Append(@event).Append("\n\n");
            }

            body.Append("data: [DONE]\n\n");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body.ToString(), Encoding.UTF8, "text/event-stream"),
            };
        });
    }

    public static async Task<List<LanguageModelStreamPart>> ReadAll(
        LanguageModelStreamResult result
    )
    {
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in result.Stream.ConfigureAwait(false))
        {
            parts.Add(part);
        }

        return parts;
    }

    public static JsonObject Body(ParityCall call) => (JsonObject)JsonNode.Parse(call.Text)!;

    public static IReadOnlyDictionary<string, JsonElement> ProviderOptions(string quiverai) =>
        new Dictionary<string, JsonElement>
        {
            ["quiverai"] = JsonDocument.Parse(quiverai).RootElement.Clone(),
        };
}
