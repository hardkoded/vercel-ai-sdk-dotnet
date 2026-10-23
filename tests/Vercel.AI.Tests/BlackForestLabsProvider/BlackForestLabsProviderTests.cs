// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.BlackForestLabs;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.BlackForestLabsImageModel;
using static Vercel.AI.Tests.BlackForestLabsVideoModel.BlackForestLabsVideoModelFixture;

namespace Vercel.AI.Tests.BlackForestLabsProvider;

/// <summary>Port of <c>black-forest-labs-provider.test.ts</c> &gt; <c>BlackForestLabs provider</c>.</summary>
public sealed class BlackForestLabsProviderTests
{
    private const string Id = "packages/black-forest-labs/src/black-forest-labs-provider.test.ts::BlackForestLabs provider::";

    private const string VideoPrompt = "A white kitten chases a butterfly across a sunlit garden.";

    /// <summary>The server of the upstream <c>createTestServer</c> call.</summary>
    private static BlackForestLabsServer CreateProviderServer()
    {
        var server = new BlackForestLabsServer();
        server.Json("https://api.example.com/v1/flux-pro-1.1", "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/poll\"}");
        server.Json("https://api.example.com/poll", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/image.png\"}}");
        server.Json("https://api.example.com/v1/flux-3-video", "{\"id\":\"req-123\",\"polling_url\":\"https://api.example.com/video-poll\"}");
        server.Json("https://api.example.com/video-poll", "{\"status\":\"Ready\",\"result\":{\"sample\":\"https://api.example.com/video.mp4\"}}");
        return server;
    }

    private static Vercel.AI.BlackForestLabs.BlackForestLabsProvider CreateProvider(
        BlackForestLabsServer? server,
        OpenAICompatibleOptions? options = null,
        int? pollIntervalMillis = null,
        int? pollTimeoutMillis = null)
    {
        return Vercel.AI.BlackForestLabs.BlackForestLabsProvider.Create(options, server, pollIntervalMillis, pollTimeoutMillis);
    }

    private static ImageModelCall ImageCall(string prompt)
    {
        return new ImageModelCall(
            prompt,
            null,
            null,
            1,
            null,
            "1:1",
            null,
            JsonDocument.Parse("{}").RootElement.Clone(),
            new Dictionary<string, string>(),
            CancellationToken.None);
    }

    [Fact]
    [UpstreamTest(Id + "creates image models via .image and .imageModel", Coverage = UpstreamCoverage.Covered)]
    public void CreatesImageModelsViaImageAndImageModel()
    {
        var provider = CreateProvider(null);

        var imageModel = provider.Image("flux-pro-1.1");
        var imageModel2 = provider.ImageModel("flux-pro-1.1-ultra");

        Assert.Equal("black-forest-labs.image", imageModel.Provider);
        Assert.Equal("flux-pro-1.1", imageModel.ModelId);
        Assert.Equal("flux-pro-1.1-ultra", imageModel2.ModelId);
        Assert.Equal("V4", Assert.IsType<Vercel.AI.BlackForestLabs.BlackForestLabsImageModel>(imageModel).SpecificationVersion);
    }

    [Fact]
    [UpstreamTest(
        Id + "configures baseURL and headers correctly",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port's user agent is ai-sdk/black-forest-labs/{version}, not ai-sdk-black-forest-labs/{version}.")]
    public async Task ConfiguresBaseUrlAndHeadersCorrectly()
    {
        var server = CreateProviderServer();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key", BaseUrl = "https://api.example.com/v1" };
        options.Headers["x-extra-header"] = "extra";
        var model = (IImageCaller)CreateProvider(server, options).ImageModel("flux-pro-1.1");

        await model.DoGenerateAsync(ImageCall("A serene mountain landscape at sunset"), CancellationToken.None);

        Assert.Equal("https://api.example.com/v1/flux-pro-1.1", server.Calls[0].Url);
        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal("test-api-key", server.Calls[0].Headers["x-key"]);
        Assert.Equal("extra", server.Calls[0].Headers["x-extra-header"]);
        var body = JsonNode.Parse(server.Calls[0].Body)!;
        Assert.Equal("A serene mountain landscape at sunset", (string?)body["prompt"]);
        Assert.Equal("1:1", (string?)body["aspect_ratio"]);

        Assert.Contains("ai-sdk/black-forest-labs/", server.Calls[0].Headers["user-agent"]);
        Assert.Contains("ai-sdk/black-forest-labs/", server.Calls[1].Headers["user-agent"]);
        Assert.Contains("ai-sdk/black-forest-labs/", server.Calls[2].Headers["user-agent"]);
    }

    [Fact]
    [UpstreamTest(Id + "uses provider polling options for timeout behavior", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesProviderPollingOptionsForTimeoutBehavior()
    {
        var server = CreateProviderServer();
        server.Json("https://api.example.com/poll", "{\"status\":\"Pending\"}");
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key", BaseUrl = "https://api.example.com/v1" };
        var model = (IImageCaller)CreateProvider(server, options, 10, 25).ImageModel("flux-pro-1.1");

        var exception = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(ImageCall("Timeout test"), CancellationToken.None));

        Assert.Contains("Black Forest Labs generation timed out.", exception.Message);
    }

    [Fact]
    [UpstreamTest(Id + "creates video models via .video and .videoModel", Coverage = UpstreamCoverage.Covered)]
    public void CreatesVideoModelsViaVideoAndVideoModel()
    {
        var provider = CreateProvider(null);

        var videoModel = provider.Video("flux-3-video");
        var videoModel2 = provider.VideoModel("flux-3-video");

        Assert.Equal("black-forest-labs.video", videoModel.Provider);
        Assert.Equal("flux-3-video", videoModel.ModelId);
        Assert.Equal("v4", videoModel.SpecificationVersion);
        var caller = Assert.IsAssignableFrom<IVideoCaller>(videoModel);
        Assert.Equal(1, caller.MaxVideosPerCall);
        Assert.True(caller.CanStart);
        Assert.True(caller.CanStatus);
        Assert.Equal("black-forest-labs.video", videoModel2.Provider);
    }

    [Fact]
    [UpstreamTest(
        Id + "configures baseURL and headers correctly for video models",
        Coverage = UpstreamCoverage.Covered,
        Note = "The port's user agent is ai-sdk/black-forest-labs/{version}, not ai-sdk-black-forest-labs/{version}.")]
    public async Task ConfiguresBaseUrlAndHeadersCorrectlyForVideoModels()
    {
        var server = CreateProviderServer();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key", BaseUrl = "https://api.example.com/v1" };
        options.Headers["x-extra-header"] = "extra";
        var model = (IVideoCaller)CreateProvider(server, options, 1).Video("flux-3-video");

        var result = await model.DoGenerateAsync(Call(), CancellationToken.None);

        Assert.Equal("https://api.example.com/v1/flux-3-video", server.Calls[0].Url);
        Assert.Equal(HttpMethod.Post, server.Calls[0].Method);
        Assert.Equal("test-api-key", server.Calls[0].Headers["x-key"]);
        Assert.Equal("extra", server.Calls[0].Headers["x-extra-header"]);
        AssertJson("{\"mode\":\"t2v\",\"prompt\":\"" + VideoPrompt + "\"}", JsonNode.Parse(server.Calls[0].Body));

        Assert.Contains("ai-sdk/black-forest-labs/", server.Calls[0].Headers["user-agent"]);

        // The signed video URL is returned rather than downloaded, so submit and
        // poll are the only two calls.
        Assert.Equal(2, server.Calls.Count);
        var video = Assert.Single(result.Videos);
        Assert.Equal("url", video.Type);
        Assert.Equal("https://api.example.com/video.mp4", video.Url);
        Assert.Equal("video/mp4", video.MediaType);
    }

    [Fact]
    [UpstreamTest(Id + "uses provider polling options for video timeout behavior", Coverage = UpstreamCoverage.Covered)]
    public async Task UsesProviderPollingOptionsForVideoTimeoutBehavior()
    {
        var server = CreateProviderServer();
        server.Json("https://api.example.com/video-poll", "{\"status\":\"Generating\"}");
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key", BaseUrl = "https://api.example.com/v1" };
        var model = (IVideoCaller)CreateProvider(server, options, 10, 25).Video("flux-3-video");

        // The reported timeout is what proves the provider-level setting reached
        // the model. The number of polls is deliberately not asserted: the video
        // model bounds polling by a wall-clock deadline rather than a fixed attempt
        // count, so how many requests fit inside 25ms depends on the machine.
        var exception = await Assert.ThrowsAsync<AiSdkException>(() => model.DoGenerateAsync(Call(), CancellationToken.None));

        Assert.Contains("Black Forest Labs video generation timed out after 25ms. Request id: req-123", exception.Message);
    }

    [Fact]
    public void DisablesAutoRedirectOnASuppliedHandler()
    {
        var clientHandler = new HttpClientHandler { AllowAutoRedirect = true };
        var socketsHandler = new SocketsHttpHandler { AllowAutoRedirect = true };
        var wrappedHandler = new HttpClientHandler { AllowAutoRedirect = true };

        Vercel.AI.BlackForestLabs.BlackForestLabsProvider.Create(null, clientHandler);
        Vercel.AI.BlackForestLabs.BlackForestLabsProvider.Create(null, socketsHandler);
        Vercel.AI.BlackForestLabs.BlackForestLabsProvider.Create(null, new PassThroughHandler { InnerHandler = new PassThroughHandler { InnerHandler = wrappedHandler } });

        Assert.False(clientHandler.AllowAutoRedirect);
        Assert.False(socketsHandler.AllowAutoRedirect);
        Assert.False(wrappedHandler.AllowAutoRedirect);
    }

    private sealed class PassThroughHandler : DelegatingHandler
    {
    }
}
