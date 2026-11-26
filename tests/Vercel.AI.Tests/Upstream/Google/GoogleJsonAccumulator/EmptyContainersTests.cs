// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Google;

namespace Vercel.AI.Tests.GoogleJsonAccumulatorTests;

/// <summary>Port of <c>google-json-accumulator.test.ts</c> &gt; <c>GoogleJSONAccumulator</c> &gt; <c>empty containers</c>.</summary>
public sealed class EmptyContainersTests
{
    private const string Prefix =
        "packages/google/src/google-json-accumulator.test.ts::GoogleJSONAccumulator > empty containers::";

    [Fact]
    [UpstreamTest(
        Prefix + "should keep empty arrays, empty objects, and nested empty arrays",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public void Should_keep_empty_arrays_empty_objects_and_nested_empty_arrays()
    {
        var (finalJson, text) = Accumulate(
            [Str("$.id", "x")],
            [Path("$.emptyArr[]")],
            [Path("$.emptyObj")],
            [Str("$.emptyStr", string.Empty)],
            [Path("$.nested.tags[]")],
            [new GooglePartialArgument { JsonPath = "$.zero", NumberValue = 0 }],
            [new GooglePartialArgument { JsonPath = "$.no", BoolValue = false }]
        );

        var expected =
            "{\"id\":\"x\",\"emptyArr\":[],\"emptyObj\":{},\"emptyStr\":\"\",\"nested\":{\"tags\":[]},\"zero\":0,\"no\":false}";
        Assert.Equal(expected, finalJson);
        Assert.Equal(expected, text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep an empty container as the last argument",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public void Should_keep_an_empty_container_as_the_last_argument()
    {
        var (finalJson, text) = Accumulate(
            [Str("$.id", "x", willContinue: true)],
            [Path("$.nested.tags[]")]
        );

        Assert.Equal("{\"id\":\"x\",\"nested\":{\"tags\":[]}}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore an array terminator after the array elements",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public void Should_ignore_an_array_terminator_after_the_array_elements()
    {
        var (finalJson, text) = Accumulate(
            [Str("$.tables[0]", "orders", willContinue: true)],
            [Path("$.tables[0]")],
            [Str("$.tables[1]", "customers")],
            [Path("$.tables[]")]
        );

        Assert.Equal("{\"tables\":[\"orders\",\"customers\"]}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore a string terminator",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public void Should_ignore_a_string_terminator()
    {
        var (finalJson, text) = Accumulate(
            [Str("$.location", "Boston", willContinue: true)],
            [Path("$.location")]
        );

        Assert.Equal("{\"location\":\"Boston\"}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should continue into an empty container when children follow",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public void Should_continue_into_an_empty_container_when_children_follow()
    {
        var (finalJson, text) = Accumulate(
            [Path("$.recipe")],
            [Str("$.recipe.name", "Lasagna")],
            [Path("$.recipe.steps[]")],
            [Str("$.recipe.steps[0]", "Cook.")]
        );

        Assert.Equal("{\"recipe\":{\"name\":\"Lasagna\",\"steps\":[\"Cook.\"]}}", finalJson);
        Assert.Equal(finalJson, text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should ignore value-less root paths",
        Coverage = UpstreamCoverage.Covered,
        Commit = "bbe2bd1163bd251ced45a72610af71a597eca4bf"
    )]
    public void Should_ignore_value_less_root_paths()
    {
        var (finalJson, text) = Accumulate([Path("$")], [Path("$[]")], [Str("$.id", "x")]);

        Assert.Equal("{\"id\":\"x\"}", finalJson);
        Assert.Equal(finalJson, text);
    }

    private static (string FinalJson, string Text) Accumulate(
        params GooglePartialArgument[][] chunks
    )
    {
        var accumulator = new GoogleJsonAccumulator();
        var text = string.Empty;
        foreach (var chunk in chunks)
        {
            text += accumulator.Process(chunk).TextDelta;
        }

        var result = accumulator.Finalize();
        return (result.FinalJson, text + result.ClosingDelta);
    }

    private static GooglePartialArgument Path(string path)
    {
        return new GooglePartialArgument { JsonPath = path };
    }

    private static GooglePartialArgument Str(string path, string value, bool willContinue = false)
    {
        return new GooglePartialArgument
        {
            JsonPath = path,
            StringValue = value,
            HasString = true,
            WillContinue = willContinue ? true : null,
        };
    }
}
