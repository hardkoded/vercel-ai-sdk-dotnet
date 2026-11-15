// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput.ConvertToDeepSeekResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.DeepSeek.ConvertToDeepSeekResponsesInput;

/// <summary>Port of <c>convert-to-deepseek-responses-input.test.ts</c> &gt; <c>convertToDeepSeekResponsesInput &gt; assistant messages</c>.</summary>
public sealed class AssistantMessagesTests
{
    private const string Prefix = TestFile + " > assistant messages::";

    private static AssistantModelMessage Assistant(params AssistantContentPart[] parts)
    {
        return new AssistantModelMessage(parts);
    }

    private static AssistantToolCallPart Call(string id, string name, string arguments, bool providerExecuted = false, Dictionary<string, JsonElement>? options = null)
    {
        return new AssistantToolCallPart(new GeneratedToolCall(id, name, arguments) { ProviderExecuted = providerExecuted }, options);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve multiple text parts and omit empty text", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_multiple_text_parts_and_omit_empty_text()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(new AssistantTextPart("Hello"), new AssistantTextPart(""), new AssistantTextPart("World")));

        JsonAssert.Equal(input, """[{"content":"Hello","role":"assistant"},{"content":"World","role":"assistant"}]""");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should convert reasoning without provider metadata", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_convert_reasoning_without_provider_metadata()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(new AssistantReasoningPart("Let me think.")));

        JsonAssert.Equal(input, """[{"content":[{"text":"Let me think.","type":"reasoning_text"}],"type":"reasoning"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("deepseek")]
    [InlineData("custom")]
    [UpstreamTest(Prefix + "should preserve reasoning item IDs from providerOptionsName %s", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_reasoning_item_IDs_from_providerOptionsName(string providerOptionsName)
    {
        var options = Opts((providerOptionsName, """{"itemId":"rs_1"}"""), ("other", """{"itemId":"wrong-id"}"""));
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, providerOptionsName, Assistant(new AssistantReasoningPart("Let me think.", options)));

        JsonAssert.Equal(input, """[{"content":[{"text":"Let me think.","type":"reasoning_text"}],"id":"rs_1","type":"reasoning"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("123")]
    [InlineData("false")]
    [InlineData("[]")]
    [InlineData("{}")]
    [UpstreamTest(Prefix + "should omit a non-string reasoning item ID: $itemId", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_omit_a_non_string_reasoning_item_ID(string itemId)
    {
        var options = Opts(("deepseek", "{\"itemId\":" + itemId + "}"));
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(new AssistantReasoningPart("Let me think.", options)));

        JsonAssert.Equal(input, """[{"content":[{"text":"Let me think.","type":"reasoning_text"}],"type":"reasoning"}]""");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve interleaved reasoning, text, and multiple function calls", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_interleaved_reasoning_text_and_multiple_function_calls()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(
            new AssistantReasoningPart("I should check the weather."),
            new AssistantTextPart("Checking now."),
            Call("call_1", "weather", """{"location":"Paris"}"""),
            Call("call_2", "weather", """{"location":"London"}"""),
            new AssistantTextPart("Waiting for the results.")));

        JsonAssert.Equal(input, """
            [{"content":[{"text":"I should check the weather.","type":"reasoning_text"}],"type":"reasoning"},
             {"content":"Checking now.","role":"assistant"},
             {"arguments":"{\"location\":\"Paris\"}","call_id":"call_1","name":"weather","type":"function_call"},
             {"arguments":"{\"location\":\"London\"}","call_id":"call_2","name":"weather","type":"function_call"},
             {"content":"Waiting for the results.","role":"assistant"}]
            """);
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve string tool arguments without double encoding", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_string_tool_arguments_without_double_encoding()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(Call("call_1", "weather", "{ \"location\": \"Paris\" }")));

        JsonAssert.Equal(input, """[{"arguments":"{ \"location\": \"Paris\" }","call_id":"call_1","name":"weather","type":"function_call"}]""");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should treat client tools named web_search as function calls", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_treat_client_tools_named_web_search_as_function_calls()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(Call("call_1", "web_search", """{"query":"weather"}""", providerExecuted: false)));

        JsonAssert.Equal(input, """[{"arguments":"{\"query\":\"weather\"}","call_id":"call_1","name":"web_search","type":"function_call"}]""");
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("deepseek")]
    [InlineData("custom")]
    [UpstreamTest(Prefix + "should replay a web search call from providerOptionsName %s and skip its result", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_replay_a_web_search_call_from_providerOptionsName_and_skip_its_result(string providerOptionsName)
    {
        var options = Opts((providerOptionsName, """
            {"webSearchCall":{"type":"web_search_call","id":"ws_1","status":"completed","action":{"type":"search","queries":["Paris weather"]}}}
            """));
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, providerOptionsName, Assistant(
            Call("ws_1", "search", "{}", providerExecuted: true, options: options),
            new AssistantToolResultPart("ws_1", "search", new ToolModelMessage("ws_1", "search", """{"status":"completed"}""", false, "json")),
            new AssistantTextPart("It is sunny.")));

        JsonAssert.Equal(input, """
            [{"action":{"queries":["Paris weather"],"type":"search"},"id":"ws_1","status":"completed","type":"web_search_call"},
             {"content":"It is sunny.","role":"assistant"}]
            """);
        Assert.Empty(warnings);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"invalid\"")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"function_call\"}")]
    [UpstreamTest(Prefix + "should skip provider-executed calls with invalid web search metadata: $webSearchCall", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_skip_provider_executed_calls_with_invalid_web_search_metadata(string webSearchCall)
    {
        var options = Opts(("deepseek", "{\"webSearchCall\":" + webSearchCall + "}"));
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(Call("ws_1", "web_search", "{}", providerExecuted: true, options: options)));

        JsonAssert.Equal(input, "[]");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should skip provider-executed calls without saved metadata", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_skip_provider_executed_calls_without_saved_metadata()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(Call("ws_1", "web_search", "{}", providerExecuted: true)));

        JsonAssert.Equal(input, "[]");
        Assert.Empty(warnings);
    }

    [Fact]
    [UpstreamTest(Prefix + "should warn and skip unsupported assistant content", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_warn_and_skip_unsupported_assistant_content()
    {
        var warnings = new List<CallWarning>();

        var input = Convert(warnings, Assistant(
            new AssistantFilePart("image/png", null, Bytes123),
            new AssistantReasoningFilePart("image/png", null, Bytes123),
            new AssistantCustomPart("other.custom"),
            new AssistantTextPart("Hello")));

        JsonAssert.Equal(input, """[{"content":"Hello","role":"assistant"}]""");
        Assert.Equal(new[] { "assistant message part type: file", "assistant message part type: reasoning-file", "assistant message part type: custom" }, warnings.Select(warning => warning.Feature));
        Assert.All(warnings, warning => Assert.Equal("unsupported", warning.Type));
    }
}
