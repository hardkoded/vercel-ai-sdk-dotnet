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

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>formatSystemInstructions</c>.</summary>
public sealed class FormatSystemInstructionsTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatSystemInstructions::should format a system string into SemConv system instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_a_system_string_into_SemConv_system_instructions()
    {
        OtelJson.Equal(
            """
            [{"content":"You are a helpful assistant.","type":"text"}]
            """,
            Formatter.FormatSystemInstructions(Instructions.From("You are a helpful assistant."))
        );
    }
}
