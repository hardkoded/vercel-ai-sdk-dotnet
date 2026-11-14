// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;
using Vercel.AI.Tencent;
using static Vercel.AI.Tests.Upstream.Tencent.TencentProvider.TencentProviderSupport;
using TencentProviderType = Vercel.AI.Tencent.TencentProvider;

namespace Vercel.AI.Tests.Upstream.Tencent.TencentProvider;

/// <summary>Port of <c>tencent-provider.test.ts</c> &gt; <c>createTencent</c> &gt; <c>language models</c>.</summary>
[Collection(TencentEnvironmentCollection.Name)]
public sealed class LanguageModelsTests : IDisposable
{
    private const string Prefix = UpstreamFile + "::createTencent > language models::";

    private readonly TencentEnvironment _env = new();

    public void Dispose()
    {
        _env.Dispose();
    }

    [Fact]
    [UpstreamTest(Prefix + "creates TokenHub chat models", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Creates_TokenHub_chat_models()
    {
        var provider = TencentProviderType.Create(new TencentOptions { ApiKey = "test-key" });

        var model = provider.LanguageModel("hy4");

        Assert.IsType<TencentChatLanguageModel>(model);
        Assert.Equal("tencent.chat", model.Provider);
        Assert.Equal("hy4", model.ModelId);
        Assert.IsType<TencentChatLanguageModel>(provider.Chat("hy3"));
        Assert.IsType<TencentChatLanguageModel>(provider.LanguageModel("hy3"));
    }

    [Fact]
    [UpstreamTest(Prefix + "calls TokenHub chat completions with the API key and usage", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Calls_TokenHub_chat_completions_with_the_API_key_and_usage()
    {
        var server = Server();
        var model = Create(new TencentOptions { ApiKey = "test-key" }, server).LanguageModel("hy4");
        var options = ChatPrompt();
        options.ProviderOptions = new Dictionary<string, System.Text.Json.JsonElement>
        {
            ["tencent"] = Vercel.AI.Operations.OperationJson.Parse("""{"reasoningEffort":"high","thinking":{"type":"enabled"}}"""),
        };

        var result = await model.DoGenerateAsync(options, CancellationToken.None);

        var call = server.Calls[0];
        Assert.Equal("Bearer test-key", call.Header("authorization"));
        Assert.Contains("ai-sdk/tencent/", call.Header("user-agent"));
        JsonAssert.Equal(System.Text.Json.Nodes.JsonNode.Parse(call.Text), """{"model":"hy4","messages":[{"role":"user","content":"Hi"}],"reasoning_effort":"high","thinking":{"type":"enabled"}}""");
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello", text.Text);
        Assert.Equal(3, result.Usage.InputTokens);
    }

    [Fact]
    [UpstreamTest(Prefix + "reads the API key from TENCENT_TOKENHUB_API_KEY", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Reads_the_API_key_from_TENCENT_TOKENHUB_API_KEY()
    {
        _env.Stub("TENCENT_TOKENHUB_API_KEY", "env-key");
        var server = Server();

        await Create(new TencentOptions(), server).LanguageModel("hy4").DoGenerateAsync(ChatPrompt(), CancellationToken.None);

        Assert.Equal("Bearer env-key", server.Calls[0].Header("authorization"));
    }

    [Fact]
    [UpstreamTest(Prefix + "throws when the API key is missing", Coverage = UpstreamCoverage.Partial, Commit = Commit, Note = "The port throws AiSdkException; it has no LoadAPIKeyError, so the error type is not asserted.")]
    public async Task Throws_when_the_API_key_is_missing()
    {
        var error = await Assert.ThrowsAsync<AiSdkException>(() => Create(new TencentOptions(), Server()).LanguageModel("hy4").DoGenerateAsync(ChatPrompt(), CancellationToken.None));

        Assert.Contains("TENCENT_TOKENHUB_API_KEY", error.Message);
    }

    [Fact]
    [UpstreamTest(Prefix + "keeps the TokenHub endpoint after workflow serialization", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Keeps_the_TokenHub_endpoint_after_workflow_serialization()
    {
        var server = Server();
        var model = (TencentChatLanguageModel)Create(new TencentOptions { ApiKey = "test-key" }, server).LanguageModel("hy4");
        var serialized = model.Serialize();
        var restored = TencentChatLanguageModel.Deserialize(serialized, "test-key", server);

        var result = await restored.DoGenerateAsync(ChatPrompt(), CancellationToken.None);

        Assert.IsType<TencentChatLanguageModel>(restored);
        Assert.Equal("tencent.chat", restored.Provider);
        Assert.Equal("https://tokenhub-intl.tencentcloudmaas.com/v1/chat/completions", server.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("Bearer test-key", server.Calls[0].Header("authorization"));
        var text = Assert.IsType<GeneratedText>(Assert.Single(result.Content));
        Assert.Equal("Hello", text.Text);
    }

    [Fact]
    [UpstreamTest(Prefix + "respects a custom baseURL and headers", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Respects_a_custom_baseURL_and_headers()
    {
        var server = Server();
        var options = new TencentOptions { ApiKey = "test-key", BaseUrl = "https://custom.example.com/v1/" };
        options.Headers["x-custom"] = "value";

        await Create(options, server).LanguageModel("hy4").DoGenerateAsync(ChatPrompt(), CancellationToken.None);

        Assert.Equal("https://custom.example.com/v1/chat/completions", server.Calls[0].Uri.AbsoluteUri);
        Assert.Equal("value", server.Calls[0].Header("x-custom"));
    }
}
