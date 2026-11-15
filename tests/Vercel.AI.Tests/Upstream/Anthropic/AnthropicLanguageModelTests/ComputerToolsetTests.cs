// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.AnthropicLanguageModelTests;

/// <summary>Port of <c>anthropic-language-model.test.ts</c> &gt; <c>computer toolset</c>.</summary>
public sealed class ComputerToolsetTests
{
    private const string Prefix = "packages/anthropic/src/anthropic-language-model.test.ts::computer toolset::";
    private const string Commit = "a161c0a665a8e561dd56905baeec94567395cb55";
    private const string ModelId = "claude-opus-5-5";

    public static IEnumerable<object[]> EachMethod()
    {
        yield return new object[] { false };
        yield return new object[] { true };
    }

    public static IEnumerable<object[]> RejectedScenarios()
    {
        foreach (var streaming in new[] { false, true })
        {
            foreach (var scenario in new[] { "disabled", "unknown member", "unknown toolset", "undeclared" })
            {
                yield return new object[] { streaming, scenario };
            }
        }
    }

    private static LanguageModelTool ComputerToolset(string name = "computer", string args = "{}")
    {
        return LanguageModelTool.ProviderDefined("anthropic.computer_toolset_20260801", name, AnthropicParity.Json(args));
    }

    private static LanguageModelCallOptions Options(IEnumerable<LanguageModelTool>? tools, string? providerOptions = null)
    {
        var options = AnthropicParity.Hello();
        options.Tools = tools?.ToArray();
        if (providerOptions != null)
        {
            options.ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = AnthropicParity.Json(providerOptions) };
        }

        return options;
    }

    private static string ContinueThread(string id)
    {
        return "{\"thread\":{\"type\":\"continue\",\"previousMessageId\":\"" + id + "\"}}";
    }

    private static ILanguageModel Model(CaptureHandler handler)
    {
        return AnthropicProvider.Create(new AnthropicOptions { ApiKey = "test-key" }, handler).LanguageModel(ModelId);
    }

    private static string Sse(params string[] chunks)
    {
        var builder = new StringBuilder();
        foreach (var chunk in chunks)
        {
            builder.Append("data: ").Append(chunk).Append("\n\n");
        }

        return builder.ToString();
    }

    private static void PrepareMemberResponse(
        CaptureHandler handler,
        bool streaming,
        string memberName = "screenshot",
        string toolsetName = "computer",
        string input = "{}",
        string? partialJson = null)
    {
        var member = new JsonObject
        {
            ["type"] = "tool_use",
            ["id"] = "toolu_member",
            ["name"] = memberName,
            ["toolset_name"] = toolsetName,
            ["input"] = streaming ? new JsonObject() : JsonNode.Parse(input),
        };
        var message = new JsonObject
        {
            ["id"] = "msg_toolset",
            ["type"] = "message",
            ["role"] = "assistant",
            ["content"] = streaming ? new JsonArray() : new JsonArray(member.DeepClone()),
            ["model"] = ModelId,
            ["stop_reason"] = streaming ? null : "tool_use",
            ["stop_sequence"] = null,
            ["usage"] = new JsonObject { ["input_tokens"] = 4, ["output_tokens"] = 30 },
        };
        if (!streaming)
        {
            handler.ContentType = "application/json";
            handler.ResponseBody = message.ToJsonString();
            return;
        }

        handler.ContentType = "text/event-stream";
        handler.ResponseBody = Sse(
            new JsonObject { ["type"] = "message_start", ["message"] = message }.ToJsonString(),
            new JsonObject { ["type"] = "content_block_start", ["index"] = 0, ["content_block"] = member }.ToJsonString(),
            new JsonObject
            {
                ["type"] = "content_block_delta",
                ["index"] = 0,
                ["delta"] = new JsonObject { ["type"] = "input_json_delta", ["partial_json"] = partialJson ?? input },
            }.ToJsonString(),
            "{\"type\":\"content_block_stop\",\"index\":0}",
            "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"tool_use\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":30}}",
            "{\"type\":\"message_stop\"}");
    }

    private sealed class Outcome
    {
        public List<LanguageModelStreamPart> StreamParts { get; } = new List<LanguageModelStreamPart>();

        public IReadOnlyList<GeneratedContent> Content { get; set; } = Array.Empty<GeneratedContent>();

        public IReadOnlyList<(string ToolCallId, string ToolName, string Input, JsonElement? Metadata)> Calls { get; set; }
            = Array.Empty<(string, string, string, JsonElement?)>();
    }

    private static async Task<Outcome> Run(ILanguageModel model, LanguageModelCallOptions options, bool streaming)
    {
        var outcome = new Outcome();
        var calls = new List<(string, string, string, JsonElement?)>();
        if (!streaming)
        {
            var result = await model.DoGenerateAsync(options, CancellationToken.None);
            outcome.Content = result.Content;
            foreach (var part in result.Content.OfType<GeneratedToolCall>())
            {
                calls.Add((part.ToolCallId, part.ToolName, part.ArgumentsJson, part.ProviderMetadata));
            }
        }
        else
        {
            var result = await model.DoStreamAsync(options, CancellationToken.None);
            try
            {
                await foreach (var part in result.Stream)
                {
                    outcome.StreamParts.Add(part);
                    if (part is ToolCallStreamPart call)
                    {
                        calls.Add((call.ToolCallId, call.ToolName, call.ArgumentsJson, call.ProviderMetadata));
                    }
                }
            }
            finally
            {
                outcome.Calls = calls;
            }
        }

        outcome.Calls = calls;
        return outcome;
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "should preserve the returned member action (streaming: %s)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_preserve_the_returned_member_action(bool streaming)
    {
        var handler = new CaptureHandler();
        PrepareMemberResponse(handler, streaming, input: "{\"action\":\"wait\",\"duration\":0}");
        var options = Options(new[] { ComputerToolset(args: "{\"configs\":{\"wait\":{\"enabled\":false}}}") });

        var outcome = await Run(Model(handler), options, streaming);

        var call = Assert.Single(outcome.Calls);
        JsonAssert.Equal(JsonNode.Parse(call.Input), "{\"action\":\"screenshot\",\"duration\":0}");
        if (streaming)
        {
            var delta = Assert.Single(outcome.StreamParts.OfType<ToolInputDeltaStreamPart>());
            Assert.Equal(call.Input, delta.Delta);
        }
    }

    [Theory]
    [MemberData(nameof(RejectedScenarios))]
    [UpstreamTest(Prefix + "should reject $scenario members (streaming: $streaming)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_reject_scenario_members(bool streaming, string scenario)
    {
        var handler = new CaptureHandler();
        PrepareMemberResponse(
            handler,
            streaming,
            memberName: scenario == "unknown member" ? "unknown" : "screenshot",
            toolsetName: scenario == "unknown toolset" ? "other" : "computer");
        var options = Options(scenario == "undeclared"
            ? Array.Empty<LanguageModelTool>()
            : new[] { ComputerToolset(args: "{\"configs\":{\"screenshot\":{\"enabled\":" + (scenario != "disabled" ? "true" : "false") + "}}}") });
        var model = Model(handler);

        if (streaming)
        {
            var parts = new List<LanguageModelStreamPart>();
            var result = await model.DoStreamAsync(options, CancellationToken.None);
            await Assert.ThrowsAsync<InvalidResponseDataException>(async () =>
            {
                await foreach (var part in result.Stream)
                {
                    parts.Add(part);
                }
            });
            Assert.DoesNotContain(parts, part => part.Type.StartsWith("tool-", StringComparison.Ordinal));
        }
        else
        {
            await Assert.ThrowsAsync<InvalidResponseDataException>(() => model.DoGenerateAsync(options, CancellationToken.None));
        }
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("1")]
    [UpstreamTest(Prefix + "should reject invalid streamed member input (%s)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_reject_invalid_streamed_member_input(string partialJson)
    {
        var handler = new CaptureHandler();
        PrepareMemberResponse(handler, true, partialJson: partialJson);

        await Assert.ThrowsAsync<InvalidResponseDataException>(
            () => Run(Model(handler), Options(new[] { ComputerToolset() }), true));
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "should preserve custom tool names on thread continuation (streaming: %s)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_preserve_custom_tool_names_on_thread_continuation(bool streaming)
    {
        var handler = new CaptureHandler();
        PrepareMemberResponse(handler, streaming);
        var options = Options(new[] { ComputerToolset("desktop") }, ContinueThread("msg_previous"));

        var outcome = await Run(Model(handler), options, streaming);

        var call = Assert.Single(outcome.Calls);
        Assert.Equal("desktop", call.ToolName);
        Assert.Equal("{\"action\":\"screenshot\"}", call.Input);
        using var body = JsonDocument.Parse(handler.Body);
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
    }

    [Theory]
    [MemberData(nameof(EachMethod))]
    [UpstreamTest(Prefix + "should reject disabled members on thread continuation (streaming: %s)", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_reject_disabled_members_on_thread_continuation(bool streaming)
    {
        var handler = new CaptureHandler();
        PrepareMemberResponse(handler, streaming);
        var options = Options(
            new[] { ComputerToolset(args: "{\"configs\":{\"screenshot\":{\"enabled\":false}}}") },
            ContinueThread("msg_previous"));

        await Assert.ThrowsAsync<InvalidResponseDataException>(() => Run(Model(handler), options, streaming));
    }

    [Fact]
    [UpstreamTest(Prefix + "should send the toolset definition without a name and map member calls to the computer tool", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_send_the_toolset_definition_without_a_name_and_map_member_calls_to_the_computer_tool()
    {
        var handler = new CaptureHandler
        {
            ResponseBody = "{\"id\":\"msg_toolset\",\"type\":\"message\",\"role\":\"assistant\",\"content\":["
                + "{\"type\":\"text\",\"text\":\"Clicking the search box.\"},"
                + "{\"type\":\"tool_use\",\"id\":\"toolu_click\",\"name\":\"left_click\",\"toolset_name\":\"computer\",\"input\":{\"coordinate\":[640,60]}},"
                + "{\"type\":\"tool_use\",\"id\":\"toolu_type\",\"name\":\"type\",\"toolset_name\":\"computer\",\"input\":{\"text\":\"pictures of cats\"}},"
                + "{\"type\":\"tool_use\",\"id\":\"toolu_screenshot\",\"name\":\"screenshot\",\"toolset_name\":\"computer\",\"input\":{}}],"
                + "\"model\":\"claude-opus-5-5\",\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}",
        };

        var result = await Model(handler).DoGenerateAsync(Options(new[] { ComputerToolset() }), CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        JsonAssert.Equal(body.RootElement.GetProperty("tools"), "[{\"type\":\"computer_toolset_20260801\"}]");
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));

        Assert.Equal(4, result.Content.Count);
        Assert.Equal("Clicking the search box.", Assert.IsType<GeneratedText>(result.Content[0]).Text);
        var expected = new[]
        {
            ("toolu_click", "{\"coordinate\":[640,60],\"action\":\"left_click\"}"),
            ("toolu_type", "{\"text\":\"pictures of cats\",\"action\":\"type\"}"),
            ("toolu_screenshot", "{\"action\":\"screenshot\"}"),
        };
        for (var index = 0; index < expected.Length; index++)
        {
            var call = Assert.IsType<GeneratedToolCall>(result.Content[index + 1]);
            Assert.Equal(expected[index].Item1, call.ToolCallId);
            Assert.Equal("computer", call.ToolName);
            Assert.Equal(expected[index].Item2, call.ArgumentsJson);
            JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"anthropic\":{\"toolsetName\":\"computer\"}}");
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should map member calls to a custom tool name", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_map_member_calls_to_a_custom_tool_name()
    {
        var handler = new CaptureHandler
        {
            ResponseBody = "{\"id\":\"msg_toolset\",\"type\":\"message\",\"role\":\"assistant\",\"content\":["
                + "{\"type\":\"tool_use\",\"id\":\"toolu_screenshot\",\"name\":\"screenshot\",\"toolset_name\":\"computer\",\"input\":{}}],"
                + "\"model\":\"claude-opus-5-5\",\"stop_reason\":\"tool_use\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}",
        };

        var result = await Model(handler).DoGenerateAsync(Options(new[] { ComputerToolset("desktop") }), CancellationToken.None);

        var call = Assert.IsType<GeneratedToolCall>(Assert.Single(result.Content));
        Assert.Equal("toolu_screenshot", call.ToolCallId);
        Assert.Equal("desktop", call.ToolName);
        Assert.Equal("{\"action\":\"screenshot\"}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(Prefix + "should send member configs", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_send_member_configs()
    {
        var handler = new CaptureHandler
        {
            ResponseBody = "{\"id\":\"msg_toolset\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"ok\"}],"
                + "\"model\":\"claude-opus-5-5\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}",
        };

        await Model(handler).DoGenerateAsync(
            Options(new[] { ComputerToolset(args: "{\"configs\":{\"zoom\":{\"enabled\":false},\"hold_key\":{\"deferLoading\":true}}}") }),
            CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        JsonAssert.Equal(
            body.RootElement.GetProperty("tools"),
            "[{\"type\":\"computer_toolset_20260801\",\"configs\":{\"zoom\":{\"enabled\":false},\"hold_key\":{\"defer_loading\":true}}}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should round-trip member calls and results with toolset_name", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_round_trip_member_calls_and_results_with_toolset_name()
    {
        var handler = new CaptureHandler
        {
            ResponseBody = "{\"id\":\"msg_toolset\",\"type\":\"message\",\"role\":\"assistant\",\"content\":[{\"type\":\"text\",\"text\":\"Done.\"}],"
                + "\"model\":\"claude-opus-5-5\",\"stop_reason\":\"end_turn\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":4,\"output_tokens\":30}}",
        };
        var options = new LanguageModelCallOptions
        {
            Tools = new[] { ComputerToolset() },
            Prompt = new ModelMessage[]
            {
                new UserModelMessage("Open the search."),
                new AssistantModelMessage(
                    null,
                    new[]
                    {
                        new GeneratedToolCall("toolu_click", "computer", "{\"action\":\"left_click\",\"coordinate\":[640,60]}"),
                        new GeneratedToolCall("toolu_screenshot", "computer", "{\"action\":\"screenshot\"}"),
                    },
                    null),
                new ToolModelMessage("toolu_click", "computer", "OK", false, "text"),
                new ToolModelMessage("toolu_screenshot", "computer", "Not executed: an earlier computer action in this turn failed.", true, "error-text"),
            },
        };

        await Model(handler).DoGenerateAsync(options, CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body);
        JsonAssert.Equal(
            body.RootElement.GetProperty("messages"),
            "[{\"content\":[{\"text\":\"Open the search.\",\"type\":\"text\"}],\"role\":\"user\"},"
            + "{\"content\":[{\"id\":\"toolu_click\",\"input\":{\"action\":\"left_click\",\"coordinate\":[640,60]},\"name\":\"left_click\",\"toolset_name\":\"computer\",\"type\":\"tool_use\"},"
            + "{\"id\":\"toolu_screenshot\",\"input\":{\"action\":\"screenshot\"},\"name\":\"screenshot\",\"toolset_name\":\"computer\",\"type\":\"tool_use\"}],\"role\":\"assistant\"},"
            + "{\"content\":[{\"content\":\"OK\",\"tool_use_id\":\"toolu_click\",\"toolset_name\":\"computer\",\"type\":\"tool_result\"},"
            + "{\"content\":\"Not executed: an earlier computer action in this turn failed.\",\"is_error\":true,\"tool_use_id\":\"toolu_screenshot\",\"toolset_name\":\"computer\",\"type\":\"tool_result\"}],\"role\":\"user\"}]");
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream member calls as a single input delta with the action injected", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public async Task Should_stream_member_calls_as_a_single_input_delta_with_the_action_injected()
    {
        var handler = new CaptureHandler
        {
            ContentType = "text/event-stream",
            ResponseBody = Sse(
                "{\"type\":\"message_start\",\"message\":{\"id\":\"msg_toolset\",\"type\":\"message\",\"role\":\"assistant\",\"model\":\"claude-opus-5-5\",\"stop_sequence\":null,\"usage\":{\"input_tokens\":10,\"output_tokens\":1},\"content\":[],\"stop_reason\":null}}",
                "{\"type\":\"content_block_start\",\"index\":0,\"content_block\":{\"type\":\"tool_use\",\"id\":\"toolu_click\",\"name\":\"left_click\",\"toolset_name\":\"computer\",\"input\":{}}}",
                "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"\"}}",
                "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"{\\\"coordinate\\\"\"}}",
                "{\"type\":\"content_block_delta\",\"index\":0,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\":[640,60]}\"}}",
                "{\"type\":\"content_block_stop\",\"index\":0}",
                "{\"type\":\"content_block_start\",\"index\":1,\"content_block\":{\"type\":\"tool_use\",\"id\":\"toolu_screenshot\",\"name\":\"screenshot\",\"toolset_name\":\"computer\",\"input\":{}}}",
                "{\"type\":\"content_block_delta\",\"index\":1,\"delta\":{\"type\":\"input_json_delta\",\"partial_json\":\"\"}}",
                "{\"type\":\"content_block_stop\",\"index\":1}",
                "{\"type\":\"message_delta\",\"delta\":{\"stop_reason\":\"tool_use\",\"stop_sequence\":null},\"usage\":{\"output_tokens\":20}}",
                "{\"type\":\"message_stop\"}"),
        };

        var outcome = await Run(Model(handler), Options(new[] { ComputerToolset() }), true);

        var parts = outcome.StreamParts
            .Where(part => part is ToolInputStartStreamPart || part is ToolInputDeltaStreamPart || part is ToolInputEndStreamPart || part is ToolCallStreamPart)
            .ToList();
        Assert.Equal(8, parts.Count);
        var expected = new[]
        {
            ("toolu_click", "{\"coordinate\":[640,60],\"action\":\"left_click\"}"),
            ("toolu_screenshot", "{\"action\":\"screenshot\"}"),
        };
        for (var index = 0; index < expected.Length; index++)
        {
            var start = Assert.IsType<ToolInputStartStreamPart>(parts[index * 4]);
            Assert.Equal(expected[index].Item1, start.Id);
            Assert.Equal("computer", start.ToolName);
            var delta = Assert.IsType<ToolInputDeltaStreamPart>(parts[(index * 4) + 1]);
            Assert.Equal(expected[index].Item1, delta.Id);
            Assert.Equal(expected[index].Item2, delta.Delta);
            var end = Assert.IsType<ToolInputEndStreamPart>(parts[(index * 4) + 2]);
            Assert.Equal(expected[index].Item1, end.Id);
            var call = Assert.IsType<ToolCallStreamPart>(parts[(index * 4) + 3]);
            Assert.Equal(expected[index].Item1, call.ToolCallId);
            Assert.Equal("computer", call.ToolName);
            Assert.Equal(expected[index].Item2, call.ArgumentsJson);
            Assert.False(call.ProviderExecuted);
            JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"anthropic\":{\"toolsetName\":\"computer\"}}");
        }
    }
}
