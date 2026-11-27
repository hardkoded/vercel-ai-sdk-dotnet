// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class WebFetch20250910OutputSchemaTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::webFetch_20250910OutputSchema::";

    [Fact]
    [UpstreamTest(
        Prefix + "should not fail validation when title is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_fail_validation_when_title_is_null()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrievedAt\":\"2025-12-08T20:46:31.114158\",\"content\":{\"type\":\"document\",\"title\":null,\"source\":{\"type\":\"text\",\"mediaType\":\"text/plain\",\"data\":\"\"}}}"
        );

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                AnthropicTools.WebFetch20250910OutputSchema(),
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
            "{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrievedAt\":\"2025-12-08T20:46:31.114158\",\"content\":{\"type\":\"document\",\"title\":\"Example Title\",\"source\":{\"type\":\"text\",\"mediaType\":\"text/plain\",\"data\":\"Some content\"}}}"
        );

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                AnthropicTools.WebFetch20250910OutputSchema(),
                document.RootElement
            )
        );
    }
}
