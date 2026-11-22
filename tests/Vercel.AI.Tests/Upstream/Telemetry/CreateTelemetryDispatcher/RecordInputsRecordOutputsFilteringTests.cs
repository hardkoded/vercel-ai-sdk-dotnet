// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Telemetry.CreateTelemetryDispatcher;

/// <summary>
/// Port of <c>create-telemetry-dispatcher.test.ts</c> &gt; <c>recordInputs / recordOutputs filtering</c>. The port dispatches
/// <c>onLanguageModelCallStart</c> where upstream's tests use <c>onStart</c>.
/// </summary>
public sealed class RecordInputsRecordOutputsFilteringTests
{
    private const string Prefix =
        "packages/ai/src/telemetry/create-telemetry-dispatcher.test.ts::recordInputs / recordOutputs filtering::";

    private static LanguageModelCallStartEvent PromptEvent() =>
        new()
        {
            CallId = "call-1",
            Instructions = Instructions.From("secret instructions"),
            Messages = [new UserModelMessage("secret prompt")],
            ModelId = "model-1",
        };

    private static async Task<LanguageModelCallStartEvent> Start(
        TelemetryOptions options,
        LanguageModelCallStartEvent e
    )
    {
        LanguageModelCallStartEvent? delivered = null;
        options.Integrations =
        [
            new TestTelemetry
            {
                OnLanguageModelCallStart = (d, _) =>
                {
                    delivered = d;
                    return Task.CompletedTask;
                },
            },
        ];
        await Vercel
            .AI.TelemetryDispatcher.Create(options)
            .OnLanguageModelCallStart!(e, CancellationToken.None);
        return delivered!;
    }

    [Fact]
    [UpstreamTest(
        Prefix + "strips input content from events sent to integrations when recordInputs is false",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Strips_input_content_from_events_sent_to_integrations_when_recordInputs_is_false()
    {
        var delivered = await Start(new TelemetryOptions { RecordInputs = false }, PromptEvent());

        Assert.Null(delivered.Instructions);
        Assert.Empty(delivered.Messages);
        Assert.Equal("model-1", delivered.ModelId);
        Assert.False(delivered.RecordInputs);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "strips output content when recordOutputs is false",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Strips_output_content_when_recordOutputs_is_false()
    {
        GenerateTextResult? delivered = null;
        var options = new TelemetryOptions
        {
            RecordOutputs = false,
            Integrations =
            [
                new TestTelemetry
                {
                    OnEnd = (r, _) =>
                    {
                        delivered = r;
                        return Task.CompletedTask;
                    },
                },
            ],
        };
        var usage = new LanguageModelUsage(1, 1, 2);
        var step = new StepResult(
            "secret reply",
            null,
            [],
            [],
            FinishReason.Stop,
            usage,
            [],
            content: [new GeneratedText("secret reply")]
        );
        var result = new GenerateTextResult(
            "secret reply",
            null,
            [step],
            FinishReason.Stop,
            usage,
            null,
            []
        )
        {
            ResponseMessages = [new AssistantModelMessage([new AssistantTextPart("secret reply")])],
        };

        await Vercel.AI.TelemetryDispatcher.Create(options).OnEnd!(result, CancellationToken.None);

        Assert.Empty(delivered!.Content);
        Assert.Equal(string.Empty, delivered.Text);
        Assert.Empty(delivered.ResponseMessages);
        Assert.Equal(1, delivered.Usage.InputTokens);
        Assert.Equal(1, delivered.Usage.OutputTokens);
        Assert.Equal(string.Empty, delivered.FinalStep.Text);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "keeps content when the flags are not set",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Keeps_content_when_the_flags_are_not_set()
    {
        var delivered = await Start(new TelemetryOptions(), PromptEvent());

        Assert.Equal("secret instructions", delivered.Instructions!.Text);
        Assert.Single(delivered.Messages);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not mutate the original event object",
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Does_not_mutate_the_original_event_object()
    {
        var original = PromptEvent();

        var delivered = await Start(new TelemetryOptions { RecordInputs = false }, original);

        Assert.Equal("secret instructions", original.Instructions!.Text);
        Assert.Single(original.Messages);
        Assert.Null(delivered.Instructions);
    }
}
