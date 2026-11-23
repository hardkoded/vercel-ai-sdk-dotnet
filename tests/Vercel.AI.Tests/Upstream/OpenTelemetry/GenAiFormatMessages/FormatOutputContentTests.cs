// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

public sealed class FormatOutputContentTests
{
    private const string Test =
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputContent::";

    [Fact]
    [UpstreamTest(
        Test + "should preserve the order and boundaries of content parts",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_preserve_the_order_and_boundaries_of_content_parts()
    {
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"tool_call","parts":[
              {"content":"Summary of earlier work.","type":"text"},
              {"content":"I should look this up.","type":"reasoning"},
              {"content":"Looking it up now.","type":"text"},
              {"arguments":{"id":7},"id":"call-1","name":"lookup","type":"tool_call"}
            ],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputContent(
                [
                    new GeneratedText("Summary of earlier work."),
                    new GeneratedReasoning("I should look this up."),
                    new GeneratedText("Looking it up now."),
                    new GeneratedToolCall("call-1", "lookup", """{"id":7}"""),
                ],
                "tool-calls"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should format all content part types",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_all_content_part_types()
    {
        var image = Convert.FromBase64String("aW1hZ2U=");
        var toolCall = new GeneratedToolCall("call-2", "delete", "{}");
        SemConvJson.AssertEqual(
            """
            [{"finish_reason":"stop","parts":[
              {"content":"","type":"reasoning"},
              {"content":"aW1hZ2U=","mime_type":"image/png","modality":"image","type":"blob"},
              {"kind":"anthropic.compaction","type":"custom"},
              {"content":"aW1hZ2U=","mime_type":"image/png","modality":"image","type":"blob"},
              {"id":"call-1","response":{"hits":1},"type":"tool_call_response"},
              {"id":"call-3","response":"Error: search failed","type":"tool_call_response"},
              {"approval_id":"approval-1","tool_call_id":"call-2","type":"tool_approval_request"},
              {"approval_id":"approval-1","approved":false,"reason":"not allowed","type":"tool_approval_response"}
            ],"role":"assistant"}]
            """,
            GenAiConventions.FormatOutputContent(
                [
                    new GeneratedReasoning(""),
                    new GeneratedReasoningFile(image, "image/png"),
                    new GeneratedCustom("anthropic.compaction"),
                    new GeneratedFile(image, "image/png"),
                    new GeneratedSource("source-1", "https://example.com", null),
                    new GeneratedToolResult("call-1", "search", """{"hits":1}""")
                    {
                        ProviderExecuted = true,
                    },
                    new GeneratedToolResult("call-3", "search", "", isError: true)
                    {
                        Error = new JsError("search failed"),
                        ProviderExecuted = false,
                    },
                    new GeneratedToolApprovalRequest("approval-1", "call-2")
                    {
                        ToolCall = toolCall,
                    },
                    new GeneratedToolApprovalResponse("approval-1", toolCall, false, "not allowed"),
                ],
                "stop"
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Test + "should skip empty text parts without provider metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_empty_text_parts_without_provider_metadata()
    {
        using var metadata = System.Text.Json.JsonDocument.Parse("""{"test":{"a":1}}""");
        var message = GenAiConventions.FormatOutputContent(
            [new GeneratedText(""), new GeneratedText("", null, metadata.RootElement.Clone())],
            "stop"
        )[0];
        SemConvJson.AssertParts("""[{"content":"","type":"text"}]""", message.Parts);
    }
}
