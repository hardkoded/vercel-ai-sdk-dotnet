// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; provider-defined tools &gt; apply_patch</c>.</summary>
public sealed class ApplyPatchTests
{
    private const string Id = "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > provider-defined tools > apply_patch::";

    private static readonly ToolNameMapping Mapping = ToolNames.CreateToolNameMapping(
        new[] { new MappedTool("provider", "apply_patch", "openai.apply_patch") },
        new Dictionary<string, string> { ["openai.apply_patch"] = "apply_patch" });

    private static OpenAIResponsesLanguageModel.ResponsesInput Convert(string callId, string input, string itemId, bool store, string? output)
    {
        var messages = new List<ModelMessage>
        {
            new AssistantModelMessage(new AssistantContentPart[]
            {
                new AssistantToolCallPart(new GeneratedToolCall(callId, "apply_patch", input, OpenAIUpstream.Json("{\"openai\":{\"itemId\":\"" + itemId + "\"}}"))),
            }),
        };
        if (output != null)
        {
            messages.Add(new ToolModelMessage(callId, "apply_patch", output, false, "json"));
        }

        return OpenAIResponsesLanguageModel.ConvertInput(messages, "system", false, null, "openai", false, store, toolNameMapping: Mapping, hasApplyPatchTool: true);
    }

    private static void AssertInput(string expected, OpenAIResponsesLanguageModel.ResponsesInput result) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), result.Input), result.Input.ToJsonString());

    [Fact]
    [UpstreamTest(Id + "should convert apply_patch tool call into item reference with store: true", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_apply_patch_tool_call_into_item_reference_with_store_true()
    {
        var result = Convert(
            "call_INoksNAffcdh5UmRTWMLk1Ne",
            """{"callId":"call_INoksNAffcdh5UmRTWMLk1Ne","operation":{"type":"create_file","path":"index.html","diff":"+<!doctype html>\n+<html></html>"}}""",
            "apc_0d5dfb28a009b1ee0169713022c3f88195a70b253d2a8cf798",
            true,
            """{"status":"completed","output":"Created index.html"}""");

        AssertInput(
            """
            [
              {"type":"item_reference","id":"apc_0d5dfb28a009b1ee0169713022c3f88195a70b253d2a8cf798"},
              {"type":"apply_patch_call_output","call_id":"call_INoksNAffcdh5UmRTWMLk1Ne","status":"completed","output":"Created index.html"}
            ]
            """,
            result);
    }

    [Fact]
    [UpstreamTest(Id + "should convert apply_patch tool call to apply_patch_call with store: false", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_apply_patch_tool_call_to_apply_patch_call_with_store_false()
    {
        var result = Convert(
            "call_INoksNAffcdh5UmRTWMLk1Ne",
            """{"callId":"call_INoksNAffcdh5UmRTWMLk1Ne","operation":{"type":"create_file","path":"index.html","diff":"+<!doctype html>\n+<html></html>"}}""",
            "apc_0d5dfb28a009b1ee0169713022c3f88195a70b253d2a8cf798",
            false,
            """{"status":"completed","output":"Created index.html"}""");

        AssertInput(
            """
            [
              {
                "type":"apply_patch_call",
                "call_id":"call_INoksNAffcdh5UmRTWMLk1Ne",
                "id":"apc_0d5dfb28a009b1ee0169713022c3f88195a70b253d2a8cf798",
                "status":"completed",
                "operation":{"type":"create_file","path":"index.html","diff":"+<!doctype html>\n+<html></html>"}
              },
              {"type":"apply_patch_call_output","call_id":"call_INoksNAffcdh5UmRTWMLk1Ne","status":"completed","output":"Created index.html"}
            ]
            """,
            result);
    }

    [Fact]
    [UpstreamTest(Id + "should convert apply_patch tool call with update_file operation with store: false", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_apply_patch_tool_call_with_update_file_operation_with_store_false()
    {
        var result = Convert(
            "call_UpdateFile123",
            """{"callId":"call_UpdateFile123","operation":{"type":"update_file","path":"src/app.ts","diff":"-old line\n+new line"}}""",
            "apc_update_file_item_id",
            false,
            null);

        AssertInput(
            """
            [{
              "type":"apply_patch_call",
              "call_id":"call_UpdateFile123",
              "id":"apc_update_file_item_id",
              "status":"completed",
              "operation":{"type":"update_file","path":"src/app.ts","diff":"-old line\n+new line"}
            }]
            """,
            result);
    }

    [Fact]
    [UpstreamTest(Id + "should convert apply_patch tool call with delete_file operation with store: false", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_apply_patch_tool_call_with_delete_file_operation_with_store_false()
    {
        var result = Convert(
            "call_DeleteFile456",
            """{"callId":"call_DeleteFile456","operation":{"type":"delete_file","path":"temp.txt"}}""",
            "apc_delete_file_item_id",
            false,
            null);

        AssertInput(
            """
            [{
              "type":"apply_patch_call",
              "call_id":"call_DeleteFile456",
              "id":"apc_delete_file_item_id",
              "status":"completed",
              "operation":{"type":"delete_file","path":"temp.txt"}
            }]
            """,
            result);
    }
}
