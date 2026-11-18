// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval.ResolveToolApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

/// <summary>Port of <c>resolve-tool-approval.test.ts</c> &gt; <c>resolveToolApproval</c> &gt; <c>GenericToolApprovalFunction (toolApproval as a function)</c>.</summary>
public sealed class GenericToolApprovalFunctionToolApprovalAsAFunctionTests
{
    private const string Id =
        "packages/ai/src/generate-text/resolve-tool-approval.test.ts::resolveToolApproval > GenericToolApprovalFunction (toolApproval as a function)::";

    [Fact]
    [UpstreamTest(
        Id + "invokes the generic function and normalizes a string status",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Invokes_the_generic_function_and_normalizes_a_string_status()
    {
        var genericCalls = 0;
        var toolDefinedCalls = 0;
        var tool = WeatherTool(
            needsApprovalFunction: (_, _) =>
            {
                toolDefinedCalls++;
                return Task.FromResult(true);
            }
        );

        var result = await ResolveAsync(
            new[] { tool },
            new ToolApprovalConfiguration(_ =>
            {
                genericCalls++;
                return Task.FromResult<ToolApprovalStatus?>("denied");
            })
        );

        AssertStatus(result, "denied");
        Assert.Equal(1, genericCalls);
        Assert.Equal(0, toolDefinedCalls);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "passes toolCall, tools, toolsContext, messages, and runtimeContext to the generic function",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_toolCall_tools_toolsContext_messages_and_runtimeContext_to_the_generic_function()
    {
        var tools = new[] { WeatherTool() };
        var toolCall = CreateToolCall();
        var toolsContext = new Dictionary<string, JsonElement>
        {
            ["weather"] = Json("{\"requestId\":\"req-1\"}"),
        };
        var runtimeContext = new Dictionary<string, object?> { ["tenantId"] = "t-1" };
        var calls = new List<GenericToolApprovalOptions>();

        await ResolveAsync(
            tools,
            new ToolApprovalConfiguration(options =>
            {
                calls.Add(options);
                return Task.FromResult<ToolApprovalStatus?>("not-applicable");
            }),
            toolCall,
            toolsContext: toolsContext,
            runtimeContext: runtimeContext
        );

        var call = Assert.Single(calls);
        Assert.Same(toolCall, call.ToolCall);
        Assert.Same(tools, call.Tools);
        Assert.Same(Messages, call.Messages);
        Assert.Same(toolsContext, call.ToolsContext);
        Assert.Same(runtimeContext, call.RuntimeContext);
    }

    [Fact]
    [UpstreamTest(
        Id + "resolves a Promise returned by the generic function",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Resolves_a_Promise_returned_by_the_generic_function()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(async _ =>
            {
                await Task.Yield();
                return "user-approval";
            })
        );

        AssertStatus(result, "user-approval");
    }

    [Fact]
    [UpstreamTest(
        Id + "treats undefined returned by the generic function as not-applicable",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Treats_undefined_returned_by_the_generic_function_as_not_applicable()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(_ => Task.FromResult<ToolApprovalStatus?>(null))
        );

        AssertStatus(result, "not-applicable");
    }

    [Fact]
    [UpstreamTest(
        Id + "treats Promise undefined returned by the generic function as not-applicable",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Treats_Promise_undefined_returned_by_the_generic_function_as_not_applicable()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(async _ =>
            {
                await Task.Yield();
                return null;
            })
        );

        AssertStatus(result, "not-applicable");
    }

    [Fact]
    [UpstreamTest(
        Id + "passes through an object status including reason from the generic function",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_through_an_object_status_including_reason_from_the_generic_function()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(_ =>
                Task.FromResult<ToolApprovalStatus?>(
                    new ToolApprovalStatus("denied", "policy block")
                )
            )
        );

        AssertStatus(result, "denied", "policy block");
    }

    [Fact]
    [UpstreamTest(
        Id + "passes the same messages and toolsContext references to the generic function",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_the_same_messages_and_toolsContext_references_to_the_generic_function()
    {
        var modelMessages = new ModelMessage[]
        {
            new UserModelMessage("first"),
            new AssistantModelMessage("second", null, null),
        };
        var toolsContext = new Dictionary<string, JsonElement>
        {
            ["weather"] = Json("{\"scope\":\"read\"}"),
            ["otherTool"] = Json("{\"flag\":true}"),
        };
        var runtimeContext = new Dictionary<string, object?> { ["traceId"] = "trace-1" };
        var calls = new List<GenericToolApprovalOptions>();

        await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(options =>
            {
                calls.Add(options);
                return Task.FromResult<ToolApprovalStatus?>("not-applicable");
            }),
            messages: modelMessages,
            toolsContext: toolsContext,
            runtimeContext: runtimeContext
        );

        var call = Assert.Single(calls);
        Assert.Same(modelMessages, call.Messages);
        Assert.Same(toolsContext, call.ToolsContext);
        Assert.Same(runtimeContext, call.RuntimeContext);
    }
}
