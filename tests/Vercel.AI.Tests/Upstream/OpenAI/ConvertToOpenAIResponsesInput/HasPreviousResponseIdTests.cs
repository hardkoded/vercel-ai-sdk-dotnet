// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.ExpandedParallelToolCallPrompt;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; hasPreviousResponseId</c>. Only the expanded parallel tool call case is here.</summary>
public sealed class HasPreviousResponseIdTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > hasPreviousResponseId::should reconstruct expanded parallel tool calls with one wrapper output", Coverage = UpstreamCoverage.Covered)]
    public void Should_reconstruct_expanded_parallel_tool_calls_with_one_wrapper_output()
    {
        var result = Convert(Create(), hasPreviousResponseId: true);

        AssertJson(
            "[{\"type\":\"function_call\",\"call_id\":\"call_parallel\",\"name\":\"parallel\",\"arguments\":" + JsonSerializer.Serialize(ParallelToolCallInput) + "},"
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":" + JsonSerializer.Serialize(ParallelToolCallOutput) + "}]",
            result.Input);
        Assert.Equal(0, result.WarningCount);
    }
}
