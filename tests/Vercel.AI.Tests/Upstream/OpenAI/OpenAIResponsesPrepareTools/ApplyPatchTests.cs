// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class ApplyPatchTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > apply_patch::should prepare apply_patch tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_apply_patch_tool()
    {
        var result = Prepare(null, ProviderTool("openai.apply_patch", "apply_patch", "{}"));

        AssertTools(result, "[{\"type\":\"apply_patch\"}]");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > apply_patch::should handle tool choice selection with apply_patch", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_tool_choice_selection_with_apply_patch()
    {
        var result = Prepare(ToolChoice.Tool("apply_patch"), ProviderTool("openai.apply_patch", "apply_patch", "{}"));

        AssertTools(result, "[{\"type\":\"apply_patch\"}]");
        AssertToolChoice(result, "{\"type\":\"apply_patch\"}");
    }

    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > apply_patch::should handle multiple tools including apply_patch", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_multiple_tools_including_apply_patch()
    {
        var result = Prepare(null, FunctionTool(), ProviderTool("openai.apply_patch", "apply_patch", "{}"));

        AssertTools(result, "[" + FunctionToolJson + ",{\"type\":\"apply_patch\"}]");
    }
}
