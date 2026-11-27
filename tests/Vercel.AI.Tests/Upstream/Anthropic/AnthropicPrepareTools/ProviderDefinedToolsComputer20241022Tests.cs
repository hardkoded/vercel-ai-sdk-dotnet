// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class ProviderDefinedToolsComputer20241022Tests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools > computer_20241022::";

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare computer_20241022 tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_computer_20241022_tool()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.computer_20241022",
                "computer",
                "{\"displayWidthPx\":800,\"displayHeightPx\":600,\"displayNumber\":1}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["computer-use-2024-10-22"],
            "[{\"display_height_px\":600,\"display_number\":1,\"display_width_px\":800,\"name\":\"computer\",\"type\":\"computer_20241022\"}]"
        );
    }
}
