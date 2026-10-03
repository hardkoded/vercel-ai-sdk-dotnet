// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Util;

namespace Vercel.AI.Tests;

public sealed class PartialJsonTests
{
    [UpstreamTest("packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should handle nullish input", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Handles_nullish_input()
    {
        var result = await PartialJson.ParseAsync(null);
        Assert.Null(result.Value);
        Assert.Equal(PartialJson.UndefinedInput, result.State);
    }

    [UpstreamTest("packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should parse valid JSON", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Parses_valid_json()
    {
        var result = await PartialJson.ParseAsync("{\"key\": \"value\"}");
        Assert.Equal(PartialJson.SuccessfulParse, result.State);
        Assert.NotNull(result.Value);
        Assert.Equal("value", result.Value!["key"]!.GetValue<string>());
    }

    [UpstreamTest("packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should repair and parse partial JSON", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Repairs_partial_json()
    {
        var result = await PartialJson.ParseAsync("{\"key\": \"value\"");
        Assert.Equal(PartialJson.RepairedParse, result.State);
        Assert.NotNull(result.Value);
        Assert.Equal("value", result.Value!["key"]!.GetValue<string>());
    }

    [UpstreamTest("packages/ai/src/util/parse-partial-json.test.ts::parsePartialJson::should handle invalid JSON that cannot be repaired", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public async Task Fails_when_text_cannot_be_repaired()
    {
        var result = await PartialJson.ParseAsync("not json at all");
        Assert.Null(result.Value);
        Assert.Equal(PartialJson.FailedParse, result.State);
    }
}
