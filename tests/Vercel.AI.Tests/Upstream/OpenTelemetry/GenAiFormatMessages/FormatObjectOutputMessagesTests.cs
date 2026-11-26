// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.OpenTelemetry;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Formatter = Vercel.AI.OpenTelemetry.GenAiFormatMessages;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>formatObjectOutputMessages</c>.</summary>
public sealed class FormatObjectOutputMessagesTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatObjectOutputMessages::should format object output as text content",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_object_output_as_text_content()
    {
        OtelJson.Equal(
            """
            [{"finish_reason":"stop","parts":[{"content":"{\"name\":\"test\"}","type":"text"}],"role":"assistant"}]
            """,
            Formatter.FormatObjectOutputMessages("{\"name\":\"test\"}", FinishReason.Stop)
        );
    }
}
