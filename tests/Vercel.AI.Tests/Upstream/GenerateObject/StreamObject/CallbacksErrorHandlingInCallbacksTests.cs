// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; callbacks &gt; error handling in callbacks</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksErrorHandlingInCallbacksTests
{
    private const string Id = Prefix + "callbacks > error handling in callbacks::";

    [Fact]
    [UpstreamTest(
        Id + "should not break the stream when a callback throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_the_stream_when_a_callback_throws()
    {
        var options = ObjectOptions(Model(TextStream("{ \"content\": \"Hello, world!\" }")));
        options.OnStart = (_, _) => throw new InvalidOperationException("onStart error");
        options.OnStepStart = (_, _) => throw new InvalidOperationException("onStepStart error");
        options.OnStepEnd = (_, _) => throw new InvalidOperationException("onStepFinish error");
        var result = Client().StreamObjectAsync(options);

        var objects = await Collect(result.PartialObjectStream());

        Assert.NotEmpty(objects);
        Assert.Equal("Hello, world!", (await result.Object).Content);
    }
}
