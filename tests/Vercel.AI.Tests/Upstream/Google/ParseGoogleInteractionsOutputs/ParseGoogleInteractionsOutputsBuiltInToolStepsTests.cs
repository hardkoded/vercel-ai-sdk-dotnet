// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Port of <c>parse-google-interactions-outputs.test.ts</c> &gt; <c>parseGoogleInteractionsOutputs</c> &gt; <c>built-in tool steps</c>.</summary>
public sealed class ParseGoogleInteractionsOutputsBuiltInToolStepsTests
{
    private const string Prefix =
        ParseGoogleInteractionsOutputsSupport.Id + "built-in tool steps::";

    [Fact]
    [UpstreamTest(
        Prefix + "generates an ID when a built-in tool call ID is empty",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Generates_an_ID_when_a_built_in_tool_call_ID_is_empty()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"google_search_call","id":"","arguments":{"query":"weather"}}]"""
        );

        var call = Assert.IsType<GeneratedToolCall>(content[0]);
        Assert.Equal("gen-id", call.ToolCallId);
        Assert.Equal("google_search", call.ToolName);
        Assert.True(call.ProviderExecuted);
    }
}
