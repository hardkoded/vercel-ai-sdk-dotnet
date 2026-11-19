// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; options.onFinish</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectOptionsOnFinishTests
{
    private const string Id = Prefix + "output = \"object\" > options.onFinish::";

    private static void AssertUsage(LanguageModelUsage usage)
    {
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Equal(3, usage.NoCacheInputTokens);
        Assert.Equal(3, usage.InputTokens);
        Assert.Null(usage.ReasoningTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Null(usage.Raw);
        Assert.Equal(13, usage.TotalTokens);
    }

    private static void AssertResponse(LanguageModelResponseMetadata response)
    {
        Assert.Null(response.Headers);
        Assert.Equal("id-0", response.Id);
        Assert.Equal("mock-model-id", response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), response.Timestamp);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be called when a valid object is generated",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's undefined warnings is an empty list in the port."
    )]
    public async Task Should_be_called_when_a_valid_object_is_generated()
    {
        GenerateObjectEndEvent<ContentResult>? result = null;
        var options = ObjectOptions(
            Model([
                ResponseMetadata(),
                new TextStartStreamPart("1"),
                Delta("{ \"content\": \"Hello, world!\" }"),
                new TextEndStreamPart("1"),
                Finish(
                    JsonSerializer.SerializeToElement(
                        new { testProvider = new { testKey = "testValue" } }
                    )
                ),
            ])
        );
        options.OnFinish = (@event, _) =>
        {
            result = @event;
            return Task.CompletedTask;
        };
        options.GenerateId = () => "test-id";

        var stream = Client().StreamObjectAsync(options).PartialObjectStream();

        // consume stream
        await Collect(stream);

        Assert.Equal("test-id", result!.CallId);
        Assert.Null(result.Error);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Equal("Hello, world!", result.Object.Content);
        Assert.Equal(
            "{\"testProvider\":{\"testKey\":\"testValue\"}}",
            result.ProviderMetadata!.Value.GetRawText()
        );
        Assert.Null(result.Reasoning);
        Assert.Null(result.Request.Body);
        AssertResponse(result.Response);
        AssertUsage(result.Usage);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    [UpstreamTest(
        Id + "should be called when object doesn't match the schema",
        Coverage = UpstreamCoverage.Covered,
        Note = "Upstream's undefined warnings is an empty list in the port."
    )]
    public async Task Should_be_called_when_object_doesn_t_match_the_schema()
    {
        GenerateObjectEndEvent<ContentResult>? result = null;
        var options = ObjectOptions(
            Model([
                ResponseMetadata(),
                .. TextStream("{ ", "\"invalid\": ", "\"Hello, ", "world", "!\"", " }"),
            ])
        );
        options.OnFinish = (@event, _) =>
        {
            result = @event;
            return Task.CompletedTask;
        };
        options.GenerateId = () => "test-id";
        var streamResult = Client().StreamObjectAsync(options);

        // consume stream
        await Collect(streamResult.PartialObjectStream());

        // consume expected error rejection
        await Assert.ThrowsAsync<NoObjectGeneratedException>(() => streamResult.Object);

        Assert.Equal("test-id", result!.CallId);
        var error = Assert.IsType<NoObjectGeneratedException>(result.Error);
        Assert.Equal("No object generated: response did not match schema.", error.Message);
        Assert.Equal(FinishReason.Stop, result.FinishReason);
        Assert.Null(result.Object);
        Assert.Null(result.ProviderMetadata);
        Assert.Null(result.Reasoning);
        Assert.Null(result.Request.Body);
        AssertResponse(result.Response);
        AssertUsage(result.Usage);
        Assert.Empty(result.Warnings);
    }
}
