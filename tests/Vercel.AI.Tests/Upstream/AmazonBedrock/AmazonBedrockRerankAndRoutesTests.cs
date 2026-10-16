// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

public sealed class AmazonBedrockRerankTests
{
    private const string Ranking = "{\"results\":[{\"index\":0,\"relevanceScore\":0.5110583305358887},{\"index\":5,\"relevanceScore\":0.30241215229034424}]}";

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should send the AWS-required `bedrockRerankingConfiguration` wire key", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_the_bedrock_reranking_configuration_wire_key()
    {
        var handler = await PostJson();
        using var document = JsonDocument.Parse(handler.Body);
        Assert.True(document.RootElement.GetProperty("rerankingConfiguration").TryGetProperty("bedrockRerankingConfiguration", out _));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_json_documents()
    {
        var handler = await PostJson();
        using var document = JsonDocument.Parse(handler.Body);
        var root = document.RootElement;
        Assert.Equal("test-token", root.GetProperty("nextToken").GetString());
        Assert.Equal("rainy day", root.GetProperty("queries")[0].GetProperty("textQuery").GetProperty("text").GetString());
        Assert.Equal("TEXT", root.GetProperty("queries")[0].GetProperty("type").GetString());
        var configuration = root.GetProperty("rerankingConfiguration").GetProperty("bedrockRerankingConfiguration");
        Assert.Equal("arn:aws:bedrock:us-west-2::foundation-model/cohere.rerank-v3-5:0", configuration.GetProperty("modelConfiguration").GetProperty("modelArn").GetString());
        Assert.Equal("test-value", configuration.GetProperty("modelConfiguration").GetProperty("additionalModelRequestFields").GetProperty("test").GetString());
        Assert.Equal(2, configuration.GetProperty("numberOfResults").GetInt32());
        Assert.Equal("BEDROCK_RERANKING_MODEL", root.GetProperty("rerankingConfiguration").GetProperty("type").GetString());
        Assert.Equal("JSON", root.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("type").GetString());
        Assert.Equal("sunny day at the beach", root.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("jsonDocument").GetProperty("example").GetString());
        Assert.Equal("rainy day in the city", root.GetProperty("sources")[1].GetProperty("inlineDocumentSource").GetProperty("jsonDocument").GetProperty("example").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_json_document_ranking()
    {
        var result = await Rank(json: true);
        Assert.Equal(0, result.Items[0].Index);
        Assert.Equal(0.5110583305358887d, result.Items[0].Score, 12);
        Assert.Equal(5, result.Items[1].Index);
        Assert.Equal(0.30241215229034424d, result.Items[1].Score, 12);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_text_documents()
    {
        var handler = await PostText();
        using var document = JsonDocument.Parse(handler.Body);
        var root = document.RootElement;
        Assert.Equal("test-token", root.GetProperty("nextToken").GetString());
        Assert.Equal("TEXT", root.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("type").GetString());
        Assert.Equal("sunny day at the beach", root.GetProperty("sources")[0].GetProperty("inlineDocumentSource").GetProperty("textDocument").GetProperty("text").GetString());
        Assert.Equal("rainy day in the city", root.GetProperty("sources")[1].GetProperty("inlineDocumentSource").GetProperty("textDocument").GetProperty("text").GetString());
        Assert.Equal("arn:aws:bedrock:us-west-2::foundation-model/cohere.rerank-v3-5:0", root.GetProperty("rerankingConfiguration").GetProperty("bedrockRerankingConfiguration").GetProperty("modelConfiguration").GetProperty("modelArn").GetString());
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_the_text_document_ranking()
    {
        var result = await Rank(json: false);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(0, result.Items[0].Index);
        Assert.Equal(5, result.Items[1].Index);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_send_call_headers_and_are_signed()
    {
        var (handler, _) = await Call(objects: true);
        Assert.Equal("config-value", handler.Headers["config-header"]);
        Assert.Equal("config-shared", handler.Headers["shared-header"]);
        Assert.StartsWith("application/json", handler.Headers["Content-Type"]);
        Assert.Contains("us-west-2/bedrock/aws4_request", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_return_no_warnings()
    {
        var (_, response) = await Call(objects: true);
        Assert.Empty(response.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_return_no_provider_metadata()
    {
        var (_, response) = await Call(objects: true);
        Assert.Null(response.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > json documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Json_documents_return_the_response_body_and_headers()
    {
        var (_, response) = await Call(objects: true);
        JsonAssert.Equal(response.Response!.Body!.Value, Ranking);
        Assert.StartsWith("application/json", response.Response.Headers!["content-type"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_send_call_headers_and_are_signed()
    {
        var (handler, _) = await Call(objects: false);
        Assert.Equal("config-value", handler.Headers["config-header"]);
        Assert.Equal("config-shared", handler.Headers["shared-header"]);
        Assert.StartsWith("application/json", handler.Headers["Content-Type"]);
        Assert.Contains("us-west-2/bedrock/aws4_request", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_return_no_warnings()
    {
        var (_, response) = await Call(objects: false);
        Assert.Empty(response.Warnings);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_return_no_provider_metadata()
    {
        var (_, response) = await Call(objects: false);
        Assert.Null(response.ProviderMetadata);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/reranking/amazon-bedrock-reranking-model.test.ts::doRerank > text documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Text_documents_return_the_response_body_and_headers()
    {
        var (_, response) = await Call(objects: false);
        JsonAssert.Equal(response.Response!.Body!.Value, Ranking);
        Assert.StartsWith("application/json", response.Response.Headers!["content-type"]);
    }

    private static async Task<(UpstreamRecordingHandler Handler, Operations.RerankModelResponse Response)> Call(bool objects)
    {
        var handler = new UpstreamRecordingHandler(Ranking);
        var documents = objects
            ? new Operations.RerankModelDocuments("object", Documents().Cast<object?>().ToArray())
            : new Operations.RerankModelDocuments("text", new object?[] { "sunny day at the beach", "rainy day in the city" });
        var headers = new Dictionary<string, string> { ["config-header"] = "config-value", ["shared-header"] = "config-shared" };
        var response = await Model(handler).DoRerankAsync(new Operations.RerankModelCall(documents, "rainy day", 2, Options(), headers, CancellationToken.None), CancellationToken.None);
        return (handler, response);
    }

    private static async Task<RerankResult> Rank(bool json)
    {
        var handler = new UpstreamRecordingHandler(Ranking);
        var model = Model(handler);
        if (json)
        {
            return await model.DoRerankObjectsAsync("rainy day", Documents(), 2, Options(), CancellationToken.None);
        }

        return await model.DoRerankAsync("rainy day", new[] { "sunny day at the beach", "rainy day in the city" }, 2, Options(), CancellationToken.None);
    }

    private static async Task<UpstreamRecordingHandler> PostJson()
    {
        var handler = new UpstreamRecordingHandler(Ranking);
        await Model(handler).DoRerankObjectsAsync("rainy day", Documents(), 2, Options(), CancellationToken.None);
        return handler;
    }

    private static async Task<UpstreamRecordingHandler> PostText()
    {
        var handler = new UpstreamRecordingHandler(Ranking);
        await Model(handler).DoRerankAsync("rainy day", new[] { "sunny day at the beach", "rainy day in the city" }, 2, Options(), CancellationToken.None);
        return handler;
    }

    private static AmazonBedrockRerankingModel Model(UpstreamRecordingHandler handler)
    {
        var provider = global::Vercel.AI.AmazonBedrock.AmazonBedrockProvider.Create(new AmazonBedrockOptions
        {
            Region = "us-west-2",
            AgentRuntimeBaseUrl = "https://bedrock-agent-runtime.us-east-1.amazonaws.com",
            AccessKeyId = "AKIA",
            SecretAccessKey = "secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        return (AmazonBedrockRerankingModel)provider.RerankingModel("cohere.rerank-v3-5:0");
    }

    private static JsonElement Options()
    {
        using var document = JsonDocument.Parse("{\"bedrock\":{\"nextToken\":\"test-token\",\"additionalModelRequestFields\":{\"test\":\"test-value\"}}}");
        return document.RootElement.Clone();
    }

    private static JsonElement[] Documents()
    {
        using var first = JsonDocument.Parse("{\"example\":\"sunny day at the beach\"}");
        using var second = JsonDocument.Parse("{\"example\":\"rainy day in the city\"}");
        return new[] { first.RootElement.Clone(), second.RootElement.Clone() };
    }
}

public sealed class BedrockMantleRouteTests
{
    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should use default base URL with region", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_default_base_url_for_the_region()
    {
        Assert.Equal(
            "https://bedrock-mantle.us-west-2.api.aws/v1/responses",
            BedrockMantleRoutes.Url("us-west-2", null, "test-model", "/responses"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should use the OpenAI route for models served under /openai/v1", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_openai_route()
    {
        Assert.Equal(
            "https://bedrock-mantle.us-west-2.api.aws/openai/v1/chat/completions",
            BedrockMantleRoutes.Url("us-west-2", null, "openai.gpt-6-sol", "/chat/completions"));
        Assert.Equal(
            "https://bedrock-mantle.us-west-2.api.aws/openai/v1/responses",
            BedrockMantleRoutes.Url("us-west-2", null, "openai.gpt-6-luna", "/responses"));
        foreach (var modelId in new[] { "openai.gpt-5.6-sol", "google.gemma-4-31b", "xai.grok-4.3" })
        {
            Assert.Equal(
                "https://bedrock-mantle.us-west-2.api.aws/openai/v1/chat/completions",
                BedrockMantleRoutes.Url("us-west-2", null, modelId, "/chat/completions"));
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should use the default route for gpt-oss and other models", Coverage = UpstreamCoverage.Covered)]
    public void Uses_the_default_route_for_gpt_oss_and_other_models()
    {
        foreach (var modelId in new[] { "openai.gpt-oss-120b", "openai.gpt-oss-safeguard-20b", "google.gemma-3-27b-it", "qwen.qwen3-32b" })
        {
            Assert.Equal(
                "https://bedrock-mantle.us-west-2.api.aws/v1/chat/completions",
                BedrockMantleRoutes.Url("us-west-2", null, modelId, "/chat/completions"));
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should use createApiKeyFetchFunction when apiKey is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Uses_a_bearer_token_when_an_api_key_is_provided()
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}");
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions
        {
            Region = "us-east-1",
            ApiKey = "test-api-key",
        }, handler);
        await provider.LanguageModel("openai.gpt-oss-20b").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hi") },
        }, CancellationToken.None);
        Assert.Equal("Bearer test-api-key", handler.Headers["Authorization"]);
        Assert.DoesNotContain("AWS4-HMAC-SHA256", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should use createSigV4FetchFunction with bedrock-mantle service when no apiKey", Coverage = UpstreamCoverage.Covered)]
    public async Task Signs_with_the_bedrock_mantle_service()
    {
        var handler = new UpstreamRecordingHandler("{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}");
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions
        {
            Region = "us-east-1",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
            UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
        }, handler);
        await provider.LanguageModel("openai.gpt-oss-20b").DoGenerateAsync(new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new UserModelMessage("Hi") },
        }, CancellationToken.None);
        Assert.Contains("us-east-1/bedrock-mantle/aws4_request", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should create a chat model with default settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Creates_a_chat_model_with_default_settings()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var provider = BedrockMantleProvider.Create(SignedOptions(), handler);
        var model = provider.LanguageModel("openai.gpt-oss-20b");
        Assert.Equal("bedrock-mantle.chat", model.Provider);
        Assert.Equal("openai.gpt-oss-20b", model.ModelId);
        await model.DoGenerateAsync(Hi(), CancellationToken.None);
        Assert.Equal("https://bedrock-mantle.us-east-1.api.aws/v1/chat/completions", handler.Uri);
        Assert.Contains("us-east-1/bedrock-mantle/aws4_request", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should create a chat model via .chat()", Coverage = UpstreamCoverage.Covered)]
    public async Task Creates_a_chat_model_via_chat()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var model = BedrockMantleProvider.Create(SignedOptions(), handler).Chat("openai.gpt-oss-20b");
        Assert.Equal("bedrock-mantle.chat", model.Provider);
        Assert.Equal("openai.gpt-oss-20b", model.ModelId);
        await model.DoGenerateAsync(Hi(), CancellationToken.None);
        Assert.Equal("https://bedrock-mantle.us-east-1.api.aws/v1/chat/completions", handler.Uri);
        Assert.Contains("us-east-1/bedrock-mantle/aws4_request", handler.Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should pass custom baseURL to the model when created", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_a_custom_base_url_to_the_model()
    {
        var options = SignedOptions();
        options.BaseUrl = "https://custom-mantle.example.com/v1";
        foreach (var modelId in new[] { "test-model", "openai.gpt-6-sol" })
        {
            var handler = new UpstreamRecordingHandler(ChatResponse);
            await BedrockMantleProvider.Create(options, handler).LanguageModel(modelId).DoGenerateAsync(Hi(), CancellationToken.None);
            Assert.Equal("https://custom-mantle.example.com/v1/chat/completions", handler.Uri);
        }
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should include custom headers with user-agent suffix", Coverage = UpstreamCoverage.Covered)]
    public async Task Sends_custom_headers_with_the_user_agent_suffix()
    {
        var handler = new UpstreamRecordingHandler(ChatResponse);
        var options = SignedOptions();
        options.Headers["Custom-Header"] = "custom-value";
        await BedrockMantleProvider.Create(options, handler).LanguageModel("test-model").DoGenerateAsync(Hi(), CancellationToken.None);
        Assert.Equal("custom-value", handler.Headers["Custom-Header"]);
        Assert.Contains("ai-sdk/amazon-bedrock/", handler.Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should provide languageModel as alias for responses", Coverage = UpstreamCoverage.Covered)]
    public void Provides_a_language_model()
    {
        var provider = BedrockMantleProvider.Create(SignedOptions());
        Assert.NotNull(provider.LanguageModel("test-model"));
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should alias .languageModel() to chat model", Coverage = UpstreamCoverage.Covered)]
    public void Aliases_language_model_to_chat()
    {
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions { AccessKeyId = "test-key", SecretAccessKey = "test-secret" });
        var model = provider.LanguageModel("test-model");
        Assert.Equal("bedrock-mantle.chat", model.Provider);
        Assert.Equal("bedrock-mantle.chat", provider.Chat("test-model").Provider);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should create a responses model via .responses()",
        Coverage = UpstreamCoverage.Partial,
        Note = "The responses model id is bedrock-mantle.responses. supportsWebSearchSourcesInclude is not a separate flag.")]
    public void Creates_a_responses_model()
    {
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions { AccessKeyId = "test-key", SecretAccessKey = "test-secret" });
        Assert.Equal("bedrock-mantle.responses", provider.Responses("test-model").Provider);
    }

    [Fact]
    [UpstreamTest("packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should have correct specificationVersion", Coverage = UpstreamCoverage.Covered)]
    public void Exposes_specification_version_v4()
    {
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions { AccessKeyId = "k", SecretAccessKey = "s" });
        Assert.Equal("v4", provider.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should throw NoSuchModelError for embeddingModel",
        Coverage = UpstreamCoverage.Partial,
        Note = "Embedding calls throw AiSdkException. There is no NoSuchModelError type.")]
    public void Rejects_embedding_models()
    {
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions { AccessKeyId = "k", SecretAccessKey = "s" });
        var exception = Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("invalid-model-id"));
        Assert.Contains("embedding", exception.Message);
        Assert.Contains("invalid-model-id", exception.Message);
    }

    [Fact]
    [UpstreamTest(
        "packages/amazon-bedrock/src/mantle/bedrock-mantle-provider.test.ts::bedrock-mantle-provider::should throw NoSuchModelError for imageModel",
        Coverage = UpstreamCoverage.Partial,
        Note = "Image calls throw AiSdkException. There is no NoSuchModelError type.")]
    public void Rejects_image_models()
    {
        var provider = BedrockMantleProvider.Create(new BedrockMantleOptions { AccessKeyId = "k", SecretAccessKey = "s" });
        var exception = Assert.Throws<AiSdkException>(() => provider.ImageModel("invalid-model-id"));
        Assert.Contains("image", exception.Message);
    }

    private const string ChatResponse = "{\"choices\":[{\"message\":{\"content\":\"ok\"},\"finish_reason\":\"stop\"}]}";

    private static BedrockMantleOptions SignedOptions()
    {
        return new BedrockMantleOptions
        {
            Region = "us-east-1",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
        };
    }

    private static LanguageModelCallOptions Hi()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("Hi") } };
    }
}
