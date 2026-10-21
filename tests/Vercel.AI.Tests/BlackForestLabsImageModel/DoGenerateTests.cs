// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.BlackForestLabsImageModel.BlackForestLabsImageModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsImageModel;

/// <summary>Port of <c>black-forest-labs-image-model.test.ts</c> &gt; <c>BlackForestLabsImageModel</c> &gt; <c>doGenerate</c>.</summary>
public sealed class DoGenerateTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-image-model.test.ts::BlackForestLabsImageModel > doGenerate::";

    [Fact]
    [UpstreamTest(Id + "parses provider options only once", Coverage = UpstreamCoverage.Covered)]
    public async Task ParsesProviderOptionsOnlyOnce()
    {
        var model = CreateBasicModel(new BlackForestLabsServer());
        var parseCount = 0;
        model.OnProviderOptionsParsed = () => parseCount++;

        await model.DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        Assert.Equal(1, parseCount);
    }

    [Fact]
    [UpstreamTest(Id + "passes the correct parameters including aspect ratio and providerOptions", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesTheCorrectParametersIncludingAspectRatioAndProviderOptions()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server);

        await model.DoGenerateAsync(Call(aspectRatio: "16:9", blackForestLabs: "{\"promptUpsampling\":true,\"unsupportedProperty\":\"value\"}"), CancellationToken.None);

        AssertJson("{\"prompt\":\"" + PromptText + "\",\"aspect_ratio\":\"16:9\",\"prompt_upsampling\":true}", server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "uses image field for flux-pro-1.0-fill input images", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesImageFieldForFluxPro10FillInputImages()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server, "flux-pro-1.0-fill");

        await model.DoGenerateAsync(Call(aspectRatio: "1:1", files: new[] { PngFile("test-image") }, mask: PngFile("test-mask")), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"aspect_ratio\":\"1:1\",\"image\":\"" + Base64("test-image") + "\",\"mask\":\"" + Base64("test-mask") + "\"}",
            server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "uses input_image field for Kontext input images", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesInputImageFieldForKontextInputImages()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server, "flux-kontext-pro");

        await model.DoGenerateAsync(Call(aspectRatio: "1:1", files: new[] { PngFile("test-image") }), CancellationToken.None);

        AssertJson(
            "{\"prompt\":\"" + PromptText + "\",\"aspect_ratio\":\"1:1\",\"input_image\":\"" + Base64("test-image") + "\"}",
            server.Calls[0].Body);
    }

    [Fact]
    [UpstreamTest(Id + "includes seed in providerMetadata images when provided by API", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesSeedInProviderMetadataImagesWhenProvidedByApi()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/poll", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/image.png\",\"seed\":12345}}");

        var result = await CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        Assert.Equal(12345, Metadata(result)!["seed"]!.GetValue<int>());
    }

    [Fact]
    [UpstreamTest(Id + "includes all cost and megapixel fields when provided by submit API", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesAllCostAndMegapixelFieldsWhenProvidedBySubmitApi()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/v1/flux-2-pro", "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\",\"cost\":0.08,\"input_mp\":1.5,\"output_mp\":2.0}");

        var result = await CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        var metadata = Metadata(result)!;
        Assert.Equal(0.08, metadata["cost"]!.GetValue<double>());
        Assert.Equal(1.5, metadata["inputMegapixels"]!.GetValue<double>());
        Assert.Equal(2.0, metadata["outputMegapixels"]!.GetValue<double>());
    }

    [Fact]
    [UpstreamTest(Id + "omits cost and megapixel fields from providerMetadata when not provided by submit API", Coverage = UpstreamCoverage.Covered)]
    public async Task OmitsCostAndMegapixelFieldsFromProviderMetadataWhenNotProvidedBySubmitApi()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/v1/flux-2-pro", "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\"}");

        var result = await CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        var metadata = Metadata(result)!.AsObject();
        Assert.NotNull(metadata);
        Assert.False(metadata.ContainsKey("cost"));
        Assert.False(metadata.ContainsKey("inputMegapixels"));
        Assert.False(metadata.ContainsKey("outputMegapixels"));
    }

    [Fact]
    [UpstreamTest(Id + "handles null cost and megapixel fields from submit API", Coverage = UpstreamCoverage.Covered)]
    public async Task HandlesNullCostAndMegapixelFieldsFromSubmitApi()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/v1/flux-2-pro", "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\",\"cost\":null,\"input_mp\":null,\"output_mp\":null}");

        var result = await CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        var metadata = Metadata(result)!.AsObject();
        Assert.NotNull(metadata);
        Assert.False(metadata.ContainsKey("cost"));
        Assert.False(metadata.ContainsKey("inputMegapixels"));
        Assert.False(metadata.ContainsKey("outputMegapixels"));
    }

    [Fact]
    [UpstreamTest(Id + "calls the expected URLs in sequence", Coverage = UpstreamCoverage.Covered)]
    public async Task CallsTheExpectedUrlsInSequence()
    {
        var server = new BlackForestLabsServer();

        await CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "16:9"), CancellationToken.None);

        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal("https://api.example.com/v1/flux-2-pro", server.Calls[0].Url);
        Assert.Equal(HttpMethod.Get, server.Calls[1].Method);
        Assert.Equal("https://api.example.com/poll?id=req-123", server.Calls[1].Url);
        Assert.Equal(HttpMethod.Get, server.Calls[2].Method);
        Assert.Equal("https://api.example.com/image.png", server.Calls[2].Url);
    }

    [Fact]
    [UpstreamTest(Id + "does not send the API key when the result URL is on a foreign origin", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotSendTheApiKeyWhenTheResultUrlIsOnAForeignOrigin()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/poll", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://cdn.evil.example/image.png\"}}");

        await CreateBasicModel(server).DoGenerateAsync(Call(), CancellationToken.None);

        var downloadCall = server.Calls.FirstOrDefault(call => call.Url == "https://cdn.evil.example/image.png");
        Assert.NotNull(downloadCall);
        Assert.False(downloadCall!.Headers.ContainsKey("x-key"));
        Assert.StartsWith("ai-sdk/", downloadCall.Headers["user-agent"], StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Id + "sends the API key when the polling URL is on a sibling bfl.ai cluster host", Coverage = UpstreamCoverage.Covered)]
    public async Task SendsTheApiKeyWhenThePollingUrlIsOnASiblingBflAiClusterHost()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server, baseUrl: "https://api.bfl.ai/v1");

        await model.DoGenerateAsync(Call(), CancellationToken.None);

        var pollCall = server.Calls.FirstOrDefault(call => call.Url.StartsWith("https://api.us1.bfl.ai/v1/get_result", StringComparison.Ordinal));
        Assert.NotNull(pollCall);
        Assert.Equal("test-key", pollCall!.Headers["x-key"]);
    }

    [Fact]
    [UpstreamTest(Id + "does not send credentials to signed image URLs on a BFL delivery host", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotSendCredentialsToSignedImageUrlsOnABflDeliveryHost()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server, baseUrl: "https://api.bfl.ai/v1", headers: new Dictionary<string, string> { ["authorization"] = "Bearer test-token" });

        await model.DoGenerateAsync(Call(), CancellationToken.None);

        var downloadCall = server.Calls.FirstOrDefault(call => call.Url == "https://delivery-us1.bfl.ai/image.png");
        Assert.NotNull(downloadCall);
        Assert.False(downloadCall!.Headers.ContainsKey("x-key"));
        Assert.False(downloadCall.Headers.ContainsKey("authorization"));
        Assert.StartsWith("ai-sdk/", downloadCall.Headers["user-agent"], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("flux-2-pro")]
    [InlineData("flux-3-image")]
    [UpstreamTest(Id + "preserves headers for same-origin custom proxy downloads with %s", Coverage = UpstreamCoverage.Covered)]
    public async Task PreservesHeadersForSameOriginCustomProxyDownloadsWith(string modelId)
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(
            server,
            modelId,
            new Dictionary<string, string> { ["authorization"] = "Bearer test-token", ["custom-provider-header"] = "provider-value" });

        await model.DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["custom-request-header"] = "request-value" }), CancellationToken.None);

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["x-key"] = "test-key",
                ["authorization"] = "Bearer test-token",
                ["custom-provider-header"] = "provider-value",
                ["custom-request-header"] = "request-value",
            }.OrderBy(pair => pair.Key),
            server.Calls[2].HeadersWithoutUserAgent().OrderBy(pair => pair.Key));
    }

    [Fact]
    [UpstreamTest(Id + "does not send credentials to signed image URLs on the configured BFL origin", Coverage = UpstreamCoverage.Covered)]
    public async Task DoesNotSendCredentialsToSignedImageUrlsOnTheConfiguredBflOrigin()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.us1.bfl.ai/v1/get_result", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.bfl.ai/image.png\"}}");
        var model = CreateBasicModel(server, baseUrl: "https://api.bfl.ai/v1", headers: new Dictionary<string, string> { ["authorization"] = "Bearer test-token" });

        await model.DoGenerateAsync(Call(), CancellationToken.None);

        Assert.False(server.Calls[2].Headers.ContainsKey("x-key"));
        Assert.False(server.Calls[2].Headers.ContainsKey("authorization"));
    }

    [Fact]
    [UpstreamTest(Id + "merges provider and request headers for submit call", Coverage = UpstreamCoverage.Covered)]
    public async Task MergesProviderAndRequestHeadersForSubmitCall()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server, headers: new Dictionary<string, string> { ["Custom-Provider-Header"] = "provider-header-value" });

        await model.DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["content-type"] = "application/json",
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
                ["x-key"] = "test-key",
            }.OrderBy(pair => pair.Key),
            server.Calls[0].HeadersWithoutUserAgent().OrderBy(pair => pair.Key));
    }

    [Fact]
    [UpstreamTest(Id + "passes merged headers to polling requests", Coverage = UpstreamCoverage.Covered)]
    public async Task PassesMergedHeadersToPollingRequests()
    {
        var server = new BlackForestLabsServer();
        var model = CreateBasicModel(server, headers: new Dictionary<string, string> { ["Custom-Provider-Header"] = "provider-header-value" });

        await model.DoGenerateAsync(Call(headers: new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }), CancellationToken.None);

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["custom-provider-header"] = "provider-header-value",
                ["custom-request-header"] = "request-header-value",
                ["x-key"] = "test-key",
            }.OrderBy(pair => pair.Key),
            server.Calls[1].HeadersWithoutUserAgent().OrderBy(pair => pair.Key));
    }

    [Fact]
    [UpstreamTest(Id + "warns and derives aspect_ratio when size is provided", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAndDerivesAspectRatioWhenSizeIsProvided()
    {
        var result = await CreateBasicModel(new BlackForestLabsServer()).DoGenerateAsync(Call(size: "1024x1024"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal(("unsupported", "size", "Deriving aspect_ratio from size. Use the width and height provider options to specify dimensions for models that support them."), Warning(warning));
    }

    [Fact]
    [UpstreamTest(Id + "warns and ignores size when both size and aspectRatio are provided", Coverage = UpstreamCoverage.Covered)]
    public async Task WarnsAndIgnoresSizeWhenBothSizeAndAspectRatioAreProvided()
    {
        var result = await CreateBasicModel(new BlackForestLabsServer()).DoGenerateAsync(Call(size: "1920x1080", aspectRatio: "16:9"), CancellationToken.None);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal(("unsupported", "size", "Black Forest Labs ignores size when aspectRatio is provided. Use the width and height provider options to specify dimensions for models that support them"), Warning(warning));
    }

    [Fact]
    [UpstreamTest(Id + "handles API errors with message and detail", Coverage = UpstreamCoverage.Covered)]
    public async Task HandlesApiErrorsWithMessageAndDetail()
    {
        var server = new BlackForestLabsServer();
        server.Respond("https://api.example.com/v1/flux-2-pro", 400, "{\"message\":\"Top-level message\",\"detail\":{\"error\":\"Invalid prompt\"}}");

        var exception = await Assert.ThrowsAsync<ApiException>(() => CreateBasicModel(server).DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Equal("{\"error\":\"Invalid prompt\"}", exception.Message);
        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("https://api.example.com/v1/flux-2-pro", exception.Url);
    }

    [Fact]
    [UpstreamTest(Id + "handles poll responses with state instead of status", Coverage = UpstreamCoverage.Covered)]
    public async Task HandlesPollResponsesWithStateInsteadOfStatus()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/poll", "{\"state\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/image.png\"}}");

        var result = await CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        Assert.Single(result.Images);
        Assert.IsType<byte[]>(result.Images[0]);
    }

    [Fact]
    [UpstreamTest(Id + "polls multiple times using configured interval until Ready", Coverage = UpstreamCoverage.Covered)]
    public async Task PollsMultipleTimesUsingConfiguredIntervalUntilReady()
    {
        var server = new BlackForestLabsServer();
        var pollHitCount = 0;
        server.Urls["https://api.example.com/poll"] = _ =>
        {
            pollHitCount += 1;
            var body = pollHitCount < 3
                ? "{\"status\":\"Pending\"}"
                : "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/image.png\"}}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        };
        var model = CreateBasicModel(server, pollIntervalMillis: 10, pollTimeoutMillis: 1000);

        await model.DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        var pollCalls = server.Calls.Where(call => call.Method == HttpMethod.Get && call.Url.StartsWith("https://api.example.com/poll", StringComparison.Ordinal));
        Assert.Equal(3, pollCalls.Count());
    }

    [Fact]
    [UpstreamTest(Id + "enforces pollTimeoutMillis while a polling request is pending", Coverage = UpstreamCoverage.Covered)]
    public async Task EnforcesPollTimeoutMillisWhileAPollingRequestIsPending()
    {
        var server = new BlackForestLabsServer();
        var pollingSignal = default(CancellationToken);
        server.Urls["https://api.example.com/poll"] = async cancellationToken =>
        {
            pollingSignal = cancellationToken;
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        };
        var model = CreateBasicModel(server, pollIntervalMillis: 10, pollTimeoutMillis: 25);

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None));

        Assert.Equal("Black Forest Labs generation timed out.", exception.Message);
        Assert.True(pollingSignal.IsCancellationRequested);
    }

    [Fact]
    [UpstreamTest(Id + "throws when poll is Ready but sample is missing", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsWhenPollIsReadyButSampleIsMissing()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/poll", "{\"status\":\"Ready\",\"result\":null}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None));

        Assert.Contains("Black Forest Labs poll response is Ready but missing result.sample", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "throws when poll returns Error or Failed", Coverage = UpstreamCoverage.Covered)]
    public async Task ThrowsWhenPollReturnsErrorOrFailed()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/poll", "{\"status\":\"Error\"}");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => CreateBasicModel(server).DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None));

        Assert.Contains("Black Forest Labs generation failed.", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "includes timestamp, headers, and modelId in response metadata", Coverage = UpstreamCoverage.Covered)]
    public async Task IncludesTimestampHeadersAndModelIdInResponseMetadata()
    {
        var testDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var model = CreateBasicModel(new BlackForestLabsServer(), currentDate: () => testDate);

        var result = await model.DoGenerateAsync(Call(aspectRatio: "1:1"), CancellationToken.None);

        Assert.Equal(testDate.UtcDateTime, result.Response.Timestamp);
        Assert.Equal("flux-2-pro", result.Response.ModelId);
        Assert.NotNull(result.Response.Headers);
    }
}
