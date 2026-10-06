// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Prodia;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

/// <summary>Prodia image, language, and video models over the multipart job API.</summary>
public sealed class ProdiaParityTests
{
    private const string ProviderTest = "packages/prodia/src/prodia-provider.test.ts::Prodia provider::";
    private const string Image = "packages/prodia/src/prodia-image-model.test.ts::ProdiaImageModel > doGenerate::";
    private const string ImageConstructor = "packages/prodia/src/prodia-image-model.test.ts::ProdiaImageModel > constructor::";
    private const string Language = "packages/prodia/src/prodia-language-model.test.ts::ProdiaLanguageModel > doGenerate::";
    private const string LanguageConstructor = "packages/prodia/src/prodia-language-model.test.ts::ProdiaLanguageModel > constructor::";
    private const string LanguageStream = "packages/prodia/src/prodia-language-model.test.ts::ProdiaLanguageModel > doStream::";
    private const string TextToVideo = "packages/prodia/src/prodia-video-model.test.ts::ProdiaVideoModel > doGenerate - txt2vid::";
    private const string ImageToVideo = "packages/prodia/src/prodia-video-model.test.ts::ProdiaVideoModel > doGenerate - img2vid::";
    private const string VideoConstructor = "packages/prodia/src/prodia-video-model.test.ts::ProdiaVideoModel > constructor::";

    private const string JobUrl = "https://api.example.com/v2/job?price=true";
    private const string ImageModelId = "inference.flux-fast.schnell.txt2img.v2";
    private const string LanguageModelId = "inference.nano-banana.img2img.v2";
    private const string VideoModelId = "inference.wan2-2.lightning.txt2vid.v0";
    private const string ImageToVideoModelId = "inference.wan2-2.lightning.img2vid.v0";
    private const string ImagePrompt = "A cute baby sea otter";
    private const string LanguagePrompt = "Describe this image";
    private const string VideoPrompt = "A cat walking on a beach";

    private const string ImageJob = "{\"id\":\"job-123\",\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:00:05Z\",\"state\":{\"current\":\"completed\"},\"config\":{\"prompt\":\"A cute baby sea otter\",\"seed\":42},\"metrics\":{\"elapsed\":2.5,\"ips\":10.5},\"price\":{\"product\":\"flux-fast.schnell\",\"dollars\":0.0025}}";
    private const string LanguageJob = "{\"id\":\"job-lang-123\",\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:00:03Z\",\"state\":{\"current\":\"completed\"},\"config\":{\"prompt\":\"Describe this image\",\"seed\":7},\"metrics\":{\"elapsed\":1.5,\"ips\":20.0},\"price\":{\"product\":\"nano-banana\",\"dollars\":0.01}}";
    private const string VideoJob = "{\"id\":\"job-vid-123\",\"created_at\":\"2025-01-01T00:00:00Z\",\"updated_at\":\"2025-01-01T00:00:10Z\",\"state\":{\"current\":\"completed\"},\"config\":{\"prompt\":\"A cat walking on a beach\",\"seed\":99},\"metrics\":{\"elapsed\":5.0,\"ips\":3.2},\"price\":{\"product\":\"wan2-2.lightning\",\"dollars\":0.05}}";

    [Fact]
    [UpstreamTest(ProviderTest + "creates image models via .image and .imageModel", Coverage = UpstreamCoverage.Covered)]
    public void CreatesImageModels()
    {
        var provider = ProdiaProvider.Create();
        var model = provider.ImageModel(ImageModelId);
        Assert.Equal("prodia.image", model.Provider);
        Assert.Equal(ImageModelId, model.ModelId);
        Assert.Equal("inference.flux.schnell.txt2img.v2", provider.ImageModel("inference.flux.schnell.txt2img.v2").ModelId);
    }

    [Fact]
    [UpstreamTest(ProviderTest + "creates language models via .languageModel", Coverage = UpstreamCoverage.Covered)]
    public void CreatesLanguageModels()
    {
        var model = ProdiaProvider.Create().LanguageModel(LanguageModelId);
        Assert.Equal("prodia.language", model.Provider);
        Assert.Equal(LanguageModelId, model.ModelId);
        Assert.Equal("V4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(ProviderTest + "creates video models via .video and .videoModel", Coverage = UpstreamCoverage.Covered)]
    public void CreatesVideoModels()
    {
        var provider = ProdiaProvider.Create();
        var model = (ProdiaVideoModel)provider.VideoModel(VideoModelId);
        Assert.Equal("prodia.video", model.Provider);
        Assert.Equal(VideoModelId, model.ModelId);
        Assert.Equal(ImageToVideoModelId, provider.VideoModel(ImageToVideoModelId).ModelId);
    }

    [Fact]
    [UpstreamTest(ProviderTest + "configures baseURL and headers correctly", Coverage = UpstreamCoverage.Covered)]
    public async Task ConfiguresBaseUrlAndHeaders()
    {
        var handler = new ParityHandler(_ => Multipart("{\"id\":\"job-123\",\"state\":{\"current\":\"completed\"},\"config\":{\"prompt\":\"test\"}}", Output("output.png", "image/png", "test-image")));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key", BaseUrl = "https://api.example.com/v2" };
        options.Headers["x-extra-header"] = "extra";
        var model = (ProdiaImageModel)ProdiaProvider.Create(options, handler).ImageModel(ImageModelId);

        await model.DoGenerateAsync(ImageCall("A serene mountain landscape at sunset"), CancellationToken.None).ConfigureAwait(false);

        var call = Assert.Single(handler.Calls);
        Assert.Equal(JobUrl, call.Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Post, call.Method);
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.Equal("extra", call.Header("x-extra-header"));
        Assert.Equal("multipart/form-data; image/png", call.Header("Accept"));
        JsonAssert.Equal(JsonNode.Parse(call.Text), "{\"type\":\"inference.flux-fast.schnell.txt2img.v2\",\"config\":{\"prompt\":\"A serene mountain landscape at sunset\"}}");
        Assert.Contains("ai-sdk/prodia/", call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(
        ProviderTest + "throws NoSuchModelError for unsupported model types",
        Coverage = UpstreamCoverage.Partial,
        Note = "EmbeddingModel throws AiSdkException. The port has no NoSuchModelError.")]
    public void ThrowsForEmbeddingModels()
    {
        Assert.Throws<AiSdkException>(() => ProdiaProvider.Create().EmbeddingModel("some-id"));
    }

    [Fact]
    [UpstreamTest(Image + "passes the correct parameters including providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task ImagePassesParametersAndProviderOptions()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(seed: 12345, options: "{\"steps\":4}"), CancellationToken.None).ConfigureAwait(false);
        AssertImageBody(handler, "{\"prompt\":\"A cute baby sea otter\",\"seed\":12345,\"steps\":4}");
    }

    [Fact]
    [UpstreamTest(Image + "includes width and height when size is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageIncludesSize()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(size: "1024x768"), CancellationToken.None).ConfigureAwait(false);
        AssertImageBody(handler, "{\"prompt\":\"A cute baby sea otter\",\"width\":1024,\"height\":768}");
    }

    [Fact]
    [UpstreamTest(Image + "provider options width/height take precedence over size", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageOptionsSizeWins()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(size: "1024x768", options: "{\"width\":512,\"height\":512}"), CancellationToken.None).ConfigureAwait(false);
        AssertImageBody(handler, "{\"prompt\":\"A cute baby sea otter\",\"width\":512,\"height\":512}");
    }

    [Fact]
    [UpstreamTest(Image + "includes style_preset when stylePreset is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageIncludesStylePreset()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(options: "{\"stylePreset\":\"anime\"}"), CancellationToken.None).ConfigureAwait(false);
        AssertImageBody(handler, "{\"prompt\":\"A cute baby sea otter\",\"style_preset\":\"anime\"}");
    }

    [Fact]
    [UpstreamTest(Image + "includes loras when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageIncludesLoras()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(options: "{\"loras\":[\"prodia/lora/flux/anime@v1\",\"prodia/lora/flux/realism@v1\"]}"), CancellationToken.None).ConfigureAwait(false);
        AssertImageBody(handler, "{\"prompt\":\"A cute baby sea otter\",\"loras\":[\"prodia/lora/flux/anime@v1\",\"prodia/lora/flux/realism@v1\"]}");
    }

    [Fact]
    [UpstreamTest(Image + "includes progressive when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageIncludesProgressive()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(options: "{\"progressive\":true}"), CancellationToken.None).ConfigureAwait(false);
        AssertImageBody(handler, "{\"prompt\":\"A cute baby sea otter\",\"progressive\":true}");
    }

    [Fact]
    [UpstreamTest(Image + "calls the correct endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageCallsJobEndpoint()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal(JobUrl, handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(Image + "sends Accept: multipart/form-data header", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageSendsMultipartAccept()
    {
        var handler = ImageHandler();
        await ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("multipart/form-data; image/png", handler.Calls[0].Header("Accept"));
    }

    [Fact]
    [UpstreamTest(Image + "merges provider and request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageMergesHeaders()
    {
        var handler = ImageHandler();
        var call = new ImageModelCall(ImagePrompt, null, null, 1, null, null, null, Json("{}"), new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }, CancellationToken.None);
        await ImageModel(handler, "provider-header-value").DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);

        var request = handler.Calls[0];
        Assert.Equal("application/json", request.Header("Content-Type"));
        Assert.Equal("provider-header-value", request.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", request.Header("Custom-Request-Header"));
        Assert.Equal("Bearer test-key", request.Header("Authorization"));
        Assert.Equal("multipart/form-data; image/png", request.Header("Accept"));
    }

    [Fact]
    [UpstreamTest(Image + "returns image bytes from multipart response", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageReturnsBytes()
    {
        var result = await ImageModel(ImageHandler()).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("test-binary-content", Encoding.UTF8.GetString(Assert.IsType<byte[]>(Assert.Single(result.Images))));
    }

    [Fact]
    [UpstreamTest(Image + "returns provider metadata from job result", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageReturnsMetadata()
    {
        var result = await ImageModel(ImageHandler()).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value.GetProperty("prodia"),
            "{\"images\":[{\"jobId\":\"job-123\",\"seed\":42,\"elapsed\":2.5,\"iterationsPerSecond\":10.5,\"createdAt\":\"2025-01-01T00:00:00Z\",\"updatedAt\":\"2025-01-01T00:00:05Z\",\"dollars\":0.0025}]}");
    }

    [Theory]
    [UpstreamTest(Image + "omits optional metadata fields when not present in job result", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Image + "omits dollars from metadata when price is absent", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Image + "omits dollars from metadata when price is null", Coverage = UpstreamCoverage.Covered)]
    [UpstreamTest(Image + "includes dollars in metadata when price is present", Coverage = UpstreamCoverage.Covered)]
    [InlineData("", "")]
    [InlineData(",\"price\":null", "")]
    [InlineData(",\"price\":{\"product\":\"flux-fast.schnell\",\"dollars\":0.005}", ",\"dollars\":0.005")]
    public async Task ImageMetadataFollowsJobFields(string price, string dollars)
    {
        var handler = new ParityHandler(_ => Multipart("{\"id\":\"job-456\",\"state\":{\"current\":\"completed\"},\"config\":{\"prompt\":\"A cute baby sea otter\"}" + price + "}", Output("output.png", "image/png", "test-binary-content")));
        var result = await ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("prodia"), "{\"images\":[{\"jobId\":\"job-456\"" + dollars + "}]}");
    }

    [Fact]
    [UpstreamTest(Image + "warns on invalid size format", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageWarnsOnInvalidSize()
    {
        var result = await ImageModel(ImageHandler()).DoGenerateAsync(ImageCall(size: "invalid"), CancellationToken.None).ConfigureAwait(false);
        var warning = Assert.Single(result.Warnings);
        Assert.Equal("unsupported", warning.Type);
        Assert.Equal("size", warning.Feature);
        Assert.Equal("Invalid size format: invalid. Expected format: WIDTHxHEIGHT (e.g., 1024x1024)", warning.Details);
    }

    [Fact]
    [UpstreamTest(
        Image + "handles API errors",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the message and status. ApiException does not carry the request URL.")]
    public async Task ImageHandlesApiErrors()
    {
        var handler = new ParityHandler(_ => Error());
        var error = await Assert.ThrowsAsync<BadRequestException>(() => ImageModel(handler).DoGenerateAsync(ImageCall(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Prompt cannot be empty", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Image + "includes timestamp, headers, and modelId in response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task ImageResponseMetadata()
    {
        var date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = new ProdiaImageModel(Provider(ImageHandler()), ImageModelId, () => date);
        var result = await model.DoGenerateAsync(ImageCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(date.UtcDateTime, result.Response.Timestamp);
        Assert.Equal(ImageModelId, result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(ImageConstructor + "exposes correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void ImageModelInformation()
    {
        var model = ImageModel(ImageHandler());
        Assert.Equal("prodia.image", model.Provider);
        Assert.Equal(ImageModelId, model.ModelId);
        Assert.Equal(1, model.MaxImagesPerCall);
    }

    [Fact]
    [UpstreamTest(LanguageConstructor + "exposes correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void LanguageModelInformation()
    {
        var model = LanguageModel(LanguageHandler());
        Assert.Equal("prodia.language", model.Provider);
        Assert.Equal(LanguageModelId, model.ModelId);
        Assert.Equal("V4", model.SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(Language + "extracts text from user message and sends correct request", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageSendsMultipartRequest()
    {
        var handler = LanguageHandler();
        await LanguageModel(handler).DoGenerateAsync(Messages(new FileContentPart("image/png", null, new byte[] { 1, 2, 3 }, null), new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Contains("multipart/form-data", handler.Calls[0].Header("Content-Type"), StringComparison.Ordinal);
        Assert.Equal(LanguagePrompt, Job(handler)["config"]!["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Language + "routes top-level-only \"image\" mediaType into multipart input with detected full MIME", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageDetectsInputMediaType()
    {
        var handler = LanguageHandler();
        var png = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
        await LanguageModel(handler).DoGenerateAsync(Messages(new FileContentPart("image", null, png, null), new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("image/png", Part(handler, "input").ContentType);
    }

    [Fact]
    [UpstreamTest(Language + "top-level-only \"image\" mediaType with undetectable bytes keeps default content-type", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageKeepsDefaultInputMediaType()
    {
        var handler = LanguageHandler();
        await LanguageModel(handler).DoGenerateAsync(Messages(new FileContentPart("image", null, new byte[] { 0x00, 0x01, 0x02 }, null), new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("image/png", Part(handler, "input").ContentType);
    }

    [Fact]
    [UpstreamTest(Language + "includes system message in prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguagePrependsSystemMessage()
    {
        var handler = LanguageHandler();
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new SystemModelMessage("You are an art critic."), new UserModelMessage("Describe this.") },
        };
        await LanguageModel(handler).DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal("You are an art critic.\nDescribe this.", Job(handler)["config"]!["prompt"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Language + "sends include_messages: true in config", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageIncludesMessages()
    {
        var handler = LanguageHandler();
        await LanguageModel(handler).DoGenerateAsync(Messages(new TextContentPart("Hello")), CancellationToken.None).ConfigureAwait(false);
        Assert.True(Job(handler)["config"]!["include_messages"]!.GetValue<bool>());
    }

    [Fact]
    [UpstreamTest(Language + "returns text content from message.txt response part", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageReturnsText()
    {
        var result = await LanguageModel(LanguageHandler()).DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("This is a beautiful landscape.", Assert.Single(result.Content.OfType<GeneratedText>()).Text);
    }

    [Fact]
    [UpstreamTest(Language + "returns image content from image.png response part", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageReturnsImage()
    {
        var result = await LanguageModel(LanguageHandler()).DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        var file = Assert.Single(result.Content.OfType<GeneratedFile>());
        Assert.Equal("image/png", file.MediaType);
        Assert.Equal("test-image-bytes", Encoding.UTF8.GetString(file.Data));
    }

    [Fact]
    [UpstreamTest(Language + "returns finish reason as stop", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageFinishesWithStop()
    {
        var result = await LanguageModel(LanguageHandler()).DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Null(result.RawFinishReason);
    }

    [Fact]
    [UpstreamTest(Language + "returns provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageReturnsMetadata()
    {
        var result = await LanguageModel(LanguageHandler()).DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value.GetProperty("prodia"),
            "{\"jobId\":\"job-lang-123\",\"seed\":7,\"elapsed\":1.5,\"iterationsPerSecond\":20.0,\"createdAt\":\"2025-01-01T00:00:00Z\",\"updatedAt\":\"2025-01-01T00:00:03Z\",\"dollars\":0.01}");
    }

    [Fact]
    [UpstreamTest(Language + "emits warnings for unsupported LLM features", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageWarnsOnUnsupportedSettings()
    {
        var options = Messages(new TextContentPart(LanguagePrompt));
        options.Temperature = 0.5;
        options.TopP = 0.9;
        options.TopK = 40;
        options.Seed = 42;
        options.MaxOutputTokens = 1000;
        options.StopSequences = new[] { "stop" };
        options.PresencePenalty = 0.1;
        options.FrequencyPenalty = 0.2;
        options.Tools = new[] { new LanguageModelTool("test", null, Json("{}")) };
        options.ToolChoice = ToolChoice.Auto;
        options.JsonSchema = Json("{}");
        options.Reasoning = "medium";

        var result = await LanguageModel(LanguageHandler()).DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);

        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
        Assert.Equal(
            new[] { "temperature", "topP", "topK", "seed", "maxOutputTokens", "stopSequences", "presencePenalty", "frequencyPenalty", "tools", "toolChoice", "responseFormat", "reasoning" },
            result.Warnings.Select(warning => warning.Message));
    }

    [Fact]
    [UpstreamTest(Language + "passes aspectRatio from provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguagePassesAspectRatio()
    {
        var handler = LanguageHandler();
        var options = Messages(new TextContentPart(LanguagePrompt));
        options.ProviderOptions = new Dictionary<string, JsonElement> { ["prodia"] = Json("{\"aspectRatio\":\"16:9\"}") };
        await LanguageModel(handler).DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("16:9", Job(handler)["config"]!["aspect_ratio"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(Language + "merges provider and request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageMergesHeaders()
    {
        var handler = LanguageHandler();
        var options = Messages(new TextContentPart(LanguagePrompt));
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-value" };
        await LanguageModel(handler, "provider-value").DoGenerateAsync(options, CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("provider-value", handler.Calls[0].Header("Custom-Provider-Header"));
        Assert.Equal("request-value", handler.Calls[0].Header("Custom-Request-Header"));
        Assert.Equal("Bearer test-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(Language + "includes timestamp and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageResponseMetadata()
    {
        var date = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var model = new ProdiaLanguageModel(Provider(LanguageHandler()), LanguageModelId, () => date);
        var result = await model.DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(date, result.ResponseTimestamp);
        Assert.Equal(LanguageModelId, result.ResponseModelId);
        Assert.NotEmpty(result.ResponseHeaders);
    }

    [Fact]
    [UpstreamTest(Language + "handles API errors", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageHandlesApiErrors()
    {
        var handler = new ParityHandler(_ => Error("{\"message\":\"Bad request\",\"detail\":\"Missing input image\"}"));
        var error = await Assert.ThrowsAsync<BadRequestException>(() => LanguageModel(handler).DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Missing input image", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(Language + "handles response with text only (no image)", Coverage = UpstreamCoverage.Covered)]
    public async Task LanguageHandlesTextOnly()
    {
        var handler = new ParityHandler(_ => Multipart(LanguageJob, Output("message.txt", "text/plain", "Just a text response")));
        var result = await LanguageModel(handler).DoGenerateAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false);
        Assert.IsType<GeneratedText>(Assert.Single(result.Content));
    }

    [Fact]
    [UpstreamTest(
        LanguageStream + "wraps doGenerate result into stream parts",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks stream-start, response metadata, the text parts, and finish. The port has no file stream part, so the image is not streamed.")]
    public async Task LanguageStreamsGenerateResult()
    {
        var handler = new ParityHandler(_ => Multipart(LanguageJob, Output("message.txt", "text/plain", "Stream test response"), Output("image.png", "image/png", "stream-image-bytes")));
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in LanguageModel(handler).DoStreamAsync(Messages(new TextContentPart(LanguagePrompt)), CancellationToken.None).ConfigureAwait(false))
        {
            parts.Add(part);
        }

        Assert.Equal(new[] { "stream-start", "response-metadata", "text-start", "text-delta", "text-end", "finish" }, parts.Select(part => part.Type));
        Assert.Equal("Stream test response", ((TextDeltaStreamPart)parts[3]).Delta);
        Assert.Equal(FinishReason.Stop, ((FinishStreamPart)parts[5]).FinishReason);
    }

    [Fact]
    [UpstreamTest(VideoConstructor + "exposes correct provider and model information", Coverage = UpstreamCoverage.Covered)]
    public void VideoModelInformation()
    {
        var model = VideoModel(VideoHandler());
        Assert.Equal("prodia.video", model.Provider);
        Assert.Equal(VideoModelId, model.ModelId);
        Assert.Equal(1, model.MaxVideosPerCall);
    }

    [Fact]
    [UpstreamTest(TextToVideo + "sends correct JSON request body with prompt", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoSendsPrompt()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"type\":\"inference.wan2-2.lightning.txt2vid.v0\",\"config\":{\"prompt\":\"A cat walking on a beach\"}}");
    }

    [Fact]
    [UpstreamTest(TextToVideo + "includes seed when provided", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoIncludesSeed()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoGenerateAsync(VideoCall(seed: 42), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"type\":\"inference.wan2-2.lightning.txt2vid.v0\",\"config\":{\"prompt\":\"A cat walking on a beach\",\"seed\":42}}");
    }

    [Fact]
    [UpstreamTest(TextToVideo + "includes resolution from provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoIncludesResolution()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoGenerateAsync(VideoCall(options: "{\"prodia\":{\"resolution\":\"720p\"}}"), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"type\":\"inference.wan2-2.lightning.txt2vid.v0\",\"config\":{\"prompt\":\"A cat walking on a beach\",\"resolution\":\"720p\"}}");
    }

    [Fact]
    [UpstreamTest(TextToVideo + "calls the correct endpoint", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoCallsJobEndpoint()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Equal(JobUrl, handler.Calls[0].Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest(TextToVideo + "sends correct Accept header", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoSendsAccept()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("multipart/form-data; video/mp4", handler.Calls[0].Header("Accept"));
    }

    [Fact]
    [UpstreamTest(TextToVideo + "sends Content-Type: application/json for txt2vid", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoSendsJsonContentType()
    {
        var handler = VideoHandler();
        await VideoModel(handler).DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("application/json", handler.Calls[0].Header("Content-Type"));
    }

    [Fact]
    [UpstreamTest(TextToVideo + "merges provider and request headers", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoMergesHeaders()
    {
        var handler = VideoHandler();
        var model = new ProdiaVideoModel(Provider(handler, "provider-value"), VideoModelId);
        await model.DoGenerateAsync(VideoCall(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-value" }), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("provider-value", handler.Calls[0].Header("Custom-Provider-Header"));
        Assert.Equal("request-value", handler.Calls[0].Header("Custom-Request-Header"));
        Assert.Equal("Bearer test-key", handler.Calls[0].Header("Authorization"));
    }

    [Fact]
    [UpstreamTest(TextToVideo + "returns video data from multipart response", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoReturnsBytes()
    {
        var result = await VideoModel(VideoHandler()).DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        var video = Assert.Single(result.Videos);
        Assert.Equal("binary", video.Type);
        Assert.Equal("video/mp4", video.MediaType);
        Assert.Equal("test-video-content", Encoding.UTF8.GetString((byte[])video.Data!));
    }

    [Fact]
    [UpstreamTest(TextToVideo + "returns provider metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoReturnsMetadata()
    {
        var result = await VideoModel(VideoHandler()).DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        JsonAssert.Equal(
            result.ProviderMetadata!.Value.GetProperty("prodia"),
            "{\"videos\":[{\"jobId\":\"job-vid-123\",\"seed\":99,\"elapsed\":5.0,\"iterationsPerSecond\":3.2,\"createdAt\":\"2025-01-01T00:00:00Z\",\"updatedAt\":\"2025-01-01T00:00:10Z\",\"dollars\":0.05}]}");
    }

    [Fact]
    [UpstreamTest(TextToVideo + "includes timestamp and modelId in response", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoResponseMetadata()
    {
        var date = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var model = new ProdiaVideoModel(Provider(VideoHandler()), VideoModelId, () => date);
        var result = await model.DoGenerateAsync(VideoCall(), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(date.UtcDateTime, result.Response.Timestamp);
        Assert.Equal(VideoModelId, result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }

    [Fact]
    [UpstreamTest(
        TextToVideo + "handles API errors",
        Coverage = UpstreamCoverage.Partial,
        Note = "Checks the message and status. ApiException does not carry the request URL.")]
    public async Task VideoHandlesApiErrors()
    {
        var handler = new ParityHandler(_ => Error());
        var error = await Assert.ThrowsAsync<BadRequestException>(() => VideoModel(handler).DoGenerateAsync(VideoCall(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Equal("Prompt cannot be empty", error.Message);
        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    [UpstreamTest(ImageToVideo + "sends multipart form-data when image is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoSendsImageAsMultipart()
    {
        var handler = VideoHandler();
        var model = new ProdiaVideoModel(Provider(handler), ImageToVideoModelId);
        await model.DoGenerateAsync(VideoCall(image: VideoModelFile.FromFile(new byte[] { 1, 2, 3, 4 }, "image/png")), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(HttpMethod.Post, handler.Calls[0].Method);
        Assert.Contains("multipart/form-data", handler.Calls[0].Header("Content-Type"), StringComparison.Ordinal);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, Part(handler, "input").Body);
    }

    [Fact]
    [UpstreamTest(ImageToVideo + "downloads a public image URL and sends it as multipart form-data", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoDownloadsPublicImage()
    {
        var handler = new ParityHandler(call => call.Uri.Host == "cdn.example.com"
            ? ParityHandler.Bytes(Encoding.UTF8.GetBytes("input-image-bytes"), "image/png")
            : Multipart(VideoJob, Output("output.mp4", "video/mp4", "test-video-content")));
        var model = new ProdiaVideoModel(Provider(handler), ImageToVideoModelId);
        await model.DoGenerateAsync(VideoCall(image: VideoModelFile.FromUrl("https://cdn.example.com/input.png")), CancellationToken.None).ConfigureAwait(false);

        Assert.Contains(handler.Calls, call => call.Uri.AbsoluteUri == "https://cdn.example.com/input.png");
        var job = Assert.Single(handler.Calls, call => call.Uri.AbsoluteUri == JobUrl);
        Assert.Equal(HttpMethod.Post, job.Method);
        Assert.Contains("multipart/form-data", job.Header("Content-Type"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(ImageToVideo + "blocks an image URL pointing at a private address (SSRF guard)", Coverage = UpstreamCoverage.Covered)]
    public async Task VideoBlocksPrivateImageUrl()
    {
        var handler = VideoHandler();
        var model = new ProdiaVideoModel(Provider(handler), ImageToVideoModelId);
        await Assert.ThrowsAsync<DownloadError>(() => model.DoGenerateAsync(VideoCall(image: VideoModelFile.FromUrl("http://169.254.169.254/latest/meta-data/")), CancellationToken.None)).ConfigureAwait(false);
        Assert.Empty(handler.Calls);
    }

    private static ProdiaProvider Provider(ParityHandler handler, string? providerHeader = null)
    {
        var options = new OpenAICompatibleOptions { ApiKey = "test-key", BaseUrl = "https://api.example.com/v2" };
        if (providerHeader != null)
        {
            options.Headers["Custom-Provider-Header"] = providerHeader;
        }

        return ProdiaProvider.Create(options, handler);
    }

    private static ProdiaImageModel ImageModel(ParityHandler handler, string? providerHeader = null)
    {
        return (ProdiaImageModel)Provider(handler, providerHeader).ImageModel(ImageModelId);
    }

    private static ILanguageModel LanguageModel(ParityHandler handler, string? providerHeader = null)
    {
        return Provider(handler, providerHeader).LanguageModel(LanguageModelId);
    }

    private static ProdiaVideoModel VideoModel(ParityHandler handler)
    {
        return (ProdiaVideoModel)Provider(handler).VideoModel(VideoModelId);
    }

    private static ParityHandler ImageHandler()
    {
        return new ParityHandler(_ => Multipart(ImageJob, Output("output.png", "image/png", "test-binary-content")));
    }

    private static ParityHandler LanguageHandler()
    {
        return new ParityHandler(_ => Multipart(LanguageJob, Output("message.txt", "text/plain", "This is a beautiful landscape."), Output("image.png", "image/png", "test-image-bytes")));
    }

    private static ParityHandler VideoHandler()
    {
        return new ParityHandler(_ => Multipart(VideoJob, Output("output.mp4", "video/mp4", "test-video-content")));
    }

    private static ImageModelCall ImageCall(string prompt = ImagePrompt, string? size = null, int? seed = null, string? options = null)
    {
        return new ImageModelCall(prompt, null, null, 1, size, null, seed, Json(options == null ? "{}" : "{\"prodia\":" + options + "}"), new Dictionary<string, string>(), CancellationToken.None);
    }

    private static VideoModelCall VideoCall(int? seed = null, string options = "{}", VideoModelFile? image = null, IReadOnlyDictionary<string, string>? headers = null)
    {
        return new VideoModelCall(VideoPrompt, 1, null, null, null, null, seed, image, null, null, null, Json(options), headers ?? new Dictionary<string, string>(), CancellationToken.None);
    }

    private static LanguageModelCallOptions Messages(params UserContentPart[] parts)
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage(parts) } };
    }

    private static void AssertImageBody(ParityHandler handler, string config)
    {
        JsonAssert.Equal(JsonNode.Parse(handler.Calls[0].Text), "{\"type\":\"" + ImageModelId + "\",\"config\":" + config + "}");
    }

    private static JsonNode Job(ParityHandler handler)
    {
        return JsonNode.Parse(Part(handler, "job").Body)!;
    }

    /// <summary>Reads one part of the captured multipart request.</summary>
    private static ProdiaMultipartPart Part(ParityHandler handler, string name)
    {
        var contentType = MediaTypeHeaderValue.Parse(handler.Calls[0].Header("Content-Type")!);
        var boundary = contentType.Parameters.Single(parameter => parameter.Name == "boundary").Value!.Trim('"');
        return ProdiaApi.ParseMultipart(handler.Calls[0].Body, boundary).Single(part => part.ContentDisposition.Contains("name=" + name + ";") || part.ContentDisposition.Contains("name=\"" + name + "\""));
    }

    private static (string FileName, string ContentType, string Body) Output(string fileName, string contentType, string body)
    {
        return (fileName, contentType, body);
    }

    private static HttpResponseMessage Multipart(string job, params (string FileName, string ContentType, string Body)[] outputs)
    {
        const string boundary = "test-boundary-12345";
        var body = new StringBuilder()
            .Append("--").Append(boundary).Append("\r\n")
            .Append("Content-Disposition: form-data; name=\"job\"; filename=\"job.json\"\r\n")
            .Append("Content-Type: application/json\r\n\r\n")
            .Append(job).Append("\r\n");
        foreach (var output in outputs)
        {
            body.Append("--").Append(boundary).Append("\r\n")
                .Append("Content-Disposition: form-data; name=\"output\"; filename=\"").Append(output.FileName).Append("\"\r\n")
                .Append("Content-Type: ").Append(output.ContentType).Append("\r\n\r\n")
                .Append(output.Body).Append("\r\n");
        }

        body.Append("--").Append(boundary).Append("--\r\n");
        return ParityHandler.Bytes(Encoding.UTF8.GetBytes(body.ToString()), "multipart/form-data; boundary=" + boundary);
    }

    private static HttpResponseMessage Error(string body = "{\"message\":\"Invalid prompt\",\"detail\":\"Prompt cannot be empty\"}")
    {
        var response = ParityHandler.Json(body);
        response.StatusCode = HttpStatusCode.BadRequest;
        return response;
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
