// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using static Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools.AnthropicPrepareToolsSupport;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class AllowedCallersForFunctionToolsProgrammaticToolCallingTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::prepareTools > allowedCallers for function tools (programmatic tool calling)::";

    [Fact]
    [UpstreamTest(
        Prefix
            + "should include allowed_callers and advanced-tool-use beta when allowedCallers is set",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_allowed_callers_and_advanced_tool_use_beta_when_allowedCallers_is_set()
    {
        var result = Prepare([
            FunctionTool(
                "query_database",
                "Query a database",
                "{\"type\":\"object\",\"properties\":{\"sql\":{\"type\":\"string\"}}}",
                "{\"anthropic\":{\"allowedCallers\":[\"code_execution_20250825\"]}}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13", "advanced-tool-use-2025-11-20"],
            "[{\"allowed_callers\":[\"code_execution_20250825\"],\"description\":\"Query a database\",\"input_schema\":{\"type\":\"object\",\"properties\":{\"sql\":{\"type\":\"string\"}}},\"name\":\"query_database\"}]"
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not include allowed_callers when not specified",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_include_allowed_callers_when_not_specified()
    {
        var result = Prepare([FunctionTool("testFunction", "A test function")]);

        Assert.Null(result.Tools![0]!["allowed_callers"]);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include both deferLoading and allowedCallers when both are set",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_both_deferLoading_and_allowedCallers_when_both_are_set()
    {
        var result = Prepare([
            FunctionTool(
                "query_database",
                "Query a database",
                "{\"type\":\"object\",\"properties\":{\"sql\":{\"type\":\"string\"}}}",
                "{\"anthropic\":{\"deferLoading\":true,\"allowedCallers\":[\"code_execution_20250825\"]}}"
            ),
        ]);

        Assert.True(result.Tools![0]!["defer_loading"]!.GetValue<bool>());
        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse("[\"code_execution_20250825\"]"),
                result.Tools[0]!["allowed_callers"]
            )
        );
        Assert.Contains("advanced-tool-use-2025-11-20", result.Betas);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should include allowed_callers with code_execution_20260120",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_include_allowed_callers_with_code_execution_20260120()
    {
        var result = Prepare([
            FunctionTool(
                "query_database",
                "Query a database",
                "{\"type\":\"object\",\"properties\":{\"sql\":{\"type\":\"string\"}}}",
                "{\"anthropic\":{\"allowedCallers\":[\"code_execution_20260120\"]}}"
            ),
        ]);

        AssertSnapshot(
            result,
            ["structured-outputs-2025-11-13", "advanced-tool-use-2025-11-20"],
            "[{\"allowed_callers\":[\"code_execution_20260120\"],\"description\":\"Query a database\",\"input_schema\":{\"type\":\"object\",\"properties\":{\"sql\":{\"type\":\"string\"}}},\"name\":\"query_database\"}]"
        );
    }
}
