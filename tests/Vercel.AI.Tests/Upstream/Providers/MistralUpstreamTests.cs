// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Mistral;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
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
        Assert.Contains("ai-sdk/mistral/" + AiSdkVersion.Version, capture.Requests[0].Headers["User-Agent"]);
    }
}
