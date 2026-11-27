// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class WebSearch20250305OutputSchemaTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::webSearch_20250305OutputSchema::";

    [Fact]
    [UpstreamTest(
        Prefix + "should not fail validation when title is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_fail_validation_when_title_is_null()
    {
        using var document = JsonDocument.Parse(
            "[{\"url\":\"https://test.com\",\"title\":null,\"pageAge\":\"April 30, 2025\",\"encryptedContent\":\"EqgfCioIARgBIiQ3YTAwMjY1Mi1mZjM5LTQ1NGUtODgxNC1kNjNjNTk1ZWI3Y...\",\"type\":\"web_search_result\"}]"
        );

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                AnthropicTools.WebSearch20250305OutputSchema(),
                document.RootElement
            )
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should accept valid response with string title",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_accept_valid_response_with_string_title()
    {
        using var document = JsonDocument.Parse(
            "[{\"url\":\"https://test.com\",\"title\":\"Test title\",\"pageAge\":\"April 30, 2025\",\"encryptedContent\":\"EqgfCioIARgBIiQ3YTAwMjY1Mi1mZjM5LTQ1NGUtODgxNC1kNjNjNTk1ZWI3Y...\",\"type\":\"web_search_result\"}]"
        );

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                AnthropicTools.WebSearch20250305OutputSchema(),
                document.RootElement
            )
        );
    }
}
