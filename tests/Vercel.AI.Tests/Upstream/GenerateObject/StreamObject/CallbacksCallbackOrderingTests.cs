// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; callbacks &gt; callback ordering</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksCallbackOrderingTests
{
    private const string Id = Prefix + "callbacks > callback ordering::";

    [Fact]
    [UpstreamTest(
        Id
            + "should fire callbacks in order: onStart -> onStepStart -> doStream -> onStepFinish -> onFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fire_callbacks_in_order_onStart_onStepStart_doStream_onStepFinish_onFinish()
    {
        var events = new List<string>();
        var model = Model(TextStream("{ \"content\": \"Hello, world!\" }"));
        var inner = model.OnStream;
        model.OnStream = call =>
        {
            events.Add("doStream");
            return inner(call);
        };
        var options = ObjectOptions(model);
        options.OnStart = (_, _) =>
        {
            events.Add("onStart");
            return Task.CompletedTask;
        };
        options.OnStepStart = (_, _) =>
        {
            events.Add("onStepStart");
            return Task.CompletedTask;
        };
        options.OnStepEnd = (_, _) =>
        {
            events.Add("onStepFinish");
            return Task.CompletedTask;
        };
        options.OnFinish = (_, _) =>
        {
            events.Add("onFinish");
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.Equal(["onStart", "onStepStart", "doStream", "onStepFinish", "onFinish"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should correlate all events with the same callId",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_correlate_all_events_with_the_same_callId()
    {
        var callIds = new List<string>();
        var options = ObjectOptions(Model(TextStream("{ \"content\": \"Hello, world!\" }")));
        options.OnStart = (@event, _) =>
        {
            callIds.Add(@event.CallId);
            return Task.CompletedTask;
        };
        options.OnStepStart = (@event, _) =>
        {
            callIds.Add(@event.CallId);
            return Task.CompletedTask;
        };
        options.OnStepEnd = (@event, _) =>
        {
            callIds.Add(@event.CallId);
            return Task.CompletedTask;
        };
        options.OnFinish = (@event, _) =>
        {
            callIds.Add(@event.CallId);
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.Equal(4, callIds.Count);
        Assert.Single(callIds.Distinct());
    }
}
