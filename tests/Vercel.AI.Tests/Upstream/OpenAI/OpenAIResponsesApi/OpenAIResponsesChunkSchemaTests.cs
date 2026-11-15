// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenAI;
using Vercel.AI.Tests.Upstream;

namespace Vercel.AI.Tests.OpenAIResponsesApi;

/// <summary>Port of <c>openai-responses-api.test.ts</c> &gt; <c>openaiResponsesChunkSchema</c>.</summary>
public sealed class OpenAIResponsesChunkSchemaTests
{
    private const string Prefix = "packages/openai/src/responses/openai-responses-api.test.ts::openaiResponsesChunkSchema::";

    private const string ItemPrefix = "packages/openai/src/responses/openai-responses-api.test.ts::openaiResponsesChunkSchema > %s::";

    private const string LocalShellPrefix = "packages/openai/src/responses/openai-responses-api.test.ts::openaiResponsesChunkSchema > %s local_shell_call::";

    public static TheoryData<string, string> RejectedItemEvents()
    {
        var data = new TheoryData<string, string>();
        foreach (var type in new[] { "response.output_item.added", "response.output_item.done" })
        {
            data.Add(type, "missing item");
            data.Add(type, "null item");
            data.Add(type, "non-object item");
            data.Add(type, "type-less item");
        }

        return data;
    }

    public static TheoryData<string> ItemTypes() => ["response.output_item.added", "response.output_item.done"];

    public static TheoryData<string, string> CompletedEvents()
    {
        var data = new TheoryData<string, string>();
        foreach (var type in new[] { "response.completed", "response.incomplete" })
        {
            data.Add(type, "missing usage");
            data.Add(type, "null usage");
        }

        return data;
    }

    public static TheoryData<string, string> RefusalEvents() => new()
    {
        {
            "response.refusal.delta",
            """{ "type": "response.refusal.delta", "content_index": 0, "delta": "I'm sorry", "item_id": "msg_1", "output_index": 0, "sequence_number": 4 }"""
        },
        {
            "response.refusal.done",
            """{ "type": "response.refusal.done", "content_index": 0, "item_id": "msg_1", "output_index": 0, "refusal": "I'm sorry, I can't assist with that.", "sequence_number": 5 }"""
        },
    };

    private static bool IsValid(string json) => OpenAIResponsesSchema.IsValidChunk(OpenAIUpstream.Json(json));

    private static string ItemEvent(string type, string itemProperty) => $$"""{ "type": "{{type}}", "output_index": 0{{itemProperty}} }""";

    [Theory]
    [MemberData(nameof(RejectedItemEvents))]
    [UpstreamTest(ItemPrefix + "rejects events with $name", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Rejects_events_with_name(string type, string name)
    {
        var itemProperty = name switch
        {
            "missing item" => string.Empty,
            "null item" => ", \"item\": null",
            "non-object item" => ", \"item\": \"invalid\"",
            _ => ", \"item\": { \"id\": \"item_1\" }",
        };

        Assert.False(IsValid(ItemEvent(type, itemProperty)));
    }

    [Theory]
    [MemberData(nameof(ItemTypes))]
    [UpstreamTest(Prefix + "keeps future item types forward compatible for %s", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Keeps_future_item_types_forward_compatible_for_s(string type)
    {
        var chunk = OpenAIUpstream.Json(ItemEvent(type, ", \"item\": { \"id\": \"item_1\", \"type\": \"future_output_item\" }"));

        Assert.True(OpenAIResponsesSchema.IsValidChunk(chunk));
        Assert.True(OpenAIResponsesSchema.IsUnknownChunk(chunk));
    }

    [Theory]
    [MemberData(nameof(ItemTypes))]
    [UpstreamTest(LocalShellPrefix + "accepts valid events", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Accepts_valid_events(string type)
    {
        var chunk = OpenAIUpstream.Json(ItemEvent(type, ", \"item\": " + LocalShellItem(type)));

        Assert.True(OpenAIResponsesSchema.IsValidChunk(chunk));
        Assert.False(OpenAIResponsesSchema.IsUnknownChunk(chunk));
    }

    [Theory]
    [MemberData(nameof(ItemTypes))]
    [UpstreamTest(LocalShellPrefix + "rejects events without output_index", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Rejects_events_without_output_index(string type)
    {
        Assert.False(IsValid($$"""{ "type": "{{type}}", "item": {{LocalShellItem(type)}} }"""));
    }

    [Fact]
    [UpstreamTest(Prefix + "accepts valid function call arguments done events", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Accepts_valid_function_call_arguments_done_events()
    {
        Assert.True(IsValid("""
            { "type": "response.function_call_arguments.done", "item_id": "item_1", "output_index": 0, "arguments": "{\"city\":\"Berlin\"}" }
            """));
    }

    [Fact]
    [UpstreamTest(Prefix + "rejects malformed function call arguments done events", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Rejects_malformed_function_call_arguments_done_events()
    {
        Assert.False(IsValid("""
            { "type": "response.function_call_arguments.done", "item_id": "item_1", "arguments": "{\"city\":\"Berlin\"}" }
            """));
    }

    [Theory]
    [MemberData(nameof(CompletedEvents))]
    [UpstreamTest(ItemPrefix + "accepts responses with $name", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Accepts_responses_with_name(string type, string name)
    {
        var response = name == "null usage" ? """{ "usage": null }""" : "{}";

        Assert.True(IsValid($$"""{ "type": "{{type}}", "response": {{response}} }"""));
    }

    [Theory]
    [MemberData(nameof(RefusalEvents))]
    [UpstreamTest(Prefix + "accepts $type events", Coverage = UpstreamCoverage.Covered, Commit = "6b97088662a270e9bb556c78d43b955869e7d9fc")]
    public void Accepts_type_events(string type, string json)
    {
        var chunk = OpenAIUpstream.Json(json);

        Assert.True(OpenAIResponsesSchema.IsValidChunk(chunk));
        Assert.False(OpenAIResponsesSchema.IsUnknownChunk(chunk));
        Assert.Equal(type, chunk.GetProperty("type").GetString());
    }

    private static string LocalShellItem(string type)
    {
        var status = type == "response.output_item.added" ? "in_progress" : "completed";
        return $$"""
            { "id": "item_1", "type": "local_shell_call", "call_id": "call_1", "status": "{{status}}", "action": { "type": "exec", "command": ["ls"], "env": {} } }
            """;
    }
}
