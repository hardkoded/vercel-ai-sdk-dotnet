// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.MoreProviders;
using HeyGenProviderType = Vercel.AI.HeyGen.HeyGenProvider;

namespace Vercel.AI.Tests.Upstream.HeyGen.HeyGenProvider;

/// <summary>Port of <c>heygen-provider.test.ts</c> &gt; <c>createHeyGen</c>.</summary>
public sealed class CreateHeyGenTests
{
    private const string Prefix = "packages/heygen/src/heygen-provider.test.ts::createHeyGen::";
    private const string KeyVariable = "HEYGEN_API_KEY";

    [Fact]
    [UpstreamTest(Prefix + "creates video models through both aliases", Coverage = UpstreamCoverage.Covered)]
    public void Creates_video_models_through_both_aliases()
    {
        var provider = HeyGenProviderType.Create();
        foreach (var model in new[] { provider.Video("heygen-video-1"), provider.VideoModel("heygen-video-1") })
        {
            Assert.Equal("heygen.video", model.Provider);
            Assert.Equal("heygen-video-1", model.ModelId);
            Assert.Equal(1, Assert.IsAssignableFrom<IVideoCaller>(model).MaxVideosPerCall);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects unsupported model types", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_unsupported_model_types()
    {
        var provider = HeyGenProviderType.Create();
        Assert.Throws<AiSdkException>(() => provider.LanguageModel("other"));
        Assert.Throws<AiSdkException>(() => provider.EmbeddingModel("other"));
        Assert.Throws<AiSdkException>(() => provider.ImageModel("other"));
        Assert.Throws<AiSdkException>(() => provider.SpeechModel("other"));
        Assert.Throws<AiSdkException>(() => provider.TranscriptionModel("other"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(Prefix + "supports explicit/environment keys, custom endpoints, headers and fetch (explicit=%s)", Coverage = UpstreamCoverage.Covered)]
    public async Task Supports_explicit_environment_keys_custom_endpoints_headers_and_fetch(bool explicitKey)
    {
        var previous = Environment.GetEnvironmentVariable(KeyVariable);
        Environment.SetEnvironmentVariable(KeyVariable, "env-key");
        try
        {
            var handler = new ParityHandler(_ => ParityHandler.Json("{\"data\":{\"status\":\"pending\"}}"));
            var options = new OpenAICompatibleOptions { ApiKey = explicitKey ? "explicit-key" : null, BaseUrl = "https://proxy.example.com/heygen/" };
            options.Headers["X-Custom"] = "custom";
            var provider = HeyGenProviderType.Create(options, handler);
            await Assert.IsAssignableFrom<IVideoCaller>(provider.Video("heygen-video-1")).DoStatusAsync(
                Status(new Dictionary<string, string> { ["X-Request"] = "request" }),
                CancellationToken.None).ConfigureAwait(false);

            var call = Assert.Single(handler.Calls);
            Assert.Equal("https://proxy.example.com/heygen/v3/models/videos/video-1", call.Uri.AbsoluteUri);
            Assert.Equal(explicitKey ? "explicit-key" : "env-key", call.Header("x-api-key"));
            Assert.Equal("custom", call.Header("x-custom"));
            Assert.Equal("request", call.Header("x-request"));
            Assert.Contains("ai-sdk/heygen/", call.Header("user-agent"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(KeyVariable, previous);
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "loads credentials lazily and reports a missing key", Coverage = UpstreamCoverage.Covered)]
    public async Task Loads_credentials_lazily_and_reports_a_missing_key()
    {
        var previous = Environment.GetEnvironmentVariable(KeyVariable);
        Environment.SetEnvironmentVariable(KeyVariable, null);
        try
        {
            var provider = HeyGenProviderType.Create(handler: new ParityHandler());
            var model = Assert.IsAssignableFrom<IVideoCaller>(provider.Video("heygen-video-1"));
            var error = await Assert.ThrowsAsync<AiSdkException>(() => model.DoStatusAsync(Status(), CancellationToken.None)).ConfigureAwait(false);
            Assert.Contains(KeyVariable, error.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable(KeyVariable, previous);
        }
    }

    private static VideoModelCall Status(IReadOnlyDictionary<string, string>? headers = null)
    {
        return new VideoModelCall(null, 0, null, null, null, null, null, null, null, null, null, OperationJson.Parse("{}"), headers ?? new Dictionary<string, string>(), CancellationToken.None, operation: OperationJson.Parse("{\"videoId\":\"video-1\",\"mode\":\"text_to_video\",\"resolution\":\"768p\"}"));
    }
}
