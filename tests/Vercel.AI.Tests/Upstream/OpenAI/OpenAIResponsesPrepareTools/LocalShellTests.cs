// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools.OpenAIResponsesPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.OpenAI.OpenAIResponsesPrepareTools;

public sealed class LocalShellTests
{
    [Fact]
    [UpstreamTest("packages/openai/src/responses/openai-responses-prepare-tools.test.ts::prepareResponsesTools > local shell::should prepare local_shell tool", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_local_shell_tool()
    {
        var result = Prepare(null, ProviderTool("openai.local_shell", "local_shell", "{}"));

        AssertTools(result, "[{\"type\":\"local_shell\"}]");
    }
}
