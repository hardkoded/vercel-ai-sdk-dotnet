// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.TogetherAI;

namespace Vercel.AI.Tests;

public sealed class TogetherAIUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should pass the correct parameters including size and seed", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_sends_width_height_and_seed()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs/FLUX.1-dev");
        model.Seed = 7;
        await model.DoGenerateAsync(new ImageCallOptions("a cat") { Size = "1024x1024" }, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("black-forest-labs/FLUX.1-dev", body["model"]!.GetValue<string>());
        Assert.Equal("a cat", body["prompt"]!.GetValue<string>());
        Assert.Equal("base64", body["response_format"]!.GetValue<string>());
        Assert.Equal(1024, body["width"]!.GetValue<int>());
        Assert.Equal(1024, body["height"]!.GetValue<int>());
        Assert.Equal(7, body["seed"]!.GetValue<int>());
        Assert.Null(body["n"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should omit seed from request body when seed is undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task An_unset_seed_is_omitted()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        await Image(capture, "black-forest-labs/FLUX.1-dev").DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None);
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["seed"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should include n parameter when requesting multiple images", Coverage = UpstreamCoverage.Covered)]
    public async Task Multiple_images_send_n()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        await Image(capture, "black-forest-labs/FLUX.1-dev").DoGenerateAsync(new ImageCallOptions("a cat") { Count = 3 }, CancellationToken.None);
        Assert.Equal(3, JsonNode.Parse(capture.Requests[0].Body)!["n"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should call the correct url", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_posts_to_images_generations()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        await Image(capture, "black-forest-labs/FLUX.1-dev").DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None);
        Assert.Equal("https://api.together.xyz/v1/images/generations", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_calls_pass_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs/FLUX.1-dev");
        model.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await model.DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None);
        Assert.Equal("yes", capture.Requests[0].Headers["X-Custom"]);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate > warnings::should return aspectRatio warning when aspectRatio is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Aspect_ratio_warns_and_is_not_sent()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs/FLUX.1-dev");
        await model.DoGenerateAsync(new ImageCallOptions("a cat") { AspectRatio = "16:9", Size = "1024x1024" }, CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Message == "This model does not support the `aspectRatio` option. Use `size` instead.");
        Assert.Null(JsonNode.Parse(capture.Requests[0].Body)!["aspect_ratio"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::Image Editing::should throw error when mask is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_mask_is_rejected()
    {
        var model = Image(new UpstreamCapture(), "black-forest-labs/FLUX.1-dev");
        model.Mask = true;
        var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None));
        Assert.Equal(
            "Together AI does not support mask-based image editing. Use FLUX Kontext models (e.g., black-forest-labs/FLUX.1-kontext-pro) with a reference image and descriptive prompt instead.",
            error.Message);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::Image Editing::should warn when multiple files are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task Extra_images_are_ignored()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[]}" };
        var model = Image(capture, "black-forest-labs/FLUX.1-dev");
        model.Files = new[]
        {
            new OpenAICompatibleImageFile("image/png", null, "https://example.test/a.png", "a.png"),
            new OpenAICompatibleImageFile("image/png", null, "https://example.test/b.png", "b.png"),
        };
        await model.DoGenerateAsync(new ImageCallOptions("edit"), CancellationToken.None);
        Assert.Contains(model.LastWarnings, warning => warning.Message == "Together AI only supports a single input image. Additional images are ignored.");
        Assert.Equal("https://example.test/a.png", JsonNode.Parse(capture.Requests[0].Body)!["image_url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should fall back to TOGETHER_AI_API_KEY with deprecation warning", Coverage = UpstreamCoverage.Covered)]
    public void The_legacy_environment_variable_warns()
    {
        WithEnvironment("TOGETHER_API_KEY", null, () =>
        {
            WithEnvironment("TOGETHER_AI_API_KEY", "legacy-key", () =>
            {
                var provider = TogetherAIProvider.Create();
                var headers = provider.CreateHeaders();
                Assert.Equal("Bearer legacy-key", headers["Authorization"]);
                Assert.Equal(
                    "TOGETHER_AI_API_KEY is deprecated and will be removed in a future release. Please use TOGETHER_API_KEY instead.",
                    provider.ApiKeyDeprecationWarning);
            });
        });
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should prefer TOGETHER_API_KEY over TOGETHER_AI_API_KEY", Coverage = UpstreamCoverage.Covered)]
    public void The_preferred_environment_variable_wins()
    {
        WithEnvironment("TOGETHER_API_KEY", "preferred", () =>
        {
            WithEnvironment("TOGETHER_AI_API_KEY", "legacy-key", () =>
            {
                var provider = TogetherAIProvider.Create();
                Assert.Equal("Bearer preferred", provider.CreateHeaders()["Authorization"]);
                Assert.Null(provider.ApiKeyDeprecationWarning);
            });
        });
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should prefer explicit apiKey over TOGETHER_AI_API_KEY", Coverage = UpstreamCoverage.Covered)]
    public void An_explicit_key_wins()
    {
        WithEnvironment("TOGETHER_AI_API_KEY", "legacy-key", () =>
        {
            var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "explicit" });
            Assert.Equal("Bearer explicit", provider.CreateHeaders()["Authorization"]);
            Assert.Null(provider.ApiKeyDeprecationWarning);
        });
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > chatModel::should set includeUsage so streaming responses report token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Chat_streams_request_usage()
    {
        var capture = Stream();
        var model = (OpenAICompatibleLanguageModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).LanguageModel("m");
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > chatModel::should set supportsStructuredOutputs for %s to %s", Coverage = UpstreamCoverage.Covered)]
    public void Structured_outputs_are_limited_to_the_flash_model()
    {
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" });
        Assert.True(provider.CreateChatModel(TogetherAIProvider.StructuredOutputModelId).SupportsStructuredOutputs);
        Assert.False(provider.CreateChatModel("meta-llama/Llama-3-8b-chat-hf").SupportsStructuredOutputs);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::getModelStructuredOutputSupport::returns structured output support for %s as %s", Coverage = UpstreamCoverage.Covered)]
    public void Structured_output_support_matches_the_model_id()
    {
        Assert.True(TogetherAIProvider.SupportsStructuredOutputs("deepseek-ai/DeepSeek-V4-Flash-0731"));
        Assert.False(TogetherAIProvider.SupportsStructuredOutputs("meta-llama/Llama-3-8b-chat-hf"));
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should send request with text documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_sends_documents_query_and_top_n()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"results\":[]}" };
        var model = (TogetherAIRerankingModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).RerankingModel("rerank-model");
        await model.DoRerankAsync("query", new[] { "a", "b" }, 2, CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.Equal("rerank-model", body["model"]!.GetValue<string>());
        Assert.Equal("query", body["query"]!.GetValue<string>());
        Assert.Equal("a", body["documents"]![0]!.GetValue<string>());
        Assert.Equal(2, body["top_n"]!.GetValue<int>());
        Assert.False(body["return_documents"]!.GetValue<bool>());
        Assert.Equal("https://api.together.xyz/v1/rerank", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_sends_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"results\":[]}" };
        var model = (TogetherAIRerankingModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).RerankingModel("rerank-model");
        model.Headers = new Dictionary<string, string?> { ["X-Custom"] = "yes" };
        await model.DoRerankAsync("query", new[] { "a" }, 1, CancellationToken.None);
        Assert.Equal("Bearer secret", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("yes", capture.Requests[0].Headers["X-Custom"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > text documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_reads_index_and_relevance_score()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"results\":[{\"index\":1,\"relevance_score\":0.9},{\"index\":0,\"relevance_score\":0.2}]}" };
        var model = (TogetherAIRerankingModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).RerankingModel("rerank-model");
        var result = await model.DoRerankAsync("query", new[] { "a", "b" }, 2, CancellationToken.None);
        Assert.Equal(1, result.Items[0].Index);
        Assert.Equal(0.9, result.Items[0].Score);
        Assert.Equal(0, result.Items[1].Index);
        Assert.Equal(capture.ResponseBody, model.LastResponseBody);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > json documents::should return result with the correct ranking", Coverage = UpstreamCoverage.Covered)]
    public async Task Stringified_documents_keep_their_ranking()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"results\":[{\"index\":0,\"relevance_score\":0.5}]}" };
        var model = (TogetherAIRerankingModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).RerankingModel("rerank-model");
        var result = await model.DoRerankAsync("query", new[] { "{\"title\":\"a\"}" }, 1, CancellationToken.None);
        Assert.Equal("{\"title\":\"a\"}", JsonNode.Parse(capture.Requests[0].Body)!["documents"]![0]!.GetValue<string>());
        Assert.Equal(0.5, result.Items[0].Score);
    }

    [Fact]
    [UpstreamTest(RerankTests + "json documents::should send request with stringified json documents", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_sends_object_documents_and_rank_fields()
    {
        var capture = RerankCapture();
        await Rerank(capture, ObjectDocuments());
        JsonAssert.Equal(JsonNode.Parse(capture.Requests[0].Body), "{\"documents\":[{\"example\":\"sunny day at the beach\"},{\"example\":\"rainy day in the city\"}],\"model\":\"Salesforce/Llama-Rank-v1\",\"query\":\"rainy day\",\"rank_fields\":[\"example\"],\"return_documents\":false,\"top_n\":2}");
    }

    [Fact]
    [UpstreamTest(RerankTests + "json documents::should send request with the correct headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_sends_bearer_and_json_headers_for_object_documents()
    {
        var capture = RerankCapture();
        await Rerank(capture, ObjectDocuments());
        Assert.Equal("Bearer test-api-key", capture.Requests[0].Headers["Authorization"]);
        Assert.StartsWith("application/json", capture.Requests[0].Headers["Content-Type"]);
    }

    [Fact]
    [UpstreamTest(RerankTests + "json documents::should return result with warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_returns_no_warnings_for_object_documents()
    {
        Assert.Empty((await Rerank(RerankCapture(), ObjectDocuments())).Warnings);
    }

    [Fact]
    [UpstreamTest(RerankTests + "text documents::should return result without warnings", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_returns_no_warnings_for_text_documents()
    {
        Assert.Empty((await Rerank(RerankCapture(), TextDocuments())).Warnings);
    }

    [Fact]
    [UpstreamTest(RerankTests + "json documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_returns_no_provider_metadata_for_object_documents()
    {
        Assert.Null((await Rerank(RerankCapture(), ObjectDocuments())).ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(RerankTests + "text documents::should not return provider metadata (use response body instead)", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_returns_no_provider_metadata_for_text_documents()
    {
        Assert.Null((await Rerank(RerankCapture(), TextDocuments())).ProviderMetadata);
    }

    [Fact]
    [UpstreamTest(RerankTests + "json documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_returns_the_response_for_object_documents()
    {
        await AssertRerankResponse(ObjectDocuments());
    }

    [Fact]
    [UpstreamTest(RerankTests + "text documents::should return result with the correct response", Coverage = UpstreamCoverage.Covered)]
    public async Task Rerank_returns_the_response_for_text_documents()
    {
        await AssertRerankResponse(TextDocuments());
    }

    private const string RerankTests = "packages/togetherai/src/reranking/togetherai-reranking-model.test.ts::doRerank > ";

    private const string RerankFixture = "{\"id\":\"oGs6Zt9-62bZhn-99529372487b1b0a\",\"object\":\"rerank\",\"model\":\"Salesforce/Llama-Rank-v1\",\"results\":[{\"index\":0,\"relevance_score\":0.6475887154399037,\"document\":{}},{\"index\":5,\"relevance_score\":0.6323295373206566,\"document\":{}}],\"usage\":{\"prompt_tokens\":2966,\"completion_tokens\":0,\"total_tokens\":2966}}";

    private static UpstreamCapture RerankCapture()
    {
        return new UpstreamCapture { ResponseBody = RerankFixture };
    }

    private static RerankModelDocuments ObjectDocuments()
    {
        return new RerankModelDocuments("object", new object?[] { new JsonObject { ["example"] = "sunny day at the beach" }, new JsonObject { ["example"] = "rainy day in the city" } });
    }

    private static RerankModelDocuments TextDocuments()
    {
        return new RerankModelDocuments("text", new object?[] { "sunny day at the beach", "rainy day in the city" });
    }

    private static async Task AssertRerankResponse(RerankModelDocuments documents)
    {
        var capture = RerankCapture();
        capture.ResponseHeaders["x-request-id"] = "req-1";
        var response = (await Rerank(capture, documents)).Response!;
        Assert.Equal("oGs6Zt9-62bZhn-99529372487b1b0a", response.Id);
        Assert.Equal("Salesforce/Llama-Rank-v1", response.ModelId);
        Assert.Equal("req-1", response.Headers!["x-request-id"]);
        JsonAssert.Equal(response.Body!.Value, RerankFixture);
    }

    private static Task<RerankModelResponse> Rerank(UpstreamCapture capture, RerankModelDocuments documents)
    {
        var model = (TogetherAIRerankingModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture).RerankingModel("Salesforce/Llama-Rank-v1");
        using var options = JsonDocument.Parse("{\"togetherai\":{\"rankFields\":[\"example\"]}}");
        return model.DoRerankAsync(new RerankModelCall(documents, "rainy day", 2, options.RootElement.Clone(), null, CancellationToken.None), CancellationToken.None);
    }

    private static TogetherAIImageModel Image(UpstreamCapture capture, string modelId)
    {
        return (TogetherAIImageModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel(modelId);
    }

    private static UpstreamCapture Stream()
    {
        return new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"delta\":{\"content\":\"Hi\"},\"finish_reason\":\"stop\"}]}"),
        };
    }

    private static void WithEnvironment(string name, string? value, Action action)
    {
        var previous = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
        try
        {
            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, previous);
        }
    }
}
