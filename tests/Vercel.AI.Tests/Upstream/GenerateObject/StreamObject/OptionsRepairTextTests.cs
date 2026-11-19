// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; options.repairText</c>.</summary>
[Collection("LogWarnings")]
public sealed class OptionsRepairTextTests
{
    private const string Id = Prefix + "options.repairText::";

    private static StreamObjectTestModel RepairModel(params string[] deltas) =>
        Model([ResponseMetadata(), .. TextStream(deltas)]);

    [Fact]
    [UpstreamTest(
        Id + "should be able to repair a JSONParseError",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_repair_a_JSONParseError()
    {
        var options = ObjectOptions(RepairModel("{ \"content\": \"provider metadata test\" "));
        options.RepairText = (context, _) =>
        {
            Assert.IsType<JsonParseException>(context.Error);
            Assert.Equal("{ \"content\": \"provider metadata test\" ", context.Text);
            return Task.FromResult<string?>(context.Text + "}");
        };
        var result = Client().StreamObjectAsync(options);

        // consume stream
        await Collect(result.PartialObjectStream());

        Assert.Equal("provider metadata test", (await result.Object).Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be able to repair a TypeValidationError",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_repair_a_TypeValidationError()
    {
        var options = ObjectOptions(RepairModel("{ \"content-a\": \"provider metadata test\" }"));
        options.RepairText = (context, _) =>
        {
            Assert.IsType<TypeValidationException>(context.Error);
            Assert.Equal("{ \"content-a\": \"provider metadata test\" }", context.Text);
            return Task.FromResult<string?>("{ \"content\": \"provider metadata test\" }");
        };
        var result = Client().StreamObjectAsync(options);

        // consume stream
        await Collect(result.PartialObjectStream());

        Assert.Equal("provider metadata test", (await result.Object).Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be able to handle repair that returns null",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_handle_repair_that_returns_null()
    {
        var options = ObjectOptions(RepairModel("{ \"content-a\": \"provider metadata test\" }"));
        options.RepairText = (context, _) =>
        {
            Assert.IsType<TypeValidationException>(context.Error);
            Assert.Equal("{ \"content-a\": \"provider metadata test\" }", context.Text);
            return Task.FromResult<string?>(null);
        };
        var result = Client().StreamObjectAsync(options);

        // consume stream
        await Collect(result.PartialObjectStream());

        var error = await Assert.ThrowsAsync<NoObjectGeneratedException>(() => result.Object);
        Assert.Contains("No object generated: response did not match schema.", error.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be able to repair JSON wrapped with markdown code blocks",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_be_able_to_repair_JSON_wrapped_with_markdown_code_blocks()
    {
        var options = ObjectOptions(RepairModel("```json\n{ \"content\": \"test message\" }\n```"));
        options.RepairText = (context, _) =>
        {
            Assert.IsType<JsonParseException>(context.Error);
            Assert.Equal("```json\n{ \"content\": \"test message\" }\n```", context.Text);

            // Remove markdown code block wrapper
            var cleaned = System.Text.RegularExpressions.Regex.Replace(
                System.Text.RegularExpressions.Regex.Replace(context.Text, "^```json\\s*", ""),
                "\\s*```$",
                ""
            );
            return Task.FromResult<string?>(cleaned);
        };
        var result = Client().StreamObjectAsync(options);

        // consume stream
        await Collect(result.PartialObjectStream());

        Assert.Equal("test message", (await result.Object).Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should throw NoObjectGeneratedError when parsing fails with repairText",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_throw_NoObjectGeneratedError_when_parsing_fails_with_repairText()
    {
        var options = ObjectOptions(RepairModel("{ broken json"));
        options.RepairText = (context, _) => Task.FromResult<string?>(context.Text + "{");
        var result = Client().StreamObjectAsync(options);

        var error = await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await Collect(result.PartialObjectStream());
            await result.Object;
        });

        var noObject = Assert.IsType<NoObjectGeneratedException>(error);
        Assert.Equal("No object generated: could not parse the response.", noObject.Message);
        Assert.Equal("id-0", noObject.Response.Id);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), noObject.Response.Timestamp);
        Assert.Equal("mock-model-id", noObject.Response.ModelId);
        Assert.Equal(13, noObject.Usage.TotalTokens);
        Assert.Equal(FinishReason.Stop, noObject.FinishReason);
    }
}
