// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tests.Upstream.ProviderUtils.StringifyWithBigIntAndCycles;

/// <summary>Port of <c>stringify-with-bigint-and-cycles.test.ts</c> &gt; <c>stringifyWithBigIntAndCycles</c>.</summary>
public sealed class StringifyWithBigIntAndCyclesTests
{
    private const string Prefix =
        "packages/provider-utils/src/stringify-with-bigint-and-cycles.test.ts::stringifyWithBigIntAndCycles::";

    [Fact]
    [UpstreamTest(
        Prefix + "preserves repeated references while replacing ancestor cycles",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_repeated_references_while_replacing_ancestor_cycles()
    {
        var shared = new Dictionary<string, object?> { ["value"] = 42 };
        var input = new Dictionary<string, object?> { ["first"] = shared, ["second"] = shared };
        input["self"] = input;
        Assert.Equal(
            "{\"first\":{\"value\":42},\"second\":{\"value\":42},\"self\":\"[Circular]\"}",
            StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(input)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves repeated array items and nested cycles",
        Coverage = UpstreamCoverage.Covered
    )]
    public void Preserves_repeated_array_items_and_nested_cycles()
    {
        var shared = new Dictionary<string, object?>
        {
            ["count"] = BigInteger.Parse("9007199254740993"),
        };
        shared["self"] = shared;
        Assert.Equal(
            "[{\"count\":\"9007199254740993\",\"self\":\"[Circular]\"},{\"count\":\"9007199254740993\",\"self\":\"[Circular]\"}]",
            StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(
                new object[] { shared, shared }
            )
        );
    }

    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("\"hello\"")]
    [InlineData("[1,2]")]
    [UpstreamTest(
        Prefix + "preserves normal JSON behavior for %j",
        Coverage = UpstreamCoverage.Partial
    )]
    public void Preserves_normal_JSON_behavior(string json)
    {
        object? value = System.Text.Json.Nodes.JsonNode.Parse(json);
        Assert.Equal(json, StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(value));
    }
}
