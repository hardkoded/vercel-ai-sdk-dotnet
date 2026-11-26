// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; callbacks &gt; error handling in callbacks</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksErrorHandlingInCallbacksTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > callbacks > error handling in callbacks::";

    [Fact]
    [UpstreamTest(
        Id + "should not break the generation when a callback throws",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_not_break_the_generation_when_a_callback_throws()
    {
        var options = ObjectOptions(new GenerateObjectTestModel());
        options.OnStart = (_, _) => throw new InvalidOperationException("onStart error");
        options.OnStepStart = (_, _) => throw new InvalidOperationException("onStepStart error");
        options.OnStepEnd = (_, _) => throw new InvalidOperationException("onStepFinish error");
        options.OnFinish = (_, _) => throw new InvalidOperationException("onFinish error");

        var result = await Client().GenerateObjectAsync(options);

        Assert.Equal("Hello, world!", result.Object.Content);
    }
}
