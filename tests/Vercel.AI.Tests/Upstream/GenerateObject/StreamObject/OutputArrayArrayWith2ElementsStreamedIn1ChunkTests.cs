// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "array" &gt; array with 2 elements streamed in 1 chunk</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputArrayArrayWith2ElementsStreamedIn1ChunkTests
{
    private const string Id =
        Prefix + "output = \"array\" > array with 2 elements streamed in 1 chunk::";

    private readonly StreamObjectResult<List<ContentResult>> _result;
    private List<ContentResult>? _onFinishObject;
    private readonly TaskCompletionSource _finished = new();

    public OutputArrayArrayWith2ElementsStreamedIn1ChunkTests()
    {
        var options = new StreamObjectOptions<List<ContentResult>>
        {
            Model = CreateTestModel([
                new TextStartStreamPart("1"),
                Delta("{\"elements\":[{\"content\":\"element 1\"},{\"content\":\"element 2\"}]}"),
                new TextEndStreamPart("1"),
                Finish(),
            ]),
            Schema = ContentSchema,
            Output = StreamObjectOutput.Array,
            Prompt = "prompt",
            SerializerOptions = Json,
            OnFinish = (@event, _) =>
            {
                _onFinishObject = @event.Object;
                _finished.TrySetResult();
                return Task.CompletedTask;
            },
        };
        _result = Client().StreamObjectAsync(options);
    }

    private static string[] Contents(IEnumerable<ContentResult> values) =>
        values.Select(v => v.Content).ToArray();

    [Fact]
    [UpstreamTest(
        Id + "should stream only complete objects in partialObjectStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_only_complete_objects_in_partialObjectStream()
    {
        Assert.Equal(
            ["[{\"content\":\"element 1\"},{\"content\":\"element 2\"}]"],
            await CollectRaw(_result.PartialObjectStream())
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should stream only complete objects in textStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_only_complete_objects_in_textStream()
    {
        Assert.Equal(
            ["[{\"content\":\"element 1\"},{\"content\":\"element 2\"}]"],
            await Collect(_result.TextStream())
        );
    }

    [Fact]
    [UpstreamTest(
        Id + "should have the correct object result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_have_the_correct_object_result()
    {
        // consume stream
        await Collect(_result.PartialObjectStream());

        Assert.Equal(["element 1", "element 2"], Contents(await _result.Object));
    }

    [Fact]
    [UpstreamTest(
        Id + "should call onFinish callback with full array",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onFinish_callback_with_full_array()
    {
        await _finished.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(["element 1", "element 2"], Contents(_onFinishObject!));
    }

    [Fact]
    [UpstreamTest(
        Id + "should stream elements individually in elementStream",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_stream_elements_individually_in_elementStream()
    {
        Assert.Equal(
            ["{\"content\":\"element 1\"}", "{\"content\":\"element 2\"}"],
            await CollectRaw(_result.ElementStream())
        );
    }
}
