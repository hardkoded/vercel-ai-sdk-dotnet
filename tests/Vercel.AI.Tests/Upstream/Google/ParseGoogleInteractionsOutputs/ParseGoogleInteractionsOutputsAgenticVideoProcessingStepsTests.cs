// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Google.ParseGoogleInteractionsOutputs;

/// <summary>Port of <c>parse-google-interactions-outputs.test.ts</c> &gt; <c>parseGoogleInteractionsOutputs</c> &gt; <c>agentic video processing steps</c>.</summary>
public sealed class ParseGoogleInteractionsOutputsAgenticVideoProcessingStepsTests
{
    private const string Prefix =
        ParseGoogleInteractionsOutputsSupport.Id + "agentic video processing steps::";

    [Fact]
    [UpstreamTest(
        Prefix + "emits custom parts with IDs and signatures",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_custom_parts_with_IDs_and_signatures()
    {
        var (content, _) = ParseGoogleInteractionsOutputsSupport.Parse(
            """
            [
              {"type":"processing_call","id":"processing-1","signature":"call-signature"},
              {"type":"processing_result","call_id":"processing-1","signature":"result-signature"}
            ]
            """,
            "interaction-1"
        );

        GoogleUpstream.JsonEqual(
            ParseGoogleInteractionsOutputsSupport.Snapshot(content),
            """
            [
              {"type":"custom","kind":"google.processing_call","providerMetadata":{"google":{"signature":"call-signature","interactionId":"interaction-1","processingId":"processing-1"}}},
              {"type":"custom","kind":"google.processing_result","providerMetadata":{"google":{"signature":"result-signature","interactionId":"interaction-1","processingCallId":"processing-1"}}}
            ]
            """
        );
    }
}
