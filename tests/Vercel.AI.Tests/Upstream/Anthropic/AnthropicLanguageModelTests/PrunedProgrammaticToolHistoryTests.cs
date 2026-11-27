// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>AnthropicLanguageModel &gt; pruned programmatic tool history</c>.</summary>
public sealed class PrunedProgrammaticToolHistoryTests
{
    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(
        "packages/anthropic/src/anthropic-language-model.test.ts::AnthropicLanguageModel > pruned programmatic tool history::should omit orphaned callers and expose the warning for %s",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_omit_orphaned_callers_and_expose_the_warning_for_method(string method)
    {
        var options = new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[]
            {
                new AssistantModelMessage(
                    new AssistantContentPart[]
                    {
                        new AssistantToolCallPart(
                            new GeneratedToolCall("lookup-call", "lookup", "{\"ticker\":\"AAPL\"}"),
                            LanguageModelTestSupport.Anthropic(
                                "{\"caller\":{\"type\":\"code_execution_20250825\",\"toolId\":\"pruned-source\"}}"
                            )
                        ),
                    }
                ),
                new ToolModelMessage("lookup-call", "lookup", "185.42", false, "text"),
                new UserModelMessage("Hello"),
            },
        };
        const string Message =
            "Omitted caller metadata for tool lookup-call because source code execution tool pruned-source is missing from the conversation history.";

        CaptureHandler handler;
        if (method == "generate")
        {
            handler = LanguageModelTestSupport.JsonFixture("anthropic-text");
            var result = await AnthropicParity.Generate(
                AnthropicParity.Client(handler),
                "claude-3-haiku-20240307",
                options
            );
            var warning = Assert.Single(result.Warnings);
            Assert.Equal("other", warning.Type);
            Assert.Equal(Message, warning.Message);
        }
        else
        {
            handler = LanguageModelTestSupport.ChunksFixture("anthropic-text");
            var chunks = await LanguageModelTestSupport.Stream(
                AnthropicParity.Client(handler),
                "claude-3-haiku-20240307",
                options
            );
            var warning = Assert.Single(Assert.IsType<StreamStartStreamPart>(chunks[0]).Warnings);
            Assert.Equal("other", warning.Type);
            Assert.Equal(Message, warning.Message);
            Assert.DoesNotContain(chunks, chunk => chunk is ErrorStreamPart);
        }

        var body = LanguageModelTestSupport.RequestBody(handler);
        LanguageModelTestSupport.AssertJson(
            "[{\"type\":\"tool_use\",\"id\":\"lookup-call\",\"name\":\"lookup\",\"input\":{\"ticker\":\"AAPL\"}}]",
            body["messages"]![0]!["content"]
        );
        LanguageModelTestSupport.AssertJson(
            "{\"type\":\"tool_result\",\"tool_use_id\":\"lookup-call\",\"content\":\"185.42\"}",
            body["messages"]![1]!["content"]![0]
        );
    }
}
