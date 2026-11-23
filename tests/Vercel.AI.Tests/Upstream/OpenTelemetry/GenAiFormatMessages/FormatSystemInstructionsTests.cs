// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;
using Vercel.AI.Prompt;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

public sealed class FormatSystemInstructionsTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::formatSystemInstructions::should format a system string into SemConv system instructions",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_format_a_system_string_into_SemConv_system_instructions()
    {
        SemConvJson.AssertEqual(
            """[{"content":"You are a helpful assistant.","type":"text"}]""",
            GenAiConventions.FormatSystemInstructions(
                Instructions.From("You are a helpful assistant.")
            )
        );
    }
}
