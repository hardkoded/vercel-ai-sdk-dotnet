// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicError;

/// <summary>Port of <c>anthropic-error.test.ts</c> &gt; <c>anthropicError &gt; anthropicErrorDataSchema</c>.</summary>
public sealed class AnthropicErrorDataSchemaTests
{
    [Theory]
    [InlineData("{\"error_code\":\"thread_not_found\",\"unsupported\":[{\"future\":true}]}")]
    [InlineData("{\"error_code\":\"thread_not_found\",\"unknown_field\":\"future\"}")]
    [InlineData("\"future details shape\"")]
    [UpstreamTest("packages/anthropic/src/anthropic-error.test.ts::anthropicError > anthropicErrorDataSchema::keeps the error message when details evolve: %j", Coverage = UpstreamCoverage.Covered, Commit = "6b101ac0765a3cf796e94692c3e3c8a3e08ff7f4")]
    public void Keeps_the_error_message_when_details_evolve(string details)
    {
        var result = AnthropicErrorData.Parse(
            "{\"type\":\"error\",\"error\":{\"type\":\"not_found_error\",\"message\":\"Thread not found\",\"details\":" + details + "}}");

        Assert.True(result.Success);
        Assert.Equal("Thread not found", result.Message);
        if (details.StartsWith("{", StringComparison.Ordinal))
        {
            Assert.Equal("thread_not_found", result.ErrorCode);
        }
    }
}
