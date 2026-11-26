// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; callbacks &gt; callback ordering</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksCallbackOrderingTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > callbacks > callback ordering::";

    [Fact]
    [UpstreamTest(
        Id + "should fire callbacks in order: onStart -> onStepStart -> onStepFinish -> onFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_fire_callbacks_in_order_onStart_onStepStart_onStepFinish_onFinish()
    {
        var events = new List<string>();
        var model = new GenerateObjectTestModel { OnGenerate = () => events.Add("doGenerate") };
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

        await Client().GenerateObjectAsync(options);

        Assert.Equal(["onStart", "onStepStart", "doGenerate", "onStepFinish", "onFinish"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should correlate all events with the same callId",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_correlate_all_events_with_the_same_callId()
    {
        var callIds = new List<string>();
        var options = ObjectOptions(new GenerateObjectTestModel());
        options.OnStart = (e, _) =>
        {
            callIds.Add(e.CallId);
            return Task.CompletedTask;
        };
        options.OnStepStart = (e, _) =>
        {
            callIds.Add(e.CallId);
            return Task.CompletedTask;
        };
        options.OnStepEnd = (e, _) =>
        {
            callIds.Add(e.CallId);
            return Task.CompletedTask;
        };
        options.OnFinish = (e, _) =>
        {
            callIds.Add(e.CallId);
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal(4, callIds.Count);
        Assert.Single(callIds.Distinct());
    }
}
