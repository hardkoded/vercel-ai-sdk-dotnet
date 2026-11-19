// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; callbacks &gt; onStart</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnStartTests
{
    private const string Id = Prefix + "callbacks > onStart::";

    [Fact]
    [UpstreamTest(
        Id + "should call onStart before the model call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStart_before_the_model_call()
    {
        var events = new List<string>();
        var model = Model(TextStream("{ \"content\": \"Hello, world!\" }"));
        var inner = model.OnStream;
        model.OnStream = options =>
        {
            events.Add("doStream");
            return inner(options);
        };
        var options = ObjectOptions(model);
        options.OnStart = (_, _) =>
        {
            events.Add("onStart");
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.Equal(["onStart", "doStream"], events);
    }
}
