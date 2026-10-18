// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of the doStream cases of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; Provider Options</c>.</summary>
public sealed class GatewayLanguageModelProviderOptionsTests
{
    private const string Prefix = "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::";

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    [UpstreamTest(Prefix + "should pass provider routing order for doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_provider_routing_order_for_doStream()
    {
        var server = PrepareStreamResponse("Hello", " world");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement> { ["gateway"] = Parse("{\"order\":[\"groq\",\"openai\"]}") },
        };

        var result = await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);
        await ReadAll(result);

        JsonAssert.Equal(server.RequestJson["providerOptions"], "{\"gateway\":{\"order\":[\"groq\",\"openai\"]}}");
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass providerTimeouts for doStream", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_providerTimeouts_for_doStream()
    {
        var server = PrepareStreamResponse("Hello", " world");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement> { ["gateway"] = Parse("{\"providerTimeouts\":{\"byok\":{\"anthropic\":3000}}}") },
        };

        var result = await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);
        await ReadAll(result);

        JsonAssert.Equal(server.RequestJson["providerOptions"], "{\"gateway\":{\"providerTimeouts\":{\"byok\":{\"anthropic\":3000}}}}");
    }
}
