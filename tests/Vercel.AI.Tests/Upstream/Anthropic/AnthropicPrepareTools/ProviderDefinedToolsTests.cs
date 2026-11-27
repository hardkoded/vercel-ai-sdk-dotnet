// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class ProviderDefinedToolsTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > provider-defined tools::";

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare bash_20241022 tool",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_bash_20241022_tool()
    {
        var result = Prepare([ProviderTool("anthropic.bash_20241022", "bash", "{}")]);

        AssertSnapshot(
            result,
            ["computer-use-2024-10-22"],
            "[{\"name\":\"bash\",\"type\":\"bash_20241022\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare text_editor_20250728 with max_characters",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_text_editor_20250728_with_max_characters()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.text_editor_20250728",
                "str_replace_based_edit_tool",
                "{\"maxCharacters\":10000}"
            ),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"max_characters\":10000,\"name\":\"str_replace_based_edit_tool\",\"type\":\"text_editor_20250728\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare text_editor_20250728 without max_characters",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_text_editor_20250728_without_max_characters()
    {
        var result = Prepare([
            ProviderTool("anthropic.text_editor_20250728", "str_replace_based_edit_tool", "{}"),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"name\":\"str_replace_based_edit_tool\",\"type\":\"text_editor_20250728\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare web_search_20250305",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_web_search_20250305()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.web_search_20250305",
                "web_search",
                "{\"maxUses\":10,\"allowedDomains\":[\"https://www.google.com\"],\"userLocation\":{\"type\":\"approximate\",\"city\":\"New York\"}}"
            ),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"allowed_domains\":[\"https://www.google.com\"],\"max_uses\":10,\"name\":\"web_search\",\"type\":\"web_search_20250305\",\"user_location\":{\"city\":\"New York\",\"type\":\"approximate\"}}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare web_search_20260209",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_web_search_20260209()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.web_search_20260209",
                "web_search",
                "{\"maxUses\":10,\"allowedDomains\":[\"https://www.google.com\"],\"userLocation\":{\"type\":\"approximate\",\"city\":\"New York\"}}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["code-execution-web-tools-2026-02-09"],
            "[{\"allowed_domains\":[\"https://www.google.com\"],\"max_uses\":10,\"name\":\"web_search\",\"type\":\"web_search_20260209\",\"user_location\":{\"city\":\"New York\",\"type\":\"approximate\"}}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare web_search_20260318 without a beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_web_search_20260318_without_a_beta_header()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.web_search_20260318",
                "web_search",
                "{\"maxUses\":10,\"allowedDomains\":[\"google.com\"],\"userLocation\":{\"type\":\"approximate\",\"city\":\"New York\"},\"responseInclusion\":\"excluded\"}"
            ),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"allowed_domains\":[\"google.com\"],\"max_uses\":10,\"name\":\"web_search\",\"type\":\"web_search_20260318\",\"user_location\":{\"city\":\"New York\",\"type\":\"approximate\"},\"response_inclusion\":\"excluded\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare web_fetch_20250910",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_web_fetch_20250910()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.web_fetch_20250910",
                "web_fetch",
                "{\"maxUses\":10,\"allowedDomains\":[\"https://www.google.com\"],\"citations\":{\"enabled\":true},\"maxContentTokens\":1000}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["web-fetch-2025-09-10"],
            "[{\"allowed_domains\":[\"https://www.google.com\"],\"citations\":{\"enabled\":true},\"max_content_tokens\":1000,\"max_uses\":10,\"name\":\"web_fetch\",\"type\":\"web_fetch_20250910\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare web_fetch_20260209",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_web_fetch_20260209()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.web_fetch_20260209",
                "web_fetch",
                "{\"maxUses\":10,\"allowedDomains\":[\"https://www.google.com\"],\"citations\":{\"enabled\":true},\"maxContentTokens\":1000}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["code-execution-web-tools-2026-02-09"],
            "[{\"allowed_domains\":[\"https://www.google.com\"],\"citations\":{\"enabled\":true},\"max_content_tokens\":1000,\"max_uses\":10,\"name\":\"web_fetch\",\"type\":\"web_fetch_20260209\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare web_fetch_20260318 without a beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_web_fetch_20260318_without_a_beta_header()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.web_fetch_20260318",
                "web_fetch",
                "{\"maxUses\":10,\"allowedDomains\":[\"google.com\"],\"citations\":{\"enabled\":true},\"maxContentTokens\":1000,\"useCache\":false,\"responseInclusion\":\"excluded\"}"
            ),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"allowed_domains\":[\"google.com\"],\"citations\":{\"enabled\":true},\"max_content_tokens\":1000,\"max_uses\":10,\"name\":\"web_fetch\",\"type\":\"web_fetch_20260318\",\"response_inclusion\":\"excluded\",\"use_cache\":false}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare tool_search_regex_20251119",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_tool_search_regex_20251119()
    {
        var result = Prepare([
            ProviderTool("anthropic.tool_search_regex_20251119", "tool_search", "{}"),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"name\":\"tool_search_tool_regex\",\"type\":\"tool_search_tool_regex_20251119\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare code_execution_20260120 without beta header",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_code_execution_20260120_without_beta_header()
    {
        var result = Prepare([
            ProviderTool("anthropic.code_execution_20260120", "code_execution", "{}"),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"name\":\"code_execution\",\"type\":\"code_execution_20260120\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare tool_search_bm25_20251119",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_tool_search_bm25_20251119()
    {
        var result = Prepare([
            ProviderTool("anthropic.tool_search_bm25_20251119", "tool_search", "{}"),
        ]);

        AssertSnapshot(
            result,
            [],
            "[{\"name\":\"tool_search_tool_bm25\",\"type\":\"tool_search_tool_bm25_20251119\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare advisor_20260301 with only the required model",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_advisor_20260301_with_only_the_required_model()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.advisor_20260301",
                "advisor",
                "{\"model\":\"claude-opus-4-7\"}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["advisor-tool-2026-03-01"],
            "[{\"model\":\"claude-opus-4-7\",\"name\":\"advisor\",\"type\":\"advisor_20260301\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should correctly prepare advisor_20260301 with all optional args",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_correctly_prepare_advisor_20260301_with_all_optional_args()
    {
        var result = Prepare([
            ProviderTool(
                "anthropic.advisor_20260301",
                "advisor",
                "{\"model\":\"claude-opus-4-7\",\"maxUses\":5,\"maxTokens\":2048,\"caching\":{\"type\":\"ephemeral\",\"ttl\":\"1h\"}}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["advisor-tool-2026-03-01"],
            "[{\"caching\":{\"ttl\":\"1h\",\"type\":\"ephemeral\"},\"max_tokens\":2048,\"max_uses\":5,\"model\":\"claude-opus-4-7\",\"name\":\"advisor\",\"type\":\"advisor_20260301\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should reject non-integer advisor_20260301 maxTokens",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_reject_non_integer_advisor_20260301_maxTokens()
    {
        Assert.Throws<TypeValidationException>(() =>
            Prepare([
                ProviderTool(
                    "anthropic.advisor_20260301",
                    "advisor",
                    "{\"model\":\"claude-opus-4-7\",\"maxTokens\":2048.5}"
                ),
            ])
        );
    }
}
