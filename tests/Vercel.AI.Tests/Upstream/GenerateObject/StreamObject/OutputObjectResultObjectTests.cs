// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; output = "object" &gt; result.object</c>.</summary>
[Collection("LogWarnings")]
public sealed class OutputObjectResultObjectTests
{
    private const string Id = Prefix + "output = \"object\" > result.object::";

    private static StreamObjectTestModel InvalidModel() =>
        Model(TextStream("{ ", "\"invalid\": ", "\"Hello, ", "world", "!\"", " }"));

    [Fact]
    [UpstreamTest(Id + "should resolve with typed object", Coverage = UpstreamCoverage.Covered)]
    public async Task Should_resolve_with_typed_object()
    {
        var result = Client()
            .StreamObjectAsync(
                ObjectOptions(
                    Model(TextStream("{ ", "\"content\": ", "\"Hello, ", "world", "!\"", " }"))
                )
            );

        // consume stream (runs in parallel)
        _ = Collect(result.PartialObjectStream());

        Assert.Equal("Hello, world!", (await result.Object).Content);
    }

    [Fact]
    [UpstreamTest(
        Id + "should reject object promise when the streamed object does not match the schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_reject_object_promise_when_the_streamed_object_does_not_match_the_schema()
    {
        var result = Client().StreamObjectAsync(ObjectOptions(InvalidModel()));

        // consume stream (runs in parallel)
        _ = Collect(result.PartialObjectStream());

        await Assert.ThrowsAsync<NoObjectGeneratedException>(() => result.Object);
    }

    [Fact]
    [UpstreamTest(
        Id
            + "should not lead to unhandled promise rejections when the streamed object does not match the schema",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_lead_to_unhandled_promise_rejections_when_the_streamed_object_does_not_match_the_schema()
    {
        var unobserved = new List<Exception>();
        void OnUnobserved(object? sender, UnobservedTaskExceptionEventArgs args)
        {
            // other tests share the process: only NoObjectGeneratedException of this test counts
            if (
                args
                    .Exception.Flatten()
                    .InnerExceptions.Any(e =>
                        e is NoObjectGeneratedException { Text: { } text }
                        && text.Contains("invalid")
                    )
            )
            {
                unobserved.Add(args.Exception);
            }
        }
        TaskScheduler.UnobservedTaskException += OnUnobserved;
        try
        {
            var result = Client().StreamObjectAsync(ObjectOptions(InvalidModel()));

            // consume stream (runs in parallel)
            await Collect(result.PartialObjectStream());
            await Task.Delay(50);

            result = null!;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.Empty(unobserved);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= OnUnobserved;
        }
    }
}
