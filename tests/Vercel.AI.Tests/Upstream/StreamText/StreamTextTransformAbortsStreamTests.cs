// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;
using static Vercel.AI.Tests.Upstream.StreamText.StreamTextTransformTestSupport;

namespace Vercel.AI.Tests.Upstream.StreamText;

/// <summary>Port of <c>stream-text.test.ts</c> &gt; <c>streamText</c> &gt; <c>options.transform</c> &gt; <c>with transformation that aborts stream</c>.</summary>
public sealed class StreamTextTransformAbortsStreamTests
{
    private const string Prefix =
        "packages/ai/src/generate-text/stream-text.test.ts::streamText > options.transform > with transformation that aborts stream::";

    // Maps to stopWordTransform.
    private static StreamTextTransform StopWord =>
        options =>
            Map(chunk =>
            {
                if (chunk is not TextDeltaPart delta)
                {
                    return [chunk];
                }

                if (!delta.Text.Contains("STOP"))
                {
                    return [chunk];
                }

                options.StopStream();
                var step = new StepResult(
                    string.Empty,
                    null,
                    [],
                    [],
                    FinishReason.Stop,
                    LanguageModelUsage.Null,
                    [],
                    provider: "mock-provider",
                    modelId: "mock-model-id"
                );
                return
                [
                    new StepFinishPart(
                        step,
                        new LanguageModelResponseMetadata(
                            "response-id",
                            DateTimeOffset.UnixEpoch,
                            "mock-model-id"
                        )
                    ),
                    new FinishPart(FinishReason.Stop, LanguageModelUsage.Null),
                ];
            });

    private static CallsModel StopModel() =>
        CallsModel.Single([
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello, "),
            new TextDeltaStreamPart("1", "STOP"),
            new TextDeltaStreamPart("1", " World"),
            new TextEndStreamPart("1"),
            Finish(),
        ]);

    private static CallsModel HelloModel() =>
        CallsModel.Single([
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", "Hello, "),
            new TextEndStreamPart("1"),
            Finish(),
        ]);

    [Fact]
    [UpstreamTest(
        Prefix + "stream should stop when STOP token is encountered",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the parts and the payload of finish-step and finish, not the performance numbers."
    )]
    public async Task Stream_should_stop_when_STOP_token_is_encountered()
    {
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = StopModel(),
                    Prompt = "test-input",
                    ExperimentalTransform = [StopWord],
                }
            );

        var parts = await PartsAsync(result);

        Assert.Equal(
            ["start", "start-step", "text-start", "text-delta", "finish-step", "finish"],
            parts.Select(part => part.Type)
        );
        Assert.Equal("Hello, ", Assert.IsType<TextDeltaPart>(parts[3]).Text);
        var stepFinish = Assert.IsType<StepFinishPart>(parts[4]);
        Assert.Equal(FinishReason.Stop, stepFinish.Step.FinishReason);
        Assert.Null(stepFinish.RawFinishReason);
        Assert.Equal("response-id", stepFinish.Response.Id);
        Assert.Equal("mock-model-id", stepFinish.Response.ModelId);
        Assert.Equal(DateTimeOffset.UnixEpoch, stepFinish.Response.Timestamp);
        var finish = Assert.IsType<FinishPart>(parts[5]);
        Assert.Equal(FinishReason.Stop, finish.FinishReason);
        Assert.Null(finish.Usage.InputTokens);
        Assert.Null(finish.Usage.OutputTokens);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "options.onStepEnd should be called",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Options_onStepEnd_should_be_called()
    {
        StepResult? received = null;
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = HelloModel(),
                    Prompt = "test-input",
                    OnStepEnd = (step, _) =>
                    {
                        received = step;
                        return Task.CompletedTask;
                    },
                    GenerateCallId = () => "test-telemetry-call-id",
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(0, received!.StepNumber);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "options.onStepEnd should be preferred over deprecated onStepFinish",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Options_onStepEnd_should_be_preferred_over_deprecated_onStepFinish()
    {
        var calls = new List<string>();
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = HelloModel(),
                    Prompt = "test-input",
                    OnStepEnd = (_, _) =>
                    {
                        calls.Add("onStepEnd");
                        return Task.CompletedTask;
                    },
#pragma warning disable CS0618 // OnStepFinish is the deprecated alias of OnStepEnd
                    OnStepFinish = (_, _) =>
                    {
                        calls.Add("onStepFinish");
                        return Task.CompletedTask;
                    },
#pragma warning restore CS0618
                }
            );

        await result.ConsumeStreamAsync();

        Assert.Equal(["onStepEnd"], calls);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "options.onStepFinish should be called",
        Coverage = UpstreamCoverage.Partial,
        Note = "Asserts the fields of the step as the port exposes them, not the full snapshot or the performance numbers."
    )]
    public async Task Options_onStepFinish_should_be_called()
    {
        StepResult? received = null;
        var result = SteeringTestSupport
            .Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = StopModel(),
                    Prompt = "test-input",
#pragma warning disable CS0618 // OnStepFinish is the deprecated alias of OnStepEnd
                    OnStepFinish = (step, _) =>
                    {
                        received = step;
                        return Task.CompletedTask;
                    },
#pragma warning restore CS0618
                    ExperimentalTransform = [StopWord],
                    GenerateCallId = () => "test-telemetry-call-id",
                }
            );

        await result.ConsumeStreamAsync();

        Assert.NotNull(received);
        Assert.Equal("test-telemetry-call-id", received!.CallId);
        var text = Assert.IsType<GeneratedText>(Assert.Single(received.Content));
        Assert.Equal("Hello, ", text.Text);
        Assert.Equal(FinishReason.Stop, received.FinishReason);
        Assert.Null(received.RawFinishReason);
        Assert.Equal("mock-provider", received.Provider);
        Assert.Equal("mock-model-id", received.ModelId);
        Assert.Equal("response-id", received.Response.Id);
        Assert.Equal(DateTimeOffset.UnixEpoch, received.Response.Timestamp);
        var message = Assert.IsType<AssistantModelMessage>(
            Assert.Single(received.Response.Messages)
        );
        Assert.Equal(
            "Hello, ",
            Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts)).Text
        );
        Assert.Equal(0, received.StepNumber);
        Assert.Null(received.Usage.InputTokens);
        Assert.Empty(received.Warnings);
    }
}
