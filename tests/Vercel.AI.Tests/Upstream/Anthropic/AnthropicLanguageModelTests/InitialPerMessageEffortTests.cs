// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; initial per-message effort</c>.</summary>
public sealed class InitialPerMessageEffortTests
{
    [Theory]
    [InlineData("generate", null)]
    [InlineData("generate", "high")]
    [InlineData("stream", null)]
    [InlineData("stream", "high")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > initial per-message effort::should send initial effort via $method with request effort $requestEffort",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_initial_effort_via_method_with_request_effort_requestEffort(
        string method,
        string? requestEffort
    )
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new SystemModelMessage("initial instructions"),
                new SystemModelMessage(
                    string.Empty,
                    LanguageModelTestSupport.Anthropic("{\"effort\":\"low\"}")
                ),
                new SystemModelMessage(
                    string.Empty,
                    LanguageModelTestSupport.Anthropic("{\"effort\":\"medium\"}")
                ),
                new UserModelMessage("Hello"),
            },
        };
        if (requestEffort != null)
        {
            options.ProviderOptions = LanguageModelTestSupport.Anthropic(
                "{\"effort\":\"" + requestEffort + "\"}"
            );
        }

        CaptureHandler handler;
        if (method == "generate")
        {
            handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
            var result = await AnthropicParity.Generate(
                AnthropicParity.Client(handler),
                "claude-fable-5-1",
                options
            );
            Assert.Empty(result.Warnings);
        }
        else
        {
            handler = LanguageModelTestSupport.ChunksFixture(
                "anthropic-message-delta-input-tokens"
            );
            var chunks = await LanguageModelTestSupport.Stream(
                AnthropicParity.Client(handler),
                "claude-fable-5-1",
                options
            );
            Assert.Empty(Assert.IsType<StreamStartStreamPart>(chunks[0]).Warnings);
            Assert.DoesNotContain(chunks, chunk => chunk is ErrorStreamPart);
        }

        var body = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            "[{\"type\":\"text\",\"text\":\"initial instructions\"}]",
            body["system"]
        );
        LanguageModelTestSupport.AssertJson(
            "[{\"role\":\"system\",\"content\":[],\"output_config\":{\"effort\":\"low\"}},{\"role\":\"system\",\"content\":[],\"output_config\":{\"effort\":\"medium\"}},{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"Hello\"}]}]",
            body["messages"]
        );
        Assert.Equal(requestEffort, body["output_config"]?["effort"]?.GetValue<string>());
        Assert.Contains(
            "mid-conversation-output-config-2026-07-01",
            handler.Headers["anthropic-beta"]
        );
    }
}
