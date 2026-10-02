// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Vercel.AI.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText;

public sealed class ToolApprovalSignatureTests
{
    private const string Secret = "test-secret-key-for-hmac-signing";

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should produce a valid signature that verifies", Coverage = UpstreamCoverage.Covered)]
    public void Signature_verifies()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", input);
        Assert.False(string.IsNullOrEmpty(signature));
        Assert.True(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "call-1", "deleteFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should reject when the approvalId is tampered", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_tampered_approval_id()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "tampered-id", "call-1", "deleteFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should reject when the toolCallId is tampered", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_tampered_tool_call_id()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "tampered-call", "deleteFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should reject when the toolName is tampered", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_tampered_tool_name()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "call-1", "readFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should reject when the input is tampered", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_tampered_input()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "call-1", "deleteFile", Json("{\"path\":\"/app/.env\"}")));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should reject when verified with a different secret", Coverage = UpstreamCoverage.Covered)]
    public void Rejects_a_different_secret()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature("different-secret", signature, "approval-1", "call-1", "deleteFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should produce the same signature for equivalent inputs with different key order", Coverage = UpstreamCoverage.Covered)]
    public void Key_order_does_not_change_the_signature()
    {
        var first = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", Json("{\"path\":\"/tmp/cache\",\"mode\":\"delete\"}"));
        var second = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "deleteFile", Json("{\"mode\":\"delete\",\"path\":\"/tmp/cache\"}"));
        Assert.Equal(first, second);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should not collide when a newline in toolName is retupled into toolCallId", Coverage = UpstreamCoverage.Covered)]
    public void Newline_in_tool_name_does_not_collide()
    {
        var input = Json("{\"path\":\"/tmp/target\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "searchDocs\ndeleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "call-1\nsearchDocs", "deleteFile", input));
        var retupled = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1\nsearchDocs", "deleteFile", input);
        Assert.NotEqual(signature, retupled);
        Assert.True(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "call-1", "searchDocs\ndeleteFile", input));
        Assert.True(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, retupled, "approval-1", "call-1\nsearchDocs", "deleteFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should not collide across the %s delimiter", Coverage = UpstreamCoverage.Covered)]
    public void Delimiters_do_not_collide()
    {
        foreach (var delimiter in new[] { "\n", "\r", "\t", "\0", "\"", "\\" })
        {
            var input = Json("{\"path\":\"/tmp/target\"}");
            var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "alpha" + delimiter + "beta", input);
            Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, signature, "approval-1", "call-1" + delimiter + "alpha", "beta", input));
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should still verify a legacy newline-format signature when no field contains a newline", Coverage = UpstreamCoverage.Covered)]
    public void Verifies_a_legacy_signature()
    {
        var input = Json("{\"path\":\"/tmp/cache\"}");
        var legacy = SignLegacy("approval-1", "call-1", "deleteFile", input);
        Assert.True(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, legacy, "approval-1", "call-1", "deleteFile", input));
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/tool-approval-signature.test.ts::signToolApproval + verifyToolApprovalSignature::should not accept a legacy signature through the retupling collision", Coverage = UpstreamCoverage.Covered)]
    public void Legacy_signature_does_not_retuple()
    {
        var input = Json("{\"path\":\"/tmp/target\"}");
        var legacy = SignLegacy("approval-1", "call-1", "searchDocs\ndeleteFile", input);
        Assert.False(ToolApprovalSignatures.VerifyToolApprovalSignature(Secret, legacy, "approval-1", "call-1\nsearchDocs", "deleteFile", input));
    }

    private static string SignLegacy(string approvalId, string toolCallId, string toolName, JsonElement input)
    {
        var digest = CanonicalHash.HashCanonical(input);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        var payload = Encoding.UTF8.GetBytes(approvalId + "\n" + toolCallId + "\n" + toolName + "\n" + digest);
        return CanonicalHash.ToBase64Url(hmac.ComputeHash(payload));
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

public sealed class CollectToolApprovalsTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should not return any tool approvals when the last message is not a tool message", Coverage = UpstreamCoverage.Covered)]
    public void Ignores_a_user_ending()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(new[] { new ApprovalHistoryMessage("user", "Hello, world!") });
        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should ignore approval request without response", Coverage = UpstreamCoverage.Covered)]
    public void Ignores_a_request_without_a_response()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(new[]
        {
            Assistant(Call("call-1", "test-input"), Request("approval-id-1", "call-1")),
            new ApprovalHistoryMessage("tool", parts: Array.Empty<ApprovalHistoryPart>()),
        });
        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should return approved approval with approved response", Coverage = UpstreamCoverage.Covered)]
    public void Returns_an_approved_response()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(History(new ApprovalResponsePart("approval-id-1", true)));
        var approval = Assert.Single(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
        Assert.Equal("approval-id-1", approval.ApprovalRequest.ApprovalId);
        Assert.Equal("call-1", approval.ToolCall.ToolCallId);
        Assert.Equal("tool1", approval.ToolCall.ToolName);
        Assert.Equal("test-input", approval.ToolCall.Input.GetProperty("value").GetString());
        Assert.True(approval.ApprovalResponse.Approved);
        Assert.Null(approval.ExistingToolResult);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should return processed approval with approved response and tool result", Coverage = UpstreamCoverage.Covered)]
    public void Skips_an_approved_call_that_already_has_a_result()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(History(
            new ApprovalResponsePart("approval-id-1", true),
            new ApprovalToolResultPart("call-1", "tool1", "text", Json("{\"ignored\":true}"))));
        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should return denied approval with denied response", Coverage = UpstreamCoverage.Covered)]
    public void Returns_a_denied_response()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(History(new ApprovalResponsePart("approval-id-1", false, "test-reason")));
        Assert.Empty(result.ApprovedToolApprovals);
        var approval = Assert.Single(result.DeniedToolApprovals);
        Assert.False(approval.ApprovalResponse.Approved);
        Assert.Equal("test-reason", approval.ApprovalResponse.Reason);
        Assert.Equal("call-1", approval.ToolCall.ToolCallId);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should return denied approval with an execution-denied tool result", Coverage = UpstreamCoverage.Covered)]
    public void Keeps_a_denied_call_with_an_execution_denied_result()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(History(
            new ApprovalResponsePart("approval-id-1", false, "test-reason"),
            new ApprovalToolResultPart("call-1", "tool1", "execution-denied", reason: "test-reason")));
        var approval = Assert.Single(result.DeniedToolApprovals);
        Assert.NotNull(approval.ExistingToolResult);
        Assert.Equal("execution-denied", approval.ExistingToolResult!.OutputType);
        Assert.Equal("test-reason", approval.ExistingToolResult.Reason);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should ignore denied approval with a non-denial tool result", Coverage = UpstreamCoverage.Covered)]
    public void Skips_a_denied_call_that_already_has_a_normal_result()
    {
        var result = ToolApprovalCollection.CollectToolApprovals(History(
            new ApprovalResponsePart("approval-id-1", false, "test-reason"),
            new ApprovalToolResultPart("call-1", "tool1", "text", Json("\"test-output\""))));
        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should throw when approval response has unknown approvalId", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_an_unknown_approval_id()
    {
        var exception = Assert.Throws<InvalidToolApprovalException>(() => ToolApprovalCollection.CollectToolApprovals(History(new ApprovalResponsePart("unknown-approval-id", true))));
        Assert.Equal("Tool approval response references unknown approvalId: \"unknown-approval-id\"", exception.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should throw when referenced tool call does not exist", Coverage = UpstreamCoverage.Covered)]
    public void Throws_when_the_tool_call_is_missing()
    {
        var messages = new[]
        {
            new ApprovalHistoryMessage("assistant", parts: new ApprovalHistoryPart[] { new ApprovalRequestPart("approval-id-1", "call-that-does-not-exist") }),
            new ApprovalHistoryMessage("tool", parts: new ApprovalHistoryPart[] { new ApprovalResponsePart("approval-id-1", true) }),
        };
        var exception = Assert.Throws<ToolCallNotFoundForApprovalException>(() => ToolApprovalCollection.CollectToolApprovals(messages));
        Assert.Equal("Tool call \"call-that-does-not-exist\" not found for approval request \"approval-id-1\".", exception.Message);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should throw for an unknown approvalId that matches the inherited object property \"%s\"", Coverage = UpstreamCoverage.Covered)]
    public void Throws_for_inherited_property_names()
    {
        foreach (var approvalId in new[] { "toString", "constructor", "valueOf", "__proto__" })
        {
            var messages = new[]
            {
                Assistant(Call("call-1", "test-input")),
                new ApprovalHistoryMessage("tool", parts: new ApprovalHistoryPart[] { new ApprovalResponsePart(approvalId, true) }),
            };
            var exception = Assert.Throws<InvalidToolApprovalException>(() => ToolApprovalCollection.CollectToolApprovals(messages));
            Assert.Equal("Tool approval response references unknown approvalId: " + JsonSerializer.Serialize(approvalId), exception.Message);
        }
    }

    [Fact]
    [UpstreamTest("packages/ai/src/generate-text/collect-tool-approvals.test.ts::collectToolApprovals::should collect pending approvals and denied approvals with execution-denied results", Coverage = UpstreamCoverage.Covered)]
    public void Collects_pending_and_denied_approvals()
    {
        var calls = new List<ApprovalHistoryPart>();
        for (var i = 1; i <= 6; i++)
        {
            calls.Add(Call("call-approval-" + i, "test-input-" + i));
            calls.Add(Request("approval-id-" + i, "call-approval-" + i));
        }

        var toolParts = new List<ApprovalHistoryPart>
        {
            new ApprovalResponsePart("approval-id-1", true),
            new ApprovalResponsePart("approval-id-2", true),
            new ApprovalResponsePart("approval-id-3", false, "test-reason"),
            new ApprovalResponsePart("approval-id-4", false),
            new ApprovalResponsePart("approval-id-5", true),
            new ApprovalToolResultPart("call-approval-5", "tool1", "text", Json("\"test-output-5\"")),
            new ApprovalResponsePart("approval-id-6", false),
            new ApprovalToolResultPart("call-approval-6", "tool1", "execution-denied"),
        };
        var result = ToolApprovalCollection.CollectToolApprovals(new[]
        {
            new ApprovalHistoryMessage("assistant", parts: calls),
            new ApprovalHistoryMessage("tool", parts: toolParts),
        });

        Assert.Equal(new[] { "approval-id-1", "approval-id-2" }, result.ApprovedToolApprovals.Select(item => item.ApprovalRequest.ApprovalId));
        Assert.Equal(new[] { "approval-id-3", "approval-id-4", "approval-id-6" }, result.DeniedToolApprovals.Select(item => item.ApprovalRequest.ApprovalId));
        Assert.Equal("test-reason", result.DeniedToolApprovals[0].ApprovalResponse.Reason);
        Assert.Null(result.DeniedToolApprovals[0].ExistingToolResult);
        Assert.Null(result.DeniedToolApprovals[1].ApprovalResponse.Reason);
        Assert.Equal("execution-denied", result.DeniedToolApprovals[2].ExistingToolResult!.OutputType);
        Assert.Null(result.DeniedToolApprovals[2].ExistingToolResult!.Reason);
        Assert.Equal("test-input-1", result.ApprovedToolApprovals[0].ToolCall.Input.GetProperty("value").GetString());
    }

    private static ApprovalHistoryMessage[] History(params ApprovalHistoryPart[] toolParts)
    {
        return new[]
        {
            Assistant(Call("call-1", "test-input"), Request("approval-id-1", "call-1")),
            new ApprovalHistoryMessage("tool", parts: toolParts),
        };
    }

    private static ApprovalHistoryMessage Assistant(params ApprovalHistoryPart[] parts)
    {
        return new ApprovalHistoryMessage("assistant", parts: parts);
    }

    private static ApprovalToolCallPart Call(string id, string value)
    {
        return new ApprovalToolCallPart(id, "tool1", Json("{\"value\":\"" + value + "\"}"));
    }

    private static ApprovalRequestPart Request(string approvalId, string toolCallId)
    {
        return new ApprovalRequestPart(approvalId, toolCallId);
    }

    private static JsonElement Json(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
