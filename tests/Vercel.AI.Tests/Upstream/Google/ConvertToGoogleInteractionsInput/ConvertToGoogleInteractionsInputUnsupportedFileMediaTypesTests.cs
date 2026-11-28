// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using S = Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput.ConvertToGoogleInteractionsInputMessageSupport;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Port of <c>convert-to-google-interactions-input.test.ts</c> &gt; <c>convertToGoogleInteractionsInput</c> &gt; <c>unsupported file media types</c>.</summary>
public sealed class ConvertToGoogleInteractionsInputUnsupportedFileMediaTypesTests
{
    private const string Prefix =
        ConvertToGoogleInteractionsInputSupport.Id + "unsupported file media types::";

    [Fact]
    [UpstreamTest(
        Prefix + "emits a warning and drops the part when media type is unrecognized",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Emits_a_warning_and_drops_the_part_when_media_type_is_unrecognized()
    {
        var result = ConvertToGoogleInteractionsInputSupport.Convert(
            new UserModelMessage(
                new UserContentPart[]
                {
                    new TextContentPart("Hi"),
                    new FileContentPart("unknown/blob", null, new byte[] { 1, 2, 3 }, null),
                }
            )
        );

        GoogleUpstream.JsonEqual(
            result.Input,
            "[{\"type\":\"user_input\",\"content\":[{\"type\":\"text\",\"text\":\"Hi\"}]}]"
        );
        GoogleUpstream.JsonEqual(
            S.Warnings(result),
            "[{\"type\":\"other\",\"message\":\"google.interactions: unsupported file media type \\\"unknown/blob\\\"; part dropped.\"}]"
        );
    }
}
