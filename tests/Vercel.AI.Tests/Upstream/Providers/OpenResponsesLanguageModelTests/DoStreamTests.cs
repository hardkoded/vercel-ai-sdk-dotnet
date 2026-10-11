// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenResponses;

namespace Vercel.AI.Tests.OpenResponsesLanguageModelTests;

/// <summary>Port of <c>open-responses-language-model.test.ts</c> &gt; <c>OpenResponsesLanguageModel &gt; doStream</c>.</summary>
public sealed class DoStreamTests
{
    private const string Prefix = "packages/open-responses/src/responses/open-responses-language-model.test.ts::OpenResponsesLanguageModel > doStream::";

    [Fact]
    [UpstreamTest(Prefix + "should send provider-native reasoning effort when streaming", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_send_provider_native_reasoning_effort_when_streaming()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag("open-responses", "{\"reasoningEffort\":\"max\"}");

        await UpstreamChat.Read(Model(capture).DoStreamAsync(options, CancellationToken.None));

        Assert.Equal("{\"effort\":\"max\"}", UpstreamChat.Body(capture)["reasoning"]!.ToJsonString());
    }

    private static OpenResponsesLanguageModel Model(UpstreamCapture capture)
    {
        return (OpenResponsesLanguageModel)OpenResponsesProvider.Create(new OpenResponsesOptions { ApiKey = "test-key", BaseUrl = "https://example.test/v1" }, capture).LanguageModel("gemma-7b-it");
    }
}
