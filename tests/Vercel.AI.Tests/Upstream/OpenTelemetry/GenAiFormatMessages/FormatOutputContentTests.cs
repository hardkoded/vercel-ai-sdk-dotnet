// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Formatter = Vercel.AI.OpenTelemetry.GenAiFormatMessages;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>formatOutputContent</c>.</summary>
public sealed class FormatOutputContentTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputContent::should preserve the order and boundaries of content parts",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public void Should_preserve_the_order_and_boundaries_of_content_parts()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"tool_call","parts":[{"content":"Summary of earlier work.","type":"text"},{"content":"I should look this up.","type":"reasoning"},{"content":"Looking it up now.","type":"text"},{"arguments":{"id":7},"id":"call-1","name":"lookup","type":"tool_call"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputContent(
                [
                    new GeneratedText("Summary of earlier work."),
                    new GeneratedReasoning("I should look this up."),
                    new GeneratedText("Looking it up now."),
                    new GeneratedToolCall("call-1", "lookup", "{\"id\":7}"),
                ],
                FinishReason.ToolCalls
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputContent::should format all content part types",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public void Should_format_all_content_part_types()
    {
        var file = Convert.FromBase64String("aW1hZ2U=");
        var toolCall = new GeneratedToolCall("call-2", "delete", "{}");
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"","type":"reasoning"},{"content":"aW1hZ2U=","mime_type":"image/png","modality":"image","type":"blob"},{"kind":"anthropic.compaction","type":"custom"},{"content":"aW1hZ2U=","mime_type":"image/png","modality":"image","type":"blob"},{"id":"call-1","response":{"hits":1},"type":"tool_call_response"},{"id":"call-3","response":"Error: search failed","type":"tool_call_response"},{"approval_id":"approval-1","reason":null,"tool_call_id":"call-2","type":"tool_approval_request"},{"approval_id":"approval-1","approved":false,"reason":"not allowed","type":"tool_approval_response"}],"role":"assistant"}]
            """,
            Formatter.FormatOutputContent(
                [
                    new GeneratedReasoning(""),
                    new GeneratedReasoningFile(file, "image/png"),
                    new GeneratedCustom("anthropic.compaction"),
                    new GeneratedFile(file, "image/png"),
                    new GeneratedSource("source-1", "https://example.com", null),
                    new GeneratedToolResult("call-1", "search", "{\"hits\":1}"),
                    new GeneratedToolResult("call-3", "search", "null", isError: true)
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
                FinishReason.Stop
            )
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatOutputContent::should skip empty text parts without provider metadata",
        Coverage = UpstreamCoverage.Covered,
        Commit = "7dc49985c58ae25456aa1f23c28f2f891266629f"
    )]
    public void Should_skip_empty_text_parts_without_provider_metadata()
    {
        var metadata = JsonDocument.Parse("{\"test\":{\"a\":1}}").RootElement.Clone();
        OtelJson.Equal(
            """
            [{"content":"","type":"text"}]
            """,
            Formatter.FormatOutputContent(
                [new GeneratedText(""), new GeneratedText("", providerMetadata: metadata)],
                FinishReason.Stop
            )[0]["parts"]
        );
    }
}
