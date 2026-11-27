// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Cohere.CohereTestSupport;

namespace Vercel.AI.Tests.Upstream.Cohere.CohereChatLanguageModel;

/// <summary>Port of <c>cohere-chat-language-model.test.ts</c> &gt; <c>doStream</c> &gt; <c>tool call</c>.</summary>
public sealed class DoStreamToolCallTests
{
    private const string Chat = "packages/cohere/src/cohere-chat-language-model.test.ts::";
    private const string Suite = Chat + "doStream > tool call::";

    private static string ToolCallLines(string name, string arguments) =>
        "{\"type\":\"tool-call-start\",\"index\":0,\"delta\":{\"message\":{\"tool_calls\":{\"id\":\"test-tool-call\",\"type\":\"function\",\"function\":{\"name\":\""
        + name
        + "\",\"arguments\":\"\"}}}}}";

    private static string Delta(string arguments) =>
        "{\"type\":\"tool-call-delta\",\"index\":0,\"delta\":{\"message\":{\"tool_calls\":{\"function\":{\"arguments\":"
        + System.Text.Json.JsonSerializer.Serialize(arguments)
        + "}}}}}";

    private const string End = "{\"type\":\"tool-call-end\",\"index\":0}";

    [Fact]
    [UpstreamTest(Suite + "should stream tool deltas", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_tool_deltas()
    {
        var options = TestPrompt();
        options.IncludeRawChunks = false;
        options.Tools = new[]
        {
            FunctionTool(
                "test-tool",
                "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}},\"required\":[\"value\"],\"additionalProperties\":false,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}"
            ),
        };

        var parts = await Stream(Model(ChunksFixture("cohere-tool-call")), options);

        Assert.Equal(
            "2941521a-b87a-45f6-9b0d-235fd66c3025",
            Assert.IsType<ResponseMetadataStreamPart>(parts[1]).Id
        );
        var start = Assert.IsType<ToolInputStartStreamPart>(parts[2]);
        Assert.Equal("weather_e8p4pn45zt0t", start.Id);
        Assert.Equal("weather", start.ToolName);
        Assert.Equal(
            new[] { "{\"", "location", "\":", " \"", "San", " Francisco", "\"}" },
            parts
                .Skip(3)
                .Take(7)
                .Select(part => Assert.IsType<ToolInputDeltaStreamPart>(part).Delta)
        );
        Assert.Equal("weather_e8p4pn45zt0t", Assert.IsType<ToolInputEndStreamPart>(parts[10]).Id);
        var call = Assert.IsType<ToolCallStreamPart>(parts[11]);
        Assert.Equal("weather_e8p4pn45zt0t", call.ToolCallId);
        Assert.Equal("weather", call.ToolName);
        Assert.Equal("{\"location\":\"San Francisco\"}", call.ArgumentsJson);
        Assert.Equal(
            "cityAttractions_pyxssbwnq9fq",
            Assert.IsType<ToolInputStartStreamPart>(parts[12]).Id
        );
        var second = Assert.IsType<ToolCallStreamPart>(parts[21]);
        Assert.Equal("cityAttractions", second.ToolName);
        Assert.Equal("{\"city\":\"San Francisco\"}", second.ArgumentsJson);
        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal("TOOL_CALL", finish.RawFinishReason);
    }

    [Fact]
    // Ports "doStream > tool call > should handle string \"null\" tool call arguments". The catalog snapshot predates that test,
    // so it has no id to link yet.
    public async Task Should_handle_string_null_tool_call_arguments()
    {
        var handler = ChunkLines(ToolCallLines("currentTime", ""), Delta("null"), End);
        var options = TestPrompt();
        options.Tools = new[]
        {
            FunctionTool(
                "currentTime",
                "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":false}"
            ),
        };

        var parts = await Stream(Model(handler), options);

        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("test-tool-call", call.ToolCallId);
        Assert.Equal("currentTime", call.ToolName);
        Assert.Equal("{}", call.ArgumentsJson);
    }

    [Fact]
    [UpstreamTest(
        Suite + "rejects prototype keys in streamed tool call arguments",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Rejects_prototype_keys_in_streamed_tool_call_arguments()
    {
        var handler = ChunkLines(
            ToolCallLines("test-tool", ""),
            Delta("{\"__proto__\":{\"polluted\":true}}"),
            End
        );
        var options = TestPrompt();
        options.IncludeRawChunks = false;
        options.Tools = new[]
        {
            FunctionTool(
                "test-tool",
                "{\"type\":\"object\",\"properties\":{},\"additionalProperties\":true,\"$schema\":\"http://json-schema.org/draft-07/schema#\"}"
            ),
        };

        var error = await Assert.ThrowsAnyAsync<Exception>(() => Stream(Model(handler), options));

        Assert.Contains("Object contains forbidden prototype property", error.Message);
    }
}
