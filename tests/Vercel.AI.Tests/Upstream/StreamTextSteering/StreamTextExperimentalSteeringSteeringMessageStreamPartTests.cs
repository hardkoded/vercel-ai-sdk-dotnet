// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using Vercel.AI.Ui;
using static Vercel.AI.Tests.Upstream.GenerateTextSteering.SteeringTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamTextSteering;

/// <summary>Port of <c>stream-text-steering.test.ts</c> &gt; <c>streamText experimental_steering</c> &gt; <c>steering message stream part</c>.</summary>
public sealed class StreamTextExperimentalSteeringSteeringMessageStreamPartTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text-steering.test.ts::streamText experimental_steering > steering message stream part::";
    private const string Commit = "b8c01eb49d82fd32efe8a2a1e4cd02fbb2b7ae97";
    private const string Prompt = "What is the weather in Paris?";

    private static async Task<UIMessage> ReadUiMessageAsync(StreamTextResult result)
    {
        var chunks = await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(result);
        UIMessage? message = null;
        await foreach (
            var uiMessage in UIMessageStreamConverter.ReadUIMessageStream(
                new ReadUIMessageStreamOptions(ToAsync(chunks))
            )
        )
        {
            message = uiMessage;
        }

        return message!;
    }

    private static async IAsyncEnumerable<JsonObject> ToAsync(JsonArray chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return (JsonObject)chunk!;
            await Task.Yield();
        }
    }

    private static async Task<List<TextStreamPart>> ReadPartsAsync(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream a delivered message before the output of its step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_stream_a_delivered_message_before_the_output_of_its_step()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(ToolCallStream, () =>
            TextStream("It is sunny in Paris, Texas.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    Tools =
                    [
                        Weather(() =>
                        {
                            Ignore(
                                steering.SteerAsync(
                                    "I meant Paris, Texas.",
                                    new SteerOptions { Id = "steer-1" }
                                )
                            );
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        var parts = await ReadPartsAsync(result);

        Assert.Equal(
            [
                "start",
                "start-step",
                "tool-call",
                "tool-result",
                "finish-step",
                "steering-message",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            ],
            parts.Select(part => part.Type)
        );
        var steeringPart = Assert.IsType<SteeringMessagePart>(
            parts.Single(part => part.Type == "steering-message")
        );
        Assert.Equal("steer-1", steeringPart.Id);
        Assert.Equal("I meant Paris, Texas.", Text(steeringPart.Message));
        Assert.Equal(1, steeringPart.StepNumber);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream a message sent before the stream starts before the first step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_stream_a_message_sent_before_the_stream_starts_before_the_first_step()
    {
        var steering = CreateSteering();
        Ignore(steering.SteerAsync("Use Celsius.", new SteerOptions { Id = "steer-1" }));

        var model = SteeringTestModel.Stream(() => TextStream("It is 20 degrees."));

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    ExperimentalSteering = steering,
                }
            );

        var types = (await ReadPartsAsync(result)).Select(part =>
            part is SteeringMessagePart steeringPart
                ? part.Type + ":" + steeringPart.StepNumber
                : part.Type
        );

        Assert.Equal(
            [
                "start",
                "steering-message:0",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            ],
            types
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should stream queued messages when they are delivered",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_stream_queued_messages_when_they_are_delivered()
    {
        var steering = CreateSteering();
        Ignore(
            steering.QueueAsync("Then update the readme.", new SteerOptions { Id = "queued-1" })
        );

        var model = SteeringTestModel.Stream(
            () => TextStream("It is sunny in Paris."),
            () => TextStream("The readme is updated.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        var types = (await ReadPartsAsync(result)).Select(part =>
            part is SteeringMessagePart steeringPart
                ? part.Type + ":" + steeringPart.Id + ":" + steeringPart.StepNumber
                : part.Type
        );

        Assert.Equal(
            [
                "start",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "steering-message:queued-1:1",
                "start-step",
                "text-start",
                "text-delta",
                "text-end",
                "finish-step",
                "finish",
            ],
            types
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should not stream messages that prepareStep removes",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_not_stream_messages_that_prepareStep_removes()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(ToolCallStream, () =>
            TextStream("It is sunny in Paris.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    Tools =
                    [
                        Weather(() =>
                        {
                            Ignore(steering.SteerAsync("I meant Paris, Texas."));
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                    PrepareStep = context => new PrepareStepUpdate
                    {
                        Messages = [.. context.InitialMessages, .. context.ResponseMessages],
                    },
                }
            );

        var types = (await ReadPartsAsync(result)).Select(part => part.Type);

        Assert.DoesNotContain("steering-message", types);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "should keep provider options of delivered messages in converted model messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_keep_provider_options_of_delivered_messages_in_converted_model_messages()
    {
        var steering = CreateSteering();
        var providerOptions = new Dictionary<string, JsonElement>
        {
            ["anthropic"] = JsonSerializer.SerializeToElement(
                new { cacheControl = new { type = "ephemeral" } }
            ),
        };
        Ignore(
            steering.SteerAsync(
                new UserModelMessage([
                    new TextContentPart("Use Celsius.") { ProviderOptions = providerOptions },
                ])
            )
        );

        var model = SteeringTestModel.Stream(() => TextStream("It is 20 degrees."));

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    ExperimentalSteering = steering,
                }
            );

        var message = await ReadUiMessageAsync(result);

        var steeringMessage = Assert.IsType<UserModelMessage>(
            UIMessageConversion.ConvertToModelMessages([message])[0]
        );
        var text = Assert.IsType<TextContentPart>(Assert.Single(steeringMessage.Content));
        Assert.Equal("Use Celsius.", text.Text);
        Assert.Equal(
            """{"anthropic":{"cacheControl":{"type":"ephemeral"}}}""",
            JsonSerializer.Serialize(text.ProviderOptions)
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix
            + "should add delivered messages to the ui message and keep them in converted model messages",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Should_add_delivered_messages_to_the_ui_message_and_keep_them_in_converted_model_messages()
    {
        var steering = CreateSteering();
        var model = SteeringTestModel.Stream(ToolCallStream, () =>
            TextStream("It is sunny in Paris, Texas.")
        );

        var result = Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = model,
                    Prompt = Prompt,
                    Tools =
                    [
                        Weather(() =>
                        {
                            Ignore(
                                steering.SteerAsync(
                                    "I meant Paris, Texas.",
                                    new SteerOptions { Id = "steer-1" }
                                )
                            );
                            return "sunny";
                        }),
                    ],
                    StopWhen = StopWhen.IsStepCount(5),
                    ExperimentalSteering = steering,
                }
            );

        var message = await ReadUiMessageAsync(result);

        Assert.Equal(
            ["step-start", "tool-weather", "steering-message", "step-start", "text"],
            message.Parts.Select(part => part.Type)
        );
        var steeringPart = Assert.IsType<SteeringMessageUIPart>(
            message.Parts.Single(part => part is SteeringMessageUIPart)
        );
        Assert.Equal("steer-1", steeringPart.Id);
        Assert.Equal(
            "I meant Paris, Texas.",
            Assert.IsType<TextUIPart>(Assert.Single(steeringPart.Parts)).Text
        );

        var modelMessages = UIMessageConversion.ConvertToModelMessages([
            new UIMessage(UIMessageRole.User, [new TextUIPart(Prompt)]) { Id = "user-1" },
            message,
        ]);

        Assert.Equal(
            ["user", "assistant", "tool", "user", "assistant"],
            modelMessages.Select(modelMessage => modelMessage.Role)
        );
        Assert.Equal(
            "I meant Paris, Texas.",
            Text(Assert.IsType<UserModelMessage>(modelMessages[3]))
        );
        // the model saw the same history before its final answer:
        Assert.Equal(
            modelMessages.Take(modelMessages.Count - 1).Select(modelMessage => modelMessage.Role),
            model.DoStreamCalls[1].Prompt.Select(promptMessage => promptMessage.Role)
        );
    }
}
