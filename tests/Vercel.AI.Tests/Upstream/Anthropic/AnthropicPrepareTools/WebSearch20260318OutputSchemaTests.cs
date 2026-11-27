// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Anthropic;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.Anthropic.AnthropicPrepareTools;

public sealed class WebSearch20260318OutputSchemaTests
{
    private const string Prefix =
        "packages/anthropic/src/anthropic-prepare-tools.test.ts::webSearch_20260318OutputSchema::";

    [Fact]
    [UpstreamTest(
        Prefix + "should not fail validation when title is null",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Should_not_fail_validation_when_title_is_null()
    {
        using var document = JsonDocument.Parse(
            "[{\"url\":\"https://test.com\",\"title\":null,\"pageAge\":\"September 15, 2026\",\"encryptedContent\":\"encrypted-content\",\"type\":\"web_search_result\"}]"
        );

        Assert.Empty(
            ToolInputSchemaValidator.Validate(
                AnthropicTools.WebSearch20260318OutputSchema(),
                document.RootElement
            )
        );
    }
}
