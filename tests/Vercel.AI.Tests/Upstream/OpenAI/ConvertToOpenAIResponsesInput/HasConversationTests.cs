// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.ExpandedParallelToolCallPrompt;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; hasConversation</c>. Only the expanded parallel tool result cases are here.</summary>
public sealed class HasConversationTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should send expanded parallel tool results back to the stored wrapper call", Coverage = UpstreamCoverage.Covered)]
    public void Should_send_expanded_parallel_tool_results_back_to_the_stored_wrapper_call()
    {
        var result = Convert(Create(), hasConversation: true);

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":" + JsonSerializer.Serialize(ParallelToolCallOutput) + "}]",
            result.Input);
        Assert.Equal(0, result.WarningCount);
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasConversation::should preserve scalar prompt cache breakpoints on expanded parallel tool results", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_scalar_prompt_cache_breakpoints_on_expanded_parallel_tool_results()
    {
        var result = Convert(Create(withPromptCacheBreakpoints: true), hasConversation: true);

        AssertJson(
            "[{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":["
            + "{\"type\":\"input_text\",\"text\":\"{\\\"temperature\\\":72}\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}},"
            + "{\"type\":\"input_text\",\"text\":\"\\nColosseum\",\"prompt_cache_breakpoint\":{\"mode\":\"explicit\"}}]}]",
            result.Input);
        Assert.Equal(0, result.WarningCount);
    }
}
