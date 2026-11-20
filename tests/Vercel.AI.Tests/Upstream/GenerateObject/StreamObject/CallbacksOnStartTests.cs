// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.GenerateObject;
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

    [Fact]
    [UpstreamTest(
        Id + "should send correct information with text prompt",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_send_correct_information_with_text_prompt()
    {
        GenerateObjectStartEvent? startEvent = null;
        var model = Model(TextStream("{ \"content\": \"Hello, world!\" }"));
        var options = new StreamObjectOptions<ContentResult>
        {
            Model = new StreamObjectTestModel
            {
                Provider = "test-provider",
                ModelId = "test-model",
                OnStream = model.OnStream,
            },
            Schema = ContentSchema,
            SchemaName = "test-schema",
            SchemaDescription = "A test schema",
            Prompt = "test-prompt",
            Temperature = 0.5,
            MaxOutputTokens = 100,
            SerializerOptions = Json,
            GenerateId = () => "test-call-id",
            OnStart = (e, _) =>
            {
                startEvent = e;
                return Task.CompletedTask;
            },
        };

        var partialObjectStream = Client().StreamObjectAsync(options).PartialObjectStream();

        await Collect(partialObjectStream);

        Assert.NotNull(startEvent);
        Assert.Equal("test-call-id", startEvent.CallId);
        Assert.Null(startEvent.FrequencyPenalty);
        Assert.Null(startEvent.Headers);
        Assert.Equal(100, startEvent.MaxOutputTokens);
        Assert.Equal(2, startEvent.MaxRetries);
        Assert.Null(startEvent.Messages);
        Assert.Equal("test-model", startEvent.ModelId);
        Assert.Equal("ai.streamObject", startEvent.OperationId);
        Assert.Equal("object", startEvent.Output);
        Assert.Null(startEvent.PresencePenalty);
        Assert.Equal("test-prompt", startEvent.Prompt);
        Assert.Equal("test-provider", startEvent.Provider);
        Assert.Null(startEvent.ProviderOptions);
        AssertJsonEqual(ContentSchema, startEvent.Schema!.Value);
        Assert.Equal("A test schema", startEvent.SchemaDescription);
        Assert.Equal("test-schema", startEvent.SchemaName);
        Assert.Null(startEvent.Seed);
        Assert.Null(startEvent.System);
        Assert.Equal(0.5, startEvent.Temperature);
        Assert.Null(startEvent.TopK);
        Assert.Null(startEvent.TopP);
    }
}
