// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Baseten;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.BasetenChatLanguageModel;

/// <summary>Shared request helper for <c>baseten-chat-language-model.test.ts</c>.</summary>
internal static class BasetenChatCalls
{
    public const string ModelId = "deepseek-ai/DeepSeek-V4-Pro-0813";
    public const string ModelUrl = "https://model-123.api.baseten.co/sync/v1";
    public const string Schema = "{\"type\":\"object\",\"properties\":{\"answer\":{\"type\":\"string\"}},\"required\":[\"answer\"],\"additionalProperties\":false}";

    private const string GenerateBody = "{\"choices\":[{\"message\":{\"content\":\"{\\\"answer\\\":\\\"PONG\\\"}\"},\"finish_reason\":\"stop\"}]}";

    public static LanguageModelCallOptions Options(bool json = true, bool schema = true, string? reasoning = "high")
    {
        return new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("You are a helpful assistant."),
                new UserModelMessage("Reply with PONG."),
            },
            JsonResponse = json,
            JsonSchema = json && schema ? UpstreamChat.Json(Schema) : null,
            Reasoning = reasoning,
        };
    }

    public static async Task<(JsonObject Body, IReadOnlyList<CallWarning> Warnings)> Call(
        bool stream,
        LanguageModelCallOptions options,
        string id = ModelId,
        bool dedicated = false)
    {
        var capture = new UpstreamCapture();
        if (stream)
        {
            capture.MediaType = "text/event-stream";
            capture.ResponseBody = UpstreamChat.Sse(
                "{\"choices\":[{\"index\":0,\"delta\":{\"content\":\"{\\\"answer\\\":\\\"PONG\\\"}\"},\"finish_reason\":\"stop\"}]}",
                "[DONE]");
        }
        else
        {
            capture.ResponseBody = GenerateBody;
        }

        var provider = BasetenProvider.Create(
            dedicated ? new BasetenOptions { ApiKey = "test-api-key", ModelUrl = ModelUrl } : new OpenAICompatibleOptions { ApiKey = "test-api-key" },
            capture);
        var model = provider.LanguageModel(id);
        IReadOnlyList<CallWarning> warnings;
        if (stream)
        {
            var parts = await UpstreamChat.Read(await model.DoStreamAsync(options, CancellationToken.None));
            warnings = parts.OfType<StreamStartStreamPart>().First().Warnings;
        }
        else
        {
            warnings = (await model.DoGenerateAsync(options, CancellationToken.None)).Warnings;
        }

        return (UpstreamChat.Body(capture), warnings);
    }
}
