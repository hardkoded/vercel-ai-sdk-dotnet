// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Alibaba;
using Vercel.AI.AssemblyAI;
using Vercel.AI.ByteDance;
using Vercel.AI.Cartesia;
using Vercel.AI.Deepgram;
using Vercel.AI.Fal;
using Vercel.AI.KlingAI;
using Vercel.AI.Luma;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Replicate;

namespace Vercel.AI.Tests;

/// <summary>Construction, auth, and the primary modality endpoint for the larger assigned providers.</summary>
public sealed class PrimaryEndpointParityTests
{
    private static readonly string[] EmbeddingValues = { "sunny day at the beach", "rainy day in the city" };

    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-embedding-model.test.ts::doEmbed::should pass the model and values", Coverage = UpstreamCoverage.Covered)]
    public async Task Alibaba_embedding_posts_texts_and_empty_parameters()
    {
        var handler = new RecordingHandler(_ => RecordingHandler.Json("{\"output\":{\"embeddings\":[]}}"));
        var provider = AlibabaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        var model = (AlibabaProvider.AlibabaEmbeddingModel)provider.EmbeddingModel("text-embedding-v4");
        await model.EmbedAsync(new AlibabaEmbeddingRequest(EmbeddingValues), CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://dashscope-intl.aliyuncs.com/api/v1/services/embeddings/text-embedding/text-embedding", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("{\"model\":\"text-embedding-v4\",\"input\":{\"texts\":[\"sunny day at the beach\",\"rainy day in the city\"]},\"parameters\":{}}", handler.Calls[0].Text);
    }

    [Fact]
    [UpstreamTest("packages/alibaba/src/alibaba-embedding-model.test.ts::doEmbed::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Alibaba_embedding_sends_bearer_custom_headers_and_user_agent()
    {
        var handler = new RecordingHandler(_ => RecordingHandler.Json("{\"output\":{\"embeddings\":[]}}"));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = AlibabaProvider.Create(options, handler);
        var model = (AlibabaProvider.AlibabaEmbeddingModel)provider.EmbeddingModel("text-embedding-v4");
        await model.EmbedAsync(new AlibabaEmbeddingRequest(EmbeddingValues) { Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" } }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/alibaba/0.0.0-test", call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/assemblyai/src/assemblyai-transcription-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task AssemblyAI_upload_sends_the_raw_key_and_user_agent()
    {
        var handler = new RecordingHandler(call => call.Uri.AbsolutePath.EndsWith("/v2/upload", StringComparison.Ordinal)
            ? RecordingHandler.Json("{\"upload_url\":\"https://cdn.example/audio\"}")
            : RecordingHandler.Json("{\"text\":\"hello\"}"));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = AssemblyAIProvider.Create(options, handler);
        await provider.TranscribeAsync("best", new AudioInput(new byte[] { 1, 2, 3 }, "audio/wav", "audio.wav"), new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("https://api.assemblyai.com/v2/upload", call.Uri.AbsoluteUri);
        Assert.Equal("test-api-key", call.Header("Authorization"));
        Assert.Equal("application/octet-stream", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/assemblyai/0.0.0-test", call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
        Assert.Equal(new byte[] { 1, 2, 3 }, call.Body);
    }

    [Fact]
    [UpstreamTest("packages/bytedance/src/bytedance-image-model.test.ts::ByteDanceImageModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task ByteDance_image_sends_json_and_custom_headers()
    {
        var handler = ImageHandler();
        var options = HeaderOptions("https://api.example.com");
        await ByteDanceProvider.Create(options, handler).GenerateImageAsync("seedream-5-0-260128", new ImageCallOptions("A salamander in a forest pond at dusk surrounded by fireflies"), RequestHeaders(), CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Equal("https://api.example.com/images/generations", call.Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/cartesia/src/cartesia-speech-model.test.ts::CartesiaSpeechModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Cartesia_speech_sends_version_and_custom_headers()
    {
        var handler = new RecordingHandler(_ => RecordingHandler.Bytes(new byte[8], "audio/mpeg"));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = CartesiaProvider.Create(options, handler);
        await provider.GenerateSpeechAsync("sonic-3.5", new SpeechCallOptions("Hello, world!") { Voice = "test-voice-id" }, new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-key", call.Header("Authorization"));
        Assert.Equal("2026-03-01", call.Header("Cartesia-Version"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
    }

    [Fact]
    [UpstreamTest("packages/cartesia/src/cartesia-speech-model.test.ts::CartesiaSpeechModel > doGenerate::should include user-agent header", Coverage = UpstreamCoverage.Covered)]
    public async Task Cartesia_speech_sends_the_sdk_user_agent()
    {
        var handler = new RecordingHandler(_ => RecordingHandler.Bytes(new byte[8], "audio/mpeg"));
        var provider = CartesiaProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        await provider.GenerateSpeechAsync("sonic-3.5", new SpeechCallOptions("Hello, world!") { Voice = "test-voice-id" }, null, CancellationToken.None).ConfigureAwait(false);
        Assert.Contains("ai-sdk/cartesia/0.0.0-test", handler.Calls[0].Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/deepgram/src/deepgram-transcription-model.test.ts::doGenerate > transcription::should pass the model", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepgram_puts_the_model_in_the_query_and_posts_raw_audio()
    {
        var audio = new byte[] { 1, 2, 3, 4 };
        var handler = new RecordingHandler(_ => RecordingHandler.Json("{\"text\":\"hello\"}"));
        var provider = DeepgramProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, handler);
        await provider.TranscribeAsync("nova-3", new AudioInput(audio, "audio/wav", "audio.wav"), null, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.deepgram.com/v1/listen?model=nova-3", handler.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("audio/wav", handler.Calls[0].Header("Content-Type"));
        Assert.Equal(audio, handler.Calls[0].Body);
        Assert.DoesNotContain("name=\"model\"", handler.Calls[0].Text, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/deepgram/src/deepgram-transcription-model.test.ts::doGenerate > transcription::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Deepgram_sends_token_auth_and_the_audio_content_type()
    {
        var handler = new RecordingHandler(_ => RecordingHandler.Json("{\"text\":\"hello\"}"));
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-key" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = DeepgramProvider.Create(options, handler);
        await provider.TranscribeAsync("nova-3", new AudioInput(new byte[] { 1 }, "audio/wav", "audio.wav"), new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("Token test-api-key", call.Header("Authorization"));
        Assert.Equal("audio/wav", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Contains("ai-sdk/deepgram/0.0.0-test", call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest("packages/fal/src/fal-image-model.test.ts::FalImageModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Fal_image_sends_json_and_custom_headers()
    {
        var handler = ImageHandler();
        var options = HeaderOptions("https://api.example.com");
        await FalProvider.Create(options, handler).GenerateImageAsync("fal-ai/qwen-image", new ImageCallOptions("A cute baby sea otter"), RequestHeaders(), CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Equal("https://api.example.com/fal-ai/qwen-image", call.Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/klingai/src/klingai-video-model.test.ts::KlingAIVideoModel > doStart::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Kling_text_to_video_sends_bearer_and_custom_headers()
    {
        var handler = ImageHandler();
        var options = new OpenAICompatibleOptions { BaseUrl = "https://api.example.com" };
        options.Headers["Authorization"] = "Bearer custom-token";
        options.Headers["X-Custom"] = "value";
        await KlingAIProvider.Create(options, handler).GenerateVideoAsync("kling-v2.6-t2v", new VideoCallOptions("A cat"), new Dictionary<string, string> { ["X-Request-Header"] = "request-value" }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("Bearer custom-token", call.Header("Authorization"));
        Assert.Equal("value", call.Header("X-Custom"));
        Assert.Equal("request-value", call.Header("X-Request-Header"));
        Assert.Equal("https://api.example.com/v1/videos/text2video", call.Uri.AbsoluteUri);
        using var body = JsonDocument.Parse(call.Text);
        Assert.Equal("kling-v2.6-t2v", body.RootElement.GetProperty("model_name").GetString());
    }

    [Fact]
    [UpstreamTest("packages/luma/src/luma-image-model.test.ts::LumaImageModel > doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Luma_image_sends_json_and_custom_headers()
    {
        var handler = ImageHandler();
        var options = HeaderOptions("https://api.example.com");
        await LumaProvider.Create(options, handler).GenerateImageAsync("test-model", new ImageCallOptions("A cute baby sea otter"), RequestHeaders(), CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Equal("https://api.example.com/dream-machine/v1/generations/image", call.Uri.AbsoluteUri);
    }

    [Fact]
    [UpstreamTest("packages/replicate/src/replicate-image-model.test.ts::doGenerate::should pass the model and the settings", Coverage = UpstreamCoverage.Covered)]
    public async Task Replicate_sends_the_model_path_and_input_settings()
    {
        var handler = ImageHandler();
        var provider = ReplicateProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-token" }, handler);
        await provider.GenerateImageAsync("black-forest-labs/flux-schnell", new ReplicateImageRequest("The Loch Ness monster getting a manicure")
        {
            Count = 1,
            AspectRatio = "3:4",
            Size = "1024x768",
            Seed = 123,
            ExtraInput = new JsonObject { ["style"] = "realistic_image" },
        }, CancellationToken.None).ConfigureAwait(false);
        Assert.Equal("https://api.replicate.com/v1/models/black-forest-labs/flux-schnell/predictions", handler.Calls[0].Uri.AbsoluteUri);
        using var body = JsonDocument.Parse(handler.Calls[0].Text);
        Assert.Equal(1, body.RootElement.EnumerateObject().Count());
        var input = body.RootElement.GetProperty("input");
        Assert.Equal(6, input.EnumerateObject().Count());
        Assert.Equal("The Loch Ness monster getting a manicure", input.GetProperty("prompt").GetString());
        Assert.Equal(1, input.GetProperty("num_outputs").GetInt32());
        Assert.Equal("3:4", input.GetProperty("aspect_ratio").GetString());
        Assert.Equal("1024x768", input.GetProperty("size").GetString());
        Assert.Equal(123, input.GetProperty("seed").GetInt32());
        Assert.Equal("realistic_image", input.GetProperty("style").GetString());
    }

    [Fact]
    [UpstreamTest("packages/replicate/src/replicate-image-model.test.ts::doGenerate::should pass headers and set the prefer header", Coverage = UpstreamCoverage.Covered)]
    public async Task Replicate_sets_prefer_wait_and_the_user_agent()
    {
        var handler = ImageHandler();
        var options = new OpenAICompatibleOptions { ApiKey = "test-api-token" };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var provider = ReplicateProvider.Create(options, handler);
        await provider.GenerateImageAsync("black-forest-labs/flux-schnell", new ReplicateImageRequest("The Loch Ness monster getting a manicure")
        {
            Headers = new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" },
        }, CancellationToken.None).ConfigureAwait(false);
        var call = handler.Calls[0];
        Assert.Equal("Bearer test-api-token", call.Header("Authorization"));
        Assert.Equal("application/json", call.Header("Content-Type"));
        Assert.Equal("provider-header-value", call.Header("Custom-Provider-Header"));
        Assert.Equal("request-header-value", call.Header("Custom-Request-Header"));
        Assert.Equal("wait", call.Header("Prefer"));
        Assert.Contains("ai-sdk/replicate/0.0.0-test", call.Header("User-Agent") ?? string.Empty, StringComparison.Ordinal);
    }

    private static RecordingHandler ImageHandler()
    {
        return new RecordingHandler(_ => RecordingHandler.Json("{\"url\":\"https://example.test/out.bin\"}"));
    }

    private static OpenAICompatibleOptions HeaderOptions(string baseUrl)
    {
        var options = new OpenAICompatibleOptions { BaseUrl = baseUrl };
        options.Headers["Custom-Provider-Header"] = "provider-header-value";
        return options;
    }

    private static Dictionary<string, string> RequestHeaders()
    {
        return new Dictionary<string, string> { ["Custom-Request-Header"] = "request-header-value" };
    }
}
