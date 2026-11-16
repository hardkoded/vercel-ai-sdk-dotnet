// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Anthropic;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput.VertexAnthropicStructuredOutputSupport;

namespace Vercel.AI.Tests.Upstream.Google.GoogleVertexAnthropicStructuredOutput;

/// <summary>Port of <c>describe.each([['Node', ...], ['Edge', ...]])('%s native-output opt-in')</c>. Edge runs the same HttpClient.</summary>
public sealed class RuntimeNativeOutputOptInTests
{
    private const string Prefix =
        "packages/google-vertex/src/anthropic/google-vertex-anthropic-structured-output.test.ts::%s native-output opt-in::";

    [Theory]
    [InlineData("generate")]
    [InlineData("stream")]
    [UpstreamTest(
        Prefix + "uses native output by default without a request override: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "4b8f00efff97de78e52667359aa33267a1036c46",
        Note = "Edge runs the same HttpClient."
    )]
    public async Task Uses_native_output_by_default_without_a_request_override(string method)
    {
        var handler = new Handler(method, true, Report);
        var model = Model("claude-sonnet-4-6", handler, true, authToken: true);

        await Call(model, method, Options(ReportSchema)).ConfigureAwait(false);

        Assert.Equal(
            "json_schema",
            handler.Body["output_config"]!["format"]!["type"]!.GetValue<string>()
        );
        Assert.Null(handler.Body["tools"]);
        Assert.Equal("Bearer test-token", handler.Headers["authorization"]);
        Assert.False(handler.Headers.ContainsKey("anthropic-beta"));
    }
}
