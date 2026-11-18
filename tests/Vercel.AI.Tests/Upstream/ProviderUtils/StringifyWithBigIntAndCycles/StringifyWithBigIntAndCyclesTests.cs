// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
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
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
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

    [Fact]
    [UpstreamTest(
        Prefix + "preserves toJSON semantics and serializes BigInt returned by toJSON",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
    )]
    public void Preserves_toJSON_semantics_and_serializes_BigInt_returned_by_toJSON()
    {
        // A JsonConverter is the .NET toJSON.
        var input = new Dictionary<string, object?>
        {
            ["date"] = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc),
            ["count"] = new CountToJson(),
            ["nested"] = new NestedToJson(),
        };
        Assert.Equal(
            "{\"date\":\"2026-11-02T00:00:00.000Z\",\"count\":\"42\",\"nested\":{\"count\":\"43\"}}",
            StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(input)
        );
    }

    [Theory]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("\"hello\"")]
    [InlineData("[1,2]")]
    [InlineData("{\"value\":1}")]
    [UpstreamTest(
        Prefix + "preserves normal JSON behavior for %j",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
    )]
    public void Preserves_normal_JSON_behavior(string json)
    {
        object? value = System.Text.Json.Nodes.JsonNode.Parse(json);
        if (json == "{\"value\":1}")
        {
            // { value: 1, absent: undefined }: a null POCO property is the undefined property.
            value = new WithAbsent { Value = 1, Absent = null };
        }

        Assert.Equal(json, StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(value));
    }

    [Fact]
    [UpstreamTest(
        "packages/provider-utils/src/stringify-with-bigint-and-cycles.test.ts::reports throwing toJSON methods instead of hiding serialization errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = "b05c249ca42b0653542f0f055af7a0df5e4e46e2"
    )]
    public void Reports_throwing_toJSON_methods_instead_of_hiding_serialization_errors()
    {
        var exception = Assert.ThrowsAny<Exception>(() =>
            StringifyWithBigIntAndCyclesUtil.StringifyWithBigIntAndCycles(new ThrowingToJson())
        );
        Assert.Contains("serialization failed", exception.ToString());
    }

    [JsonConverter(typeof(CountConverter))]
    private sealed class CountToJson;

    private sealed class CountConverter : JsonConverter<CountToJson>
    {
        public override CountToJson Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter w, CountToJson v, JsonSerializerOptions o) =>
            JsonSerializer.Serialize(w, new BigInteger(42), o);
    }

    [JsonConverter(typeof(NestedConverter))]
    private sealed class NestedToJson;

    private sealed class NestedConverter : JsonConverter<NestedToJson>
    {
        public override NestedToJson Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o) =>
            throw new NotSupportedException();

        public override void Write(Utf8JsonWriter w, NestedToJson v, JsonSerializerOptions o) =>
            JsonSerializer.Serialize(
                w,
                new Dictionary<string, object?> { ["count"] = new BigInteger(43) },
                o
            );
    }

    [JsonConverter(typeof(ThrowingConverter))]
    private sealed class ThrowingToJson;

    private sealed class ThrowingConverter : JsonConverter<ThrowingToJson>
    {
        public override ThrowingToJson Read(
            ref Utf8JsonReader r,
            Type t,
            JsonSerializerOptions o
        ) => throw new NotSupportedException();

        public override void Write(Utf8JsonWriter w, ThrowingToJson v, JsonSerializerOptions o) =>
            throw new InvalidOperationException("serialization failed");
    }

    private sealed class WithAbsent
    {
        [JsonPropertyName("value")]
        public int Value { get; init; }

        [JsonPropertyName("absent")]
        public int? Absent { get; init; }
    }
}
