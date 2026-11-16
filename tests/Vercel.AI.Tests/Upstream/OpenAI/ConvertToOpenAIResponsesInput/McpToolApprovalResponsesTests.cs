// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.ConvertToOpenAIResponsesInput;

/// <summary>Port of <c>convert-to-openai-responses-input.test.ts</c> &gt; <c>convertToOpenAIResponsesInput &gt; MCP tool approval responses</c>.</summary>
public sealed class McpToolApprovalResponsesTests
{
    private static JsonArray Convert(
        IReadOnlyList<ModelMessage> prompt,
        bool store,
        bool hasConversation = false,
        bool hasPreviousResponseId = false
    ) =>
        OpenAIResponsesLanguageModel
            .ConvertInput(
                prompt,
                "system",
                false,
                null,
                "openai",
                false,
                store,
                hasConversation: hasConversation,
                hasPreviousResponseId: hasPreviousResponseId
            )
            .Input;

    private static void AssertJson(string expected, JsonNode? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual?.ToJsonString());

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should not reference an MCP approval request from a previous response",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_reference_an_MCP_approval_request_from_a_previous_response()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "mcp-approval-previous-response",
                    true,
                    providerExecuted: true
                ),
            },
            store: true,
            hasPreviousResponseId: true
        );

        AssertJson(
            """[{"type":"mcp_approval_response","approval_request_id":"mcp-approval-previous-response","approve":true}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should not reference an MCP approval request from a conversation",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_reference_an_MCP_approval_request_from_a_conversation()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "mcp-approval-conversation",
                    false,
                    providerExecuted: true
                ),
            },
            store: true,
            hasConversation: true
        );

        AssertJson(
            """[{"type":"mcp_approval_response","approval_request_id":"mcp-approval-conversation","approve":false}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should convert approved tool-approval-response to mcp_approval_response with store: true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_approved_tool_approval_response_to_mcp_approval_response_with_store_true()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "mcp-approval-123",
                    true,
                    providerExecuted: true
                ),
            },
            store: true
        );

        AssertJson(
            """[{"id":"mcp-approval-123","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"mcp-approval-123","approve":true}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should convert denied tool-approval-response to mcp_approval_response with store: true",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_denied_tool_approval_response_to_mcp_approval_response_with_store_true()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "mcp-approval-456",
                    false,
                    providerExecuted: true
                ),
            },
            store: true
        );

        AssertJson(
            """[{"id":"mcp-approval-456","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"mcp-approval-456","approve":false}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should convert tool-approval-response to mcp_approval_response without item_reference when store: false",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_convert_tool_approval_response_to_mcp_approval_response_without_item_reference_when_store_false()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "mcp-approval-789",
                    true,
                    providerExecuted: true
                ),
            },
            store: false
        );

        AssertJson(
            """[{"type":"mcp_approval_response","approval_request_id":"mcp-approval-789","approve":true}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should skip duplicate tool-approval-response with same approvalId",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_duplicate_tool_approval_response_with_same_approvalId()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "duplicate-approval",
                    true,
                    providerExecuted: true
                ),
                new ToolApprovalResponseModelMessage(
                    "duplicate-approval",
                    true,
                    providerExecuted: true
                ),
            },
            store: true
        );

        AssertJson(
            """[{"id":"duplicate-approval","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"duplicate-approval","approve":true}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should handle multiple different tool-approval-responses",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_multiple_different_tool_approval_responses()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage("approval-1", true, providerExecuted: true),
                new ToolApprovalResponseModelMessage("approval-2", false, providerExecuted: true),
            },
            store: true
        );

        AssertJson(
            """[{"id":"approval-1","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"approval-1","approve":true},{"id":"approval-2","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"approval-2","approve":false}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should skip execution-denied output when it has approvalId in providerOptions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_skip_execution_denied_output_when_it_has_approvalId_in_providerOptions()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "denied-approval",
                    false,
                    providerExecuted: true
                ),
                new ToolModelMessage(
                    "call-123",
                    "mcp_tool",
                    "User denied the tool execution",
                    false,
                    "execution-denied"
                )
                {
                    OutputProviderOptions = JsonDocument
                        .Parse("""{"openai":{"approvalId":"denied-approval"}}""")
                        .RootElement.Clone(),
                },
            },
            store: true
        );

        AssertJson(
            """[{"id":"denied-approval","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"denied-approval","approve":false}]""",
            result
        );
    }

    [Fact]
    [UpstreamTest(
        "packages/openai/src/responses/convert-to-openai-responses-input.test.ts::convertToOpenAIResponsesInput > MCP tool approval responses::should handle tool-approval-response mixed with regular tool results",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_handle_tool_approval_response_mixed_with_regular_tool_results()
    {
        var result = Convert(
            new ModelMessage[]
            {
                new ToolApprovalResponseModelMessage(
                    "approval-for-mcp",
                    true,
                    providerExecuted: true
                ),
                new ToolModelMessage(
                    "regular-call-1",
                    "calculator",
                    """{"result":42}""",
                    false,
                    "json"
                ),
            },
            store: true
        );

        AssertJson(
            """[{"id":"approval-for-mcp","type":"item_reference"},{"type":"mcp_approval_response","approval_request_id":"approval-for-mcp","approve":true},{"type":"function_call_output","call_id":"regular-call-1","output":"{\"result\":42}"}]""",
            result
        );
    }
}
