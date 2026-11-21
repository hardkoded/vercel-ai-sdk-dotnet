// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using static Vercel.AI.ProviderUtils.StringifyWithBigIntAndCyclesUtil;

namespace Vercel.AI.Tests.ConvertToAnthropicPrompt;

/// <summary>Port of <c>convert-to-anthropic-prompt.test.ts</c> &gt; <c>cache control &gt; assistant message</c>.</summary>
public sealed class CacheControlAssistantMessageTests
{
    private const string Prefix =
        "packages/anthropic/src/convert-to-anthropic-prompt.test.ts::cache control > assistant message::";

    private const string UserJson =
        "{\"role\":\"user\",\"content\":[{\"type\":\"text\",\"text\":\"user-content\"}]}";

    private static IReadOnlyDictionary<string, JsonElement> Ephemeral()
    {
        using var document = JsonDocument.Parse("{\"cacheControl\":{\"type\":\"ephemeral\"}}");
        return new Dictionary<string, JsonElement> { ["anthropic"] = document.RootElement.Clone() };
    }

    private static void AssertMessages(string expected, AnthropicPromptResult result) =>
        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(expected), result.Messages),
            result.Messages.ToJsonString()
        );

    private static AnthropicPromptResult Run(params ModelMessage[] prompt) =>
        AnthropicPrompt.Convert(prompt);

    private static AssistantModelMessage ToolCall(string id, string name, string arguments) =>
        new([new AssistantToolCallPart(new GeneratedToolCall(id, name, arguments))]);

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on assistant message text part with part cache control",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_set_cache_control_on_assistant_message_text_part_with_part_cache_control()
    {
        var result = Run(
            new UserModelMessage("user-content"),
            new AssistantModelMessage([new AssistantTextPart("test", Ephemeral())])
        );

        AssertMessages(
            "["
                + UserJson
                + ",{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"test\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set cache_control on assistant tool call part with part cache control",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_set_cache_control_on_assistant_tool_call_part_with_part_cache_control()
    {
        var result = Run(
            new UserModelMessage("user-content"),
            new AssistantModelMessage([
                new AssistantToolCallPart(
                    new GeneratedToolCall("test-id", "test-tool", "{\"some\":\"arg\"}"),
                    Ephemeral()
                ),
            ])
        );

        AssertMessages(
            "["
                + UserJson
                + ",{\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"name\":\"test-tool\",\"id\":\"test-id\",\"input\":{\"some\":\"arg\"},\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should wrap non-object (invalid) tool call input in an object",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_wrap_non_object_invalid_tool_call_input_in_an_object()
    {
        var result = Run(ToolCall("call-1", "cityAttractions", "{ \"city\": \"San Francisco\", }"));

        AssertMessages(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"name\":\"cityAttractions\",\"id\":\"call-1\",\"input\":{\"rawInvalidInput\":\"{ \\\"city\\\": \\\"San Francisco\\\", }\"}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should parse valid JSON string tool call input into an object",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_parse_valid_JSON_string_tool_call_input_into_an_object()
    {
        var result = Run(ToolCall("call-1", "cityAttractions", "{\"city\":\"Paris\",\"count\":5}"));

        AssertMessages(
            "[{\"role\":\"assistant\",\"content\":[{\"type\":\"tool_use\",\"name\":\"cityAttractions\",\"id\":\"call-1\",\"input\":{\"city\":\"Paris\",\"count\":5}}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should safely serialize tool results with BigInt and circular structures",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Should_safely_serialize_tool_results_with_BigInt_and_circular_structures()
    {
        var circularValue = new Dictionary<string, object?>
        {
            ["id"] = BigInteger.Parse("9007199254740993", CultureInfo.InvariantCulture),
        };
        circularValue["self"] = circularValue;

        var result = Run(
            new ToolModelMessage(
                "call-1",
                "cityAttractions",
                StringifyWithBigIntAndCycles(circularValue),
                false
            )
        );

        AssertMessages(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"call-1\",\"content\":\"{\\\"id\\\":\\\"9007199254740993\\\",\\\"self\\\":\\\"[Circular]\\\"}\"}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should serialize tool results that reuse non-circular objects",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66",
        Note = "The port's tool output is JSON text, so the shared object graph goes through StringifyWithBigIntAndCycles before the converter."
    )]
    public void Should_serialize_tool_results_that_reuse_non_circular_objects()
    {
        var shared = new Dictionary<string, object?> { ["city"] = "Paris" };

        var result = Run(
            new ToolModelMessage(
                "call-1",
                "cityAttractions",
                StringifyWithBigIntAndCycles(
                    new Dictionary<string, object?> { ["first"] = shared, ["second"] = shared }
                ),
                false
            )
        );

        AssertMessages(
            "[{\"role\":\"user\",\"content\":[{\"type\":\"tool_result\",\"tool_use_id\":\"call-1\",\"content\":\"{\\\"first\\\":{\\\"city\\\":\\\"Paris\\\"},\\\"second\\\":{\\\"city\\\":\\\"Paris\\\"}}\"}]}]",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should set cache_control on last assistant message part with message cache control",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4ae6986cd8dcebdd31a41317c1cdfafd426c1e66"
    )]
    public void Should_set_cache_control_on_last_assistant_message_part_with_message_cache_control()
    {
        var result = Run(
            new UserModelMessage("user-content"),
            new AssistantModelMessage([
                new AssistantTextPart("part1"),
                new AssistantTextPart("part2"),
            ])
            {
                ProviderOptions = Ephemeral(),
            }
        );

        AssertMessages(
            "["
                + UserJson
                + ",{\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"part1\"},{\"type\":\"text\",\"text\":\"part2\",\"cache_control\":{\"type\":\"ephemeral\"}}]}]",
            result
        );
    }
}
