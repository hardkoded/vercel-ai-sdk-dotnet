// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.OpenTelemetry;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.OpenTelemetry.GenAiFormatMessages;

public sealed class ExtractSystemFromPromptTests
{
    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::extractSystemFromPrompt::should extract system message from prompt",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_extract_system_message_from_prompt()
    {
        var prompt = new ModelMessage[]
        {
            new SystemModelMessage("Be helpful"),
            new UserModelMessage("Hello"),
        };
        Assert.Equal("Be helpful", GenAiConventions.ExtractSystemFromPrompt(prompt));
    }

    [Fact]
    [UpstreamTest(
        "packages/otel/src/gen-ai-format-messages.test.ts::extractSystemFromPrompt::should return undefined when no system message",
        Coverage = UpstreamCoverage.Covered,
        Commit = "d03d74ad2fef36a1bdd0636b3aa72b44f3d95f71"
    )]
    public void Should_return_undefined_when_no_system_message()
    {
        var prompt = new ModelMessage[] { new UserModelMessage("Hello") };
        Assert.Null(GenAiConventions.ExtractSystemFromPrompt(prompt));
    }
}
