// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; custom provider name support &gt; doStream</c>.</summary>
public sealed class CustomProviderNameSupportDoStreamTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > custom provider name support > doStream::";

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

    private static SseStreamHandler PrepareCustomStreamResponse()
    {
        return DoStreamSupport.Stream(
            """{"type":"message_start","message":{"id":"msg_custom_stream","type":"message","role":"assistant","content":[],"model":"claude-3-haiku-20240307","stop_reason":null,"stop_sequence":null,"usage":{"input_tokens":10,"output_tokens":0}}}""",
            """{"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}""",
            """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}""",
            """{"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":" World!"}}""",
            """{"type":"content_block_stop","index":0}""",
            """{"type":"message_delta","delta":{"stop_reason":"end_turn","stop_sequence":null},"usage":{"output_tokens":20}}""",
            """{"type":"message_stop"}""",
            "[DONE]"
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
        var customProvider = CreateCustomProvider(PrepareCustomStreamResponse());

        var chunks = await DoStreamSupport.Collect(
            customProvider.LanguageModel("claude-3-haiku-20240307"),
            Options("{\"anthropic\":{\"sendReasoning\":true}}")
        );
        var finishChunk = chunks.OfType<FinishStreamPart>().First();

        // Only 'anthropic' key should be in providerMetadata
        Assert.True(finishChunk.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.Equal(
            new[] { "anthropic" },
            finishChunk.ProviderMetadata!.Value.EnumerateObject().Select(property => property.Name)
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
        var customProvider = CreateCustomProvider(PrepareCustomStreamResponse());

        var chunks = await DoStreamSupport.Collect(
            customProvider.LanguageModel("claude-3-haiku-20240307"),
            Options("{\"my-custom-anthropic\":{\"sendReasoning\":true}}")
        );
        var finishChunk = chunks.OfType<FinishStreamPart>().First();

        // Both 'anthropic' and custom key should be in providerMetadata
        Assert.True(finishChunk.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.True(
            finishChunk.ProviderMetadata!.Value.TryGetProperty("my-custom-anthropic", out _)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should only include \"anthropic\" key in providerMetadata when no providerOptions used",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_only_include_anthropic_key_in_providerMetadata_when_no_providerOptions_used()
    {
        var customProvider = CreateCustomProvider(PrepareCustomStreamResponse());

        var chunks = await DoStreamSupport.Collect(
            customProvider.LanguageModel("claude-3-haiku-20240307"),
            Options(null)
        );
        var finishChunk = chunks.OfType<FinishStreamPart>().First();

        // Only 'anthropic' key should be in providerMetadata (default)
        Assert.True(finishChunk.ProviderMetadata!.Value.TryGetProperty("anthropic", out _));
        Assert.Equal(
            new[] { "anthropic" },
            finishChunk.ProviderMetadata!.Value.EnumerateObject().Select(property => property.Name)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept providerOptions with custom provider name key",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_accept_providerOptions_with_custom_provider_name_key()
    {
        var handler = PrepareCustomStreamResponse();
        var customProvider = CreateCustomProvider(handler);

        await customProvider
            .LanguageModel("claude-3-haiku-20240307")
            .DoStreamAsync(
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
}
