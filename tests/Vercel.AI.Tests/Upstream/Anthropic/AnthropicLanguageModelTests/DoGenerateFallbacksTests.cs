// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; doGenerate &gt; fallbacks</c>.</summary>
public sealed class DoGenerateFallbacksTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > doGenerate > fallbacks::";

    [Fact]
    [UpstreamTest(
        Prefix + "should pass fallbacks to the request body and add the beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_fallbacks_to_the_request_body_and_add_the_beta_header()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = AnthropicParity.WithProvider(
            "{\"fallbacks\":[{\"model\":\"claude-opus-4-8\",\"max_tokens\":8192,\"thinking\":{\"type\":\"disabled\"},\"speed\":\"fast\"}]}"
        );
        options.MaxOutputTokens = 1024;

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-fable-5", options);

        LanguageModelTestSupport.AssertJson(
            """
            {
              "fallbacks": [
                {
                  "max_tokens": 8192,
                  "model": "claude-opus-4-8",
                  "speed": "fast",
                  "thinking": { "type": "disabled" }
                }
              ],
              "max_tokens": 1024,
              "messages": [
                { "content": [{ "text": "Hello", "type": "text" }], "role": "user" }
              ],
              "model": "claude-fable-5"
            }
            """,
            LanguageModelTestSupport.RequestBody(handler)
        );

        Assert.Equal("server-side-fallback-2026-06-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should pass fallbacks \"default\" through and add the 2026-07-01 beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_fallbacks_default_through_and_add_the_2026_07_01_beta_header()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5",
            AnthropicParity.WithProvider("{\"fallbacks\":\"default\"}")
        );

        var body = LanguageModelTestSupport.RequestBody(handler);
        Assert.Equal("default", body["fallbacks"]!.GetValue<string>());
        Assert.Equal("server-side-fallback-2026-07-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not add the beta header when fallbacks is an empty array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_add_the_beta_header_when_fallbacks_is_an_empty_array()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");

        await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5",
            AnthropicParity.WithProvider("{\"fallbacks\":[]}")
        );

        var body = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(body["fallbacks"]);
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should add the fallback beta when replaying a fallback block without a fallback chain",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_add_the_fallback_beta_when_replaying_a_fallback_block_without_a_fallback_chain()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("question"),
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantCustomPart(
                            "anthropic.fallback",
                            LanguageModelTestSupport.Anthropic(
                                "{\"type\":\"fallback\",\"from\":{\"model\":\"claude-opus-5-5\"},\"to\":{\"model\":\"claude-opus-4-8\"}}"
                            )
                        ),
                        new AssistantTextPart("the answer"),
                    }
                ),
                new UserModelMessage("follow-up"),
            },
            MaxOutputTokens = 16,
        };

        await AnthropicParity.Generate(AnthropicParity.Client(handler), "claude-opus-4-8", options);

        var body = LanguageModelTestSupport.RequestBody(handler);
        Assert.Null(body["fallbacks"]);
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"fallback\",\"from\":{\"model\":\"claude-opus-5-5\"},\"to\":{\"model\":\"claude-opus-4-8\"}}",
            body["messages"]![1]!["content"]![0]
        );
        Assert.Equal("server-side-fallback-2026-06-01", handler.Headers["anthropic-beta"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should preserve the fallback content block and surface the fallback iteration",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_the_fallback_content_block_and_surface_the_fallback_iteration()
    {
        var handler = LanguageModelTestSupport.JsonFixture("anthropic-fallback");

        var result = await AnthropicParity.Generate(
            AnthropicParity.Client(handler),
            "claude-fable-5"
        );

        LanguageModelTestSupport.AssertJson(
            """
            [
              {
                "type": "custom",
                "kind": "anthropic.fallback",
                "providerMetadata": {
                  "anthropic": {
                    "from": { "model": "claude-fable-5" },
                    "to": { "model": "claude-opus-4-8" },
                    "type": "fallback"
                  }
                }
              },
              {
                "type": "text",
                "text": "The printing press was invented by Johannes Gutenberg around 1440."
              }
            ]
            """,
            LanguageModelTestSupport.Snapshot(result.Content)
        );

        // The fallback hop is preserved in the Anthropic-specific iterations.
        JsonAssert.Equal(
            result.ProviderMetadata!.Value.GetProperty("anthropic").GetProperty("iterations"),
            """
            [
              {
                "inputTokens": 408,
                "model": "claude-fable-5",
                "outputTokens": 0,
                "type": "message"
              },
              {
                "inputTokens": 412,
                "model": "claude-opus-4-8",
                "outputTokens": 264,
                "type": "fallback_message"
              }
            ]
            """
        );

        // Top-level usage reflects the served (fallback) answer, not the
        // blocked primary attempt (which had 0 output tokens).
        Assert.Equal(412, result.Usage.InputTokens);
        Assert.Equal(264, result.Usage.OutputTokens);
    }
}
