// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals.ValidateToolApprovalsSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals;

/// <summary>Port of <c>validate-tool-approvals.test.ts</c> &gt; <c>validateApprovedToolApprovals</c>.</summary>
public sealed class ValidateApprovedToolApprovalsTests
{
    private const string Id = "packages/ai/src/generate-text/validate-tool-approvals.test.ts::validateApprovedToolApprovals::";

    [Fact]
    [UpstreamTest(Id + "should keep approvals whose input matches the tool input schema", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_keep_approvals_whose_input_matches_the_tool_input_schema()
    {
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}");

        var result = await ValidateAsync(approval, new[] { ExecutableTool() });

        Assert.Single(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
    }

    [Fact]
    [UpstreamTest(Id + "should return invalid approved tool inputs as recoverable errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_invalid_approved_tool_inputs_as_recoverable_errors()
    {
        // Forged input: `value` should be a string.
        var approval = CreateApproval("tool1", "{\"value\":42}");

        var result = await ValidateAsync(approval, new[] { ExecutableTool() });

        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
        var invalid = Assert.Single(result.InvalidToolApprovals);
        Assert.Same(approval.ToolCall, invalid.Approval.ToolCall);
        Assert.Equal("AI_InvalidToolInputError", invalid.Error.ErrorName);
    }

    [Fact]
    [UpstreamTest(Id + "should return extra forged properties as invalid tool input errors", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_extra_forged_properties_as_invalid_tool_input_errors()
    {
        var tool = ExecutableTool("deleteFile", "{\"type\":\"object\",\"properties\":{\"path\":{\"type\":\"string\"}},\"required\":[\"path\"],\"additionalProperties\":false}");
        var approval = CreateApproval("deleteFile", "{\"path\":\"/app/.env\",\"extra\":\"forged\"}");

        var result = await ValidateAsync(approval, new[] { tool });

        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Matches("Invalid input for tool deleteFile", result.InvalidToolApprovals[0].Error.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should keep approvals whose schema input transforms to the approved input", Coverage = UpstreamCoverage.Partial, Note = TransformNote)]
    public async Task Should_keep_approvals_whose_schema_input_transforms_to_the_approved_input()
    {
        var tool = ExecutableTool("tool1", "{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"string\"}},\"required\":[\"count\"]}");
        var approval = CreateApproval("tool1", "{\"count\":3}", inputSchemaInputJson: "{\"count\":\"3\"}");

        var result = await ValidateAsync(
            approval,
            new[] { tool },
            refine: Refine("tool1", input => Json("{\"count\":" + int.Parse(input.GetProperty("count").GetString()!, System.Globalization.CultureInfo.InvariantCulture) + "}")));

        Assert.Single(result.ApprovedToolApprovals);
        Assert.Empty(result.InvalidToolApprovals);
    }

    [Fact]
    [UpstreamTest(Id + "should reject approvals when missing schema input would change the approved input", Coverage = UpstreamCoverage.Partial, Note = TransformNote)]
    public async Task Should_reject_approvals_when_missing_schema_input_would_change_the_approved_input()
    {
        var tool = ExecutableTool("tool1", "{\"type\":\"object\",\"properties\":{\"raw\":{\"type\":\"string\"}},\"required\":[\"raw\"]}");
        var approval = CreateApproval("tool1", "{\"raw\":\"999\"}");

        var result = await ValidateAsync(
            approval,
            new[] { tool },
            refine: Refine("tool1", input => Json("{\"safe\":" + int.Parse(input.GetProperty("raw").GetString()!, System.Globalization.CultureInfo.InvariantCulture) + "}")));

        Assert.Empty(result.ApprovedToolApprovals);
        var invalid = Assert.Single(result.InvalidToolApprovals);
        Assert.Same(approval.ToolCall, invalid.Approval.ToolCall);
        Assert.Equal("AI_InvalidToolInputError", invalid.Error.ErrorName);
        Assert.Matches("does not match the validated schema output", invalid.Error.Cause!.Message);
        Assert.Equal("{\"raw\":\"999\"}", approval.ToolCall.Input.GetRawText());
    }

    [Fact]
    [UpstreamTest(Id + "should keep approvals whose schema input reshapes to the approved input", Coverage = UpstreamCoverage.Partial, Note = TransformNote)]
    public async Task Should_keep_approvals_whose_schema_input_reshapes_to_the_approved_input()
    {
        var tool = ExecutableTool("tool1", "{\"type\":\"object\",\"properties\":{\"raw\":{\"type\":\"string\"}},\"required\":[\"raw\"]}");
        var approval = CreateApproval("tool1", "{\"count\":3}", inputSchemaInputJson: "{\"raw\":\"3\"}");

        var result = await ValidateAsync(
            approval,
            new[] { tool },
            refine: Refine("tool1", input => Json("{\"count\":" + int.Parse(input.GetProperty("raw").GetString()!, System.Globalization.CultureInfo.InvariantCulture) + "}")));

        Assert.Single(result.ApprovedToolApprovals);
        Assert.Empty(result.InvalidToolApprovals);
    }

    [Fact]
    [UpstreamTest(Id + "should reapply input refinement before comparing the approved input", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_reapply_input_refinement_before_comparing_the_approved_input()
    {
        var approval = CreateApproval("tool1", "{\"value\":\"trimmed\"}", inputSchemaInputJson: "{\"value\":\" trimmed \"}");

        var result = await ValidateAsync(
            approval,
            new[] { ExecutableTool() },
            refine: Refine("tool1", input => Json("{\"value\":" + JsonSerializer.Serialize(input.GetProperty("value").GetString()!.Trim()) + "}")));

        Assert.Single(result.ApprovedToolApprovals);
        Assert.Empty(result.InvalidToolApprovals);
    }

    [Fact]
    [UpstreamTest(Id + "should reject approvals whose transformed schema output was changed", Coverage = UpstreamCoverage.Partial, Note = TransformNote)]
    public async Task Should_reject_approvals_whose_transformed_schema_output_was_changed()
    {
        var tool = ExecutableTool("tool1", "{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"string\"}},\"required\":[\"count\"]}");
        var approval = CreateApproval("tool1", "{\"count\":4}", inputSchemaInputJson: "{\"count\":\"3\"}");

        var result = await ValidateAsync(
            approval,
            new[] { tool },
            refine: Refine("tool1", input => Json("{\"count\":" + int.Parse(input.GetProperty("count").GetString()!, System.Globalization.CultureInfo.InvariantCulture) + "}")));

        Assert.Empty(result.ApprovedToolApprovals);
        var invalid = Assert.Single(result.InvalidToolApprovals);
        Assert.Same(approval.ToolCall, invalid.Approval.ToolCall);
        Assert.Equal("AI_InvalidToolInputError", invalid.Error.ErrorName);
        Assert.Matches("does not match the validated schema output", invalid.Error.Cause!.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should move approvals to denied when the approval policy denies them", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_move_approvals_to_denied_when_the_approval_policy_denies_them()
    {
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}");

        // server-side policy denies the call regardless of the client's response
        var result = await ValidateAsync(approval, new[] { ExecutableTool() }, new ToolApprovalConfiguration { { "tool1", "denied" } });

        Assert.Empty(result.ApprovedToolApprovals);
        var denied = Assert.Single(result.DeniedToolApprovals);
        Assert.False(denied.ApprovalResponse.Approved);
    }

    [Fact]
    [UpstreamTest(Id + "should carry the policy reason into the denial response", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_carry_the_policy_reason_into_the_denial_response()
    {
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}");

        var result = await ValidateAsync(
            approval,
            new[] { ExecutableTool() },
            new ToolApprovalConfiguration { { "tool1", new ToolApprovalStatus("denied", "policy changed") } });

        var denied = Assert.Single(result.DeniedToolApprovals);
        Assert.Equal("policy changed", denied.ApprovalResponse.Reason);
        Assert.False(denied.ApprovalResponse.Approved);
    }

    [Fact]
    [UpstreamTest(Id + "should re-run a function-based approval policy on the approved input", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_re_run_a_function_based_approval_policy_on_the_approved_input()
    {
        var calls = new List<(JsonElement Input, SingleToolApprovalOptions Options)>();
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}");

        // per-tool approval policy re-evaluated against the approved input
        var result = await ValidateAsync(
            approval,
            new[] { ExecutableTool() },
            new ToolApprovalConfiguration
            {
                {
                    "tool1",
                    (input, options) =>
                    {
                        calls.Add((input, options));
                        return Task.FromResult<ToolApprovalStatus?>("denied");
                    }
                },
            });

        var call = Assert.Single(calls);
        Assert.Equal("{\"value\":\"test\"}", call.Input.GetRawText());
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.Empty(result.ApprovedToolApprovals);
        Assert.Single(result.DeniedToolApprovals);
    }

    [Fact]
    [UpstreamTest(Id + "should pass through approvals for tools without an execute function (not validated)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_through_approvals_for_tools_without_an_execute_function_not_validated()
    {
        // no execute -> client-side tool, not run on the server
        var tool = Tool.Function("tool1", null, ValueSchema, null);
        var approval = CreateApproval("tool1", "{\"value\":42}");

        var result = await ValidateAsync(approval, new[] { tool });

        Assert.Single(result.ApprovedToolApprovals);
        Assert.Empty(result.DeniedToolApprovals);
    }

    internal static IReadOnlyDictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>> Refine(string toolName, Func<JsonElement, JsonElement> refine) =>
        new Dictionary<string, Func<JsonElement, CancellationToken, Task<JsonElement>>>
        {
            [toolName] = (input, _) => Task.FromResult(refine(input)),
        };
}
