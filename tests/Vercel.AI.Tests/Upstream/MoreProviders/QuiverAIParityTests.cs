// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.QuiverAI;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests;

/// <summary>Keeps tests that set <c>QUIVERAI_*</c> variables from racing other tests.</summary>
[CollectionDefinition("QuiverAIEnvironment", DisableParallelization = true)]
public sealed class QuiverAIEnvironmentCollection
{
}

/// <summary>QuiverAI SVG generation, vectorization, animation, and editing.</summary>
[Collection("QuiverAIEnvironment")]
public sealed class QuiverAIParityTests
{
    private const string Provider = "packages/quiverai/src/quiverai-provider.test.ts::createQuiverAI::";
    private const string Reference = "packages/quiverai/src/prepare-quiverai-image-reference.test.ts::prepareQuiverAIImageReference::";
    private const string Integration = "packages/quiverai/src/quiverai-generate-image.test.ts::QuiverAI generateImage integration::";
    private const string ErrorHandler = "packages/quiverai/src/quiverai-image-model.test.ts::quiveraiFailedResponseHandler::";

    private const string GenerateSvg = "<svg viewBox=\"0 0 10 10\"><rect width=\"10\" height=\"10\"/></svg>";
    private const string VectorizeSvg = "<svg viewBox=\"0 0 4 4\"><path d=\"M0 0L4 4\"/></svg>";
    private const string AnimateSvg = "<svg viewBox=\"0 0 10 10\"><circle cx=\"5\" cy=\"5\" r=\"4\"><animate attributeName=\"r\" values=\"3;4;3\" dur=\"1200ms\" repeatCount=\"indefinite\"/></circle></svg>";
    private const string EditSvg = "<svg viewBox=\"0 0 10 10\"><rect width=\"10\" height=\"10\" fill=\"blue\"/></svg>";
    private const string SourceUrl = "https://example.com/source.svg";

    private static readonly string[] CanonicalModelIds = { "arrow-1", "arrow-1.1", "arrow-1.1-max", "arrow-2", "arrow-2-telos" };

    [Fact]
    [UpstreamTest(Reference + "prepares URL references", Coverage = UpstreamCoverage.Covered)]
    public void PreparesUrlReferences()
    {
        JsonAssert.Equal(QuiverAIImageReference.Prepare(new Uri("https://example.com/reference.svg")), "{\"url\":\"https://example.com/reference.svg\"}");
        JsonAssert.Equal(QuiverAIImageReference.Prepare("http://example.com/reference.png"), "{\"url\":\"http://example.com/reference.png\"}");
    }

    [Fact]
    [UpstreamTest(Reference + "prepares binary references", Coverage = UpstreamCoverage.Covered)]
    public void PreparesBinaryReferences()
    {
        var svg = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"></svg>");
        JsonAssert.Equal(QuiverAIImageReference.Prepare(svg), "{\"base64\":\"" + Convert.ToBase64String(svg) + "\"}");
    }

    [Fact]
    [UpstreamTest(Reference + "prepares base64 and data URL references", Coverage = UpstreamCoverage.Covered)]
    public void PreparesBase64AndDataUrlReferences()
    {
        var base64 = Base64("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"></svg>");
        JsonAssert.Equal(QuiverAIImageReference.Prepare(base64), "{\"base64\":\"" + base64 + "\"}");
        JsonAssert.Equal(QuiverAIImageReference.Prepare("data:image/svg+xml;base64," + base64), "{\"base64\":\"" + base64 + "\"}");
    }

    [Theory]
    [UpstreamTest(Reference + "rejects invalid string reference input: %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("ftp://example.com/reference.png")]
    [InlineData("not base64!")]
    [InlineData("data:image/bmp;base64,Qk0=")]
    public void RejectsInvalidStringReferences(string input)
    {
        Assert.Throws<InvalidArgumentException>(() => QuiverAIImageReference.Prepare(input));
    }

    [Fact]
    [UpstreamTest(Reference + "rejects unsupported binary reference input", Coverage = UpstreamCoverage.Covered)]
    public void RejectsUnsupportedBinaryReferences()
    {
        Assert.Throws<InvalidArgumentException>(() => QuiverAIImageReference.Prepare(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    [UpstreamTest(Integration + "rejects structurally malformed SVG input before networking", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateImageRejectsMalformedSvg()
    {
        var handler = Handler();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = Model(handler, "arrow-2"),
            Prompt = new ImagePrompt("Make the icon blue.", new object?[] { Encoding.UTF8.GetBytes("<svg><g></svg>") }),
            ProviderOptions = Json("{\"quiverai\":{\"operation\":\"edit\"}}"),
        })).ConfigureAwait(false);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(Integration + "rejects multiple outputs in a single edit request before networking", Coverage = UpstreamCoverage.Covered)]
    public async Task GenerateImageRejectsMultipleEditOutputs()
    {
        var handler = Handler();
        await Assert.ThrowsAsync<InvalidArgumentException>(() => GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = Model(handler, "arrow-2"),
            Prompt = new ImagePrompt("Make the icon blue.", new object?[] { Encoding.UTF8.GetBytes("<svg><rect width=\"10\" height=\"10\"/></svg>") }),
            N = 2,
            ProviderOptions = Json("{\"quiverai\":{\"operation\":\"edit\"}}"),
        })).ConfigureAwait(false);
        Assert.Empty(handler.Calls);
    }

    [Fact]
    [UpstreamTest(
        Integration + "generates multiple edits through separate requests (middleware: %s)",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers the case without middleware. The port has no image model middleware.")]
    public async Task GenerateImageSplitsEditsIntoSeparateRequests()
    {
        const string source = "<svg><rect width=\"10\" height=\"10\"/></svg>";
        var handler = Handler();
        var result = await GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = Model(handler, "arrow-2"),
            Prompt = new ImagePrompt("Make the icon blue.", new object?[] { Encoding.UTF8.GetBytes(source) }),
            N = 2,
            MaxImagesPerCall = 1,
            ProviderOptions = Json("{\"quiverai\":{\"operation\":\"edit\"}}"),
        }).ConfigureAwait(false);

        Assert.Equal(2, result.Images.Count);
        Assert.All(result.Images, image => Assert.Equal(EditSvg, Encoding.UTF8.GetString(image.Data)));
        Assert.Equal(2, handler.Calls.Count);
        Assert.Equal("https://api.quiver.ai/v1/svgs/edits", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(Body(handler, 0), "{\"model\":\"arrow-2\",\"prompt\":\"Make the icon blue.\",\"svg_source\":{\"base64\":\"" + Base64(source) + "\"},\"stream\":false}");
        Assert.Equal(handler.Calls[0].Text, handler.Calls[1].Text);
    }

    [Fact]
    [UpstreamTest(ErrorHandler + "maps QuiverAI error envelopes into API call errors", Coverage = UpstreamCoverage.Covered)]
    public async Task MapsErrorEnvelopes()
    {
        var handler = new ParityHandler(_ => Status(429, "{\"status\":429,\"code\":\"rate_limit\",\"message\":\"Slow down.\",\"request_id\":\"req_1\"}"));
        var error = await Assert.ThrowsAsync<RateLimitException>(() => Model(handler, "arrow-1").DoGenerateAsync(Call(), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("Slow down.", error.Message, StringComparison.Ordinal);
        Assert.Equal(429, error.StatusCode);
        Assert.True(ProviderUtils.ProviderHttp.IsRetryable(error.StatusCode));
        var data = JsonNode.Parse(error.ResponseBody!)!;
        Assert.Equal("rate_limit", data["code"]!.GetValue<string>());
        Assert.Equal("req_1", data["request_id"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(ErrorHandler + "marks client errors as non-retryable", Coverage = UpstreamCoverage.Covered)]
    public async Task MarksClientErrorsNonRetryable()
    {
        var handler = new ParityHandler(_ => Status(400, "{\"status\":400,\"code\":\"bad_request\",\"message\":\"Prompt is invalid.\",\"request_id\":\"req_2\"}"));
        var error = await Assert.ThrowsAsync<BadRequestException>(() => Model(handler, "arrow-1").DoGenerateAsync(Call(), CancellationToken.None)).ConfigureAwait(false);
        Assert.False(ProviderUtils.ProviderHttp.IsRetryable(error.StatusCode));
        Assert.Equal(400, error.StatusCode);
        Assert.Contains("Prompt is invalid.", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "uses the default base URL and auth headers", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesDefaultBaseUrlAndAuthHeaders()
    {
        var handler = Handler();
        var result = await Model(handler, "arrow-1").DoGenerateAsync(Call(), CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(GenerateSvg, Encoding.UTF8.GetString(Assert.IsType<byte[]>(Assert.Single(result.Images))));
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("quiverai"), "{\"images\":[{\"index\":0,\"mimeType\":\"image/svg+xml\"}]}");
        Assert.Equal(12, result.Usage!.InputTokens);
        Assert.Equal(9, result.Usage.OutputTokens);
        Assert.Equal(21, result.Usage.TotalTokens);
        var call = Assert.Single(handler.Calls);
        Assert.Equal("https://api.quiver.ai/v1/svgs/generations", call.Uri.AbsoluteUri);
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.StartsWith("application/json", call.Header("Content-Type"), StringComparison.Ordinal);
        Assert.Contains("ai-sdk/quiverai/", call.Header("User-Agent"), StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "reads the base URL and API key from the environment", Coverage = UpstreamCoverage.Covered)]
    public async Task ReadsBaseUrlAndApiKeyFromEnvironment()
    {
        await WithEnvironment("env-api-key", "https://env.quiver.ai/v1", async () =>
        {
            var handler = Handler();
            await QuiverAIProvider.Create(null, handler).ImageModel("arrow-1").DoGenerateAsync(new ImageCallOptions("Draw a square icon."), CancellationToken.None).ConfigureAwait(false);
            var call = Assert.Single(handler.Calls);
            Assert.Equal("https://env.quiver.ai/v1/svgs/generations", call.Uri.AbsoluteUri);
            Assert.Equal("Bearer env-api-key", call.Header("Authorization"));
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "throws when the QuiverAI API key is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsWhenApiKeyIsMissing()
    {
        await WithEnvironment(null, null, async () =>
        {
            var handler = Handler();
            var model = QuiverAIProvider.Create(null, handler).ImageModel("arrow-1");
            var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(new ImageCallOptions("Draw a square icon."), CancellationToken.None)).ConfigureAwait(false);
            Assert.Equal("QuiverAI API key is missing. Pass it using the 'apiKey' parameter or the QUIVERAI_API_KEY environment variable.", error.Message);
            Assert.Empty(handler.Calls);
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "prefers explicit options and exposes image factory methods", Coverage = UpstreamCoverage.Covered)]
    public async Task PrefersExplicitOptions()
    {
        await WithEnvironment("env-api-key", "https://env.quiver.ai/v1", async () =>
        {
            var handler = Handler();
            var options = new OpenAICompatibleOptions { ApiKey = "override-api-key", BaseUrl = "https://override.quiver.ai/v1" };
            options.Headers["X-QuiverAI-Test"] = "1";
            var provider = QuiverAIProvider.Create(options, handler);
            Assert.Equal("arrow-1", provider.ImageModel("arrow-1").ModelId);
            Assert.Equal("quiverai.image", provider.ImageModel("arrow-1").Provider);

            await provider.ImageModel("arrow-1").DoGenerateAsync(new ImageCallOptions("Draw a square icon."), CancellationToken.None).ConfigureAwait(false);
            var call = Assert.Single(handler.Calls);
            Assert.Equal("https://override.quiver.ai/v1/svgs/generations", call.Uri.AbsoluteUri);
            Assert.Equal("Bearer override-api-key", call.Header("Authorization"));
            Assert.Equal("1", call.Header("X-QuiverAI-Test"));
        }).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(
        Provider + "throws for unsupported language and embedding models",
        Coverage = UpstreamCoverage.Partial,
        Note = "Both factories throw AiSdkException. The port has no NoSuchModelError or textEmbeddingModel alias.")]
    public void ThrowsForUnsupportedModels()
    {
        var provider = CreateProvider(Handler());
        Assert.Throws<AiSdkException>(() => provider.LanguageModel("chat-model"));
        Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("embed-model"));
    }

    [Fact]
    [UpstreamTest(Provider + "supports all canonical Quiver model ids", Coverage = UpstreamCoverage.Covered)]
    public async Task SupportsCanonicalModelIds()
    {
        var handler = Handler();
        var provider = CreateProvider(handler);
        foreach (var modelId in CanonicalModelIds)
        {
            var model = (QuiverAIImageModel)provider.ImageModel(modelId);
            Assert.Equal(modelId, model.ModelId);
            Assert.Equal("quiverai.image", model.Provider);
            var result = await model.DoGenerateAsync(Call(), CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(GenerateSvg, Encoding.UTF8.GetString((byte[])result.Images[0]!));
            Assert.Equal(modelId, result.Response.ModelId);
        }

        Assert.Equal(CanonicalModelIds.Length, handler.Calls.Count);
        Assert.Equal(CanonicalModelIds, handler.Calls.Select(call => JsonNode.Parse(call.Text)!["model"]!.GetValue<string>()));
    }

    [Fact]
    [UpstreamTest(Provider + "vectorizes an image when requested through providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task VectorizesThroughProviderOptions()
    {
        var handler = Handler();
        var result = await Model(handler, "arrow-1").DoGenerateAsync(
            Call(null, new[] { ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png") }, options: "{\"operation\":\"vectorize\"}"),
            CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(VectorizeSvg, Encoding.UTF8.GetString((byte[])result.Images[0]!));
        Assert.Equal("https://api.quiver.ai/v1/svgs/vectorizations", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("arrow-1", Body(handler, 0)["model"]!.GetValue<string>());
        JsonAssert.Equal(Body(handler, 0)["image"], "{\"base64\":\"AQID\"}");
    }

    [Fact]
    [UpstreamTest(Provider + "animates binary SVG input without an instruction", Coverage = UpstreamCoverage.Covered)]
    public async Task AnimatesBinarySvgWithoutInstruction()
    {
        const string source = "<svg xmlns=\"http://www.w3.org/2000/svg\"><circle r=\"4\"/></svg>";
        var handler = Handler();
        var result = await Model(handler, "arrow-2").DoGenerateAsync(
            Call(null, new[] { ImageModelFile.FromFile(Encoding.UTF8.GetBytes(source), "image/png") }, options: "{\"operation\":\"animate\"}"),
            CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(AnimateSvg, Encoding.UTF8.GetString((byte[])result.Images[0]!));
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("quiverai"), "{\"images\":[{\"index\":0,\"mimeType\":\"image/svg+xml\",\"loopPeriodMs\":1200,\"openingAnimationMs\":null}]}");
        Assert.Equal("https://api.quiver.ai/v1/svgs/animations", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(Body(handler, 0), "{\"model\":\"arrow-2\",\"svg_source\":{\"base64\":\"" + Base64(source) + "\"},\"stream\":false}");
    }

    [Fact]
    [UpstreamTest(Provider + "forwards an animation instruction and supported options", Coverage = UpstreamCoverage.Covered)]
    public async Task ForwardsAnimationInstructionAndOptions()
    {
        var handler = Handler();
        await Model(handler, "arrow-2-telos").DoGenerateAsync(
            Call("Make the circle pulse gently.", new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"animate\",\"temperature\":0.4,\"maxOutputTokens\":4096,\"reasoningEffort\":\"medium\"}"),
            CancellationToken.None).ConfigureAwait(false);

        JsonAssert.Equal(Body(handler, 0), "{\"model\":\"arrow-2-telos\",\"svg_source\":{\"url\":\"https://example.com/source.svg\"},\"prompt\":\"Make the circle pulse gently.\",\"temperature\":0.4,\"max_output_tokens\":4096,\"reasoning_effort\":\"medium\",\"stream\":false}");
    }

    [Theory]
    [UpstreamTest(Provider + "normalizes $name animation input", Coverage = UpstreamCoverage.Covered)]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NormalizesAnimationInput(bool dataUrl)
    {
        var base64 = Base64("<svg xmlns=\"http://www.w3.org/2000/svg\"/>");
        var handler = Handler();
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = Model(handler, "arrow-2"),
            Prompt = new ImagePrompt(null, new object?[] { dataUrl ? "data:image/svg+xml;base64," + base64 : base64 }),
            ProviderOptions = Json("{\"quiverai\":{\"operation\":\"animate\"}}"),
        }).ConfigureAwait(false);

        JsonAssert.Equal(Body(handler, 0)["svg_source"], "{\"base64\":\"" + base64 + "\"}");
    }

    [Theory]
    [UpstreamTest(Provider + "rejects animation requests with $name", Coverage = UpstreamCoverage.Covered)]
    [InlineData("no source SVG", 0, 1, "arrow-2")]
    [InlineData("multiple source SVGs", 2, 1, "arrow-2")]
    [InlineData("batched outputs", 1, 2, "arrow-2")]
    [InlineData("unsupported model", 1, 1, "arrow-1.1")]
    public async Task RejectsInvalidAnimationRequests(string name, int fileCount, int n, string modelId)
    {
        Assert.NotEmpty(name);
        var files = fileCount == 0 ? null : Enumerable.Range(1, fileCount).Select(index => ImageModelFile.FromUrl("https://example.com/source-" + index + ".svg")).ToArray();
        await AssertRejected(modelId, Call(null, files, n, "{\"operation\":\"animate\"}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects $name for animation", Coverage = UpstreamCoverage.Covered)]
    [InlineData("non-SVG binary input")]
    [InlineData("non-HTTP URL input")]
    [InlineData("malformed URL input")]
    public async Task RejectsInvalidAnimationSources(string name)
    {
        var file = name switch
        {
            "non-SVG binary input" => ImageModelFile.FromFile(new byte[] { 1, 2, 3 }, "image/png"),
            "non-HTTP URL input" => ImageModelFile.FromUrl("file:///source.svg"),
            _ => ImageModelFile.FromUrl("https://"),
        };
        await AssertRejected("arrow-2", Call(null, new[] { file }, options: "{\"operation\":\"animate\"}")).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "rejects animation source SVGs above the base64 size limit", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsOversizedAnimationSource()
    {
        var oversized = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><!--" + new string('a', 800_000) + "--></svg>");
        var error = await AssertRejected("arrow-2", Call(null, new[] { ImageModelFile.FromFile(oversized, "image/svg+xml") }, options: "{\"operation\":\"animate\"}")).ConfigureAwait(false);
        Assert.Contains("accepts at most 1066668 base64 characters", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "rejects animation masks and operation-specific provider options", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsAnimationMasksAndOptions()
    {
        var source = ImageModelFile.FromUrl(SourceUrl);
        var masked = await AssertRejected("arrow-2", Call(null, new[] { source }, options: "{\"operation\":\"animate\"}", mask: source)).ConfigureAwait(false);
        Assert.Contains("does not support masks", masked.Message, StringComparison.Ordinal);
        var cropped = await AssertRejected("arrow-2", Call(null, new[] { source }, options: "{\"operation\":\"animate\",\"autoCrop\":true}")).ConfigureAwait(false);
        Assert.Contains("does not support providerOptions.quiverai.autoCrop", cropped.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "rejects empty animation instructions", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsEmptyAnimationInstructions()
    {
        var error = await AssertRejected("arrow-2", Call("   ", new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"animate\"}")).ConfigureAwait(false);
        Assert.Contains("requires a non-empty prompt", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects invalid animation options: %j", Coverage = UpstreamCoverage.Covered)]
    [InlineData("\"temperature\":2.1")]
    [InlineData("\"maxOutputTokens\":65537")]
    public async Task RejectsInvalidAnimationOptions(string option)
    {
        await AssertRejected("arrow-2", Call(null, new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"animate\"," + option + "}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "edits a binary SVG with %s and returns SVG bytes", Coverage = UpstreamCoverage.Covered)]
    [InlineData("arrow-2")]
    [InlineData("arrow-2-telos")]
    public async Task EditsBinarySvg(string modelId)
    {
        const string source = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><rect width=\"10\" height=\"10\" fill=\"red\"/></svg>";
        const string reference = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><circle cx=\"5\" cy=\"5\" r=\"4\" fill=\"blue\"/></svg>";
        var references = new JsonArray(new JsonObject { ["url"] = "https://example.com/reference.png" }, QuiverAIImageReference.Prepare(Encoding.UTF8.GetBytes(reference)));
        var options = new JsonObject
        {
            ["operation"] = "edit",
            ["referenceImages"] = references,
            ["maxReviewSteps"] = 2,
            ["reasoningEffort"] = "high",
            ["maxOutputTokens"] = 4096,
            ["orchestratorMaxOutputTokens"] = 2048,
            ["shallowMaxOutputTokens"] = 1024,
            ["temperature"] = 0.3,
        };
        var handler = Handler();
        var result = await Model(handler, modelId).DoGenerateAsync(
            Call("Change the rectangle fill to blue.", new[] { ImageModelFile.FromFile(Encoding.UTF8.GetBytes(source), "image/svg+xml") }, options: options.ToJsonString()),
            CancellationToken.None).ConfigureAwait(false);

        Assert.Equal(EditSvg, Encoding.UTF8.GetString(Assert.IsType<byte[]>(result.Images[0])));
        Assert.Equal(14, result.Usage!.InputTokens);
        Assert.Equal(11, result.Usage.OutputTokens);
        Assert.Equal(25, result.Usage.TotalTokens);
        Assert.Equal("https://api.quiver.ai/v1/svgs/edits", handler.Calls[0].Uri.AbsoluteUri);
        JsonAssert.Equal(
            Body(handler, 0),
            "{\"model\":\"" + modelId + "\",\"prompt\":\"Change the rectangle fill to blue.\",\"svg_source\":{\"base64\":\"" + Base64(source) + "\"},"
            + "\"reference_images\":[{\"url\":\"https://example.com/reference.png\"},{\"base64\":\"" + Base64(reference) + "\"}],"
            + "\"max_review_steps\":2,\"reasoning_effort\":\"high\","
            + "\"settings\":{\"max_output_tokens\":4096,\"orchestrator_max_output_tokens\":2048,\"shallow_max_output_tokens\":1024,\"temperature\":0.3},\"stream\":false}");
    }

    [Fact]
    [UpstreamTest(Provider + "forwards an HTTP SVG source URL for editing", Coverage = UpstreamCoverage.Covered)]
    public async Task ForwardsHttpSvgSourceUrl()
    {
        var handler = Handler();
        await Model(handler, "arrow-2").DoGenerateAsync(Call("Make the wordmark bolder.", new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"edit\"}"), CancellationToken.None).ConfigureAwait(false);

        var body = Body(handler, 0);
        JsonAssert.Equal(body["svg_source"], "{\"url\":\"https://example.com/source.svg\"}");
        Assert.False(body.AsObject().ContainsKey("settings"));
    }

    [Fact]
    [UpstreamTest(Provider + "accepts base64 SVG source data for editing", Coverage = UpstreamCoverage.Covered)]
    public async Task AcceptsBase64SvgSource()
    {
        var base64 = Base64("<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>");
        var handler = Handler();
        await GenerateImage.GenerateImageAsync(new GenerateImageRequest
        {
            Model = Model(handler, "arrow-2"),
            Prompt = new ImagePrompt("Add a blue background.", new object?[] { base64 }),
            ProviderOptions = Json("{\"quiverai\":{\"operation\":\"edit\"}}"),
        }).ConfigureAwait(false);

        JsonAssert.Equal(Body(handler, 0)["svg_source"], "{\"base64\":\"" + base64 + "\"}");
    }

    [Fact]
    [UpstreamTest(Provider + "accepts a structurally valid SVG followed by an XML comment", Coverage = UpstreamCoverage.Covered)]
    public async Task AcceptsSvgFollowedByComment()
    {
        const string source = "<?xml version=\"1.0\"?><svg xmlns=\"http://www.w3.org/2000/svg\"><g><path d=\"M0 0\"/></g></svg><!-- exported by editor -->";
        var handler = Handler();
        await Model(handler, "arrow-2").DoGenerateAsync(Call("Make the path blue.", new[] { ImageModelFile.FromFile(Encoding.UTF8.GetBytes(source), "image/svg+xml") }, options: "{\"operation\":\"edit\"}"), CancellationToken.None).ConfigureAwait(false);

        JsonAssert.Equal(Body(handler, 0)["svg_source"], "{\"base64\":\"" + Base64(source) + "\"}");
    }

    [Theory]
    [UpstreamTest(Provider + "rejects $name SVG edit sources", Coverage = UpstreamCoverage.Covered)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    public async Task RejectsInvalidEditSourceCounts(int count)
    {
        var files = count < 0 ? null : Enumerable.Range(1, count).Select(index => ImageModelFile.FromUrl("https://example.com/source-" + index + ".svg")).ToArray();
        await AssertRejected("arrow-2", Call("Make the icon blue.", files, options: "{\"operation\":\"edit\"}")).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "rejects oversized binary SVG source data before editing", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsOversizedEditSource()
    {
        var oversized = Encoding.UTF8.GetBytes("<svg>" + new string(' ', 200_000) + "</svg>");
        await AssertRejected("arrow-2", Call("Make the icon blue.", new[] { ImageModelFile.FromFile(oversized, "image/svg+xml") }, options: "{\"operation\":\"edit\"}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects a $name SVG edit instruction", Coverage = UpstreamCoverage.Covered)]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("over 4000 characters")]
    public async Task RejectsInvalidEditInstructions(string? prompt)
    {
        var text = prompt == "over 4000 characters" ? new string('a', 4001) : prompt;
        await AssertRejected("arrow-2", Call(text, new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"edit\"}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(
        Provider + "rejects $name before SVG editing",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers the non-HTTP URL, raster, incomplete, and malformed sources. Image model files hold bytes in the port, so invalid base64 fails while generateImage decodes the prompt.")]
    [InlineData("non-HTTP source URL")]
    [InlineData("raster source data")]
    [InlineData("incomplete SVG source data")]
    [InlineData("structurally malformed SVG source data")]
    public async Task RejectsInvalidEditSources(string name)
    {
        var file = name switch
        {
            "non-HTTP source URL" => ImageModelFile.FromUrl("ftp://example.com/source.svg"),
            "raster source data" => ImageModelFile.FromFile(new byte[] { 0x89, 0x50, 0x4e, 0x47 }, "image/png"),
            "incomplete SVG source data" => ImageModelFile.FromFile(Encoding.UTF8.GetBytes("<svg>"), "image/svg+xml"),
            _ => ImageModelFile.FromFile(Encoding.UTF8.GetBytes("<svg><g></svg>"), "image/svg+xml"),
        };
        await AssertRejected("arrow-2", Call("Make the icon blue.", new[] { file }, options: "{\"operation\":\"edit\"}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects $name before SVG editing #2", Coverage = UpstreamCoverage.Covered)]
    [InlineData("more than four reference images")]
    [InlineData("a non-HTTP reference URL")]
    [InlineData("invalid base64 reference data")]
    [InlineData("unsupported reference data")]
    public async Task RejectsInvalidEditReferences(string name)
    {
        var references = name switch
        {
            "more than four reference images" => string.Join(",", Enumerable.Repeat("{\"url\":\"https://example.com/reference.png\"}", 5)),
            "a non-HTTP reference URL" => "{\"url\":\"file:///tmp/reference.png\"}",
            "invalid base64 reference data" => "{\"base64\":\"not base64!\"}",
            _ => "{\"base64\":\"" + Convert.ToBase64String(new byte[] { 1, 2, 3 }) + "\"}",
        };
        await AssertRejected("arrow-2", Call("Make the icon blue.", new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"edit\",\"referenceImages\":[" + references + "]}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects invalid SVG edit settings: %j", Coverage = UpstreamCoverage.Covered)]
    [InlineData("\"maxReviewSteps\":-1")]
    [InlineData("\"maxReviewSteps\":6")]
    [InlineData("\"maxReviewSteps\":1.5")]
    [InlineData("\"maxOutputTokens\":65537")]
    [InlineData("\"orchestratorMaxOutputTokens\":0")]
    [InlineData("\"orchestratorMaxOutputTokens\":65537")]
    [InlineData("\"shallowMaxOutputTokens\":0")]
    [InlineData("\"shallowMaxOutputTokens\":65537")]
    [InlineData("\"temperature\":-0.1")]
    [InlineData("\"temperature\":2.1")]
    public async Task RejectsInvalidEditSettings(string option)
    {
        await AssertRejected("arrow-2", Call("Make the icon blue.", new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"edit\"," + option + "}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects SVG editing with $name", Coverage = UpstreamCoverage.Covered)]
    [InlineData("an unsupported model")]
    [InlineData("multiple outputs")]
    [InlineData("a mask")]
    public async Task RejectsInvalidEditCalls(string name)
    {
        var files = new[] { ImageModelFile.FromUrl(SourceUrl) };
        var call = name switch
        {
            "multiple outputs" => Call("Make the icon blue.", files, 2, "{\"operation\":\"edit\"}"),
            "a mask" => Call("Make the icon blue.", files, options: "{\"operation\":\"edit\"}", mask: ImageModelFile.FromUrl("https://example.com/mask.svg")),
            _ => Call("Make the icon blue.", files, options: "{\"operation\":\"edit\"}"),
        };
        await AssertRejected(name == "an unsupported model" ? "arrow-1" : "arrow-2", call).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects generation and vectorization options for SVG editing: %j", Coverage = UpstreamCoverage.Covered)]
    [InlineData("\"instructions\":\"Use a flat style.\"")]
    [InlineData("\"attributes\":{\"viewBox\":{\"minX\":0,\"minY\":0,\"width\":10,\"height\":10}}")]
    [InlineData("\"topP\":0.9")]
    [InlineData("\"presencePenalty\":0.2")]
    [InlineData("\"autoCrop\":true")]
    [InlineData("\"targetSize\":1024")]
    public async Task RejectsNonEditOptionsForEditing(string option)
    {
        await AssertRejected("arrow-2", Call("Make the icon blue.", new[] { ImageModelFile.FromUrl(SourceUrl) }, options: "{\"operation\":\"edit\"," + option + "}")).ConfigureAwait(false);
    }

    [Theory]
    [UpstreamTest(Provider + "rejects edit-only options for %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("generate")]
    [InlineData("vectorize")]
    [InlineData("animate")]
    public async Task RejectsEditOnlyOptions(string operation)
    {
        var files = operation == "generate" ? null : new[] { ImageModelFile.FromUrl(SourceUrl) };
        await AssertRejected("arrow-2", Call("Draw a square icon.", files, options: "{\"operation\":\"" + operation + "\",\"maxReviewSteps\":1}")).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "forwards docs-backed generation options and reference images", Coverage = UpstreamCoverage.Covered)]
    public async Task ForwardsGenerationOptionsAndReferences()
    {
        var handler = Handler();
        await Model(handler, "arrow-1").DoGenerateAsync(
            Call(
                "Draw a square icon.",
                new[] { ImageModelFile.FromUrl("https://example.com/reference-1.png"), ImageModelFile.FromFile(new byte[] { 4, 5, 6 }, "image/png") },
                options: "{\"instructions\":\"Use a flat monochrome style with clean geometry.\",\"temperature\":0.4,\"topP\":0.95,\"presencePenalty\":0.2,\"maxOutputTokens\":4096}"),
            CancellationToken.None).ConfigureAwait(false);

        JsonAssert.Equal(
            Body(handler, 0),
            "{\"model\":\"arrow-1\",\"n\":1,\"prompt\":\"Draw a square icon.\",\"temperature\":0.4,\"top_p\":0.95,\"presence_penalty\":0.2,\"max_output_tokens\":4096,\"stream\":false,"
            + "\"instructions\":\"Use a flat monochrome style with clean geometry.\",\"references\":[{\"url\":\"https://example.com/reference-1.png\"},{\"base64\":\"BAUG\"}]}");
    }

    [Fact]
    [UpstreamTest(Provider + "accepts up to 16 reference images for arrow-1.1-max", Coverage = UpstreamCoverage.Covered)]
    public async Task AcceptsSixteenReferencesForArrowMax()
    {
        var handler = Handler();
        await Model(handler, "arrow-1.1-max").DoGenerateAsync(Call("Draw a square icon.", References(16)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("arrow-1.1-max", Body(handler, 0)["model"]!.GetValue<string>());
        Assert.Equal(16, Body(handler, 0)["references"]!.AsArray().Count);
    }

    [Fact]
    [UpstreamTest(Provider + "rejects more than 16 reference images for arrow-1.1-max", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsSeventeenReferencesForArrowMax()
    {
        await AssertRejected("arrow-1.1-max", Call("Draw a square icon.", References(17))).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "forwards docs-backed vectorize options", Coverage = UpstreamCoverage.Covered)]
    public async Task ForwardsVectorizeOptions()
    {
        var handler = Handler();
        await Model(handler, "arrow-1").DoGenerateAsync(
            Call(null, new[] { ImageModelFile.FromUrl("https://example.com/logo.png") }, options: "{\"operation\":\"vectorize\",\"temperature\":0.4,\"topP\":0.95,\"presencePenalty\":0.2,\"maxOutputTokens\":4096,\"autoCrop\":true,\"targetSize\":1024}"),
            CancellationToken.None).ConfigureAwait(false);

        JsonAssert.Equal(
            Body(handler, 0),
            "{\"model\":\"arrow-1\",\"image\":{\"url\":\"https://example.com/logo.png\"},\"temperature\":0.4,\"top_p\":0.95,\"presence_penalty\":0.2,\"max_output_tokens\":4096,\"stream\":false,\"auto_crop\":true,\"target_size\":1024}");
    }

    [Theory]
    [UpstreamTest(Provider + "forwards Arrow 2 options for %s on both endpoints", Coverage = UpstreamCoverage.Covered)]
    [InlineData("arrow-2")]
    [InlineData("arrow-2-telos")]
    public async Task ForwardsArrowTwoOptions(string modelId)
    {
        var handler = Handler();
        foreach (var operation in new[] { "generate", "vectorize" })
        {
            var result = await Model(handler, modelId).DoGenerateAsync(
                Call(
                    "Draw a square icon.",
                    new[] { ImageModelFile.FromUrl("https://example.com/reference.png") },
                    options: "{\"operation\":\"" + operation + "\",\"reasoningEffort\":\"high\",\"attributes\":{\"viewBox\":{\"minX\":-10,\"minY\":0,\"width\":100,\"height\":50}},\"maxOutputTokens\":65536}"),
                CancellationToken.None).ConfigureAwait(false);
            Assert.Single(result.Images);
            Assert.True(result.Usage!.TotalTokens > 0);
            Assert.False(result.ProviderMetadata!.Value.GetProperty("quiverai").TryGetProperty("credits", out _));
        }

        for (var index = 0; index < 2; index++)
        {
            var body = Body(handler, index);
            Assert.Equal(modelId, body["model"]!.GetValue<string>());
            Assert.Equal("high", body["reasoning_effort"]!.GetValue<string>());
            JsonAssert.Equal(body["attributes"], "{\"viewBox\":{\"minX\":-10,\"minY\":0,\"width\":100,\"height\":50}}");
            Assert.Equal(65536, body["max_output_tokens"]!.GetValue<int>());
            Assert.False(body["stream"]!.GetValue<bool>());
        }

        Assert.Equal(1, Body(handler, 0)["n"]!.GetValue<int>());
        Assert.False(Body(handler, 1).AsObject().ContainsKey("n"));
    }

    [Theory]
    [UpstreamTest(Provider + "rejects output budgets above the limit for %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("arrow-2")]
    [InlineData("arrow-2-telos")]
    public async Task RejectsOutputBudgetsAboveLimit(string modelId)
    {
        var error = await AssertRejected(modelId, Call(options: "{\"maxOutputTokens\":65537}")).ConfigureAwait(false);
        Assert.Contains("supports at most 65536 output tokens", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "retains the legacy output budget allowance", Coverage = UpstreamCoverage.Covered)]
    public async Task RetainsLegacyOutputBudget()
    {
        var handler = Handler();
        await Model(handler, "arrow-1.1").DoGenerateAsync(Call(options: "{\"maxOutputTokens\":131072}"), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(131072, Body(handler, 0)["max_output_tokens"]!.GetValue<int>());
    }

    [Theory]
    [UpstreamTest(Provider + "leaves model-specific reference limits to the API for %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("arrow-2")]
    [InlineData("arrow-2-telos")]
    [InlineData("future-model")]
    public async Task LeavesReferenceLimitsToApi(string modelId)
    {
        var handler = Handler();
        await Model(handler, modelId).DoGenerateAsync(Call("Draw a square icon.", References(16)), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(16, Body(handler, 0)["references"]!.AsArray().Count);
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Model(handler, modelId).DoGenerateAsync(Call("Draw a square icon.", References(17)), CancellationToken.None)).ConfigureAwait(false);
        Assert.Contains("supports up to 16 reference images", error.Message, StringComparison.Ordinal);
        Assert.Single(handler.Calls);
    }

    [Theory]
    [UpstreamTest(Provider + "retains the four-reference limit for %s", Coverage = UpstreamCoverage.Covered)]
    [InlineData("arrow-1")]
    [InlineData("arrow-1.0")]
    [InlineData("arrow-1.1")]
    public async Task RetainsFourReferenceLimit(string modelId)
    {
        var error = await AssertRejected(modelId, Call("Draw a square icon.", References(5))).ConfigureAwait(false);
        Assert.Contains("supports up to 4 reference images", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [UpstreamTest(Provider + "preserves fixed-credit billing metadata (%s credits) without requiring usage", Coverage = UpstreamCoverage.Covered)]
    [InlineData(0)]
    [InlineData(20)]
    public async Task PreservesFixedCreditMetadata(int credits)
    {
        var handler = new ParityHandler(_ => ParityHandler.Json("{\"id\":\"svg-gen-1\",\"created\":1713374400,\"data\":[{\"svg\":\"<svg/>\",\"mime_type\":\"image/svg+xml\"}],\"credits\":" + credits + "}"));
        var result = await Model(handler, "arrow-1.1").DoGenerateAsync(Call(), CancellationToken.None).ConfigureAwait(false);
        Assert.Null(result.Usage);
        JsonAssert.Equal(result.ProviderMetadata!.Value.GetProperty("quiverai"), "{\"credits\":" + credits + ",\"images\":[{\"index\":0,\"mimeType\":\"image/svg+xml\"}]}");
    }

    [Theory]
    [UpstreamTest(Provider + "rejects invalid Arrow 2 options: %j", Coverage = UpstreamCoverage.Covered)]
    [InlineData("{\"reasoningEffort\":\"none\"}")]
    [InlineData("{\"attributes\":{\"viewBox\":{\"minX\":0,\"minY\":0,\"width\":0,\"height\":100}}}")]
    [InlineData("{\"attributes\":{\"viewBox\":{\"minX\":0,\"minY\":0,\"width\":100,\"height\":-1}}}")]
    public async Task RejectsInvalidArrowTwoOptions(string options)
    {
        await AssertRejected("arrow-2", Call(options: options)).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "rejects vectorization batching instead of silently returning fewer outputs", Coverage = UpstreamCoverage.Covered)]
    public async Task RejectsVectorizationBatching()
    {
        var error = await AssertRejected("arrow-2", Call("Draw a square icon.", new[] { ImageModelFile.FromUrl("https://example.com/logo.png") }, 2, "{\"operation\":\"vectorize\"}")).ConfigureAwait(false);
        Assert.Contains("Set maxImagesPerCall to 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Provider + "fails fast when vectorize is requested without an input image", Coverage = UpstreamCoverage.Covered)]
    public async Task FailsFastWhenVectorizeHasNoImage()
    {
        await AssertRejected("arrow-1", Call(null, options: "{\"operation\":\"vectorize\"}")).ConfigureAwait(false);
    }

    [Fact]
    [UpstreamTest(Provider + "warns on unsupported call options", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsOnUnsupportedCallOptions()
    {
        var call = new ImageModelCall("Draw a square icon.", null, null, 1, "1024x1024", "1:1", 42, Json("{}"), new Dictionary<string, string>(), CancellationToken.None);
        var result = await Model(Handler(), "arrow-1").DoGenerateAsync(call, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal(new[] { "size", "aspectRatio", "seed" }, result.Warnings.Select(warning => warning.Feature));
        Assert.All(result.Warnings, warning => Assert.Equal("unsupported", warning.Type));
    }

    private static ParityHandler Handler()
    {
        return new ParityHandler(call => ParityHandler.Json(call.Uri.AbsolutePath switch
        {
            var path when path.EndsWith("/vectorizations", StringComparison.Ordinal) => Fixture("svg-vec-1", 1713374460, VectorizeSvg, 11, 7, 18),
            var path when path.EndsWith("/edits", StringComparison.Ordinal) => Fixture("svg-edit-1", 1713374520, EditSvg, 14, 11, 25),
            var path when path.EndsWith("/animations", StringComparison.Ordinal) =>
                "{\"id\":\"svg-animation-1\",\"created\":1713374520,\"data\":[{\"svg\":" + JsonSerializer.Serialize(AnimateSvg) + ",\"mime_type\":\"image/svg+xml\",\"loop_period_ms\":1200,\"opening_animation_ms\":null}],\"usage\":{\"total_tokens\":24,\"input_tokens\":13,\"output_tokens\":11}}",
            _ => Fixture("svg-gen-1", 1713374400, GenerateSvg, 12, 9, 21),
        }));
    }

    private static string Fixture(string id, long created, string svg, int input, int output, int total)
    {
        return "{\"id\":\"" + id + "\",\"created\":" + created + ",\"data\":[{\"svg\":" + JsonSerializer.Serialize(svg) + ",\"mime_type\":\"image/svg+xml\"}],"
            + "\"usage\":{\"total_tokens\":" + total + ",\"input_tokens\":" + input + ",\"output_tokens\":" + output + "}}";
    }

    private static HttpResponseMessage Status(int status, string json)
    {
        var response = ParityHandler.Json(json);
        response.StatusCode = (HttpStatusCode)status;
        return response;
    }

    private static QuiverAIProvider CreateProvider(ParityHandler handler)
    {
        return QuiverAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
    }

    private static QuiverAIImageModel Model(ParityHandler handler, string modelId)
    {
        return (QuiverAIImageModel)CreateProvider(handler).ImageModel(modelId);
    }

    private static ImageModelCall Call(string? prompt = "Draw a square icon.", IReadOnlyList<ImageModelFile>? files = null, int n = 1, string? options = null, ImageModelFile? mask = null)
    {
        var providerOptions = Json(options == null ? "{}" : "{\"quiverai\":" + options + "}");
        return new ImageModelCall(prompt, files, mask, n, null, null, null, providerOptions, new Dictionary<string, string>(), CancellationToken.None);
    }

    private static ImageModelFile[] References(int count)
    {
        return Enumerable.Range(1, count).Select(index => ImageModelFile.FromUrl("https://example.com/reference-" + index + ".png")).ToArray();
    }

    private static async Task<InvalidArgumentException> AssertRejected(string modelId, ImageModelCall call)
    {
        var handler = Handler();
        var error = await Assert.ThrowsAsync<InvalidArgumentException>(() => Model(handler, modelId).DoGenerateAsync(call, CancellationToken.None)).ConfigureAwait(false);
        Assert.Empty(handler.Calls);
        return error;
    }

    private static JsonNode Body(ParityHandler handler, int index)
    {
        return JsonNode.Parse(handler.Calls[index].Text)!;
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static string Base64(string text)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    }

    private static async Task WithEnvironment(string? apiKey, string? baseUrl, Func<Task> body)
    {
        var previousKey = Environment.GetEnvironmentVariable("QUIVERAI_API_KEY");
        var previousUrl = Environment.GetEnvironmentVariable("QUIVERAI_BASE_URL");
        Environment.SetEnvironmentVariable("QUIVERAI_API_KEY", apiKey);
        Environment.SetEnvironmentVariable("QUIVERAI_BASE_URL", baseUrl);
        try
        {
            await body().ConfigureAwait(false);
        }
        finally
        {
            Environment.SetEnvironmentVariable("QUIVERAI_API_KEY", previousKey);
            Environment.SetEnvironmentVariable("QUIVERAI_BASE_URL", previousUrl);
        }
    }
}
