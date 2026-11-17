// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages.ConvertToDeepSeekChatMessagesSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekChatMessages;

/// <summary>Port of <c>convert-to-deepseek-chat-messages.test.ts</c> &gt; <c>convertToDeepSeekChatMessages &gt; tool argument and output serialization resilience</c>.</summary>
public sealed class ToolArgumentAndOutputSerializationResilienceTests
{
    private const string Prefix =
        TestFile + " > tool argument and output serialization resilience::";

    [Fact]
    [UpstreamTest(
        Prefix + "preserves string tool call arguments without double-stringifying",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Preserves_string_tool_call_arguments_without_double_stringifying()
    {
        var result = Run(
            "deepseek-chat",
            [Assistant(Call("call-1", "search", """{"query":"deepseek reasoner","limit":3}"""))]
        );

        JsonAssert.Equal(
            result.Messages[0],
            """
            {"role":"assistant","content":"",
             "tool_calls":[{"id":"call-1","type":"function",
               "function":{"name":"search","arguments":"{\"query\":\"deepseek reasoner\",\"limit\":3}"}}]}
            """
        );
    }

    [Theory]
    [InlineData("{\"query\":")]
    [InlineData("unparseable")]
    [UpstreamTest(
        Prefix + "preserves malformed argument replay: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f"
    )]
    public void Preserves_malformed_argument_replay(string input)
    {
        var result = Run("deepseek-chat", [Assistant(Call("call-1", "search", input))]);

        // JSON.stringify(input): the text wrapped in quotes with inner quotes escaped.
        var stringified = "\"" + input.Replace("\"", "\\\"") + "\"";
        var expected = new JsonObject
        {
            ["role"] = "assistant",
            ["content"] = "",
            ["tool_calls"] = new JsonArray(
                new JsonObject
                {
                    ["id"] = "call-1",
                    ["type"] = "function",
                    ["function"] = new JsonObject
                    {
                        ["name"] = "search",
                        ["arguments"] = stringified,
                    },
                }
            ),
        };
        Assert.True(
            JsonNode.DeepEquals(result.Messages[0], expected),
            result.Messages[0]!.ToJsonString() + " != " + expected.ToJsonString()
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves reused non-circular objects in tool outputs",
        Coverage = UpstreamCoverage.Partial,
        Commit = "3bea041f28e65f371e640d2c70c302776a46387f",
        Note = "The port's tool output is JSON text, so a shared object reference cannot be expressed. The text repeats the object."
    )]
    public void Preserves_reused_non_circular_objects_in_tool_outputs()
    {
        var result = Run(
            "deepseek-chat",
            [
                ToolJson(
                    "call-4",
                    "getData",
                    """{"first":{"city":"Paris"},"second":{"city":"Paris"}}"""
                ),
            ]
        );

        var content = result.Messages[0]!["content"]!.GetValue<string>();
        Assert.Contains("\"first\":{\"city\":\"Paris\"}", content);
        Assert.Contains("\"second\":{\"city\":\"Paris\"}", content);
        Assert.DoesNotContain("[Circular]", content);
    }
}
