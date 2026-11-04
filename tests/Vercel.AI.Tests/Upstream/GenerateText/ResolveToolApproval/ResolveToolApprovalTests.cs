// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using static Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval.ResolveToolApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

/// <summary>Port of <c>resolve-tool-approval.test.ts</c> &gt; <c>resolveToolApproval</c>.</summary>
public sealed class ResolveToolApprovalTests
{
    private const string Id = "packages/ai/src/generate-text/resolve-tool-approval.test.ts::resolveToolApproval::";

    private const string ContextNote = "The port has no tool context or runtime context, so toolContext and runtimeContext are not checked.";

    [Fact]
    [UpstreamTest(Id + "returns not-applicable when neither user-defined nor tool-defined approval is configured", Coverage = UpstreamCoverage.Covered)]
    public async Task Returns_not_applicable_when_neither_user_defined_nor_tool_defined_approval_is_configured()
    {
        var result = await ResolveAsync(new[] { WeatherTool() }, null);

        AssertStatus(result, "not-applicable");
    }

    [Fact]
    [UpstreamTest(Id + "normalizes a user-defined static string approval value before checking tool-defined approval", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_a_user_defined_static_string_approval_value_before_checking_tool_defined_approval()
    {
        var toolDefinedCalls = 0;
        var tool = WeatherTool(needsApprovalFunction: (_, _) =>
        {
            toolDefinedCalls++;
            return Task.FromResult(true);
        });

        var result = await ResolveAsync(new[] { tool }, new ToolApprovalConfiguration { { "weather", "denied" } });

        AssertStatus(result, "denied");
        Assert.Equal(0, toolDefinedCalls);
    }

    [Fact]
    [UpstreamTest(Id + "passes through a user-defined object approval value including its reason", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_a_user_defined_object_approval_value_including_its_reason()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration { { "weather", new ToolApprovalStatus("denied", "blocked by policy") } });

        AssertStatus(result, "denied", "blocked by policy");
    }

    [Fact]
    [UpstreamTest(Id + "uses a user-defined approval callback before tool-defined approval", Coverage = UpstreamCoverage.Partial, Note = ContextNote)]
    public async Task Uses_a_user_defined_approval_callback_before_tool_defined_approval()
    {
        var toolDefinedCalls = 0;
        var userDefined = new List<(JsonElement Input, SingleToolApprovalOptions Options)>();
        var tool = WeatherTool(needsApprovalFunction: (_, _) =>
        {
            toolDefinedCalls++;
            return Task.FromResult(true);
        });

        var result = await ResolveAsync(
            new[] { tool },
            new ToolApprovalConfiguration
            {
                {
                    "weather",
                    (input, options) =>
                    {
                        userDefined.Add((input, options));
                        return Task.FromResult<ToolApprovalStatus?>("approved");
                    }
                },
            });

        AssertStatus(result, "approved");
        var call = Assert.Single(userDefined);
        Assert.Equal("{\"city\":\"Berlin\"}", call.Input.GetRawText());
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.Same(Messages, call.Options.Messages);
        Assert.Equal(0, toolDefinedCalls);
    }

    [Fact]
    [UpstreamTest(Id + "passes through a reason returned by a user-defined approval callback", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_a_reason_returned_by_a_user_defined_approval_callback()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration
            {
                { "weather", (_, _) => Task.FromResult<ToolApprovalStatus?>(new ToolApprovalStatus("approved", "trusted internal tool")) },
            });

        AssertStatus(result, "approved", "trusted internal tool");
    }

    [Fact]
    [UpstreamTest(Id + "passes tool input and options to a user-defined approval callback without a context schema", Coverage = UpstreamCoverage.Partial, Note = ContextNote)]
    public async Task Passes_tool_input_and_options_to_a_user_defined_approval_callback_without_a_context_schema()
    {
        var userDefined = new List<(JsonElement Input, SingleToolApprovalOptions Options)>();

        await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration
            {
                {
                    "weather",
                    (input, options) =>
                    {
                        userDefined.Add((input, options));
                        return Task.FromResult<ToolApprovalStatus?>("user-approval");
                    }
                },
            });

        var call = Assert.Single(userDefined);
        Assert.Equal("{\"city\":\"Berlin\"}", call.Input.GetRawText());
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.Same(Messages, call.Options.Messages);
    }

    [Fact]
    [UpstreamTest(Id + "normalizes a string status returned by a user-defined approval callback", Coverage = UpstreamCoverage.Covered)]
    public async Task Normalizes_a_string_status_returned_by_a_user_defined_approval_callback()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration
            {
                { "weather", (_, _) => Task.FromResult<ToolApprovalStatus?>("user-approval") },
            });

        AssertStatus(result, "user-approval");
    }

    [Fact]
    [UpstreamTest(Id + "maps tool-defined boolean approval to the public approval state", Coverage = UpstreamCoverage.Covered)]
    public async Task Maps_tool_defined_boolean_approval_to_the_public_approval_state()
    {
        var approvedToolResult = await ResolveAsync(new[] { WeatherTool(needsApproval: true) }, null);
        var notApplicableToolResult = await ResolveAsync(new[] { WeatherTool(needsApproval: false) }, null);

        AssertStatus(approvedToolResult, "user-approval");
        AssertStatus(notApplicableToolResult, "not-applicable");
    }

    [Fact]
    [UpstreamTest(Id + "passes tool input and validated context to a tool-defined approval callback", Coverage = UpstreamCoverage.Partial, Note = ContextNote)]
    public async Task Passes_tool_input_and_validated_context_to_a_tool_defined_approval_callback()
    {
        var toolDefined = new List<(JsonElement Input, ToolNeedsApprovalOptions Options)>();
        var tool = WeatherTool(needsApprovalFunction: (input, options) =>
        {
            toolDefined.Add((input, options));
            return Task.FromResult(true);
        });

        var result = await ResolveAsync(new[] { tool }, null);

        AssertStatus(result, "user-approval");
        var call = Assert.Single(toolDefined);
        Assert.Equal("{\"city\":\"Berlin\"}", call.Input.GetRawText());
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.Same(Messages, call.Options.Messages);
    }
}
