// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Alibaba;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Anthropic;
using Vercel.AI.AssemblyAI;
using Vercel.AI.Azure;
using Vercel.AI.Baseten;
using Vercel.AI.BlackForestLabs;
using Vercel.AI.ByteDance;
using Vercel.AI.Cartesia;
using Vercel.AI.Cerebras;
using Vercel.AI.Cohere;
using Vercel.AI.Deepgram;
using Vercel.AI.DeepInfra;
using Vercel.AI.DeepSeek;
using Vercel.AI.ElevenLabs;
using Vercel.AI.Fal;
using Vercel.AI.Fireworks;
using Vercel.AI.FishAudio;
using Vercel.AI.Gateway;
using Vercel.AI.Gladia;
using Vercel.AI.GmiCloud;
using Vercel.AI.Google;
using Vercel.AI.Groq;
using Vercel.AI.HuggingFace;
using Vercel.AI.Hume;
using Vercel.AI.KlingAI;
using Vercel.AI.Luma;
using Vercel.AI.MiniMax;
using Vercel.AI.Mistral;
using Vercel.AI.Moonshot;
using Vercel.AI.OpenAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.OpenResponses;
using Vercel.AI.Perplexity;
using Vercel.AI.Prodia;
using Vercel.AI.Provider;
using Vercel.AI.QuiverAI;
using Vercel.AI.Replicate;
using Vercel.AI.RevAI;
using Vercel.AI.TogetherAI;
using Vercel.AI.Voyage;
using Vercel.AI.Xai;
using Vercel.AI.Zai;

namespace Vercel.AI.Tests;

public sealed class ProviderRequestTests
{
    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Each_provider_posts_its_public_route(string path, string header, string expected, Func<HttpMessageHandler, ProviderBase> create, string kind)
    {
        var handler = new ScriptedHandler();
        var provider = create(handler);
        await Invoke(provider, kind);
        Assert.Contains(path, handler.Uri);
        Assert.True(handler.Headers.TryGetValue(header, out var actual), "Missing header " + header);
        Assert.StartsWith(expected, actual);
    }

    [Fact]
    public async Task Gateway_sends_the_v4_specification_header()
    {
        var handler = new ScriptedHandler();
        var provider = GatewayProvider.Create(new GatewayOptions { ApiKey = "secret" }, handler);
        await provider.LanguageModel("openai/gpt-4.1-mini").DoGenerateAsync(Prompt(), CancellationToken.None);
        Assert.Equal("4", handler.Headers["ai-language-model-specification-version"]);
        Assert.Equal("0.0.1", handler.Headers["ai-gateway-protocol-version"]);
        Assert.Equal("api-key", handler.Headers["ai-gateway-auth-method"]);
        Assert.Equal("openai/gpt-4.1-mini", handler.Headers["ai-language-model-id"]);
        Assert.Contains("\"prompt\"", handler.Body);
    }

    [Fact]
    public async Task Bedrock_signs_with_sigv4()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://bedrock-runtime.us-east-1.amazonaws.com/model/demo/converse");
        var payload = System.Text.Encoding.UTF8.GetBytes("{}");
        AwsSigV4.Sign(request, payload, "us-east-1", "bedrock", "AKIA", "secret", null, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));
        Assert.True(request.Headers.TryGetValues("Authorization", out var values));
        var authorization = string.Join(" ", values);
        Assert.StartsWith("AWS4-HMAC-SHA256", authorization);
        Assert.Contains("us-east-1/bedrock/aws4_request", authorization);
    }

    public static IEnumerable<object[]> Cases()
    {
        yield return Chat("alibaba", "dashscope-intl.aliyuncs.com", h => AlibabaProvider.Create(Key(), h));
        yield return Chat("baseten", "inference.baseten.co", h => BasetenProvider.Create(Key(), h));
        yield return Chat("cerebras", "api.cerebras.ai", h => CerebrasProvider.Create(Key(), h));
        yield return Chat("deepinfra", "api.deepinfra.com", h => DeepInfraProvider.Create(Key(), h));
        yield return Chat("deepseek", "api.deepseek.com", h => DeepSeekProvider.Create(Key(), h));
        yield return Chat("fireworks", "api.fireworks.ai", h => FireworksProvider.Create(Key(), h));
        yield return Chat("groq", "api.groq.com", h => GroqProvider.Create(Key(), h));
        yield return Chat("huggingface", "router.huggingface.co", h => HuggingFaceProvider.Create(Key(), h));
        yield return Chat("minimax", "api.minimax.io", h => MiniMaxProvider.Create(Key(), h));
        yield return Chat("moonshot", "api.moonshot.ai", h => MoonshotProvider.Create(Key(), h));
        yield return Chat("perplexity", "api.perplexity.ai", h => PerplexityProvider.Create(Key(), h));
        yield return Chat("together", "api.together.xyz", h => TogetherAIProvider.Create(Key(), h));
        yield return Chat("xai", "api.x.ai", h => XaiProvider.Create(Key(), h));
        yield return Chat("zai", "api.z.ai", h => ZaiProvider.Create(Key(), h));
        yield return Chat("gmicloud", "api.gmi-serving.com", h => GmiCloudProvider.Create(Key(), h));
        yield return Chat("mistral", "api.mistral.ai", h => MistralProvider.Create(Key(), h));
        yield return Row("openai/deployments/m/chat/completions", "api-key", "secret", h => AzureOpenAIProvider.Create(new AzureOpenAIOptions { ApiKey = "secret", ResourceName = "demo" }, h), "chat");
        yield return Row("/language-model", "Authorization", "Bearer secret", h => GatewayProvider.Create(new GatewayOptions { ApiKey = "secret" }, h), "chat");
        yield return Row("/embedding-model", "Authorization", "Bearer secret", h => GatewayProvider.Create(new GatewayOptions { ApiKey = "secret" }, h), "gateway-embed");
        yield return Row("/image-model", "Authorization", "Bearer secret", h => GatewayProvider.Create(new GatewayOptions { ApiKey = "secret" }, h), "gateway-image");
        yield return Row("/chat/completions", "Authorization", "Bearer secret", h => OpenAIProvider.Create(new OpenAIOptions { ApiKey = "secret" }, h), "chat");
        yield return Row("/responses", "Authorization", "Bearer secret", h => OpenAIProvider.Create(new OpenAIOptions { ApiKey = "secret", UseResponsesApi = true }, h), "chat");
        yield return Row("/audio/speech", "Authorization", "Bearer secret", h => OpenAIProvider.Create(new OpenAIOptions { ApiKey = "secret" }, h), "speech");
        yield return Row("/v1/messages", "x-api-key", "secret", h => AnthropicProvider.Create(new AnthropicOptions { ApiKey = "secret" }, h), "chat");
        yield return Row("aws-external-anthropic", "x-api-key", "secret", h => AnthropicAwsProvider.Create(new AnthropicOptions { ApiKey = "secret", BaseUrl = "https://aws-external-anthropic.us-east-1.api.aws" }, h), "chat");
        yield return Row(":generateContent", "x-goog-api-key", "secret", h => GoogleProvider.Create(new GoogleOptions { ApiKey = "secret" }, h), "chat");
        yield return Row("publishers/google", "Authorization", "Bearer secret", h => GoogleVertexProvider.Create(new VertexOptions { ApiKey = "secret", Project = "demo", Region = "us-central1" }, h), "chat");
        yield return Row("/converse", "Authorization", "AWS4-HMAC-SHA256", h => AmazonBedrockProvider.Create(new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret", UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) }, h), "chat");
        yield return Row("api.cohere.com/v2/chat", "Authorization", "Bearer secret", h => CohereProvider.Create(new CohereOptions { ApiKey = "secret" }, h), "chat");
        yield return Row("/rerank", "Authorization", "Bearer secret", h => CohereProvider.Create(new CohereOptions { ApiKey = "secret" }, h), "rerank");
        yield return Row("/responses", "Authorization", "Bearer secret", h => OpenResponsesProvider.Create(new OpenResponsesOptions { ApiKey = "secret" }, h), "chat");
        yield return Media("/v2/transcript", "Authorization", "secret", h => AssemblyAIProvider.Create(Key(), h), "transcription");
        yield return Media("/flux-pro-1.1", "x-key", "secret", h => BlackForestLabsProvider.Create(Key(), h), "image");
        yield return Media("/images/generations", "Authorization", "Bearer secret", h => ByteDanceProvider.Create(Key(), h), "image");
        yield return Media("/tts/bytes", "Authorization", "Bearer secret", h => CartesiaProvider.Create(Key(), h), "speech");
        yield return Media("/v1/listen", "Authorization", "Token secret", h => DeepgramProvider.Create(Key(), h), "transcription");
        yield return Media("/v1/text-to-speech/", "xi-api-key", "secret", h => ElevenLabsProvider.Create(Key(), h), "speech");
        yield return Media("/fal-ai/flux/dev", "Authorization", "Key secret", h => FalProvider.Create(Key(), h), "image");
        yield return Media("/v1/tts", "Authorization", "Bearer secret", h => FishAudioProvider.Create(Key(), h), "speech");
        yield return Media("/v2/transcription", "x-gladia-key", "secret", h => GladiaProvider.Create(Key(), h), "transcription");
        yield return Media("/v0/tts", "X-Hume-Api-Key", "secret", h => HumeProvider.Create(Key(), h), "speech");
        yield return Media("/v1/videos/text2video", "Authorization", "Bearer secret", h => KlingAIProvider.Create(Key(), h), "video");
        yield return Media("/dream-machine/v1/generations", "Authorization", "Bearer secret", h => LumaProvider.Create(Key(), h), "video");
        yield return Media("/job", "Authorization", "Bearer secret", h => ProdiaProvider.Create(Key(), h), "image");
        yield return Media("api.quiver.ai", "Authorization", "Bearer secret", h => QuiverAIProvider.Create(Key(), h), "image");
        yield return Media("/models/m/predictions", "Authorization", "Bearer secret", h => ReplicateProvider.Create(Key(), h), "image");
        yield return Media("/speechtotext/v1/jobs", "Authorization", "Bearer secret", h => RevAIProvider.Create(Key(), h), "transcription");
        yield return Media("/embeddings", "Authorization", "Bearer secret", h => VoyageProvider.Create(Key(), h), "embedding");
    }

    private static object[] Chat(string name, string host, Func<HttpMessageHandler, ProviderBase> create)
    {
        return Row(host, "Authorization", "Bearer secret", create, "chat");
    }

    private static object[] Media(string path, string header, string expected, Func<HttpMessageHandler, ProviderBase> create, string kind)
    {
        return Row(path, header, expected, create, kind);
    }

    private static object[] Row(string path, string header, string expected, Func<HttpMessageHandler, ProviderBase> create, string kind)
    {
        return new object[] { path, header, expected, create, kind };
    }

    private static OpenAICompatibleOptions Key()
    {
        return new OpenAICompatibleOptions { ApiKey = "secret" };
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("hi") } };
    }

    private static async Task Invoke(ProviderBase provider, string kind)
    {
        switch (kind)
        {
            case "chat":
                await provider.LanguageModel("m").DoGenerateAsync(Prompt(), CancellationToken.None).ConfigureAwait(false);
                break;
            case "speech":
                await provider.SpeechModel("m").DoGenerateAsync(new SpeechCallOptions("hello") { Voice = "default" }, CancellationToken.None).ConfigureAwait(false);
                break;
            case "transcription":
                await provider.TranscriptionModel("m").DoTranscribeAsync(new AudioInput(new byte[] { 1, 2 }, "audio/wav", "a.wav"), CancellationToken.None).ConfigureAwait(false);
                break;
            case "image":
                await provider.ImageModel("m").DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None).ConfigureAwait(false);
                break;
            case "video":
                await provider.VideoModel("m").DoGenerateAsync(new VideoCallOptions("a wave"), CancellationToken.None).ConfigureAwait(false);
                break;
            case "embedding":
                await provider.EmbeddingModel("m").DoEmbedAsync(new[] { "hello" }, CancellationToken.None).ConfigureAwait(false);
                break;
            case "gateway-embed":
                await provider.EmbeddingModel("m").DoEmbedAsync(new[] { "hello" }, CancellationToken.None).ConfigureAwait(false);
                break;
            case "gateway-image":
                await provider.ImageModel("m").DoGenerateAsync(new ImageCallOptions("a cat"), CancellationToken.None).ConfigureAwait(false);
                break;
            case "rerank":
                await provider.RerankingModel("m").DoRerankAsync("query", new[] { "doc" }, null, CancellationToken.None).ConfigureAwait(false);
                break;
            default:
                throw new InvalidOperationException(kind);
        }
    }
}
