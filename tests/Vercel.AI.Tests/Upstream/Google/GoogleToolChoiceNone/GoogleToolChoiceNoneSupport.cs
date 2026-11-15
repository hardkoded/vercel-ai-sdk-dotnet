// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.GoogleToolChoiceNone;

/// <summary>Synthetic provider configurations shared by the toolChoice none tests.</summary>
internal static class GoogleToolChoiceNoneSupport
{
    public static readonly string[] Models = { "gemini-2.5-flash", "gemini-3-flash-preview" };

    public static readonly string[] Providers = { "google.generative-ai", "google.vertex.chat" };

    public static readonly string[] Modes = { "generate", "stream" };

    public static string ProviderToolJson(string provider)
    {
        return provider == "google.generative-ai"
            ? "{\"id\":\"google.file_search\",\"name\":\"retrieval\",\"args\":{\"fileSearchStoreNames\":[\"fileSearchStores/synthetic-store\"]}}"
            : "{\"id\":\"google.vertex_rag_store\",\"name\":\"retrieval\",\"args\":{\"ragCorpus\":\"projects/synthetic/locations/us-central1/ragCorpora/unused\"}}";
    }

    public static string ExpectedJson(string provider)
    {
        return provider == "google.generative-ai"
            ? "{\"fileSearch\":{\"fileSearchStoreNames\":[\"fileSearchStores/synthetic-store\"]}}"
            : "{\"retrieval\":{\"vertex_rag_store\":{\"rag_resources\":{\"rag_corpus\":\"projects/synthetic/locations/us-central1/ragCorpora/unused\"}}}}";
    }

    public static LanguageModelCallOptions Options(string provider, bool withFunction, ToolChoice choice)
    {
        var key = provider == "google.generative-ai" ? "google" : "googleVertex";
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Say hello.") },
            ToolChoice = choice,
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"" + key + "\":{\"providerTools\":[" + ProviderToolJson(provider) + "]}}"),
        };
        if (withFunction)
        {
            options.Tools = new[] { GoogleUpstream.Function("lookup", "{\"type\":\"object\",\"properties\":{}}", "Synthetic function") };
        }

        return options;
    }

    public static ILanguageModel Model(string provider, string modelId, RecordingHandler handler)
    {
        return provider == "google.generative-ai"
            ? GoogleUpstream.Client(handler).LanguageModel(modelId)
            : GoogleVertexProvider.Create(new VertexOptions { Project = "synthetic", Region = "us-central1", ApiKey = "test-api-key" }, handler).LanguageModel(modelId);
    }

    public static RecordingHandler Handler(string mode, JsonArray parts)
    {
        var body = new JsonObject
        {
            ["candidates"] = new JsonArray
            {
                new JsonObject
                {
                    ["content"] = new JsonObject { ["role"] = "model", ["parts"] = parts },
                    ["finishReason"] = "STOP",
                },
            },
        };
        return mode == "generate"
            ? new RecordingHandler { ResponseText = body.ToJsonString() }
            : new RecordingHandler { ResponseText = "data: " + body.ToJsonString() + "\n\n", MediaType = "text/event-stream" };
    }
}
