// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall.ExecuteToolCallSupport;

namespace Vercel.AI.Tests.Upstream.GenerateText.ExecuteToolCall;

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>dynamic tools</c>.</summary>
public sealed class DynamicToolsTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > dynamic tools::";

    [Fact]
    [UpstreamTest(
        Prefix + "should set dynamic: true for dynamic tools on success",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_dynamic_true_for_dynamic_tools_on_success()
    {
        var result = await Execute(
            CreateToolCall(dynamic: true),
            [
                ValueTool(
                    (_, _) => Task.FromResult(JsonSerializer.Serialize("dynamic-result")),
                    dynamic: true
                ),
            ]
        );

        var output = result!.Value.Output;
        Assert.False(output.IsError);
        Assert.True(output.Dynamic);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should set dynamic: true for dynamic tools on error",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_set_dynamic_true_for_dynamic_tools_on_error()
    {
        var result = await Execute(
            CreateToolCall(dynamic: true),
            [FailingTool(new InvalidOperationException("error"), dynamic: true)]
        );

        var output = result!.Value.Output;
        Assert.True(output.IsError);
        Assert.True(output.Dynamic);
    }
}
