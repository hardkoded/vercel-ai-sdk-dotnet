// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.ConvertToGoogleMessages;

/// <summary>Port of <c>convert-to-google-messages.test.ts</c> &gt; <c>thought signatures</c>.</summary>
public sealed class ThoughtSignaturesTests
{
    [Theory]
    [InlineData("google")]
    [InlineData("vertex")]
    [InlineData("googleVertex")]
    [UpstreamTest(
        "packages/google/src/convert-to-google-messages.test.ts::thought signatures::preserves standalone empty text and reasoning signatures from %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "fd7e8a75d719314f79528f0c9fda734bb83e68f5"
    )]
    public void Preserves_standalone_empty_text_and_reasoning_signatures(string ns)
    {
        var text = new GoogleTextAssistantPart("")
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                "{\"" + ns + "\":{\"thoughtSignature\":\"text-signature\"}}"
            ),
        };
        var reasoning = new GoogleReasoningAssistantPart("")
        {
            ProviderOptions = GoogleUpstream.ProviderOptions(
                "{\"" + ns + "\":{\"thoughtSignature\":\"reasoning-signature\"}}"
            ),
        };
        var options = new GoogleMessageOptions { ProviderOptionNames = ["googleVertex", "vertex"] };

        var prompt = GoogleMessages.Convert(
            new GoogleTurn[]
            {
                new GoogleAssistantTurn(new GoogleAssistantPart[] { text, reasoning }),
            },
            options
        );

        GoogleUpstream.JsonEqual(
            prompt.Contents,
            "[{\"role\":\"model\",\"parts\":[{\"text\":\"\",\"thoughtSignature\":\"text-signature\"},{\"text\":\"\",\"thought\":true,\"thoughtSignature\":\"reasoning-signature\"}]}]"
        );
    }
}
