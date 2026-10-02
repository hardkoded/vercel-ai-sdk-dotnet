// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

public sealed class OpenAICompatibleToolStreamTests
{
    [Fact]
    public async Task Missing_tool_call_id_is_generated()
    {
        var parts = await ReadParts(
            Chunk(Tool(0, null, "lookup", "{\"q\":1}")) + Done());

        var call = Assert.Single(ToolCalls(parts));
        Assert.False(string.IsNullOrWhiteSpace(call.ToolCallId));
        Assert.NotEqual("call_0", call.ToolCallId);
        Assert.Equal("lookup", call.ToolName);
        Assert.Equal("{\"q\":1}", call.ArgumentsJson);
        Assert.IsType<FinishStreamPart>(parts[parts.Count - 1]);
    }

    [Fact]
    public async Task ShouldKeepIdLessToolCallsDistinctWhenTheIndexIsReused()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, null, "read_file", "{\"path\":\"p0\"}")) +
            Chunk(Tool(0, null, "write_file", "{\"path\":\"p1\"}")) +
            Chunk(Tool(0, null, "read_file", "{\"path\":\"p2\"}")) +
            Done());

        Assert.Equal(3, calls.Count);
        Assert.Equal("read_file", calls[0].ToolName);
        Assert.Equal("{\"path\":\"p0\"}", calls[0].ArgumentsJson);
        Assert.Equal("write_file", calls[1].ToolName);
        Assert.Equal("{\"path\":\"p1\"}", calls[1].ArgumentsJson);
        Assert.Equal("read_file", calls[2].ToolName);
        Assert.Equal("{\"path\":\"p2\"}", calls[2].ArgumentsJson);
        Assert.False(string.IsNullOrWhiteSpace(calls[0].ToolCallId));
        Assert.False(string.IsNullOrWhiteSpace(calls[1].ToolCallId));
        Assert.False(string.IsNullOrWhiteSpace(calls[2].ToolCallId));
        Assert.Equal(3, new HashSet<string> { calls[0].ToolCallId, calls[1].ToolCallId, calls[2].ToolCallId }.Count);
    }

    [Fact]
    public async Task Reused_index_with_a_new_name_stays_a_separate_call()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, null, "read_file", "{\"path\":\"a\"}")) +
            Chunk(Tool(0, null, "write_file", "{\"path\":\"b\"}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.False(string.IsNullOrWhiteSpace(calls[0].ToolCallId));
        Assert.False(string.IsNullOrWhiteSpace(calls[1].ToolCallId));
        Assert.NotEqual(calls[0].ToolCallId, calls[1].ToolCallId);
        Assert.Equal("read_file", calls[0].ToolName);
        Assert.Equal("{\"path\":\"a\"}", calls[0].ArgumentsJson);
        Assert.Equal("write_file", calls[1].ToolName);
        Assert.Equal("{\"path\":\"b\"}", calls[1].ArgumentsJson);
    }

    [Fact]
    public async Task Repeated_tool_call_id_receives_a_different_id()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "dup", "read_file", "{}")) +
            Chunk(Tool(1, "dup", "write_file", "{}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.Equal("dup", calls[0].ToolCallId);
        Assert.Equal("read_file", calls[0].ToolName);
        Assert.Equal("{}", calls[0].ArgumentsJson);
        Assert.False(string.IsNullOrWhiteSpace(calls[1].ToolCallId));
        Assert.NotEqual("dup", calls[1].ToolCallId);
        Assert.Equal("write_file", calls[1].ToolName);
        Assert.Equal("{}", calls[1].ArgumentsJson);
    }

    [Fact]
    public async Task Interleaved_same_name_calls_with_distinct_ids_stay_separate()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "same_tool", "{\"value\":")) +
            Chunk(Tool(0, "call_2", "same_tool", "{\"value\":2}")) +
            Chunk(Tool(0, "call_1", null, "1}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.Equal("call_1", calls[0].ToolCallId);
        Assert.Equal("same_tool", calls[0].ToolName);
        Assert.Equal("{\"value\":1}", calls[0].ArgumentsJson);
        Assert.Equal("call_2", calls[1].ToolCallId);
        Assert.Equal("same_tool", calls[1].ToolName);
        Assert.Equal("{\"value\":2}", calls[1].ArgumentsJson);
    }

    [Fact]
    public async Task Argument_continuation_after_a_complete_object_starts_a_new_call()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "lookup", "{\"q\":1}")) +
            Chunk(Tool(0, "call_1", "lookup", "{\"q\":2}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.Equal("call_1", calls[0].ToolCallId);
        Assert.Equal("lookup", calls[0].ToolName);
        Assert.Equal("{\"q\":1}", calls[0].ArgumentsJson);
        Assert.False(string.IsNullOrWhiteSpace(calls[1].ToolCallId));
        Assert.NotEqual("call_1", calls[1].ToolCallId);
        Assert.Equal("lookup", calls[1].ToolName);
        Assert.Equal("{\"q\":2}", calls[1].ArgumentsJson);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Blank_function_name_does_not_merge_into_the_previous_call(string name)
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "valid_tool", "{\"value\":1}")) +
            Chunk(Tool(1, "call_2", name, "{\"value\":2}")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("valid_tool", call.ToolName);
        Assert.Equal("{\"value\":1}", call.ArgumentsJson);
    }

    [Fact]
    public async Task Incomplete_object_followed_by_a_nested_object_stays_one_call()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "calculate", "{\"value\":")) +
            Chunk(Tool(0, "call_1", "calculate", "{\"nested\":true}}")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("calculate", call.ToolName);
        Assert.Equal("{\"value\":{\"nested\":true}}", call.ArgumentsJson);
    }

    [Fact]
    public async Task Scalar_argument_prefix_stays_one_call()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "calculate", "1")) +
            Chunk(Tool(0, "call_1", "calculate", "2")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("12", call.ArgumentsJson);
    }

    [Fact]
    public async Task Changed_id_keeps_the_first_id_when_index_and_name_match()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "read_file", "{\"pa")) +
            Chunk(Tool(0, "call_other", "read_file", "th\":\"a\"}")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal("call_1", call.ToolCallId);
        Assert.Equal("read_file", call.ToolName);
        Assert.Equal("{\"path\":\"a\"}", call.ArgumentsJson);
    }

    [Fact]
    public async Task Argument_only_blank_name_blank_id_and_unexpected_id_append()
    {
        var argumentOnly = await ReadTools(
            Chunk(Tool(0, "call_1", "lookup", "{\"q\":")) +
            Chunk(Tool(0, null, null, "1}")) +
            Done());
        Assert.Equal("{\"q\":1}", Assert.Single(argumentOnly).ArgumentsJson);

        var blankName = await ReadTools(
            Chunk(Tool(0, "call_1", "read_file", "{\"pa")) +
            Chunk(Tool(null, "call_1", "", "th\":\"a\"}")) +
            Done());
        var named = Assert.Single(blankName);
        Assert.Equal("call_1", named.ToolCallId);
        Assert.Equal("read_file", named.ToolName);
        Assert.Equal("{\"path\":\"a\"}", named.ArgumentsJson);

        var blankId = await ReadTools(
            Chunk(Tool(0, "call_1", "read_file", "{\"pa")) +
            Chunk(Tool(0, "   ", null, "th\":\"a\"}")) +
            Done());
        Assert.Equal("call_1", Assert.Single(blankId).ToolCallId);
        Assert.Equal("{\"path\":\"a\"}", blankId[0].ArgumentsJson);

        var unexpected = await ReadTools(
            Chunk(Tool(0, "call_1", "read_file", "{\"pa")) +
            Chunk(Tool(0, "unexpected", null, "th\":\"a\"}")) +
            Done());
        Assert.Equal("call_1", Assert.Single(unexpected).ToolCallId);
        Assert.Equal("{\"path\":\"a\"}", unexpected[0].ArgumentsJson);
    }

    [Fact]
    public async Task Ambiguous_delta_is_dropped()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "read_file", "{\"path\":\"a\"}")) +
            Chunk(Tool(1, "call_2", "write_file", "{\"path\":\"b\"}")) +
            Chunk(Tool(null, null, null, "{\"extra\":true}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.Equal("{\"path\":\"a\"}", calls[0].ArgumentsJson);
        Assert.Equal("{\"path\":\"b\"}", calls[1].ArgumentsJson);
    }

    [Fact]
    public async Task Indexed_calls_emit_in_index_order()
    {
        var calls = await ReadTools(
            Chunk(Tool(1, "call_1", "second", "{}")) +
            Chunk(Tool(0, "call_0", "first", "{}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.Equal("first", calls[0].ToolName);
        Assert.Equal("second", calls[1].ToolName);
    }

    [Fact]
    public async Task Mixed_omitted_index_keeps_arrival_order()
    {
        var calls = await ReadTools(
            Chunk(Tool(null, "call_a", "first", "{}")) +
            Chunk(Tool(0, "call_b", "second", "{\"v\":")) +
            Chunk(Tool(0, null, null, "1}")) +
            Done());

        Assert.Equal(2, calls.Count);
        Assert.Equal("first", calls[0].ToolName);
        Assert.Equal("call_a", calls[0].ToolCallId);
        Assert.Equal("{}", calls[0].ArgumentsJson);
        Assert.Equal("second", calls[1].ToolName);
        Assert.Equal("call_b", calls[1].ToolCallId);
        Assert.Equal("{\"v\":1}", calls[1].ArgumentsJson);
    }

    [Fact]
    public async Task Nonblank_ids_and_names_keep_surrounding_spaces()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, " spaced ", " same_tool ", "{\"value\":")) +
            Chunk(Tool(0, " spaced ", null, "0}")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal(" spaced ", call.ToolCallId);
        Assert.Equal(" same_tool ", call.ToolName);
        Assert.Equal("{\"value\":0}", call.ArgumentsJson);
    }

    [Fact]
    public async Task Brace_inside_a_string_does_not_finish_the_object()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "lookup", "{\"a\":\"x}y")) +
            Chunk(Tool(0, "call_1", "lookup", "{\"z\":1}")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal("{\"a\":\"x}y{\"z\":1}", call.ArgumentsJson);
    }

    [Fact]
    public async Task Escaped_quote_does_not_finish_the_string()
    {
        var calls = await ReadTools(
            Chunk(Tool(0, "call_1", "lookup", "{\"a\":\"\\\"}")) +
            Chunk(Tool(0, "call_1", "lookup", "{\"b\":1}")) +
            Done());

        var call = Assert.Single(calls);
        Assert.Equal("{\"a\":\"\\\"}{\"b\":1}", call.ArgumentsJson);
    }

    private static List<ToolCallStreamPart> ToolCalls(List<LanguageModelStreamPart> parts)
    {
        var calls = new List<ToolCallStreamPart>();
        foreach (var part in parts)
        {
            if (part is ToolCallStreamPart call)
            {
                calls.Add(call);
            }
        }

        return calls;
    }

    private static async Task<List<ToolCallStreamPart>> ReadTools(string body)
    {
        return ToolCalls(await ReadParts(body));
    }

    private static async Task<List<LanguageModelStreamPart>> ReadParts(string body)
    {
        var provider = OpenAICompatibleProvider.Create(
            new OpenAICompatibleOptions { ApiKey = "secret", ProviderName = "openai-compatible" },
            new SseHandler(body));
        var parts = new List<LanguageModelStreamPart>();
        await foreach (var part in provider.LanguageModel("m").DoStreamAsync(Prompt(), CancellationToken.None))
        {
            parts.Add(part);
        }

        return parts;
    }

    private static LanguageModelCallOptions Prompt()
    {
        return new LanguageModelCallOptions { Prompt = new ModelMessage[] { new UserModelMessage("hi") } };
    }

    private static string Chunk(JsonObject tool)
    {
        var payload = new JsonObject
        {
            ["choices"] = new JsonArray
            {
                new JsonObject
                {
                    ["delta"] = new JsonObject
                    {
                        ["tool_calls"] = new JsonArray { tool },
                    },
                },
            },
        };
        return "data: " + payload.ToJsonString() + "\n\n";
    }

    private static JsonObject Tool(int? index, string? id, string? name, string? arguments)
    {
        var tool = new JsonObject();
        if (index.HasValue)
        {
            tool["index"] = index.Value;
        }

        if (id != null)
        {
            tool["id"] = id;
        }

        var function = new JsonObject();
        if (name != null)
        {
            function["name"] = name;
        }

        if (arguments != null)
        {
            function["arguments"] = arguments;
        }

        if (function.Count > 0)
        {
            tool["function"] = function;
        }

        return tool;
    }

    private static string Done()
    {
        return "data: {\"choices\":[{\"delta\":{},\"finish_reason\":\"tool_calls\"}],\"usage\":{\"prompt_tokens\":1,\"completion_tokens\":1,\"total_tokens\":2}}\n\n"
            + "data: [DONE]\n\n";
    }

    private sealed class SseHandler : HttpMessageHandler
    {
        private readonly string _body;

        public SseHandler(string body)
        {
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_body, Encoding.UTF8, "text/event-stream"),
            });
        }
    }
}
