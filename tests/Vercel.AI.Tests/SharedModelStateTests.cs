// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Anthropic;
using Vercel.AI.DeepInfra;
using Vercel.AI.Fireworks;
using Vercel.AI.Gateway;
using Vercel.AI.Google;
using Vercel.AI.OpenAI;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.TogetherAI;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests;

/// <summary>Regression tests for per-call state. Models are shared, so two concurrent calls must not see each other.</summary>
public sealed class SharedModelStateTests
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    private static readonly ImageModelFile Png = ImageModelFile.FromFile(new byte[] { 137, 80, 78, 71 }, "image/png", "a.png");

    [Fact]
    public async Task OpenAICompatible_image_calls_keep_their_own_inputs_and_results()
    {
        using var handler = new Rendezvous(2, "{\"data\":[]}");
        var provider = OpenAICompatibleProvider.Create(new OpenAICompatibleOptions { ProviderName = "test", BaseUrl = "https://my.api.com/v1", ApiKey = "k", SupportsImages = true }, handler);
        var model = provider.ImageModel("dall-e-3");

        var (a, b) = await Both(
            model.DoGenerateAsync(new ImageCallOptions("one") { Seed = 1, Files = new[] { Png }, Headers = Caller("a") }, CancellationToken.None),
            model.DoGenerateAsync(new ImageCallOptions("two") { Headers = Caller("b") }, CancellationToken.None));

        Assert.EndsWith("/images/edits", handler.Requests["a"].Uri.AbsolutePath);
        Assert.EndsWith("/images/generations", handler.Requests["b"].Uri.AbsolutePath);
        Assert.Contains(a.CallWarnings, warning => warning.Message == "seed");
        Assert.DoesNotContain(b.CallWarnings, warning => warning.Message == "seed");
        Assert.Equal("a", a.Response!.Headers["x-call"]);
        Assert.Equal("b", b.Response!.Headers["x-call"]);
    }

    [Fact]
    public async Task Fireworks_image_calls_keep_their_own_inputs_and_results()
    {
        using var handler = new Rendezvous(2, "PNG");
        var model = (FireworksImageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("accounts/fireworks/models/flux-1-dev-fp8");

        var (a, b) = await Both(
            model.DoGenerateAsync(new ImageCallOptions("one") { Seed = 7, Files = new[] { Png, Png }, Headers = Caller("a") }, CancellationToken.None),
            model.DoGenerateAsync(new ImageCallOptions("two") { Headers = Caller("b") }, CancellationToken.None));

        Assert.Equal(7, JsonNode.Parse(handler.Requests["a"].Body)!["seed"]!.GetValue<int>());
        Assert.Null(JsonNode.Parse(handler.Requests["b"].Body)!["seed"]);
        Assert.Contains(a.CallWarnings, warning => warning.Message.StartsWith("Fireworks only supports a single input image", StringComparison.Ordinal));
        Assert.Empty(b.CallWarnings);
        Assert.Equal("a", a.Response!.Headers["x-call"]);
        Assert.Equal("b", b.Response!.Headers["x-call"]);
    }

    [Fact]
    public async Task DeepInfra_image_calls_keep_their_own_inputs_and_results()
    {
        using var handler = new Rendezvous(2, "{\"images\":[],\"data\":[]}");
        var model = (DeepInfraImageModel)DeepInfraProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("black-forest-labs/FLUX.1-dev");

        var (a, b) = await Both(
            model.DoGenerateAsync(new ImageCallOptions("one") { Files = new[] { Png }, Headers = Caller("a") }, CancellationToken.None),
            model.DoGenerateAsync(new ImageCallOptions("two") { Seed = 9, Headers = Caller("b") }, CancellationToken.None));

        Assert.EndsWith("/images/edits", handler.Requests["a"].Uri.AbsolutePath);
        Assert.Equal(9, JsonNode.Parse(handler.Requests["b"].Body)!["seed"]!.GetValue<int>());
        Assert.Equal("a", a.Response!.Headers["x-call"]);
        Assert.Equal("b", b.Response!.Headers["x-call"]);
    }

    [Fact]
    public async Task TogetherAI_image_calls_keep_their_own_inputs_and_results()
    {
        using var handler = new Rendezvous(2, "{\"data\":[]}");
        var model = (TogetherAIImageModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).ImageModel("black-forest-labs/FLUX.1-schnell");

        var (a, b) = await Both(
            model.DoGenerateAsync(new ImageCallOptions("one") { Seed = 5, AspectRatio = "16:9", Headers = Caller("a") }, CancellationToken.None),
            model.DoGenerateAsync(new ImageCallOptions("two") { Headers = Caller("b") }, CancellationToken.None));

        Assert.Equal(5, JsonNode.Parse(handler.Requests["a"].Body)!["seed"]!.GetValue<int>());
        Assert.Null(JsonNode.Parse(handler.Requests["b"].Body)!["seed"]);
        Assert.Single(a.CallWarnings);
        Assert.Empty(b.CallWarnings);
        Assert.Equal("a", a.Response!.Headers["x-call"]);
        Assert.Equal("b", b.Response!.Headers["x-call"]);
    }

    [Fact]
    public async Task OpenAICompatible_embedding_calls_keep_their_own_inputs_and_results()
    {
        using var handler = new Rendezvous(2, "{\"data\":[{\"embedding\":[0.5]}],\"usage\":{\"prompt_tokens\":1}}");
        var provider = OpenAICompatibleProvider.Create(new OpenAICompatibleOptions { ProviderName = "test-provider", BaseUrl = "https://my.api.com/v1", ApiKey = "k" }, handler);
        var model = (OpenAICompatibleEmbeddingModel)provider.EmbeddingModel("text-embedding-3-large");

        var (a, b) = await Both(
            model.DoEmbedAsync(new EmbeddingCallOptions(new[] { "x" }) { Dimensions = 64, Headers = Caller("a") }, CancellationToken.None),
            model.DoEmbedAsync(new EmbeddingCallOptions(new[] { "y" }) { User = "u-2", Headers = Caller("b") }, CancellationToken.None));

        var bodyA = JsonNode.Parse(handler.Requests["a"].Body)!;
        var bodyB = JsonNode.Parse(handler.Requests["b"].Body)!;
        Assert.Equal(64, bodyA["dimensions"]!.GetValue<int>());
        Assert.Null(bodyA["user"]);
        Assert.Null(bodyB["dimensions"]);
        Assert.Equal("u-2", bodyB["user"]!.GetValue<string>());
        Assert.Equal("a", a.Response!.Headers["x-call"]);
        Assert.Equal("b", b.Response!.Headers["x-call"]);
    }

    [Fact]
    public async Task Embedding_warnings_belong_to_the_call_that_raised_them()
    {
        using var handler = new Rendezvous(2, "{\"data\":[{\"embedding\":[0.5]}]}");
        var provider = OpenAICompatibleProvider.Create(new OpenAICompatibleOptions { ProviderName = "test-provider", BaseUrl = "https://my.api.com/v1", ApiKey = "k" }, handler);
        var model = provider.EmbeddingModel("text-embedding-3-large");
        var deprecated = new Dictionary<string, JsonElement> { ["openai-compatible"] = JsonDocument.Parse("{\"dimensions\":8}").RootElement.Clone() };

        var (a, b) = await Both(
            model.DoEmbedAsync(new[] { "x" }, deprecated, CancellationToken.None),
            model.DoEmbedAsync(new[] { "y" }, null, CancellationToken.None));

        Assert.Contains(a.Warnings, warning => warning.Type == "deprecated");
        Assert.Empty(b.Warnings);
    }

    [Fact]
    public async Task TogetherAI_rerank_calls_keep_their_own_headers_and_responses()
    {
        using var handler = new Rendezvous(2, "{\"id\":\"r\",\"results\":[{\"index\":0,\"relevance_score\":0.5}]}");
        var model = (TogetherAIRerankingModel)TogetherAIProvider.Create(new OpenAICompatibleOptions { ApiKey = "k" }, handler).RerankingModel("Salesforce/Llama-Rank-v1");
        var documents = new RerankModelDocuments("text", new object?[] { "a" });

        var (a, b) = await Both(
            model.DoRerankAsync(new RerankModelCall(documents, "q", 1, null, new Dictionary<string, string> { ["X-Call"] = "a" }, CancellationToken.None), CancellationToken.None),
            model.DoRerankAsync(new RerankModelCall(documents, "q", 1, null, new Dictionary<string, string> { ["X-Call"] = "b" }, CancellationToken.None), CancellationToken.None));

        Assert.Equal("a", a.Response!.Headers!["x-call"]);
        Assert.Equal("b", b.Response!.Headers!["x-call"]);
    }

    [Fact]
    public void XaiProvider_does_not_change_the_options_it_was_given()
    {
        var options = new OpenAICompatibleOptions();
        options.Headers["X-Custom"] = "1";

        var provider = XaiProvider.Create(options);
        provider.Options.Headers["X-Other"] = "2";

        Assert.Equal("xai", provider.Options.ProviderName);
        Assert.Equal(XaiProvider.DefaultBaseUrl, provider.Options.BaseUrl);
        Assert.Equal("openai-compatible", options.ProviderName);
        Assert.Equal("https://api.openai.com/v1", options.BaseUrl);
        Assert.Equal("OPENAI_API_KEY", options.ApiKeyEnvironmentVariable);
        Assert.True(options.SupportsEmbeddings);
        Assert.False(options.SupportsImages);
        Assert.Equal(new[] { "X-Custom" }, options.Headers.Keys);
        Assert.Equal("1", provider.Options.Headers["X-Custom"]);
    }

    [Fact]
    public void AnthropicAwsProvider_does_not_change_the_options_it_was_given()
    {
        var options = new AnthropicOptions();

        var provider = AnthropicAwsProvider.Create(options);

        Assert.Equal("ANTHROPIC_AWS_API_KEY", provider.Options.ApiKeyEnvironmentVariable);
        Assert.Equal("ANTHROPIC_API_KEY", options.ApiKeyEnvironmentVariable);
    }

    [Fact]
    public async Task Vertex_partner_providers_create_one_inner_client_for_concurrent_callers()
    {
        var maas = new GoogleVertexMaasProvider("project", "us-central1");
        var xai = new GoogleVertexXaiProvider("project", "us-central1");
        Assert.False(maas.ClientCreated);
        Assert.False(xai.ClientCreated);

        var maasModels = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => maas.LanguageModel("meta/llama"))));
        var xaiModels = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => xai.LanguageModel("xai/grok"))));

        Assert.True(maas.ClientCreated);
        Assert.True(xai.ClientCreated);
        Assert.Single(maasModels.Select(InnerProvider).Distinct());
        Assert.Single(xaiModels.Select(InnerProvider).Distinct());
    }

    [Fact]
    public async Task AmazonBedrock_embedding_calls_keep_their_own_headers()
    {
        using var handler = new Rendezvous(2, "{\"embedding\":[0.5],\"inputTextTokenCount\":1}");
        var provider = AmazonBedrockProvider.Create(new AmazonBedrockOptions { AccessKeyId = "AKIA", SecretAccessKey = "secret" }, handler);
        var model = (AmazonBedrockEmbeddingModel)provider.EmbeddingModel("amazon.titan-embed-text-v2:0");

        await Both(
            model.DoEmbedAsync(new EmbeddingCallOptions(new[] { "x" }) { Headers = Caller("a") }, CancellationToken.None),
            model.DoEmbedAsync(new EmbeddingCallOptions(new[] { "y" }) { Headers = Caller("b") }, CancellationToken.None));

        Assert.Contains("\"x\"", handler.Requests["a"].Body);
        Assert.Contains("\"y\"", handler.Requests["b"].Body);
    }

    [Fact]
    public void VertexProvider_does_not_change_the_options_it_was_given()
    {
        var options = new VertexOptions { Project = "p" };
        options.Headers["X-Custom"] = "1";

        var provider = new GoogleVertexProvider(new HttpClient(), options);
        provider.Options.Headers["X-Other"] = "2";

        Assert.Equal(string.Empty, options.BaseUrl);
        Assert.Equal("us-central1", options.Region);
        Assert.Equal(new[] { "X-Custom" }, options.Headers.Keys);
        Assert.Contains("p", provider.Options.BaseUrl, StringComparison.Ordinal);
        Assert.Equal("1", provider.Options.Headers["X-Custom"]);
    }

    [Fact]
    public void OpenAICompatible_providers_do_not_change_the_options_they_were_given()
    {
        var creates = typeof(OpenAICompatibleProvider).Assembly.GetTypes()
            .Where(type => typeof(OpenAICompatibleProvider).IsAssignableFrom(type) && !type.IsAbstract)
            .Select(type => type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { typeof(OpenAICompatibleOptions), typeof(HttpMessageHandler) }, null))
            .Where(method => method != null)
            .ToList();
        Assert.True(creates.Count > 30);

        var failures = new List<string>();
        foreach (var create in creates)
        {
            var options = new OpenAICompatibleOptions();
            options.Headers["X-Custom"] = "1";
            var before = Snapshot(options);

            try
            {
                create!.Invoke(null, new object?[] { options, new Rendezvous(1, "{}") });
            }
            catch (TargetInvocationException)
            {
                // A provider may reject the bare options. It must still leave them as they were.
            }

            if (Snapshot(options) != before)
            {
                failures.Add(create!.DeclaringType!.Name);
            }
        }

        Assert.Empty(failures);
    }

    private static string Snapshot(OpenAICompatibleOptions options)
    {
        return string.Join("|", options.ProviderName, options.BaseUrl, options.ApiKey, options.ApiKeyEnvironmentVariable, options.ApiKeyStyle, options.ApiKeyHeaderName,
            options.SupportsEmbeddings, options.SupportsImages, options.DownloadHttpClient != null,
            string.Join(",", options.AdditionalApiKeyEnvironmentVariables), string.Join(",", options.Headers.Select(pair => pair.Key + "=" + pair.Value)),
            string.Join(",", options.QueryParameters.Select(pair => pair.Key + "=" + pair.Value)));
    }

    [Fact]
    public async Task GenerateImageAsync_forwards_every_image_option_to_the_model()
    {
        var model = new CapturingImageModel();
        var client = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "k" }));
        var providerOptions = new Dictionary<string, JsonElement> { ["p"] = JsonDocument.Parse("{}").RootElement.Clone() };

        await client.GenerateImageAsync(new GenerateImageOptions
        {
            Model = model,
            Prompt = "x",
            Seed = 3,
            Files = new[] { Png },
            Mask = Png,
            ProviderOptions = providerOptions,
            Headers = Caller("a"),
        });

        var call = model.Call!;
        Assert.Equal(3, call.Seed);
        Assert.Same(Png, call.Files!.Single());
        Assert.Same(Png, call.Mask);
        Assert.Same(providerOptions, call.ProviderOptions);
        Assert.Equal("a", call.Headers!["X-Call"]);
    }

    [Fact]
    public async Task OpenAI_image_model_sends_an_edit_request_with_the_forwarded_options()
    {
        using var handler = new Rendezvous(1, "{\"data\":[]}");
        var model = new OpenAIImageModel(OpenAIProvider.Create(new OpenAIOptions { ApiKey = "k" }, handler), "gpt-image-1");
        var providerOptions = new Dictionary<string, JsonElement> { ["openai"] = JsonDocument.Parse("{\"quality\":\"high\"}").RootElement.Clone() };

        await model.DoGenerateAsync(new ImageCallOptions("p") { Files = new[] { Png }, Mask = Png, Headers = Caller("a") }, CancellationToken.None);

        Assert.EndsWith("/images/edits", handler.Requests["a"].Uri.AbsolutePath);
        Assert.Contains("name=mask", handler.Requests["a"].Body, StringComparison.Ordinal);

        using var generate = new Rendezvous(1, "{\"data\":[]}");
        var plain = new OpenAIImageModel(OpenAIProvider.Create(new OpenAIOptions { ApiKey = "k" }, generate), "gpt-image-1");
        await plain.DoGenerateAsync(new ImageCallOptions("p") { ProviderOptions = providerOptions, Headers = Caller("b") }, CancellationToken.None);
        Assert.EndsWith("/images/generations", generate.Requests["b"].Uri.AbsolutePath);
        Assert.Equal("high", JsonNode.Parse(generate.Requests["b"].Body)!["quality"]!.GetValue<string>());
    }

    private sealed class CapturingImageModel : IImageModel
    {
        public ImageCallOptions? Call { get; private set; }

        public string Provider => "test";

        public string ModelId => "m";

        public Task<ImageGenerationResult> DoGenerateAsync(ImageCallOptions options, CancellationToken cancellationToken)
        {
            Call = options;
            return Task.FromResult(new ImageGenerationResult(Array.Empty<Vercel.AI.Provider.GeneratedImage>()));
        }
    }

    private static OpenAICompatibleProvider InnerProvider(ILanguageModel model)
    {
        var field = model.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Single(candidate => typeof(OpenAICompatibleProvider).IsAssignableFrom(candidate.FieldType));
        return (OpenAICompatibleProvider)field.GetValue(model)!;
    }

    private static Dictionary<string, string?> Caller(string name)
    {
        return new Dictionary<string, string?> { ["X-Call"] = name };
    }

    private static async Task<(T A, T B)> Both<T>(Task<T> first, Task<T> second)
    {
        var results = await Task.WhenAll(first, second).WaitAsync(Limit);
        return (results[0], results[1]);
    }

    /// <summary>Holds every request until all expected callers are in flight, then answers each one. This makes the calls overlap.</summary>
    private sealed class Rendezvous : HttpMessageHandler
    {
        private readonly int _expected;
        private readonly string _body;
        private readonly TaskCompletionSource<bool> _all = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _arrived;

        public Rendezvous(int expected, string body)
        {
            _expected = expected;
            _body = body;
        }

        public ConcurrentDictionary<string, (Uri Uri, string Body)> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var caller = request.Headers.TryGetValues("X-Call", out var values) ? values.Single() : "none";
            var body = request.Content == null ? string.Empty : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            Requests[caller] = (request.RequestUri!, body);
            if (Interlocked.Increment(ref _arrived) == _expected)
            {
                _all.TrySetResult(true);
            }

            await _all.Task.WaitAsync(Limit, cancellationToken).ConfigureAwait(false);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_body, Encoding.UTF8, "application/json") };
            response.Headers.TryAddWithoutValidation("x-call", caller);
            return response;
        }
    }
}
