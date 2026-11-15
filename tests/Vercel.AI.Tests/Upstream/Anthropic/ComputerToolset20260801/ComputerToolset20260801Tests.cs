// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.ComputerToolset20260801;

/// <summary>Port of <c>computer-toolset_20260801.test.ts</c> &gt; <c>computerToolset_20260801</c>.</summary>
public sealed class ComputerToolset20260801Tests
{
    private const string Prefix = "packages/anthropic/src/tool/computer-toolset_20260801.test.ts::computerToolset_20260801::";
    private const string Commit = "a161c0a665a8e561dd56905baeec94567395cb55";

    public static IEnumerable<object[]> Members() =>
        AnthropicTools.ComputerToolset20260801Members.Select(member => new object[] { member });

    private static bool Accepts(Tool tool, string action, string extra = "")
    {
        using var document = JsonDocument.Parse("{\"action\":\"" + action + "\"" + extra + "}");
        return ToolInputSchemaValidator.Validate(tool.InputSchema, document.RootElement).Count == 0;
    }

    [Theory]
    [MemberData(nameof(Members))]
    [UpstreamTest(Prefix + "should allow %s by default", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_allow_member_by_default(string action)
    {
        var tool = AnthropicTools.ComputerToolset20260801();

        Assert.True(Accepts(tool, action));
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject disabled members in its local input schema", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_disabled_members_in_its_local_input_schema()
    {
        var tool = AnthropicTools.ComputerToolset20260801(new Dictionary<string, AnthropicComputerToolsetMemberConfig>
        {
            ["wait"] = new() { Enabled = false },
        });

        Assert.False(Accepts(tool, "wait", ",\"duration\":0"));
    }

    [Fact]
    [UpstreamTest(Prefix + "should preserve enabled and deferred members", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_preserve_enabled_and_deferred_members()
    {
        var tool = AnthropicTools.ComputerToolset20260801(new Dictionary<string, AnthropicComputerToolsetMemberConfig>
        {
            ["screenshot"] = new() { Enabled = true },
            ["wait"] = new() { DeferLoading = true },
        });

        foreach (var action in new[] { "screenshot", "wait", "cursor_position" })
        {
            Assert.True(Accepts(tool, action));
        }
    }

    [Fact]
    [UpstreamTest(Prefix + "should reject every member when all are disabled", Coverage = UpstreamCoverage.Covered, Commit = Commit)]
    public void Should_reject_every_member_when_all_are_disabled()
    {
        var tool = AnthropicTools.ComputerToolset20260801(
            AnthropicTools.ComputerToolset20260801Members.ToDictionary(
                member => member,
                _ => new AnthropicComputerToolsetMemberConfig { Enabled = false }));

        foreach (var action in AnthropicTools.ComputerToolset20260801Members)
        {
            Assert.False(Accepts(tool, action));
        }
    }
}
