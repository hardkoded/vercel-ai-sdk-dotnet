// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Util;
using StreamParsers = Vercel.AI.Util.ArrayOutputStream;

namespace Vercel.AI.Tests.Upstream.Util.ArrayOutputStream;

/// <summary>Port of <c>array-output-stream.test.ts</c> &gt; <c>createArrayOutputStreamParser</c>.</summary>
public sealed class CreateArrayOutputStreamParserTests
{
    private const string Prefix =
        "packages/ai/src/util/array-output-stream.test.ts::createArrayOutputStreamParser::";

    private static readonly JsonNode NumberSchema = JsonNode.Parse("{\"type\":\"number\"}")!;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(10000)]
    [UpstreamTest(
        Prefix + "parses nested values and escaped strings in chunks of %i characters",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public void Parses_nested_values_and_escaped_strings_in_chunks(int chunkSize)
    {
        var values = JsonNode
            .Parse(
                "[{\"nested\":[\"a,]}\\\\\\\"\",{\"number\":-1.25e30}],\"unicode\":\"中文🌍\"},[false,null,[1,2]],-12.5e-3,\"escaped \\\\ and \\\" quotes\",true,null]"
            )!
            .AsArray();
        var validateCalls = 0;
        var parser = StreamParsers.CreateArrayOutputStreamParser(
            element =>
            {
                validateCalls++;
                return Vercel.AI.ProviderUtils.JsonParseResult.Ok(element, element);
            },
            true
        );
        var text = "{\"elements\":" + values.ToJsonString() + "}";
        var snapshots = new List<IReadOnlyList<JsonElement>>();
        var json = new System.Text.StringBuilder();
        for (var i = 0; i < text.Length; i += chunkSize)
        {
            var result = parser(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
            Assert.NotSame(ArrayOutputStreamParseResult.Fallback, result);
            if (result != null)
            {
                snapshots.Add(result.Partial);
                json.Append(result.TextDelta);
            }
        }

        Assert.True(
            JsonNode.DeepEquals(
                values,
                JsonSerializer.SerializeToNode(snapshots[snapshots.Count - 1])
            )
        );
        Assert.True(JsonNode.DeepEquals(values, JsonNode.Parse(json.ToString())));
        Assert.Equal(values.Count, validateCalls);

        // Earlier snapshots are immutable as the stream grows.
        var lengths = snapshots.Select(value => value.Count).ToArray();
        Assert.Equal(lengths.Distinct().OrderBy(length => length).ToArray(), lengths);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "withholds the last element until a new element starts or the object closes",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public void Withholds_the_last_element_until_a_new_element_starts_or_the_object_closes()
    {
        var parser = StreamParsers.CreateArrayOutputStreamParser(NumberSchema, true);

        var first = parser("{\"elements\":[1,")!;
        Assert.Empty(first.Partial);
        Assert.Equal("[", first.TextDelta);

        var second = parser("2")!;
        Assert.Equal(new[] { "1" }, second.Partial.Select(value => value.GetRawText()));
        Assert.Equal("1", second.TextDelta);

        Assert.Null(parser("]"));

        var last = parser("}")!;
        Assert.Equal(new[] { "1", "2" }, last.Partial.Select(value => value.GetRawText()));
        Assert.Equal(",2]", last.TextDelta);
    }

    [Theory]
    [InlineData("{\"other\":[],\"elements\":[1]}")]
    [InlineData("{\"elem\\u0065nts\":[1]}")]
    [InlineData("{\"elements\":[1,]}")]
    [InlineData("{\"elements\":[1e,2]}")]
    [InlineData("{\"elements\":[1],\"elements\":[2]}")]
    [InlineData("{\"elements\":[1]} trailing")]
    [UpstreamTest(
        Prefix + "falls back for unsupported or malformed JSON: %s",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public void Falls_back_for_unsupported_or_malformed_JSON(string text)
    {
        var parser = StreamParsers.CreateArrayOutputStreamParser(NumberSchema, true);
        Assert.Same(ArrayOutputStreamParseResult.Fallback, parser(text));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "preserves transformed elements and skips invalid elements when requested",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public void Preserves_transformed_elements_and_skips_invalid_elements_when_requested()
    {
        // The schema transforms a number into its string.
        var parser = StreamParsers.CreateArrayOutputStreamParser(
            element =>
                element.ValueKind == JsonValueKind.Number
                    ? Vercel.AI.ProviderUtils.JsonParseResult.Ok(
                        JsonSerializer.SerializeToElement(element.GetRawText()),
                        element
                    )
                    : Vercel.AI.ProviderUtils.JsonParseResult.Fail(
                        new Vercel.AI.ProviderUtils.TypeValidationException(
                            "Expected number",
                            element
                        ),
                        element
                    ),
            true
        );

        var result = parser("{\"elements\":[1,\"invalid\",2]}")!;
        Assert.Equal(
            new[] { "\"1\"", "\"2\"" },
            result.Partial.Select(value => value.GetRawText())
        );
        Assert.Equal("[\"1\",\"2\"]", result.TextDelta);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "falls back on schema errors for legacy array output",
        Coverage = UpstreamCoverage.Covered,
        Commit = "f630e0d735688a2e50c8d904436cb0f3ea25f33c"
    )]
    public void Falls_back_on_schema_errors_for_legacy_array_output()
    {
        var parser = StreamParsers.CreateArrayOutputStreamParser(NumberSchema, false);
        Assert.Same(
            ArrayOutputStreamParseResult.Fallback,
            parser("{\"elements\":[1,\"invalid\",2]}")
        );
    }
}
