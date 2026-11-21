// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Numerics;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil;
using Converter = Vercel.AI.Mistral.ConvertToMistralChatMessages;

namespace Vercel.AI.Tests.Upstream.Mistral.ConvertToMistralChatMessages;

/// <summary>Port of <c>convert-to-mistral-chat-messages.test.ts</c> &gt; <c>tool argument and output serialization resilience</c>.</summary>
public sealed class ToolArgumentAndOutputSerializationResilienceTests
{
    private const string Prefix =
        "packages/mistral/src/convert-to-mistral-chat-messages.test.ts::top-level-only media type resolution > tool argument and output serialization resilience::";

    private static JsonArray Run(params ModelMessage[] prompt) => Converter.Convert(prompt);

    private static AssistantModelMessage Assistant(string id, string name, string arguments) =>
        new(
            new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall(id, name, arguments)),
            }
        );

    private static JsonNode Expected(string arguments) =>
        new JsonObject
        {
            ["role"] = "assistant",
            ["content"] = "",
            ["prefix"] = true,
            ["tool_calls"] = new JsonArray(
                new JsonObject
                {
                    ["id"] = "call-1",
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = "search",
                        ["arguments"] = arguments,
                    },
                }
            ),
        };

    [Fact]
    [UpstreamTest(
        Prefix + "preserves string tool call arguments without double-stringifying",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Preserves_string_tool_call_arguments_without_double_stringifying()
    {
        const string input = "{\"query\":\"paris\",\"limit\":5}";
        var result = Run(Assistant("call-1", "search", input));

        Assert.True(JsonNode.DeepEquals(result[0], Expected(input)), result[0]!.ToJsonString());
    }

    [Theory]
    [InlineData("{\"query\":")]
    [InlineData("unparseable")]
    [UpstreamTest(
        Prefix + "preserves malformed argument replay: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Preserves_malformed_argument_replay(string input)
    {
        var result = Run(Assistant("call-1", "search", input));

        // JSON.stringify(input): the text wrapped in quotes with inner quotes escaped.
        var stringified = "\"" + input.Replace("\"", "\\\"") + "\"";
        Assert.True(
            JsonNode.DeepEquals(result[0], Expected(stringified)),
            result[0]!.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "safely serializes BigInt and circular structures in tool call arguments",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool call input is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Safely_serializes_BigInt_and_circular_structures_in_tool_call_arguments()
    {
        var cyclicInput = new Dictionary<string, object?>
        {
            ["id"] = BigInteger.Parse("12345678901234567890", CultureInfo.InvariantCulture),
        };
        cyclicInput["self"] = cyclicInput;

        var result = Run(Assistant("call-2", "process", StringifyWithBigIntAndCycles(cyclicInput)));

        var args = result[0]!["tool_calls"]![0]!["function"]!["arguments"]!.GetValue<string>();
        Assert.Contains("\"id\":\"12345678901234567890\"", args);
        Assert.Contains("\"self\":\"[Circular]\"", args);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "safely serializes BigInt and circular structures in tool responses",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Safely_serializes_BigInt_and_circular_structures_in_tool_responses()
    {
        var cyclicOutput = new Dictionary<string, object?>
        {
            ["count"] = BigInteger.Parse("999999999999999999", CultureInfo.InvariantCulture),
        };
        cyclicOutput["self"] = cyclicOutput;

        var result = Run(
            new ToolModelMessage(
                "call-3",
                "getData",
                StringifyWithBigIntAndCycles(cyclicOutput),
                false
            )
        );

        var content = result[0]!["content"]!.GetValue<string>();
        Assert.Contains("\"count\":\"999999999999999999\"", content);
        Assert.Contains("\"self\":\"[Circular]\"", content);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves reused non-circular objects in tool outputs",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the shared object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Preserves_reused_non_circular_objects_in_tool_outputs()
    {
        var shared = new Dictionary<string, object?> { ["city"] = "Paris" };

        var result = Run(
            new ToolModelMessage(
                "call-4",
                "getData",
                StringifyWithBigIntAndCycles(
                    new Dictionary<string, object?> { ["first"] = shared, ["second"] = shared }
                ),
                false
            )
        );

        var content = result[0]!["content"]!.GetValue<string>();
        Assert.Contains("\"first\":{\"city\":\"Paris\"}", content);
        Assert.Contains("\"second\":{\"city\":\"Paris\"}", content);
        Assert.DoesNotContain("[Circular]", content);
    }
}
