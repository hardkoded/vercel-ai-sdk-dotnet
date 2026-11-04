// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals.ValidateApprovedToolApprovalsTests;
using static Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals.ValidateToolApprovalsSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ValidateToolApprovals;

/// <summary>Port of <c>validate-tool-approvals.test.ts</c> &gt; <c>validateApprovedToolApprovals</c> &gt; <c>signature verification (experimental_toolApprovalSecret)</c>.</summary>
public sealed class SignatureVerificationExperimentalToolApprovalSecretTests
{
    private const string Id = "packages/ai/src/generate-text/validate-tool-approvals.test.ts::validateApprovedToolApprovals > signature verification (experimental_toolApprovalSecret)::";

    private const string Secret = "test-secret-for-signature";

    private const string CountSchema = "{\"type\":\"object\",\"properties\":{\"count\":{\"type\":\"number\"}},\"required\":[\"count\"]}";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [UpstreamTest(Id + "should preserve signed transformed input or reject it when schema input is missing (schema input: %s)", Coverage = UpstreamCoverage.Partial, Note = TransformNote)]
    public async Task Should_preserve_signed_transformed_input_or_reject_it_when_schema_input_is_missing(bool hasInputSchemaInput)
    {
        var tool = ExecutableTool("tool1", CountSchema);
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "tool1", Json("{\"count\":2}"));
        var approval = CreateApproval("tool1", "{\"count\":2}", signature, hasInputSchemaInput ? "{\"count\":1}" : null);
        var policyCalls = 0;
        SingleToolApprovalOptions? policyOptions = null;
        System.Text.Json.JsonElement policyInput = default;

        var result = await ValidateAsync(
            approval,
            new[] { tool },
            new ToolApprovalConfiguration
            {
                {
                    "tool1",
                    (input, options) =>
                    {
                        policyCalls++;
                        policyInput = input;
                        policyOptions = options;
                        return Task.FromResult<ToolApprovalStatus?>("user-approval");
                    }
                },
            },
            Secret,
            Refine("tool1", input => Json("{\"count\":" + (input.GetProperty("count").GetInt32() + 1) + "}")));

        Assert.Equal("{\"count\":2}", approval.ToolCall.Input.GetRawText());
        if (hasInputSchemaInput)
        {
            Assert.Same(approval, Assert.Single(result.ApprovedToolApprovals));
            Assert.Empty(result.InvalidToolApprovals);
            Assert.Equal(1, policyCalls);
            Assert.Equal("{\"count\":2}", policyInput.GetRawText());
            Assert.NotNull(policyOptions);
        }
        else
        {
            Assert.Empty(result.ApprovedToolApprovals);
            var invalid = Assert.Single(result.InvalidToolApprovals);
            Assert.Matches("does not match the validated schema output", invalid.Error.Cause!.Message);
            Assert.Equal(0, policyCalls);
        }
    }

    [Fact]
    [UpstreamTest(Id + "should pass when the signature is valid", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_pass_when_the_signature_is_valid()
    {
        var input = Json("{\"value\":\"test\"}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-signed", "call-1", "tool1", input);
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}", signature, approvalId: "approval-signed");

        var result = await ValidateAsync(approval, new[] { ExecutableTool() }, secret: Secret);

        Assert.Single(result.ApprovedToolApprovals);
    }

    [Fact]
    [UpstreamTest(Id + "should not transform a signed input after verification when schema input metadata is missing", Coverage = UpstreamCoverage.Partial, Note = TransformNote)]
    public async Task Should_not_transform_a_signed_input_after_verification_when_schema_input_metadata_is_missing()
    {
        var tool = ExecutableTool("tool1", "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\"}},\"required\":[\"value\"]}");
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-signed", "call-1", "tool1", Json("{\"value\":1}"));
        var approval = CreateApproval("tool1", "{\"value\":1}", signature, approvalId: "approval-signed");

        var result = await ValidateAsync(
            approval,
            new[] { tool },
            secret: Secret,
            refine: Refine("tool1", input => Json("{\"value\":" + (input.GetProperty("value").GetInt32() + 1) + "}")));

        Assert.Empty(result.ApprovedToolApprovals);
        var invalid = Assert.Single(result.InvalidToolApprovals);
        Assert.Equal("{\"value\":1}", invalid.Approval.ToolCall.Input.GetRawText());
        Assert.Matches("does not match the validated schema output", invalid.Error.Cause!.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when the signature is missing and secret is configured", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_the_signature_is_missing_and_secret_is_configured()
    {
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}");

        var error = await Assert.ThrowsAsync<InvalidToolApprovalSignatureError>(() => ValidateAsync(approval, new[] { ExecutableTool() }, secret: Secret));

        Assert.Matches("missing signature", error.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should throw when the signature is invalid (tampered input)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_throw_when_the_signature_is_invalid_tampered_input()
    {
        var signature = ToolApprovalSignatures.SignToolApproval(Secret, "approval-1", "call-1", "tool1", Json("{\"value\":\"original\"}"));
        var approval = CreateApproval("tool1", "{\"value\":\"tampered\"}", signature);

        var error = await Assert.ThrowsAsync<InvalidToolApprovalSignatureError>(() => ValidateAsync(approval, new[] { ExecutableTool() }, secret: Secret));

        Assert.Matches("invalid signature", error.Message);
    }

    [Fact]
    [UpstreamTest(Id + "should ignore signature when no secret is configured (forward compatible)", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_ignore_signature_when_no_secret_is_configured_forward_compatible()
    {
        var approval = CreateApproval("tool1", "{\"value\":\"test\"}", "some-random-signature");

        var result = await ValidateAsync(approval, new[] { ExecutableTool() });

        Assert.Single(result.ApprovedToolApprovals);
    }
}
