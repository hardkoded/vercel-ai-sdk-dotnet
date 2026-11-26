// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; callbacks &gt; onStepFinish</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnStepFinishTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > callbacks > onStepFinish::";

    [Fact]
    [UpstreamTest(
        Id + "should call onStepFinish after the model call with raw result",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepFinish_after_the_model_call_with_raw_result()
    {
        var events = new List<string>();
        var model = new GenerateObjectTestModel { OnGenerate = () => events.Add("doGenerate") };
        var options = ObjectOptions(model);
        options.OnStepEnd = (_, _) =>
        {
            events.Add("onStepFinish");
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal(["doGenerate", "onStepFinish"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should provide the raw objectText and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_provide_the_raw_objectText_and_usage()
    {
        GenerateObjectStepEndEvent? stepFinishEvent = null;
        var options = ObjectOptions(
            new GenerateObjectTestModel { Provider = "test-provider", ModelId = "test-model" }
        );
        options.OnStepEnd = (e, _) =>
        {
            stepFinishEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.NotNull(stepFinishEvent);
        Assert.Equal(0, stepFinishEvent.StepNumber);
        Assert.Equal("test-provider", stepFinishEvent.Provider);
        Assert.Equal("test-model", stepFinishEvent.ModelId);
        Assert.Equal("{ \"content\": \"Hello, world!\" }", stepFinishEvent.ObjectText);
        Assert.Equal(FinishReason.Stop, stepFinishEvent.FinishReason);
        Assert.Equal(10, stepFinishEvent.Usage.InputTokens);
        Assert.Equal(20, stepFinishEvent.Usage.OutputTokens);
        Assert.Equal(30, stepFinishEvent.Usage.TotalTokens);
        Assert.NotNull(stepFinishEvent.CallId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include reasoning in step finish event",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_reasoning_in_step_finish_event()
    {
        GenerateObjectStepEndEvent? stepFinishEvent = null;
        var model = new GenerateObjectTestModel
        {
            Generate = _ =>
                Result("{ \"content\": \"Hello\" }", new GeneratedReasoning("thinking...")),
        };
        var options = ObjectOptions(model);
        options.OnStepEnd = (e, _) =>
        {
            stepFinishEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal("thinking...", stepFinishEvent!.Reasoning);
    }
}
