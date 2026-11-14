// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using static Vercel.AI.Tests.ConvertToOpenAIResponsesInput.ExpandedParallelToolCallPrompt;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; compact parallel tool metadata</c>.</summary>
public sealed class CompactParallelToolMetadataTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > compact parallel tool metadata::round-trips serialized metadata with %j", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public void Round_trips_serialized_metadata(bool hasConversation, bool reverseResults)
    {
        var prompt = Create(compactMetadata: true, reverseResults: reverseResults);

        var result = Convert(prompt, hasConversation: hasConversation, hasPreviousResponseId: !hasConversation);

        AssertJson(
            "[" + (hasConversation
                ? string.Empty
                : "{\"type\":\"function_call\",\"call_id\":\"call_parallel\",\"name\":\"parallel\",\"arguments\":" + JsonSerializer.Serialize(ParallelToolCallInput) + "},")
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_parallel\",\"output\":" + JsonSerializer.Serialize(ParallelToolCallOutput) + "}]",
            result.Input);
        Assert.Equal(0, result.WarningCount);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("conflicting")]
    [UpstreamTest("packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > compact parallel tool metadata::keeps child calls separate when wrapper input is %s", Coverage = UpstreamCoverage.Covered, Commit = "83d6b7da4df4bdf62f2cc17d3a8a5bb4647b1023")]
    public void Keeps_child_calls_separate_when_wrapper_input_is(string kind)
    {
        var prompt = Create(
            compactMetadata: true,
            changeMetadata: (index, metadata) =>
            {
                if (kind == "missing")
                {
                    metadata.Remove("input");
                }
                else if (index == 1)
                {
                    metadata["input"] = "{}";
                }
            });

        var result = Convert(prompt, hasPreviousResponseId: true);

        AssertJson(
            "[{\"type\":\"function_call\",\"call_id\":\"call_parallel_0\",\"name\":\"weather\",\"arguments\":\"{\\\"location\\\":\\\"San Francisco\\\"}\"},"
            + "{\"type\":\"function_call\",\"call_id\":\"call_parallel_1\",\"name\":\"cityAttractions\",\"arguments\":\"{\\\"city\\\":\\\"Rome\\\"}\"},"
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_parallel_0\",\"output\":\"{\\\"temperature\\\":72}\"},"
            + "{\"type\":\"function_call_output\",\"call_id\":\"call_parallel_1\",\"output\":\"Colosseum\"}]",
            result.Input);
    }
}
