// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Net;
using System.Text;
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
    private const string Prompt = "A cute baby sea otter";

    private const string DiffusionModel = "stabilityai/stable-diffusion-xl";

    private const string ImageResponse = "{\"id\":\"test-id\",\"data\":[{\"index\":0,\"b64_json\":\"dGVzdA==\"}],\"model\":\"stabilityai/stable-diffusion-xl\",\"object\":\"list\"}";

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

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should omit diffusion options for non-diffusion models", Coverage = UpstreamCoverage.Partial, Note = "Upstream also warns about aspectRatio whenever size is set; .NET warns only when AspectRatio is set.")]
    public async Task Non_diffusion_models_drop_diffusion_options_and_seed()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Image(capture, "google/gemini-3-pro-image");
        model.Seed = 42;
        model.ProviderOptions = Options("{\"steps\":20,\"guidance\":3.5,\"negative_prompt\":\"blurry\",\"disable_safety_checker\":true,\"additional_param\":\"value\"}");
        await model.DoGenerateAsync(new ImageCallOptions(Prompt) { Size = "1264x848" }, CancellationToken.None);
        Assert.Equal(
            "{\"model\":\"google/gemini-3-pro-image\",\"prompt\":\"A cute baby sea otter\",\"response_format\":\"base64\",\"width\":1264,\"height\":848,\"additional_param\":\"value\"}",
            capture.Requests[0].Body);
        var warning = Assert.Single(model.LastWarnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("The google/gemini-3-pro-image model does not support the `seed` option.", warning.Message);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should handle API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_errors_use_the_provider_message()
    {
        var capture = new UpstreamCapture { Status = HttpStatusCode.BadRequest, ResponseBody = "{\"error\":{\"message\":\"Bad Request\"}}" };
        var error = await Assert.ThrowsAnyAsync<ApiException>(() => Image(capture, DiffusionModel).DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None));
        Assert.Equal("Bad Request", error.Message);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate::should respect the abort signal", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_honors_cancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, new PendingHandler());
        var generating = provider.ImageModel(DiffusionModel).DoGenerateAsync(new ImageCallOptions(Prompt), cancellation.Token);
        cancellation.Cancel();
        await Assert.ThrowsAsync<ApiUserAbortException>(() => generating);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate > response metadata::should include timestamp, headers and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_records_timestamp_headers_and_model_id()
    {
        var stamp = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, new UpstreamCapture { ResponseBody = ImageResponse });
        var model = new TogetherAIImageModel(provider, DiffusionModel, () => stamp);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal(stamp, model.LastResponseTimestamp);
        Assert.Equal(DiffusionModel, model.ModelId);
        Assert.NotEmpty(model.LastResponseHeaders);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::doGenerate > response metadata::should include response headers from API call", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_generation_returns_the_response_headers()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        capture.ResponseHeaders["x-request-id"] = "test-request-id";
        var model = Image(capture, DiffusionModel);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("test-request-id", model.LastResponseHeaders["x-request-id"]);
        Assert.StartsWith("application/json", model.LastResponseHeaders["content-type"], StringComparison.Ordinal);
        Assert.Equal(Encoding.UTF8.GetByteCount(ImageResponse).ToString(CultureInfo.InvariantCulture), model.LastResponseHeaders["content-length"]);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::constructor::should expose correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void Image_model_exposes_provider_model_and_limit()
    {
        var model = Image(new UpstreamCapture(), DiffusionModel);
        Assert.Equal("togetherai.image", model.Provider);
        Assert.Equal(DiffusionModel, model.ModelId);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::Image Editing::should send image_url when URL file is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task A_url_file_is_sent_as_image_url()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Image(capture, DiffusionModel);
        model.Files = new[] { new OpenAICompatibleImageFile("image/jpeg", null, "https://example.com/input.jpg", null) };
        await model.DoGenerateAsync(new ImageCallOptions("Make the shirt yellow"), CancellationToken.None);
        Assert.Equal(
            "{\"model\":\"stabilityai/stable-diffusion-xl\",\"prompt\":\"Make the shirt yellow\",\"response_format\":\"base64\",\"image_url\":\"https://example.com/input.jpg\"}",
            capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::Image Editing::should convert Uint8Array file to data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task File_bytes_are_sent_as_a_data_uri()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Image(capture, DiffusionModel);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, null, null) };
        await model.DoGenerateAsync(new ImageCallOptions("Transform this image"), CancellationToken.None);
        var body = JsonNode.Parse(capture.Requests[0].Body)!;
        Assert.StartsWith("data:image/png;base64,", body["image_url"]!.GetValue<string>(), StringComparison.Ordinal);
        Assert.Equal("Transform this image", body["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::Image Editing::should convert file with base64 string data to data URI", Coverage = UpstreamCoverage.Covered)]
    public async Task Base64_file_data_round_trips_into_the_data_uri()
    {
        const string Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Image(capture, DiffusionModel);
        model.Files = new[] { new OpenAICompatibleImageFile("image/png", Convert.FromBase64String(Png), null, null) };
        await model.DoGenerateAsync(new ImageCallOptions("Edit this"), CancellationToken.None);
        Assert.Equal("data:image/png;base64," + Png, JsonNode.Parse(capture.Requests[0].Body)!["image_url"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-image-model.test.ts::Image Editing::should pass provider options with image editing", Coverage = UpstreamCoverage.Covered)]
    public async Task Provider_options_are_merged_into_an_edit()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Image(capture, DiffusionModel);
        model.Files = new[] { new OpenAICompatibleImageFile("image/jpeg", null, "https://example.com/input.jpg", null) };
        model.ProviderOptions = Options("{\"steps\":28,\"guidance\":3.5}");
        await model.DoGenerateAsync(new ImageCallOptions("Transform the style"), CancellationToken.None);
        Assert.Equal(
            "{\"model\":\"stabilityai/stable-diffusion-xl\",\"prompt\":\"Transform the style\",\"response_format\":\"base64\",\"image_url\":\"https://example.com/input.jpg\",\"steps\":28,\"guidance\":3.5}",
            capture.Requests[0].Body);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should create a TogetherAIProvider instance with default options", Coverage = UpstreamCoverage.Covered)]
    public void Default_options_read_the_together_api_key()
    {
        WithEnvironment("TOGETHER_AI_API_KEY", null, () =>
        {
            WithEnvironment("TOGETHER_API_KEY", "env-key", () =>
            {
                var provider = TogetherAIProvider.Create();
                Assert.Equal("TOGETHER_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
                Assert.Equal("Bearer env-key", provider.CreateHeaders()["Authorization"]);
            });
        });
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should create a TogetherAIProvider instance with custom options", Coverage = UpstreamCoverage.Covered)]
    public void Custom_options_set_the_key_base_url_and_headers()
    {
        var options = new OpenAICompatibleOptions { ApiKey = "custom-key", BaseUrl = "https://custom.url" };
        options.Headers["Custom-Header"] = "value";
        var provider = TogetherAIProvider.Create(options);
        var headers = provider.CreateHeaders();
        Assert.Equal("Bearer custom-key", headers["Authorization"]);
        Assert.Equal("value", headers["Custom-Header"]);
        Assert.Equal("https://custom.url", provider.Options.BaseUrl);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > createTogetherAI::should return a chat model when called as a function", Coverage = UpstreamCoverage.Covered)]
    public void The_default_language_model_is_a_chat_model()
    {
        Assert.IsType<OpenAICompatibleLanguageModel>(TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).LanguageModel("foo-model-id"));
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > chatModel::should construct a chat model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Chat_models_use_the_openai_compatible_chat_model()
    {
        var model = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CreateChatModel("together-chat-model");
        Assert.IsType<OpenAICompatibleLanguageModel>(model);
        Assert.Equal("together-chat-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > completionModel::should construct a completion model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Completion_models_use_the_openai_compatible_completion_model()
    {
        var model = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).CompletionModel("together-completion-model");
        Assert.IsType<OpenAICompatibleCompletionLanguageModel>(model);
        Assert.Equal("together-completion-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > completionModel::should set includeUsage so streaming responses report token usage", Coverage = UpstreamCoverage.Covered)]
    public async Task Completion_streams_request_usage()
    {
        var capture = new UpstreamCapture
        {
            MediaType = "text/event-stream",
            ResponseBody = UpstreamChat.Sse("{\"choices\":[{\"text\":\"Hi\",\"finish_reason\":\"stop\"}]}"),
        };
        var model = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).CompletionModel("together-completion-model");
        await UpstreamChat.Read(model.DoStreamAsync(UpstreamChat.Prompt(), CancellationToken.None));
        Assert.True(UpstreamChat.Body(capture)["stream_options"]!["include_usage"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > embeddingModel::should construct a text embedding model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public void Embedding_models_use_the_openai_compatible_embedding_model()
    {
        var model = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }).EmbeddingModel("together-embedding-model");
        Assert.IsType<OpenAICompatibleEmbeddingModel>(model);
        Assert.Equal("together-embedding-model", model.ModelId);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > image::should construct an image model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_models_use_the_together_image_model_and_base_url()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var model = Assert.IsType<TogetherAIImageModel>(TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).ImageModel(DiffusionModel));
        Assert.Equal("togetherai.image", model.Provider);
        Assert.Equal(DiffusionModel, model.ModelId);
        await model.DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("https://api.together.xyz/v1/images/generations", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > image::should pass custom baseURL to image model", Coverage = UpstreamCoverage.Covered)]
    public async Task Image_models_use_a_custom_base_url()
    {
        var capture = new UpstreamCapture { ResponseBody = ImageResponse };
        var provider = TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret", BaseUrl = "https://custom.url/" }, capture);
        await provider.ImageModel(DiffusionModel).DoGenerateAsync(new ImageCallOptions(Prompt), CancellationToken.None);
        Assert.Equal("https://custom.url/images/generations", capture.Requests[0].Uri!.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/togetherai/src/togetherai-provider.test.ts::TogetherAIProvider > rerankingModel::should construct a reranking model with correct configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Reranking_models_use_the_together_reranking_model_and_base_url()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"results\":[]}" };
        var model = Assert.IsType<TogetherAIRerankingModel>(TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).RerankingModel("Salesforce/Llama-Rank-v1"));
        Assert.Equal("Salesforce/Llama-Rank-v1", model.ModelId);
        await model.DoRerankAsync("query", new[] { "a" }, 1, CancellationToken.None);
        Assert.Equal("https://api.together.xyz/v1/rerank", capture.Requests[0].Uri!.AbsoluteUri);
    }

    private static IReadOnlyDictionary<string, JsonElement> Options(string togetherai)
    {
        using var document = JsonDocument.Parse(togetherai);
        return new Dictionary<string, JsonElement> { ["togetherai"] = document.RootElement.Clone() };
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

    private sealed class PendingHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("The request was not cancelled.");
        }
    }
}
