// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.GoogleVertexLanguageModel;

/// <summary>Port of <c>google-vertex-language-model.test.ts</c>.</summary>
public sealed class GoogleVertexLanguageModelTests
{
    [Theory]
    [InlineData("gemini-4.0-flash")]
    [InlineData("gemini-future-latest")]
    [InlineData("models/gemini-4.0-flash")]
    [UpstreamTest("packages/google-vertex/src/google-vertex-language-model.test.ts::should send current tools and thinking config for %s through Vertex", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_current_tools_and_thinking_config_for_model_through_Vertex(string modelId)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hello") },
            Reasoning = "high",
            Tools = new[] { GoogleUpstream.Function("lookup", "{\"type\":\"object\",\"properties\":{}}", "Lookup") },
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"googleVertex\":{\"providerTools\":[" +
                "{\"id\":\"google.google_search\",\"args\":{}}," +
                "{\"id\":\"google.enterprise_web_search\",\"args\":{}}," +
                "{\"id\":\"google.url_context\",\"args\":{}}," +
                "{\"id\":\"google.code_execution\",\"args\":{}}," +
                "{\"id\":\"google.file_search\",\"args\":{\"fileSearchStoreNames\":[\"fileSearchStores/example-store\"]}}," +
                "{\"id\":\"google.vertex_rag_store\",\"args\":{\"ragCorpus\":\"projects/p/locations/l/ragCorpora/c\"}}]}}"),
        };

        var prepared = GoogleRequest.Prepare(modelId, "google.vertex.chat", options, false);

        GoogleUpstream.JsonEqual(prepared.Body["generationConfig"]!["thinkingConfig"]!, "{\"thinkingLevel\":\"high\"}");
        GoogleUpstream.JsonEqual(prepared.Body["tools"]!, "[{\"googleSearch\":{}},{\"enterpriseWebSearch\":{}},{\"urlContext\":{}},{\"codeExecution\":{}},{\"fileSearch\":{\"fileSearchStoreNames\":[\"fileSearchStores/example-store\"]}},{\"retrieval\":{\"vertex_rag_store\":{\"rag_resources\":{\"rag_corpus\":\"projects/p/locations/l/ragCorpora/c\"}}}},{\"functionDeclarations\":[{\"name\":\"lookup\",\"description\":\"Lookup\",\"parametersJsonSchema\":{\"type\":\"object\",\"properties\":{}}}]}]");
        Assert.Equal("VALIDATED", prepared.Body["toolConfig"]!["functionCallingConfig"]!["mode"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }
}
