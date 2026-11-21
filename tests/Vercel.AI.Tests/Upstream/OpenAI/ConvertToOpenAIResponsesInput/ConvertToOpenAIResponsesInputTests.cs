// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Numerics;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using static Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of the <c>convert-to-openai-responses-input.test.ts</c> tests that sit outside any <c>describe</c>.</summary>
public sealed class ConvertToOpenAIResponsesInputTests
{
    private const string Prefix =
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::";

    [Fact]
    [UpstreamTest(
        Prefix + "preserves shared tool argument objects while replacing real cycles",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool call input is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Preserves_shared_tool_argument_objects_while_replacing_real_cycles()
    {
        var shared = new Dictionary<string, object?> { ["value"] = 42 };
        var input = new Dictionary<string, object?> { ["a"] = shared, ["b"] = shared };
        input["self"] = input;

        var result = OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[]
            {
                new AssistantModelMessage(
                    null,
                    new[]
                    {
                        new GeneratedToolCall(
                            "call_shared",
                            "inspect",
                            StringifyWithBigIntAndCycles(input)
                        ),
                    },
                    null
                ),
            },
            "system",
            false,
            null,
            "openai",
            false,
            false
        );

        Assert.Equal(
            "{\"a\":{\"value\":42},\"b\":{\"value\":42},\"self\":\"[Circular]\"}",
            result.Input[0]!["arguments"]!.GetValue<string>()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "serializes tool results with BigInt and real cycles",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Serializes_tool_results_with_BigInt_and_real_cycles()
    {
        var value = new Dictionary<string, object?> { ["count"] = new BigInteger(42) };
        value["self"] = value;

        var result = OpenAIResponsesLanguageModel.ConvertInput(
            new ModelMessage[]
            {
                new ToolModelMessage(
                    "call1",
                    "inspect",
                    StringifyWithBigIntAndCycles(value),
                    false
                ),
            },
            "system",
            false,
            null,
            "openai",
            false,
            false
        );

        Assert.Equal(
            "{\"count\":\"42\",\"self\":\"[Circular]\"}",
            result.Input[0]!["output"]!.GetValue<string>()
        );
    }
}
