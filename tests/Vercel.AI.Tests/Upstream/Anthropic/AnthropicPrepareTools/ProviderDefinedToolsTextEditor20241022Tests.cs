// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class ProviderDefinedToolsTextEditor20241022Tests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > text_editor_20241022::";

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare text_editor_20241022 tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_text_editor_20241022_tool()
    {
        var result = Prepare([ProviderTool("anthropic.text_editor_20241022", "text_editor", "{}")]);

        AssertSnapshot(
            result,
            ["computer-use-2024-10-22"],
            "[{\"name\":\"str_replace_editor\",\"type\":\"text_editor_20241022\"}]"
        );
    }
}
