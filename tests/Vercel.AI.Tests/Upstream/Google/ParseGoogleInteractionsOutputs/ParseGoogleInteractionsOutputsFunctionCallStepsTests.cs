// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Port of <c>parse-google-interactions-outputs.test.ts</c> &gt; <c>parseGoogleInteractionsOutputs</c> &gt; <c>function_call steps</c>.</summary>
public sealed class ParseGoogleInteractionsOutputsFunctionCallStepsTests
{
    private const string Prefix =
        ParseGoogleInteractionsOutputsSupport.Id + "function_call steps::";

    [Fact]
    [UpstreamTest(
        Prefix + "captures function_call.signature into providerMetadata.google.signature",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Captures_function_call_signature_into_providerMetadata_google_signature()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"function_call","id":"call_abc","name":"doThing","arguments":{},"signature":"fn-sig-BBB"}]"""
        );

        var call = Assert.IsType<GeneratedToolCall>(content[0]);
        Assert.Equal("call_abc", call.ToolCallId);
        Assert.Equal("doThing", call.ToolName);
        GoogleUpstream.JsonEqual(
            call.ProviderMetadata!.Value,
            """{"google":{"signature":"fn-sig-BBB"}}"""
        );
    }
}
