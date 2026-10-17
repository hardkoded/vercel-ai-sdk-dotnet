// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesLanguageModelTests;

/// <summary>Port of <c>openai-responses-language-model.test.ts</c> &gt; <c>OpenAIResponsesLanguageModel &gt; doStream &gt; custom tool</c>.</summary>
public sealed class DoStreamCustomToolTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-language-model.test.ts::OpenAIResponsesLanguageModel > doStream > custom tool::";

    public static TheoryData<string[]> RawDeltas => new()
    {
        { new[] { "text(", "\"probe\"", ");\n" } },
        { new[] { "path\\", "\t\r\n", "\u0000" } },
        { new[] { "\"", "quoted", "\"" } },
        { Array.Empty<string>() },
    };

    [Theory]
    [MemberData(nameof(RawDeltas))]
    [UpstreamTest(
        Prefix + "should preserve raw custom tool input deltas: %j",
        Coverage = UpstreamCoverage.Partial,
        Note = "Covers the text, escape, quote and empty cases. The lone surrogate case (a UTF-16 pair split across two deltas) cannot be written: .NET strings reject a lone surrogate when parsing JSON. See #569.")]
    public async Task Should_preserve_raw_custom_tool_input_deltas(string[] deltas)
    {
        JsonObject Item(string input) => new()
        {
            ["type"] = "custom_tool_call",
            ["id"] = "ct_probe",
            ["call_id"] = "call_probe",
            ["name"] = "exec",
            ["status"] = "completed",
            ["input"] = input,
        };

        var events = new List<JsonNode>
        {
            new JsonObject { ["type"] = "response.output_item.added", ["output_index"] = 0, ["item"] = Item(string.Empty) },
        };
        events.AddRange(deltas.Select(delta => (JsonNode)new JsonObject
        {
            ["type"] = "response.custom_tool_call_input.delta",
            ["item_id"] = "ct_probe",
            ["output_index"] = 0,
            ["delta"] = delta,
        }));
        events.Add(new JsonObject { ["type"] = "response.output_item.done", ["output_index"] = 0, ["item"] = Item(string.Concat(deltas)) });

        var parts = await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events));

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        var start = Assert.Single(parts.OfType<ToolInputStartStreamPart>());
        Assert.Equal("call_probe", start.Id);
        Assert.Equal("exec", start.ToolName);
        Assert.Equal(ToolInputEncoding.Text, start.InputEncoding);
        Assert.Equal(deltas, parts.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta));
        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal(string.Concat(deltas), JsonNode.Parse(call.ArgumentsJson)!.GetValue<string>());
        Assert.True(parts.FindIndex(part => part is ToolInputEndStreamPart) < parts.FindIndex(part => part is ToolCallStreamPart));
    }

    [Fact]
    [UpstreamTest(Prefix + "should stream custom tool call", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_custom_tool_call()
    {
        var events = OpenAIResponsesSupport.Fixture("openai-custom-tool.1.chunks.txt")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonNode.Parse(line)!);

        var parts = await OpenAIResponsesSupport.Stream(OpenAIResponsesSupport.Sse(events));

        Assert.Empty(parts.OfType<ErrorStreamPart>());
        Assert.Empty(Assert.IsType<StreamStartStreamPart>(parts[0]).Warnings);
        var metadata = Assert.IsType<ResponseMetadataStreamPart>(parts[1]);
        Assert.Equal("resp_custom_tool_test_001", metadata.Id);
        Assert.Equal("gpt-5.2-codex", metadata.ModelId);
        Assert.Equal(DateTimeOffset.Parse("2025-03-06T10:42:10.000Z"), metadata.Timestamp);

        var start = Assert.IsType<ToolInputStartStreamPart>(parts[2]);
        Assert.Equal("call_custom_sql_001", start.Id);
        Assert.Equal("write_sql", start.ToolName);
        Assert.Equal(ToolInputEncoding.Text, start.InputEncoding);
        Assert.Equal(new[] { "SELECT * ", "FROM users ", "WHERE age > 25" }, parts.OfType<ToolInputDeltaStreamPart>().Select(part => part.Delta));
        Assert.All(parts.OfType<ToolInputDeltaStreamPart>(), part => Assert.Equal("call_custom_sql_001", part.Id));
        Assert.Equal("call_custom_sql_001", Assert.Single(parts.OfType<ToolInputEndStreamPart>()).Id);

        var call = Assert.Single(parts.OfType<ToolCallStreamPart>());
        Assert.Equal("call_custom_sql_001", call.ToolCallId);
        Assert.Equal("write_sql", call.ToolName);
        Assert.Equal("\"SELECT * FROM users WHERE age > 25\"", call.ArgumentsJson);
        JsonAssert.Equal(call.ProviderMetadata!.Value, "{\"openai\":{\"itemId\":\"ct_abc123def456\"}}");

        var finish = Assert.IsType<FinishStreamPart>(parts[^1]);
        Assert.Equal(FinishReason.ToolCalls, finish.FinishReason);
        Assert.Equal(50, finish.Usage.InputTokens);
        Assert.Equal(20, finish.Usage.OutputTokens);
        Assert.Equal(70, finish.Usage.TotalTokens);
        JsonAssert.Equal(finish.ProviderMetadata!.Value.GetProperty("openai").GetProperty("responseId"), "\"resp_custom_tool_test_001\"");
    }
}
