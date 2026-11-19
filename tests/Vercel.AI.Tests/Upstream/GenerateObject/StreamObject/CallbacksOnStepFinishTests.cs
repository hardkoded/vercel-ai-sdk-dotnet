// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.StreamObject.StreamObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.StreamObject;

/// <summary>Port of <c>stream-object.test.ts</c> &gt; <c>streamObject &gt; callbacks &gt; onStepFinish</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnStepFinishTests
{
    private const string Id = Prefix + "callbacks > onStepFinish::";

    [Fact]
    [UpstreamTest(
        Id + "should call onStepFinish after streaming completes",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onStepFinish_after_streaming_completes()
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
        options.OnStepEnd = (_, _) =>
        {
            events.Add("onStepFinish");
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.Contains("doStream", events);
        Assert.Contains("onStepFinish", events);
        Assert.True(events.IndexOf("doStream") < events.IndexOf("onStepFinish"));
    }

    [Fact]
    [UpstreamTest(
        Id + "should provide the raw objectText and usage",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_provide_the_raw_objectText_and_usage()
    {
        GenerateObjectStepEndEvent? stepFinishEvent = null;
        var model = Model(TextStream("{ \"content\": \"Hello, world!\" }"));
        var options = ObjectOptions(
            new StreamObjectTestModel
            {
                Provider = "test-provider",
                ModelId = "test-model",
                OnStream = model.OnStream,
            }
        );
        options.OnStepEnd = (@event, _) =>
        {
            stepFinishEvent = @event;
            return Task.CompletedTask;
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.Equal(0, stepFinishEvent!.StepNumber);
        Assert.Equal("test-provider", stepFinishEvent.Provider);
        Assert.Equal("test-model", stepFinishEvent.ModelId);
        Assert.Equal("{ \"content\": \"Hello, world!\" }", stepFinishEvent.ObjectText);
        Assert.Equal(FinishReason.Stop, stepFinishEvent.FinishReason);
        Assert.Equal(TestUsage.InputTokens, stepFinishEvent.Usage.InputTokens);
        Assert.Equal(TestUsage.NoCacheInputTokens, stepFinishEvent.Usage.NoCacheInputTokens);
        Assert.Equal(TestUsage.OutputTokens, stepFinishEvent.Usage.OutputTokens);
        Assert.Equal(TestUsage.TextTokens, stepFinishEvent.Usage.TextTokens);
        Assert.Equal(TestUsage.TotalTokens, stepFinishEvent.Usage.TotalTokens);
        Assert.NotNull(stepFinishEvent.CallId);
    }
}
