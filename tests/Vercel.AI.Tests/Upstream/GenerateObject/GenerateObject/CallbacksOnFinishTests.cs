// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject.GenerateObjectTestSupport;

namespace Vercel.AI.Tests.Upstream.GenerateObject.GenerateObject;

/// <summary>Port of <c>generate-object.test.ts</c> &gt; <c>generateObject &gt; callbacks &gt; onFinish</c>.</summary>
[Collection("LogWarnings")]
public sealed class CallbacksOnFinishTests
{
    private const string Id =
        "packages/ai/src/generate-object/generate-object.test.ts::generateObject > callbacks > onFinish::";

    [Fact]
    [UpstreamTest(
        Id + "should call onFinish after parsing with the typed object",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_call_onFinish_after_parsing_with_the_typed_object()
    {
        var events = new List<string>();
        var model = new GenerateObjectTestModel { OnGenerate = () => events.Add("doGenerate") };
        var options = ObjectOptions(model);
        options.OnFinish = (_, _) =>
        {
            events.Add("onFinish");
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal(["doGenerate", "onFinish"], events);
    }

    [Fact]
    [UpstreamTest(
        Id + "should provide the parsed object and metadata",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_provide_the_parsed_object_and_metadata()
    {
        GenerateObjectEndEvent<ContentResult>? finishEvent = null;
        var metadata = System.Text.Json.JsonSerializer.SerializeToElement(
            new Dictionary<string, object> { ["test"] = new { key = "value" } }
        );
        var model = new GenerateObjectTestModel
        {
            Provider = "test-provider",
            ModelId = "test-model",
            Generate = _ => new LanguageModelGenerateResult(
                [new GeneratedText("{ \"content\": \"Hello, world!\" }")],
                FinishReason.Stop,
                new LanguageModelUsage(10, 20, 30),
                providerMetadata: metadata
            ),
        };
        var options = ObjectOptions(model);
        options.OnFinish = (e, _) =>
        {
            finishEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.NotNull(finishEvent);
        Assert.Equal("Hello, world!", finishEvent.Object.Content);
        Assert.Equal(FinishReason.Stop, finishEvent.FinishReason);
        Assert.Equal(10, finishEvent.Usage.InputTokens);
        Assert.Equal(20, finishEvent.Usage.OutputTokens);
        Assert.Equal(30, finishEvent.Usage.TotalTokens);
        Assert.Equal(metadata.GetRawText(), finishEvent.ProviderMetadata!.Value.GetRawText());
        Assert.NotNull(finishEvent.CallId);
    }

    [Fact]
    [UpstreamTest(
        Id + "should include reasoning in finish event",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_include_reasoning_in_finish_event()
    {
        GenerateObjectEndEvent<ContentResult>? finishEvent = null;
        var model = new GenerateObjectTestModel
        {
            Generate = _ =>
                Result("{ \"content\": \"Hello\" }", new GeneratedReasoning("thinking...")),
        };
        var options = ObjectOptions(model);
        options.OnFinish = (e, _) =>
        {
            finishEvent = e;
            return Task.CompletedTask;
        };

        await Client().GenerateObjectAsync(options);

        Assert.Equal("thinking...", finishEvent!.Reasoning);
    }
}
