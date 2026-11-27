// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel.GatewayLanguageModelSupport;

namespace Vercel.AI.Tests.Upstream.Gateway.GatewayLanguageModel;

/// <summary>Port of <c>gateway-language-model.test.ts</c> &gt; <c>GatewayLanguageModel &gt; Provider Options</c>.</summary>
public sealed class GatewayLanguageModelProviderOptionsTests
{
    private const string Prefix =
        "packages/gateway/src/gateway-language-model.test.ts::GatewayLanguageModel > Provider Options::";

    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    [UpstreamTest(
        Prefix + "should pass provider routing order for doStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_provider_routing_order_for_doStream()
    {
        var server = PrepareStreamResponse("Hello", " world");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"order\":[\"groq\",\"openai\"]}"),
            },
        };

        var result = await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);
        await ReadAll(result);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"order\":[\"groq\",\"openai\"]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerTimeouts for doStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_providerTimeouts_for_doStream()
    {
        var server = PrepareStreamResponse("Hello", " world");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"providerTimeouts\":{\"byok\":{\"anthropic\":3000}}}"),
            },
        };

        var result = await CreateTestModel(server).DoStreamAsync(options, CancellationToken.None);
        await ReadAll(result);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"providerTimeouts\":{\"byok\":{\"anthropic\":3000}}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass provider routing order for doGenerate",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_provider_routing_order_for_doGenerate()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"order\":[\"bedrock\",\"anthropic\"]}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"order\":[\"bedrock\",\"anthropic\"]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass single provider in order array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_single_provider_in_order_array()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"order\":[\"openai\"]}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"order\":[\"openai\"]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should validate provider options against schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_validate_provider_options_against_schema()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"order\":[\"anthropic\",\"bedrock\",\"openai\"]}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"order\":[\"anthropic\",\"bedrock\",\"openai\"]}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass providerTimeouts for doGenerate",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_providerTimeouts_for_doGenerate()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse(
                    "{\"providerTimeouts\":{\"byok\":{\"openai\":5000,\"anthropic\":2000}}}"
                ),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"providerTimeouts\":{\"byok\":{\"openai\":5000,\"anthropic\":2000}}}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass zeroDataRetention option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_zeroDataRetention_option()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"zeroDataRetention\":true}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"zeroDataRetention\":true}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass disallowPromptTraining option",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_disallowPromptTraining_option()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"disallowPromptTraining\":true}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"disallowPromptTraining\":true}}"
        );
    }

    [Fact]
    [UpstreamTest(Prefix + "should pass quotaEntityId option", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_quotaEntityId_option()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"quotaEntityId\":\"entity-123\"}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"quotaEntityId\":\"entity-123\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass quotaEntityId with other options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_quotaEntityId_with_other_options()
    {
        var server = PrepareJsonResponse("Test response");
        var options = new LanguageModelCallOptions
        {
            Prompt = TestPrompt.Prompt,
            ProviderOptions = new Dictionary<string, JsonElement>
            {
                ["gateway"] = Parse("{\"quotaEntityId\":\"entity-123\",\"user\":\"user-456\"}"),
            },
        };

        await CreateTestModel(server).DoGenerateAsync(options, CancellationToken.None);

        JsonAssert.Equal(
            server.RequestJson["providerOptions"],
            "{\"gateway\":{\"quotaEntityId\":\"entity-123\",\"user\":\"user-456\"}}"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should work without provider options",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_work_without_provider_options()
    {
        var server = PrepareJsonResponse("Test response");

        var result = await CreateTestModel(server)
            .DoGenerateAsync(TestPrompt, CancellationToken.None);

        Assert.Null(server.RequestJson["providerOptions"]);
        Assert.Equal(
            "Test response",
            Assert.IsType<GeneratedText>(Assert.Single(result.Content)).Text
        );
    }
}
