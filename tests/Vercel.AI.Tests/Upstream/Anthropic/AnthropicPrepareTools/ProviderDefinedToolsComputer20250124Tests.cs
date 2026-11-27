// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class ProviderDefinedToolsComputer20250124Tests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20250124::";

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare computer_20250124 tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_computer_20250124_tool()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.computer_20250124",
                "computer",
                "{\"displayWidthPx\":1024,\"displayHeightPx\":768,\"displayNumber\":1}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["computer-use-2025-01-24"],
            "[{\"display_height_px\":768,\"display_number\":1,\"display_width_px\":1024,\"name\":\"computer\",\"type\":\"computer_20250124\"}]"
        );
    }
}
