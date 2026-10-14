// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ComputerTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > computer::should prepare computer tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_computer_tool()
    {
        var result = Prepare(null, ProviderTool("openai.computer", "computer", "{}"));

        AssertTools(result, "[{\"type\":\"computer\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > computer::should handle computer tool choice", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_computer_tool_choice()
    {
        var result = Prepare(ToolChoice.Tool("computer"), ProviderTool("openai.computer", "computer", "{}"));

        AssertTools(result, "[{\"type\":\"computer\"}]");
        AssertToolChoice(result, "{\"type\":\"computer\"}");
    }
}
