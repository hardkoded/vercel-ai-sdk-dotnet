// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "enum"</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputEnumTests
{
    private const string Id = Prefix + "output = \"enum\"::";

    private static StreamObjectOptions<string> EnumOptions(
        ILanguageModel model,
        params string[] values
    ) =>
        new()
        {
            Model = model,
            Output = StreamObjectOutput.Enum,
            EnumValues = values,
            Prompt = "prompt",
        };

    [Fact]
    [UpstreamTest(Id + "should stream an enum value", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_stream_an_enum_value()
    {
        var model = CreateTestModel([
            new TextStartStreamPart("1"),
            Delta("{ "),
            Delta("\"result\": "),
            Delta("\"su"),
            Delta("nny"),
            Delta("\""),
            Delta(" }"),
            new TextEndStreamPart("1"),
            Finish(),
        ]);

        var result = Client().StreamObjectAsync(EnumOptions(model, "sunny", "rainy", "snowy"));

        Assert.Equal(["\"sunny\""], await CollectRaw(result.PartialObjectStream()));
        AssertJsonEqual(
            "{\"$schema\":\"http://json-schema.org/draft-07/schema#\",\"additionalProperties\":false,\"properties\":{\"result\":{\"enum\":[\"sunny\",\"rainy\",\"snowy\"],\"type\":\"string\"}},\"required\":[\"result\"],\"type\":\"object\"}",
            model.Calls[0].JsonSchema!.Value
        );
        Assert.Null(model.Calls[0].JsonSchemaName);
        Assert.Null(model.Calls[0].JsonSchemaDescription);
    }

    [Fact]
    [UpstreamTest(Id + "should not stream incorrect values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_not_stream_incorrect_values()
    {
        var model = Model([
            new TextStartStreamPart("1"),
            Delta("{ "),
            Delta("\"result\": "),
            Delta("\"foo"),
            Delta("bar"),
            Delta("\""),
            Delta(" }"),
            new TextEndStreamPart("1"),
            Finish(),
        ]);

        var result = Client().StreamObjectAsync(EnumOptions(model, "sunny", "rainy", "snowy"));

        Assert.Empty(await Collect(result.PartialObjectStream()));
    }

    [Fact]
    [UpstreamTest(Id + "should handle ambiguous values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_ambiguous_values()
    {
        var model = CreateTestModel([
            new TextStartStreamPart("1"),
            Delta("{ "),
            Delta("\"result\": "),
            Delta("\"foo"),
            Delta("bar"),
            Delta("\""),
            Delta(" }"),
            Finish(),
        ]);

        var result = Client().StreamObjectAsync(EnumOptions(model, "foobar", "foobar2"));

        Assert.Equal(["\"foo\"", "\"foobar\""], await CollectRaw(result.PartialObjectStream()));
    }

    [Fact]
    [UpstreamTest(Id + "should handle non-ambiguous values", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_handle_non_ambiguous_values()
    {
        var model = CreateTestModel([
            new TextStartStreamPart("1"),
            Delta("{ "),
            Delta("\"result\": "),
            Delta("\"foo"),
            Delta("bar"),
            Delta("\""),
            Delta(" }"),
            new TextEndStreamPart("1"),
            Finish(),
        ]);

        var result = Client().StreamObjectAsync(EnumOptions(model, "foobar", "barfoo"));

        Assert.Equal(["\"foobar\""], await CollectRaw(result.PartialObjectStream()));
    }
}
