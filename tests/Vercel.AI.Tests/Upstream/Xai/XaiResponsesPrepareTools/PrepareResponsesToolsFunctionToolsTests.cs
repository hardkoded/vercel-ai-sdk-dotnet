// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; function tools</c>.</summary>
public sealed class PrepareResponsesToolsFunctionToolsTests
{
    [Fact]
    [UpstreamTest(
        PrepareResponsesToolsSupport.TestFile + " > function tools::should prepare function tools",
        Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_function_tools()
    {
        var tool = new LanguageModelTool(
            "weather",
            "get weather information",
            UpstreamChat.Json("""{"type":"object","properties":{"location":{"type":"string"}},"required":["location"],"additionalProperties":false}"""));

        var result = XaiResponsesTools.Prepare(new[] { tool }, null);

        JsonAssert.Equal(result.Tools, """
            [{"description":"get weather information","name":"weather","type":"function",
              "parameters":{"additionalProperties":false,"properties":{"location":{"type":"string"}},"required":["location"],"type":"object"}}]
            """);
    }
}
