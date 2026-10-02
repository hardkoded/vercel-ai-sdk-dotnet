// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class ParsePartialJsonTests
{
    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should handle nullish input",
        Coverage = UpstreamCoverage.Covered)]
    public void Returns_undefined_input_for_null()
    {
        var result = JsonRepair.ParsePartialJson(null);
        Assert.Null(result.Value);
        Assert.Equal("undefined-input", result.State);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should parse valid JSON",
        Coverage = UpstreamCoverage.Covered)]
    public void Parses_valid_json()
    {
        var result = JsonRepair.ParsePartialJson("{\"key\": \"value\"}");
        Assert.Equal("successful-parse", result.State);
        Assert.Equal("value", result.Value!["key"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should repair and parse partial JSON",
        Coverage = UpstreamCoverage.Covered)]
    public void Repairs_and_parses_partial_json()
    {
        var result = JsonRepair.ParsePartialJson("{\"key\": \"value\"");
        Assert.Equal("repaired-parse", result.State);
        Assert.Equal("value", result.Value!["key"]!.GetValue<string>());
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should handle invalid JSON that cannot be repaired",
        Coverage = UpstreamCoverage.Covered)]
    public void Fails_when_the_text_cannot_be_repaired()
    {
        var result = JsonRepair.ParsePartialJson("not json at all");
        Assert.Equal("failed-parse", result.State);
        Assert.Null(result.Value);
    }
}
