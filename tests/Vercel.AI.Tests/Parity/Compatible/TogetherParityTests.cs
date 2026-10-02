// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.TogetherAI;

namespace Vercel.AI.Tests;

public sealed class TogetherParityTests
{
    private const string Ranking = "{\"id\":\"oGs6Zt9-62bZhn-99529372487b1b0a\",\"model\":\"Salesforce/Llama-Rank-v1\",\"object\":\"rerank\",\"results\":[{\"index\":0,\"relevance_score\":0.6475887154399037},{\"index\":5,\"relevance_score\":0.6323295373206566}]}";

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should create a TogetherAIProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_together_api_key_variable()
    {
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.Equal(TogetherAIProvider.DefaultBaseUrl, provider.Options.BaseUrl);
        Assert.Equal("TOGETHER_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should create a TogetherAIProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public void Accepts_a_custom_base_url_and_key()
    {
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions
        {
            ApiKey = "custom-key",
            BaseUrl = "https://custom.url",
        });
        Assert.Equal("https://custom.url", provider.Options.BaseUrl);
        Assert.Equal("custom-key", provider.Options.ApiKey);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should fall back to TOGETHER_AI_API_KEY with deprecation warning", Coverage = UpstreamCoverage.Covered)]
    public async Task Falls_back_to_the_deprecated_key_and_warns()
    {
        var previousNew = Environment.GetEnvironmentVariable("TOGETHER_API_KEY");
        var previousOld = Environment.GetEnvironmentVariable("TOGETHER_AI_API_KEY");
        TogetherAIProvider.ClearDeprecationWarnings();
        Environment.SetEnvironmentVariable("TOGETHER_API_KEY", null);
        Environment.SetEnvironmentVariable("TOGETHER_AI_API_KEY", "old-key");
        try
        {
            var handler = new CaptureHandler();
            var provider = TogetherAIProvider.Create(handler: handler);
            await provider.LanguageModel("m").DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } }, CancellationToken.None);
            Assert.Contains(TogetherAIProvider.DeprecatedApiKeyMessage, TogetherAIProvider.DeprecationWarnings);
            Assert.Equal("Bearer old-key", handler.RequestHeaders["Authorization"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TOGETHER_API_KEY", previousNew);
            Environment.SetEnvironmentVariable("TOGETHER_AI_API_KEY", previousOld);
            TogetherAIProvider.ClearDeprecationWarnings();
        }
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should prefer TOGETHER_API_KEY over TOGETHER_AI_API_KEY", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_the_current_key_without_a_warning()
    {
        var previousNew = Environment.GetEnvironmentVariable("TOGETHER_API_KEY");
        var previousOld = Environment.GetEnvironmentVariable("TOGETHER_AI_API_KEY");
        TogetherAIProvider.ClearDeprecationWarnings();
        Environment.SetEnvironmentVariable("TOGETHER_API_KEY", "new-key");
        Environment.SetEnvironmentVariable("TOGETHER_AI_API_KEY", "old-key");
        try
        {
            var handler = new CaptureHandler();
            var provider = TogetherAIProvider.Create(handler: handler);
            await provider.LanguageModel("m").DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } }, CancellationToken.None);
            Assert.DoesNotContain(TogetherAIProvider.DeprecatedApiKeyMessage, TogetherAIProvider.DeprecationWarnings);
            Assert.Equal("Bearer new-key", handler.RequestHeaders["Authorization"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TOGETHER_API_KEY", previousNew);
            Environment.SetEnvironmentVariable("TOGETHER_AI_API_KEY", previousOld);
            TogetherAIProvider.ClearDeprecationWarnings();
        }
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should prefer explicit apiKey over TOGETHER_AI_API_KEY", Coverage = UpstreamCoverage.Covered)]
    public async Task Prefers_an_explicit_key_without_a_warning()
    {
        var previousOld = Environment.GetEnvironmentVariable("TOGETHER_AI_API_KEY");
        TogetherAIProvider.ClearDeprecationWarnings();
        Environment.SetEnvironmentVariable("TOGETHER_AI_API_KEY", "old-key");
        try
        {
            var handler = new CaptureHandler();
            var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "explicit-key" }, handler);
            await provider.LanguageModel("m").DoGenerateAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } }, CancellationToken.None);
            Assert.DoesNotContain(TogetherAIProvider.DeprecatedApiKeyMessage, TogetherAIProvider.DeprecationWarnings);
            Assert.Equal("Bearer explicit-key", handler.RequestHeaders["Authorization"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TOGETHER_AI_API_KEY", previousOld);
            TogetherAIProvider.ClearDeprecationWarnings();
        }
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > chatModel::should set includeUsage so streaming responses report token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_streams_request_include_usage()
    {
        var handler = new CaptureHandler
        {
            ServerSentEvents = "data: {\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1}}\n\n",
        };
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, handler);
        await foreach (var _ in provider.LanguageModel("m").DoStreamAsync(new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } }, CancellationToken.None))
        {
        }

        Assert.Contains("\"include_usage\":true", handler.Body, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > chatModel::should set supportsStructuredOutputs for %s to %s", Coverage = UpstreamCoverage.Covered)]
    public void Structured_outputs_are_limited_to_deepseek_v4_flash()
    {
        Assert.True(TogetherAIProvider.SupportsStructuredOutputs("deepseek-ai/DeepSeek-V4-Flash-0731"));
        Assert.False(TogetherAIProvider.SupportsStructuredOutputs("meta-llama/Llama-3.3-70B-Instruct-Turbo"));
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::getModelStructuredOutputSupport::returns structured output support for %s as %s", Coverage = UpstreamCoverage.Covered)]
    public void Structured_output_helper_matches_the_model_allow_list()
    {
        Assert.True(TogetherAIProvider.SupportsStructuredOutputs("deepseek-ai/DeepSeek-V4-Flash-0731"));
        Assert.False(TogetherAIProvider.SupportsStructuredOutputs("deepseek-ai/DeepSeek-V3"));
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > rerankingModel::should construct a reranking model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Reranking_model_uses_the_together_rerank_provider()
    {
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        var model = provider.RerankingModel("Salesforce/Llama-Rank-v1");
        Assert.IsType<TogetherRerankingModel>(model);
        Assert.Equal("togetherai.reranking", model.Provider);
        Assert.Equal("Salesforce/Llama-Rank-v1", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_documents_as_objects()
    {
        var (model, handler) = Ranker();
        await model.RerankAsync("rainy day", JsonNode.Parse("[{\"example\":\"sunny day at the beach\"},{\"example\":\"rainy day in the city\"}]")!.AsArray(), 2, new[] { "example" }, CancellationToken.None);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("sunny day at the beach", body["documents"]![0]!["example"]!.GetValue<string>());
        Assert.Equal("rainy day", body["query"]!.GetValue<string>());
        Assert.Equal("example", body["rank_fields"]![0]!.GetValue<string>());
        Assert.False(body["return_documents"]!.GetValue<bool>());
        Assert.Equal(2, body["top_n"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_rerank_uses_bearer_auth()
    {
        var (model, handler) = Ranker();
        await model.RerankAsync("rainy day", new JsonArray(), 2, null, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_rerank_has_no_warnings()
    {
        var result = await Rank();
        Assert.Null(result.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_rerank_returns_scores()
    {
        var result = await Rank();
        Assert.Equal(0, result.Ranking[0].Index);
        Assert.Equal(0.6475887154399037, result.Ranking[0].Score);
        Assert.Equal(5, result.Ranking[1].Index);
        Assert.Equal(0.6323295373206566, result.Ranking[1].Score);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_rerank_has_no_provider_metadata()
    {
        var result = await Rank();
        Assert.Null(result.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_rerank_returns_the_response_id()
    {
        var result = await Rank();
        Assert.Equal("oGs6Zt9-62bZhn-99529372487b1b0a", result.ResponseId);
        Assert.Equal("Salesforce/Llama-Rank-v1", result.ResponseModelId);
        Assert.Contains("oGs6Zt9-62bZhn-99529372487b1b0a", result.RawBody, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_documents()
    {
        var (model, handler) = Ranker();
        await model.DoRerankAsync("rainy day", new[] { "sunny day at the beach", "rainy day in the city" }, 2, CancellationToken.None);
        var body = JsonNode.Parse(handler.Body)!;
        Assert.Equal("sunny day at the beach", body["documents"]![0]!.GetValue<string>());
        Assert.Equal("rainy day in the city", body["documents"]![1]!.GetValue<string>());
        Assert.False(body["return_documents"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_rerank_uses_bearer_auth()
    {
        var (model, handler) = Ranker();
        await model.DoRerankAsync("rainy day", new[] { "sunny day at the beach" }, 1, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.RequestHeaders["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_rerank_has_no_warnings()
    {
        Assert.Null((await Rank()).Warnings);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_rerank_returns_scores()
    {
        var result = await Rank();
        Assert.Equal(2, result.Ranking.Count);
        Assert.Equal(0.6323295373206566, result.Ranking[1].Score);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_rerank_has_no_provider_metadata()
    {
        Assert.Null((await Rank()).ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_rerank_returns_the_response_id()
    {
        var result = await Rank();
        Assert.Equal("Salesforce/Llama-Rank-v1", result.ResponseModelId);
        Assert.Equal("oGs6Zt9-62bZhn-99529372487b1b0a", result.ResponseId);
    }

    private static async Task<TogetherRerankResult> Rank()
    {
        var (model, _) = Ranker();
        var documents = new JsonArray();
        documents.Add(JsonValue.Create("a"));
        return await model.RerankAsync("rainy day", documents, 2, new[] { "example" }, CancellationToken.None);
    }

    private static (TogetherRerankingModel Model, CaptureHandler Handler) Ranker()
    {
        var handler = new CaptureHandler { ResponseBody = Ranking };
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        return ((TogetherRerankingModel)provider.RerankingModel("Salesforce/Llama-Rank-v1"), handler);
    }
}
