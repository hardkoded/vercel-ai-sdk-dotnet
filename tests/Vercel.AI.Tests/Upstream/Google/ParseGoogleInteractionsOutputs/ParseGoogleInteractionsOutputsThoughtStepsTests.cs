// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Port of <c>parse-google-interactions-outputs.test.ts</c> &gt; <c>parseGoogleInteractionsOutputs</c> &gt; <c>thought steps</c>.</summary>
public sealed class ParseGoogleInteractionsOutputsThoughtStepsTests
{
    private const string Prefix = ParseGoogleInteractionsOutputsSupport.Id + "thought steps::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "emits a reasoning part with providerMetadata.google.signature when the thought step carries a signature",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_reasoning_part_with_providerMetadata_google_signature_when_the_thought_step_carries_a_signature()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"thought","signature":"thought-sig-AAA","summary":[{"type":"text","text":"I am thinking."}]}]"""
        );

        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """[{"type":"reasoning","text":"I am thinking.","providerMetadata":{"google":{"signature":"thought-sig-AAA"}}}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "stamps providerMetadata.google.interactionId on every output part when interactionId is provided",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Stamps_providerMetadata_google_interactionId_on_every_output_part_when_interactionId_is_provided()
    {
        var (content, hasFunctionCall) = ParseGoogleInteractionsOutputsSupport.Parse(
            """
            [
              {"type":"thought","signature":"thought-sig","summary":[{"type":"text","text":"planning..."}]},
              {"type":"model_output","content":[{"type":"text","text":"answer"}]},
              {"type":"function_call","id":"call_x","name":"getWeather","arguments":{"loc":"NYC"},"signature":"fn-sig"}
            ]
            """,
            "v1_test-interaction"
        );

        Assert.True(hasFunctionCall);
        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """
            [
              {"type":"reasoning","text":"planning...","providerMetadata":{"google":{"interactionId":"v1_test-interaction","signature":"thought-sig"}}},
              {"type":"text","text":"answer","providerMetadata":{"google":{"interactionId":"v1_test-interaction"}}},
              {"type":"tool-call","toolCallId":"call_x","toolName":"getWeather","input":"{\"loc\":\"NYC\"}","providerMetadata":{"google":{"interactionId":"v1_test-interaction","signature":"fn-sig"}}}
            ]
            """
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "omits providerMetadata when no signature and no interactionId are present",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Omits_providerMetadata_when_no_signature_and_no_interactionId_are_present()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """[{"type":"model_output","content":[{"type":"text","text":"hello"}]}]"""
        );

        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """[{"type":"text","text":"hello"}]"""
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "skips user_input steps (server echo of client input)",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Skips_user_input_steps_server_echo_of_client_input()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """
            [
              {"type":"user_input","content":[{"type":"text","text":"hi"}]},
              {"type":"model_output","content":[{"type":"text","text":"hello"}]}
            ]
            """
        );

        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """[{"type":"text","text":"hello"}]"""
        );
    }
}
