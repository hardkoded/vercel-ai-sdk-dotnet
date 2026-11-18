// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval.ResolveToolApprovalSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ResolveToolApproval;

/// <summary>Port of <c>resolve-tool-approval.test.ts</c> &gt; <c>resolveToolApproval</c> &gt; <c>SingleToolApprovalFunction (per-tool approval)</c>.</summary>
public sealed class SingleToolApprovalFunctionPerToolApprovalTests
{
    private const string Id =
        "packages/ai/src/generate-text/resolve-tool-approval.test.ts::resolveToolApproval > SingleToolApprovalFunction (per-tool approval)::";

    [Fact]
    [UpstreamTest(
        Id
            + "passes the same messages reference and validated toolContext to the per-tool function",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_the_same_messages_reference_and_validated_toolContext_to_the_per_tool_function()
    {
        var modelMessages = new ModelMessage[]
        {
            new UserModelMessage("step 1"),
            new UserModelMessage("step 2"),
        };
        var runtimeContext = new Dictionary<string, object?>();
        var calls = new List<(JsonElement Input, SingleToolApprovalOptions Options)>();

        await ResolveAsync(
            new[] { WeatherTool(contextSchema: ApiKeySchema) },
            new ToolApprovalConfiguration
            {
                {
                    "weather",
                    (input, options) =>
                    {
                        calls.Add((input, options));
                        return Task.FromResult<ToolApprovalStatus?>("approved");
                    }
                },
            },
            messages: modelMessages,
            toolsContext: new Dictionary<string, JsonElement>
            {
                ["weather"] = Json("{\"apiKey\":\"secret\"}"),
            },
            runtimeContext: runtimeContext
        );

        var call = Assert.Single(calls);
        Assert.Equal("{\"apiKey\":\"secret\"}", call.Options.ToolContext!.Value.GetRawText());
        Assert.Same(runtimeContext, call.Options.RuntimeContext);
        Assert.Equal("{\"city\":\"Berlin\"}", call.Input.GetRawText());
        Assert.Equal("call-1", call.Options.ToolCallId);
        Assert.Same(modelMessages, call.Options.Messages);
    }

    [Fact]
    [UpstreamTest(
        Id + "passes toolsContext entry through as toolContext after schema validation",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Passes_toolsContext_entry_through_as_toolContext_after_schema_validation()
    {
        var calls = new List<SingleToolApprovalOptions>();

        await ResolveAsync(
            new[]
            {
                // Stands in for z.object({ apiKey: z.string(), region: z.string().default('us-east-1') }).
                WeatherTool(contextSchema: value =>
                {
                    var apiKey = ApiKeySchema(value).GetProperty("apiKey").GetString();
                    var region = value.TryGetProperty("region", out var r)
                        ? r.GetString()
                        : "us-east-1";
                    return Json("{\"apiKey\":\"" + apiKey + "\",\"region\":\"" + region + "\"}");
                }),
            },
            new ToolApprovalConfiguration
            {
                {
                    "weather",
                    (_, options) =>
                    {
                        calls.Add(options);
                        return Task.FromResult<ToolApprovalStatus?>("not-applicable");
                    }
                },
            },
            // The runtime value before validation omits `region`; the schema applies its default.
            toolsContext: new Dictionary<string, JsonElement>
            {
                ["weather"] = Json("{\"apiKey\":\"k\"}"),
            }
        );

        var call = Assert.Single(calls);
        Assert.Equal(
            "{\"apiKey\":\"k\",\"region\":\"us-east-1\"}",
            call.ToolContext!.Value.GetRawText()
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "treats undefined returned by a per-tool approval function as not-applicable",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Treats_undefined_returned_by_a_per_tool_approval_function_as_not_applicable()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration
            {
                { "weather", (_, _) => Task.FromResult<ToolApprovalStatus?>(null) },
            }
        );

        AssertStatus(result, "not-applicable");
    }

    [Fact]
    [UpstreamTest(
        Id + "treats Promise undefined returned by a per-tool approval function as not-applicable",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Treats_Promise_undefined_returned_by_a_per_tool_approval_function_as_not_applicable()
    {
        var result = await ResolveAsync(
            new[] { WeatherTool() },
            new ToolApprovalConfiguration
            {
                {
                    "weather",
                    async (_, _) =>
                    {
                        await Task.Yield();
                        return null;
                    }
                },
            }
        );

        AssertStatus(result, "not-applicable");
    }
}
