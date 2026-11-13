// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.GoogleJsonAccumulatorTests;

/// <summary>Port of <c>google-json-accumulator.test.ts</c> &gt; <c>GoogleJSONAccumulator</c> &gt; <c>array index bounds</c>.</summary>
public sealed class ArrayIndexBoundsTests
{
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
