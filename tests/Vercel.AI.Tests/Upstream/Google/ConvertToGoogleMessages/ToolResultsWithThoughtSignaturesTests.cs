// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;

namespace Vercel.AI.Tests.ConvertToGoogleMessages;

/// <summary>Port of <c>convert-to-google-messages.test.ts</c> &gt; <c>tool results with thought signatures</c>.</summary>
public sealed class ToolResultsWithThoughtSignaturesTests
{
    [Fact]
    [UpstreamTest("packages/google/src/convert-to-google-messages.test.ts::tool results with thought signatures::should include thought signature on functionCall but not on functionResponse", Coverage = UpstreamCoverage.Covered)]
    public void Should_include_thought_signature_on_functionCall_but_not_on_functionResponse()
    {
        var call = new GoogleToolCallAssistantPart("readdata", "{\"userId\":\"123\"}")
        {
            ToolCallId = "call1",
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"sig_original\"}}"),
        };
        var result = new GoogleToolResultPart("readdata")
        {
            ToolCallId = "call1",
            OutputKind = "error-text",
            Output = "file not found",
            ProviderOptions = GoogleUpstream.ProviderOptions("{\"google\":{\"thoughtSignature\":\"sig_original\"}}"),
        };

        var prompt = GoogleMessages.Convert(
            new GoogleTurn[]
            {
                new GoogleAssistantTurn(new GoogleAssistantPart[] { call }),
                new GoogleToolTurn(new[] { result }),
            },
            new GoogleMessageOptions());

        GoogleUpstream.JsonEqual(
            prompt.Contents[0]!["parts"]![0]!,
            "{\"functionCall\":{\"id\":\"call1\",\"args\":{\"userId\":\"123\"},\"name\":\"readdata\"},\"thoughtSignature\":\"sig_original\"}");
        GoogleUpstream.JsonEqual(
            prompt.Contents[1]!["parts"]![0]!,
            "{\"functionResponse\":{\"id\":\"call1\",\"name\":\"readdata\",\"response\":{\"error\":\"file not found\",\"name\":\"readdata\"}}}");
        Assert.False(prompt.Contents[1]!["parts"]![0]!.AsObject().ContainsKey("thoughtSignature"));
    }
}
