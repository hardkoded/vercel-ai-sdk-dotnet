// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; tool messages &gt; custom tool: %s</c>.</summary>
public sealed class ToolMessagesCustomToolTests
{
    [Theory]
    [InlineData("error-text", "E42", "{\"error\":\"E42\"}")]
    [InlineData("error-json", "{\"code\":\"E42\"}", "{\"error\":{\"code\":\"E42\"}}")]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > tool messages > custom tool: %s::should wrap tool errors $output",
        Coverage = UpstreamCoverage.Partial,
        Note = "Runs the custom tool: false case only. The port has no custom_tool_call_output, no output-schema tool set, and no prompt-cache breakpoint on tool results.")]
    public void Should_wrap_tool_errors(string outputType, string value, string expected)
    {
        var prepared = OpenAIResponsesLanguageModel.Prepare("gpt-4o", new LanguageModelCallOptions
        {
            Prompt = new ModelMessage[] { new ToolModelMessage("call_error", "deploy", value, isError: true, outputType: outputType) },
        }, false);

        var item = Assert.Single(prepared.Body["input"]!.AsArray())!;
        Assert.Equal("function_call_output", item["type"]!.GetValue<string>());
        Assert.Equal("call_error", item["call_id"]!.GetValue<string>());
        Assert.Equal(expected, item["output"]!.GetValue<string>());
        Assert.Empty(prepared.Warnings);
    }
}
