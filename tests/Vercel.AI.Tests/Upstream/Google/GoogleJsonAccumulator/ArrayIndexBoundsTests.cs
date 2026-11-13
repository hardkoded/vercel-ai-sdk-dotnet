// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.GoogleJsonAccumulatorTests;

/// <summary>Port of <c>google-json-accumulator.test.ts</c> &gt; <c>GoogleJSONAccumulator</c> &gt; <c>array index bounds</c>.</summary>
public sealed class ArrayIndexBoundsTests
{
    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip dotted indexes that leave a gap in an existing array", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_dotted_indexes_that_leave_a_gap_in_an_existing_array()
    {
        var (finalJson, text) = Accumulate(
            Str("$.items[0]", "a"),
            Str("$.items.3", "ignored"),
            Str("$.items[1]", "b"));
        Assert.Equal("{\"items\":[\"a\",\"b\"]}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip dotted indexes that leave a gap in nested arrays", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_dotted_indexes_that_leave_a_gap_in_nested_arrays()
    {
        var (finalJson, text) = Accumulate(
            Number("$.rows[0][0]", 1),
            Number("$.rows.0.3", 2),
            Str("$.rows.3.name", "ignored"),
            Number("$.rows[0][1]", 3));
        Assert.Equal("{\"rows\":[[1,3]]}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip dotted indexes that leave a gap in an empty array", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_dotted_indexes_that_leave_a_gap_in_an_empty_array()
    {
        var (finalJson, text) = Accumulate(
            new GooglePartialArgument { JsonPath = "$.items[]" },
            new GooglePartialArgument { JsonPath = "$.items.2" },
            Number("$.items[0]", 1));
        Assert.Equal("{\"items\":[1]}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should stream contiguous dotted indexes in existing arrays", Coverage = UpstreamCoverage.Covered)]
    public void Should_stream_contiguous_dotted_indexes_in_existing_arrays()
    {
        var (finalJson, text) = Accumulate(
            new GooglePartialArgument { JsonPath = "$.rows[0][0]", StringValue = "a", HasString = true, WillContinue = true },
            Str("$.rows.0.0", "b"),
            Str("$.rows.0.1", "c"),
            Str("$.rows.1[0]", "d"));
        Assert.Equal("{\"rows\":[[\"ab\",\"c\"],[\"d\"]]}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("3")]
    [InlineData("01")]
    [InlineData("-0")]
    [InlineData("1e0")]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should preserve dotted object key %s", Coverage = UpstreamCoverage.Covered)]
    public void Should_preserve_dotted_object_key(string key)
    {
        var (finalJson, text) = Accumulate(Str($"$.items.{key}.name", "a"));
        Assert.Equal($"{{\"items\":{{\"{key}\":{{\"name\":\"a\"}}}}}}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip an index that does not start a new array", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_an_index_that_does_not_start_a_new_array()
    {
        var (finalJson, text) = Accumulate(Number("$.items[20000000]", 1));
        Assert.Equal("{}", finalJson);
        Assert.Equal("{}", text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip an index that leaves a gap in an array", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_an_index_that_leaves_a_gap_in_an_array()
    {
        var (finalJson, text) = Accumulate(Str("$.items[0]", "a"), Str("$.items[5]", "b"), Str("$.items[1]", "c"));
        Assert.Equal("{\"items\":[\"a\",\"c\"]}", finalJson);
        Assert.Equal("{\"items\":[\"a\",\"c\"]}", text);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip an index that does not start an array replacing null", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_an_index_that_does_not_start_an_array_replacing_null()
    {
        var (finalJson, _) = Accumulate(
            new GooglePartialArgument { JsonPath = "$.items", HasNull = true },
            Number("$.items[20000000]", 1),
            new GooglePartialArgument { JsonPath = "$.rows[0]", HasNull = true },
            Number("$.rows[0][3000000]", 2));
        Assert.True(finalJson.Length < 100);
    }

    [Fact]
    [UpstreamTest("packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > array index bounds::should skip an index that leaves a gap in a nested array", Coverage = UpstreamCoverage.Covered)]
    public void Should_skip_an_index_that_leaves_a_gap_in_a_nested_array()
    {
        var (finalJson, text) = Accumulate(
            Number("$.rows[0][0]", 1),
            Number("$.rows[0][3000000]", 2),
            Str("$.rows[2000000].name", "x"));
        Assert.Equal("{\"rows\":[[1]]}", finalJson);
        Assert.Equal("{\"rows\":[[1]]}", text);
    }

    private static (string FinalJson, string Text) Accumulate(params GooglePartialArgument[] partialArgs)
    {
        var accumulator = new GoogleJsonAccumulator();
        var text = string.Empty;
        foreach (var arg in partialArgs)
        {
            text += accumulator.Process(new[] { arg }).TextDelta;
        }

        var result = accumulator.Finalize();
        return (result.FinalJson, text + result.ClosingDelta);
    }

    private static GooglePartialArgument Str(string path, string value)
    {
        return new GooglePartialArgument { JsonPath = path, StringValue = value, HasString = true };
    }

    private static GooglePartialArgument Number(string path, double value)
    {
        return new GooglePartialArgument { JsonPath = path, NumberValue = value };
    }
}
