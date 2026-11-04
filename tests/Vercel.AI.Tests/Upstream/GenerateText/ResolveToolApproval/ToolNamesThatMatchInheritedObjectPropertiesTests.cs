// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using static Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval.ResolveToolApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

/// <summary>Port of <c>resolve-tool-approval.test.ts</c> &gt; <c>resolveToolApproval</c> &gt; <c>tool names that match inherited object properties</c>.</summary>
public sealed class ToolNamesThatMatchInheritedObjectPropertiesTests
{
    private const string Id = "packages/ai/src/generate-text/resolve-tool-approval.test.ts::resolveToolApproval > tool names that match inherited object properties::";

    [Theory]
    [InlineData("constructor")]
    [InlineData("toString")]
    [InlineData("valueOf")]
    [UpstreamTest(Id + "does not execute a tool named \"${toolName}\" when the fallback is denied", Coverage = UpstreamCoverage.Covered)]
    public async Task Does_not_execute_a_tool_named_when_the_fallback_is_denied(string toolName)
    {
        var result = await ResolveAsync(
            new[] { WeatherTool(toolName) },
            // simulates the total map produced by wrapMcpTools
            new ToolApprovalConfiguration { { toolName, "denied" } },
            CreateToolCall(toolName));

        AssertStatus(result, "denied");
    }

    [Theory]
    [InlineData("constructor")]
    [InlineData("toString")]
    [InlineData("valueOf")]
    [UpstreamTest(Id + "pauses a tool named \"${toolName}\" for approval when the fallback is user-approval", Coverage = UpstreamCoverage.Covered)]
    public async Task Pauses_a_tool_named_for_approval_when_the_fallback_is_user_approval(string toolName)
    {
        var result = await ResolveAsync(
            new[] { WeatherTool(toolName) },
            new ToolApprovalConfiguration { { toolName, "user-approval" } },
            CreateToolCall(toolName));

        AssertStatus(result, "user-approval");
    }

    [Theory]
    [InlineData("constructor")]
    [InlineData("toString")]
    [InlineData("valueOf")]
    [UpstreamTest(Id + "treats an unconfigured tool named \"${toolName}\" as not-applicable rather than reading the inherited property", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_an_unconfigured_tool_named_as_not_applicable_rather_than_reading_the_inherited_property(string toolName)
    {
        var result = await ResolveAsync(
            new[] { WeatherTool(toolName) },
            // plain map that does NOT list the colliding name
            new ToolApprovalConfiguration { { "weather", "denied" } },
            CreateToolCall(toolName));

        AssertStatus(result, "not-applicable");
    }
}
