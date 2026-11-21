// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; callbacks &gt; onStepStart</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnStepStartTests
{
    private const string Id = Prefix + "callbacks > onStepStart::";

    [Fact]
    [UpstreamTest(
        Id + "should call onStepStart before the model call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepStart_before_the_model_call()
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
        options.OnStepStart = (_, _) =>
        {
            events.Add("onStepStart");
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.Equal(["onStepStart", "doStream"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should provide stepNumber 0 and model info",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_provide_stepNumber_0_and_model_info()
    {
        GenerateObjectStepStartEvent? stepStartEvent = null;
        var model = Model(TextStream("{ \"content\": \"Hello, world!\" }"));
        var options = ObjectOptions(
            new StreamObjectTestModel
            {
                Provider = "test-provider",
                ModelId = "test-model",
                OnStream = model.OnStream,
            }
        );
        options.OnStepStart = (e, _) =>
        {
            stepStartEvent = e;
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.NotNull(stepStartEvent);
        Assert.Equal(0, stepStartEvent.StepNumber);
        Assert.Equal("test-provider", stepStartEvent.Provider);
        Assert.Equal("test-model", stepStartEvent.ModelId);
        Assert.NotNull(stepStartEvent.CallId);
        Assert.NotNull(stepStartEvent.PromptMessages);
    }
}
