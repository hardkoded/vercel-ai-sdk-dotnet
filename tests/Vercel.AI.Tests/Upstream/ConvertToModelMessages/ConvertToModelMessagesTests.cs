// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.ConvertToModelMessages.ConvertToModelMessagesSupport;

namespace Vercel.AI.Tests.ConvertToModelMessages;

/// <summary>Port of <c>convert-to-model-messages.test.ts</c> &gt; <c>convertToModelMessages</c>.</summary>
[Collection("LogWarnings")]
public sealed class ConvertToModelMessagesTests : WarningsOff
{
    private const string Suite = Prefix + "convertToModelMessages::";

    [UpstreamTest(
        Suite + "should not include filename for user file parts when not provided",
        Coverage = UpstreamCoverage.Covered
    )]
    [Fact]
    public void Should_not_include_filename_for_user_file_parts_when_not_provided()
    {
        var result = Convert(
            UiMessages(
                """
                [{"role":"user","parts":[{"type":"file","mediaType":"image/jpeg","url":"https://example.com/image.jpg"}]}]
                """
            )
        );

        AssertEqual(
            """
            [{"role":"user","content":[{"type":"file","mediaType":"image/jpeg","data":{"type":"url","url":"https://example.com/image.jpg"}}]}]
            """,
            result
        );
    }
}
