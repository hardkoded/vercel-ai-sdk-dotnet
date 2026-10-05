// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class MistralUpstreamTests
{
    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-chat-language-model.test.ts::doGenerate::should pass headers", Coverage = UpstreamCoverage.Covered)]
    public async Task Generate_passes_headers_and_the_mistral_user_agent()
    {
        var capture = new UpstreamCapture();
        var provider = MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-api-key" }, capture);
        provider.Options.Headers["Custom-Provider-Header"] = "provider-header-value";
        var options = UpstreamChat.Prompt();
        options.Headers = new Dictionary<string, string?> { ["Custom-Request-Header"] = "request-header-value" };
        await ((OpenAICompatibleLanguageModel)provider.LanguageModel("mistral-small-latest")).DoGenerateAsync(options, CancellationToken.None);
        Assert.Equal("https://api.mistral.ai/v1/chat/completions", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("Bearer test-api-key", capture.Requests[0].Headers["Authorization"]);
        Assert.Equal("provider-header-value", capture.Requests[0].Headers["Custom-Provider-Header"]);
        Assert.Equal("request-header-value", capture.Requests[0].Headers["Custom-Request-Header"]);
        Assert.Contains("ai-sdk/mistral/0.0.0", capture.Requests[0].Headers["User-Agent"]);
    }

    [Fact]
    [UpstreamTest("packages/mistral/src/mistral-embedding-model.test.ts::doEmbed::should extract embedding", Coverage = UpstreamCoverage.Covered)]
    public async Task Embeddings_use_the_mistral_embeddings_route()
    {
        var capture = new UpstreamCapture { ResponseBody = "{\"data\":[{\"embedding\":[0.1,0.2]}],\"usage\":{\"prompt_tokens\":4}}" };
        var model = (OpenAICompatibleEmbeddingModel)MistralProvider.Create(new OpenAICompatibleOptions { ApiKey = "secret" }, capture).EmbeddingModel("mistral-embed");
        var result = await model.DoEmbedAsync(new[] { "hello" }, null, CancellationToken.None);
        Assert.Equal(0.1f, result.Embeddings[0][0]);
        Assert.Equal(4, result.Tokens);
        Assert.Equal("https://api.mistral.ai/v1/embeddings", capture.Requests[0].Uri!.GetLeftPart(UriPartial.Path));
        Assert.Equal("mistral-embed", model.ModelId);
    }
}
