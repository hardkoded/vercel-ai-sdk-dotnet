// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; callbacks &gt; onStepStart</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnStepStartTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > callbacks > onStepStart::";

    [Fact]
    [UpstreamTest(
        Id + "should call onStepStart before the model call",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepStart_before_the_model_call()
    {
        var events = new List<string>();
        var model = new GenerateObjectTestModel { OnGenerate = () => events.Add("doGenerate") };
        var options = ObjectOptions(model);
        options.OnStepStart = (_, _) =>
        {
            events.Add("onStepStart");
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal(["onStepStart", "doGenerate"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should provide stepNumber 0 and model info",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_provide_stepNumber_0_and_model_info()
    {
        GenerateObjectStepStartEvent? stepStartEvent = null;
        var options = ObjectOptions(
            new GenerateObjectTestModel { Provider = "test-provider", ModelId = "test-model" }
        );
        options.OnStepStart = (e, _) =>
        {
            stepStartEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.NotNull(stepStartEvent);
        Assert.Equal(0, stepStartEvent.StepNumber);
        Assert.Equal("test-provider", stepStartEvent.Provider);
        Assert.Equal("test-model", stepStartEvent.ModelId);
        Assert.NotNull(stepStartEvent.CallId);
        Assert.NotNull(stepStartEvent.PromptMessages);
    }
}
