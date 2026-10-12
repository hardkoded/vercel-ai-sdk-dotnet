// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Xai;
using static Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools.PrepareResponsesToolsSupport;

namespace Vercel.AI.Tests.Upstream.Xai.XaiResponsesPrepareTools;

/// <summary>Port of <c>xai-responses-prepare-tools.test.ts</c> &gt; <c>prepareResponsesTools &gt; code_execution</c>.</summary>
public sealed class PrepareResponsesToolsCodeExecutionTests
{
    private const string Prefix = TestFile + " > code_execution::";

    [Fact]
    [UpstreamTest(Prefix + "should prepare code_execution tool as code_interpreter", Coverage = UpstreamCoverage.Covered)]
    public void Should_prepare_code_execution_tool_as_code_interpreter()
    {
        var result = XaiResponsesTools.Prepare(new[] { ProviderTool("xai.code_execution", "code_execution") }, null);

        JsonAssert.Equal(result.Tools, """[{"type":"code_interpreter"}]""");
    }
}
