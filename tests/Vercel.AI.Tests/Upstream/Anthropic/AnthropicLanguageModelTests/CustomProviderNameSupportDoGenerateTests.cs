// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; custom provider name support &gt; doGenerate</c>.</summary>
public sealed class CustomProviderNameSupportDoGenerateTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > custom provider name support > doGenerate::";

    private static AnthropicProvider CreateCustomProvider(HttpMessageHandler handler)
    {
        var counter = 0;
        return AnthropicProvider.Create(
            new AnthropicOptions
            {
                ApiKey = "test-api-key",
                Name = "my-custom-anthropic",
                GenerateId = () => "id-" + counter++,
            },
            handler
        );
    }

    private static LanguageModelCallOptions Options(string? providerOptions, bool withTools = false)
    {
        var options = AnthropicParity.Hello();
        if (providerOptions != null)
        {
            var parsed = new Dictionary<string, JsonElement>();
            using var document = JsonDocument.Parse(providerOptions);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                parsed[property.Name] = property.Value.Clone();
            }

            options.ProviderOptions = parsed;
        }

        if (withTools)
        {
            options.Tools = new[]
            {
                new LanguageModelTool(
                    "testTool",
                    null,
                    AnthropicParity.Json("""{"type":"object","properties":{}}""")
                ),
            };
        }

        return options;
    }

    private static CaptureHandler PrepareCustomJsonResponse()
    {
        return AnthropicParity.Ok(
            """{"id":"msg_custom_test","type":"message","role":"assistant","content":[{"type":"text","text":"Hello, World!"}],"model":"claude-3-haiku-20240307","stop_reason":"end_turn","stop_sequence":null,"usage":{"input_tokens":10,"output_tokens":20}}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should only include \"anthropic\" key in providerMetadata when providerOptions uses \"anthropic\" key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_anthropic_key_in_providerMetadata_when_providerOptions_uses_anthropic_key()
    {
        var handler = PrepareCustomJsonResponse();
        var customProvider = CreateCustomProvider(handler);

        var result = await customProvider
            .LanguageModel("claude-3-haiku-20240307")
            .DoGenerateAsync(
                Options("{\"anthropic\":{\"sendReasoning\":true}}"),
                CancellationToken.None
            );

        // Only 'anthropic' key should be in providerMetadata
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.Equal(
            new[] { "anthropic" },
            result.ProviderMetadata!.Value.EnumerateObject().Select(property => property.Name)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should include both \"anthropic\" and custom key in providerMetadata when providerOptions uses custom key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_both_anthropic_and_custom_key_in_providerMetadata_when_providerOptions_uses_custom_key()
    {
        var handler = PrepareCustomJsonResponse();
        var customProvider = CreateCustomProvider(handler);

        var result = await customProvider
            .LanguageModel("claude-3-haiku-20240307")
            .DoGenerateAsync(
                Options("{\"my-custom-anthropic\":{\"sendReasoning\":true}}"),
                CancellationToken.None
            );

        // Both 'anthropic' and custom key should be in providerMetadata
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("my-custom-anthropic", out _));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should only include \"anthropic\" key in providerMetadata when no providerOptions used",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_anthropic_key_in_providerMetadata_when_no_providerOptions_used()
    {
        var handler = PrepareCustomJsonResponse();
        var customProvider = CreateCustomProvider(handler);

        var result = await customProvider
            .LanguageModel("claude-3-haiku-20240307")
            .DoGenerateAsync(Options(null), CancellationToken.None);

        // Only 'anthropic' key should be in providerMetadata (default)
        Assert.True(result.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.Equal(
            new[] { "anthropic" },
            result.ProviderMetadata!.Value.EnumerateObject().Select(property => property.Name)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept providerOptions with custom provider name key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_providerOptions_with_custom_provider_name_key()
    {
        var handler = PrepareCustomJsonResponse();
        var customProvider = CreateCustomProvider(handler);

        await customProvider
            .LanguageModel("claude-3-haiku-20240307")
            .DoGenerateAsync(
                Options(
                    "{\"my-custom-anthropic\":{\"disableParallelToolUse\":true}}",
                    withTools: true
                ),
                CancellationToken.None
            );

        JsonAssert.Equal(
            JsonNode.Parse(handler.Body)!["tool_choice"],
            """{"type":"auto","disable_parallel_tool_use":true}"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should accept providerOptions with canonical \"anthropic\" key for backward compatibility",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_providerOptions_with_canonical_anthropic_key_for_backward_compatibility()
    {
        var handler = PrepareCustomJsonResponse();
        var customProvider = CreateCustomProvider(handler);

        await customProvider
            .LanguageModel("claude-3-haiku-20240307")
            .DoGenerateAsync(
                Options("{\"anthropic\":{\"disableParallelToolUse\":true}}", withTools: true),
                CancellationToken.None
            );

        JsonAssert.Equal(
            JsonNode.Parse(handler.Body)!["tool_choice"],
            """{"type":"auto","disable_parallel_tool_use":true}"""
        );
    }
}
