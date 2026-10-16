// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.KlingAI;
using Vercel.AI.Tests.MoreProviders;
using Vercel.AI.Tests.Upstream.KlingAI.KlingAIVideoModel;
using static Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider.KlingAIProviderSupport;
using KlingAIProviderType = Vercel.AI.KlingAI.KlingAIProvider;

namespace Vercel.AI.Tests.Upstream.KlingAI.KlingAIProvider;

/// <summary>Port of <c>klingai-provider.test.ts</c> &gt; <c>createKlingAI &gt; video</c>.</summary>
[Collection(KlingAIEnvironmentCollection.Name)]
public sealed class CreateKlingAIVideoTests : IDisposable
{
    private const string Prefix = "packages/klingai/src/klingai-provider.test.ts::createKlingAI > video::";

    private readonly KlingAIEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "should construct a video model with default configuration", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_construct_a_video_model_with_default_configuration()
    {
        var provider = KlingAIProviderType.Create(new KlingAIOptions { ApiKey = "test-api-key" });

        var model = provider.Video(ModelId);

        Assert.Equal(ModelId, model.ModelId);
        Assert.Equal("klingai.video", model.Provider);
        // The base URL is observed through the request the model sends.
        var call = await SendAsync(new KlingAIOptions { ApiKey = "test-api-key" });
        Assert.Equal("https://api-singapore.klingai.com/v1/videos/text2video/task-abc-123", call.Uri.ToString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should respect custom baseURL", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_respect_custom_baseURL()
    {
        var call = await SendAsync(new KlingAIOptions { ApiKey = "test-api-key", BaseUrl = "https://custom.klingai.example.com" });

        Assert.Equal("https://custom.klingai.example.com/v1/videos/text2video/task-abc-123", call.Uri.ToString());
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass custom fetch function", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_custom_fetch_function()
    {
        // The custom handler is the port's fetch: every request must go through it.
        var server = new KlingAIVideoModelSupport.Server();

        await SendAsync(server, new KlingAIOptions { ApiKey = "test-api-key" });

        Assert.Single(server.Handler.Calls);
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass headers as async function", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_headers_as_async_function()
    {
        // Headers are computed per request, not when the model is created.
        var server = new KlingAIVideoModelSupport.Server();
        var provider = KlingAIProviderType.Create(new KlingAIOptions { AccessKey = "test-ak", SecretKey = "test-sk" }, server.Handler);
        var model = (Vercel.AI.KlingAI.KlingAIVideoModel)provider.Video(ModelId);

        await model.DoStatusAsync(KlingAIVideoModelSupport.Status(), CancellationToken.None);
        _env.Stub("KLINGAI_API_KEY", "late-api-key");
        await model.DoStatusAsync(KlingAIVideoModelSupport.Status(), CancellationToken.None);

        Assert.StartsWith("Bearer ", server.Calls[0].Header("Authorization"));
        Assert.Equal("test-ak", (string?)KlingAIJwt.Decode(server.Calls[0].Header("Authorization")!.Substring("Bearer ".Length), 1)["iss"]);
        Assert.Equal("test-ak", (string?)KlingAIJwt.Decode(server.Calls[1].Header("Authorization")!.Substring("Bearer ".Length), 1)["iss"]);
    }
}
