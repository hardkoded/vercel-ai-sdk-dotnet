// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval.ResolveToolApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

/// <summary>Port of <c>resolve-tool-approval.test.ts</c> &gt; <c>resolveToolApproval</c> &gt; <c>GenericToolApprovalFunction (toolApproval as a function)</c>.</summary>
public sealed class GenericToolApprovalFunctionToolApprovalAsAFunctionTests
{
    private const string Id = "packages/ai/src/generate-text/resolve-tool-approval.test.ts::resolveToolApproval > GenericToolApprovalFunction (toolApproval as a function)::";

    private const string ContextNote = "The port has no tool context or runtime context, so toolsContext and runtimeContext are not checked.";

    [Fact]
    [UpstreamTest(Id + "invokes the generic function and normalizes a string status", Coverage = UpstreamCoverage.Covered)]
    public async Task Invokes_the_generic_function_and_normalizes_a_string_status()
    {
        var genericCalls = 0;
        var toolDefinedCalls = 0;
        var tool = WeatherTool(needsApprovalFunction: (_, _) =>
        {
            toolDefinedCalls++;
            return Task.FromResult(true);
        });

        var result = await ResolveAsync(
            new[] { tool },
            new ToolApprovalConfiguration(_ =>
            {
                genericCalls++;
                return Task.FromResult<ToolApprovalStatus?>("denied");
            }));

        AssertStatus(result, "denied");
        Assert.Equal(1, genericCalls);
        Assert.Equal(0, toolDefinedCalls);
    }

    [Fact]
    [UpstreamTest(Id + "passes toolCall, tools, toolsContext, messages, and runtimeContext to the generic function", Coverage = UpstreamCoverage.Partial, Note = ContextNote)]
    public async Task Passes_toolCall_tools_toolsContext_messages_and_runtimeContext_to_the_generic_function()
    {
        var tools = new[] { WeatherTool() };
        var toolCall = CreateToolCall();
        var calls = new List<GenericToolApprovalOptions>();

        await ResolveAsync(
            tools,
            new ToolApprovalConfiguration(options =>
            {
                calls.Add(options);
                return Task.FromResult<ToolApprovalStatus?>("not-applicable");
            }),
            toolCall);

        var call = Assert.Single(calls);
        Assert.Same(toolCall, call.ToolCall);
        Assert.Same(tools, call.Tools);
        Assert.Same(Messages, call.Messages);
    }

    [Fact]
    [UpstreamTest(Id + "resolves a Promise returned by the generic function", Coverage = UpstreamCoverage.Covered)]
    public async Task Resolves_a_Promise_returned_by_the_generic_function()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(async _ =>
            {
                await Task.Yield();
                return "user-approval";
            }));

        AssertStatus(result, "user-approval");
    }

    [Fact]
    [UpstreamTest(Id + "treats undefined returned by the generic function as not-applicable", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_undefined_returned_by_the_generic_function_as_not_applicable()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(_ => Task.FromResult<ToolApprovalStatus?>(null)));

        AssertStatus(result, "not-applicable");
    }

    [Fact]
    [UpstreamTest(Id + "treats Promise undefined returned by the generic function as not-applicable", Coverage = UpstreamCoverage.Covered)]
    public async Task Treats_Promise_undefined_returned_by_the_generic_function_as_not_applicable()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(async _ =>
            {
                await Task.Yield();
                return null;
            }));

        AssertStatus(result, "not-applicable");
    }

    [Fact]
    [UpstreamTest(Id + "passes through an object status including reason from the generic function", Coverage = UpstreamCoverage.Covered)]
    public async Task Passes_through_an_object_status_including_reason_from_the_generic_function()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(_ => Task.FromResult<ToolApprovalStatus?>(new ToolApprovalStatus("denied", "policy block"))));

        AssertStatus(result, "denied", "policy block");
    }

    [Fact]
    [UpstreamTest(Id + "passes the same messages and toolsContext references to the generic function", Coverage = UpstreamCoverage.Partial, Note = ContextNote)]
    public async Task Passes_the_same_messages_and_toolsContext_references_to_the_generic_function()
    {
        var modelMessages = new ModelMessage[] { new UserModelMessage("first"), new AssistantModelMessage("second", null, null) };
        var calls = new List<GenericToolApprovalOptions>();

        await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration(options =>
            {
                calls.Add(options);
                return Task.FromResult<ToolApprovalStatus?>("not-applicable");
            }),
            messages: modelMessages);

        Assert.Same(modelMessages, Assert.Single(calls).Messages);
    }
}
