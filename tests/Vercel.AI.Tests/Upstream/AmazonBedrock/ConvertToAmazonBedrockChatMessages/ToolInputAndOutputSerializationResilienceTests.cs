// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Numerics;
using System.Text.Json.Nodes;
using Vercel.AI.AmazonBedrock;
using Vercel.AI.Provider;
using static Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil;

namespace Vercel.AI.Tests.Upstream.AmazonBedrock.ConvertToAmazonBedrockChatMessages;

/// <summary>Port of <c>convert-to-amazon-bedrock-chat-messages.test.ts</c> &gt; <c>top-level-only mediaType resolution &gt; tool input and output serialization resilience</c>.</summary>
public sealed class ToolInputAndOutputSerializationResilienceTests
{
    private const string Prefix =
        "packages/amazon-bedrock/src/convert-to-amazon-bedrock-chat-messages.test.ts::top-level-only mediaType resolution > tool input and output serialization resilience::";

    private const string ModelId = "meta.llama3-70b-instruct-v1:0";

    private static JsonArray Run(params ModelMessage[] prompt)
    {
        AmazonBedrockMessages.Convert(ModelId, prompt, out _, out var messages);
        return messages;
    }

    private static AssistantModelMessage Assistant(string id, string name, string arguments) =>
        new([new AssistantToolCallPart(new GeneratedToolCall(id, name, arguments))]);

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    [Fact]
    [UpstreamTest(
        Prefix + "parses valid JSON string tool input into structured object",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Parses_valid_JSON_string_tool_input_into_structured_object()
    {
        var messages = Run(
            Assistant("call-1", "search", "{\"query\":\"bedrock converse\",\"limit\":5}")
        );

        AssertJson(
            "{\"role\":\"assistant\",\"content\":[{\"toolUse\":{\"toolUseId\":\"call-1\",\"name\":\"search\",\"input\":{\"query\":\"bedrock converse\",\"limit\":5}}}]}",
            messages[0]
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "falls back to rawInvalidInput for malformed JSON string tool input",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Falls_back_to_rawInvalidInput_for_malformed_JSON_string_tool_input()
    {
        var messages = Run(Assistant("call-2", "search", "not-valid-json"));

        AssertJson(
            "{\"role\":\"assistant\",\"content\":[{\"toolUse\":{\"toolUseId\":\"call-2\",\"name\":\"search\",\"input\":{\"rawInvalidInput\":\"not-valid-json\"}}}]}",
            messages[0]
        );
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

        var messages = Run(
            new ToolModelMessage(
                "call-3",
                "getData",
                StringifyWithBigIntAndCycles(cyclicOutput),
                false
            )
        );

        var text = messages[0]!["content"]![0]!["toolResult"]!["content"]![0]![
            "text"
        ]!.GetValue<string>();
        Assert.Contains("\"count\":\"999999999999999999\"", text);
        Assert.Contains("\"self\":\"[Circular]\"", text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves reused non-circular objects in tool responses",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the shared object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Preserves_reused_non_circular_objects_in_tool_responses()
    {
        var shared = new Dictionary<string, object?> { ["city"] = "Paris" };

        var messages = Run(
            new ToolModelMessage(
                "call-4",
                "getData",
                StringifyWithBigIntAndCycles(
                    new Dictionary<string, object?> { ["first"] = shared, ["second"] = shared }
                ),
                false
            )
        );

        var text = messages[0]!["content"]![0]!["toolResult"]!["content"]![0]![
            "text"
        ]!.GetValue<string>();
        Assert.Contains("\"first\":{\"city\":\"Paris\"}", text);
        Assert.Contains("\"second\":{\"city\":\"Paris\"}", text);
        Assert.DoesNotContain("[Circular]", text);
    }
}
