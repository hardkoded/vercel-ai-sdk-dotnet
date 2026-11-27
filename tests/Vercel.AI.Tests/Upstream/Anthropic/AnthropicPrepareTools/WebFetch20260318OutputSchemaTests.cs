// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class WebFetch20260318OutputSchemaTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::webFetch_20260318OutputSchema::";

    [Fact]
    [UpstreamTest(
        Prefix + "should not fail validation when title is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_fail_validation_when_title_is_null()
    {
        using var document = JsonDocument.Parse(
            "{\"type\":\"web_fetch_result\",\"url\":\"https://test.com\",\"retrievedAt\":\"2026-09-15T20:00:00Z\",\"content\":{\"type\":\"document\",\"title\":null,\"source\":{\"type\":\"text\",\"mediaType\":\"text/plain\",\"data\":\"\"}}}"
        );

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                AnthropicTools.WebFetch20260318OutputSchema(),
                document.RootElement
            )
        );
    }
}
