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

/// <summary>Port of <c>gen-ai-format-messages.test.ts</c> &gt; <c>extractSystemFromPrompt</c>.</summary>
public sealed class ExtractSystemFromPromptTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::extractSystemFromPrompt::should extract system message from prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_extract_system_message_from_prompt()
    {
        ModelMessage[] prompt =
        [
            new SystemModelMessage("Be helpful"),
            new UserModelMessage("Hello"),
        ];
        Assert.Equal("Be helpful", Formatter.ExtractSystemFromPrompt(prompt));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::extractSystemFromPrompt::should return undefined when no system message",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_return_undefined_when_no_system_message()
    {
        ModelMessage[] prompt = [new UserModelMessage("Hello")];
        Assert.Null(Formatter.ExtractSystemFromPrompt(prompt));
    }
}
