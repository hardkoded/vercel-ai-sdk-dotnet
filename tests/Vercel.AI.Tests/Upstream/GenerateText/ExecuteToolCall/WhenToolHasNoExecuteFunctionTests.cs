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

/// <summary>Port of <c>execute-tool-call.test.ts</c> &gt; <c>executeToolCall</c> &gt; <c>when tool has no execute function</c>.</summary>
public sealed class WhenToolHasNoExecuteFunctionTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/execute-tool-call.test.ts::executeToolCall > when tool has no execute function::";

    [Fact]
    [UpstreamTest(Prefix + "should return undefined", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_return_undefined()
    {
        var result = await Execute(
            CreateToolCall(),
            [Tool.Function("testTool", null, ValueSchema, null)]
        );

        Assert.Null(result);
    }
}
