// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Fireworks;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Tests.FireworksChatLanguageModelTests;

/// <summary>Port of <c>fireworks-chat-language-model.test.ts</c> &gt; <c>Fireworks chat prepareRequest</c>.</summary>
public sealed class FireworksChatPrepareRequestTests
{
    private const string UpstreamCommit = "c9c6402cbdfce3f82b36b4073236043b1946dd06";
    private const string Prefix = "packages/fireworks/src/fireworks-chat-language-model.test.ts::Fireworks chat prepareRequest::";

    [Fact]
    [UpstreamTest(Prefix + "should apply the Fireworks request body transform", Coverage = UpstreamCoverage.Covered, Commit = UpstreamCommit)]
    public void Should_apply_the_Fireworks_request_body_transform()
    {
        var capture = new UpstreamCapture();
        var options = UpstreamChat.Prompt();
        options.ProviderOptions = UpstreamChat.Bag(
            "fireworks",
            "{\"promptCacheKey\":\"cache-key\",\"serviceTier\":\"priority\",\"thinking\":{\"type\":\"enabled\",\"budgetTokens\":1024},\"reasoningHistory\":\"interleaved\"}");

        var body = Model(capture).PrepareRequest(options).Body;

        Assert.Equal("cache-key", body["prompt_cache_key"]!.GetValue<string>());
        Assert.Equal("priority", body["service_tier"]!.GetValue<string>());
        Assert.Equal("enabled", body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal(1024, body["thinking"]!["budget_tokens"]!.GetValue<int>());
        Assert.Equal("interleaved", body["reasoning_history"]!.GetValue<string>());
        Assert.False(body.ContainsKey("promptCacheKey"));
        Assert.False(body.ContainsKey("serviceTier"));
        Assert.False(body.ContainsKey("reasoningHistory"));
        Assert.Empty(capture.Requests);
    }

    private static OpenAICompatibleLanguageModel Model(UpstreamCapture capture)
    {
        return (OpenAICompatibleLanguageModel)FireworksProvider.Create(new OpenAICompatibleOptions { ApiKey = "test-key" }, capture).LanguageModel("accounts/fireworks/models/gpt-oss-120b");
    }
}
