// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

public sealed class FormatObjectOutputMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatObjectOutputMessages::should format object output as text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_object_output_as_text_content()
    {
        SemConvJson.AssertEqual(
            """[{"finish_reason":"stop","parts":[{"content":"{\"name\":\"test\"}","type":"text"}],"role":"assistant"}]""",
            GenAiConventions.FormatObjectOutputMessages("""{"name":"test"}""", "stop")
        );
    }
}
