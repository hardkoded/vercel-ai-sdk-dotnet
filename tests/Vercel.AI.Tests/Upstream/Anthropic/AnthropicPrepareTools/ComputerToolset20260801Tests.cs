// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Anthropic;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class ComputerToolset20260801Tests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::computer_toolset_20260801::";

    [Fact]
    [UpstreamTest(
        Prefix + "should prepare the toolset without a name or beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_prepare_the_toolset_without_a_name_or_beta_header()
    {
        var result = Prepare([
            ProviderTool("anthropic.computer_toolset_20260801", "computer", "{}"),
        ]);

        AssertSnapshot(result, [], "[{\"type\":\"computer_toolset_20260801\"}]");
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should map member configs to snake_case",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_map_member_configs_to_snake_case()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.computer_toolset_20260801",
                "computer",
                "{\"configs\":{\"zoom\":{\"enabled\":false},\"wait\":{\"enabled\":true,\"deferLoading\":true}}}"
            ),
        ]);

        AssertTools(
            result,
            "[{\"configs\":{\"wait\":{\"defer_loading\":true,\"enabled\":true},\"zoom\":{\"enabled\":false}},\"type\":\"computer_toolset_20260801\"}]"
        );
    }
}
