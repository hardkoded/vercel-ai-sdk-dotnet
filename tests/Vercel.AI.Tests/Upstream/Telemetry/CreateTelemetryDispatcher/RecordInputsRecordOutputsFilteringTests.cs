// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.Util;
using static Vercel.AI.Tests.Upstream.Operations.Extract.ExtractSupport;

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
                        delivered = (GenerateTextResult)r;
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

        var originalToolCall = new GeneratedToolCall("tc-1", "search", "{\"q\":\"secret\"}");
        ToolExecutionStartEvent? deliveredTool = null;
        var toolDispatcher = Vercel.AI.TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                RecordInputs = false,
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnToolExecutionStart = (d, _) =>
                        {
                            deliveredTool = d;
                            return Task.CompletedTask;
                        },
                    },
                ],
            }
        );
        await toolDispatcher.OnToolExecutionStart!(
            new ToolExecutionStartEvent(originalToolCall, []),
            CancellationToken.None
        );

        Assert.Equal("secret instructions", original.Instructions!.Text);
        Assert.Single(original.Messages);
        Assert.Null(delivered.Instructions);
        Assert.Equal("{\"q\":\"secret\"}", originalToolCall.ArgumentsJson);
        Assert.Equal("{}", deliveredTool!.ToolCall.ArgumentsJson);
        Assert.Equal("search", deliveredTool.ToolCall.ToolName);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "strips extraction fields and the reference date when recordInputs is false",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Strips_extraction_fields_and_the_reference_date_when_recordInputs_is_false()
    {
        ExtractStartEvent? delivered = null;
        var telemetry = TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnExtractStart = (item, _) =>
                        {
                            delivered = item;
                            return Task.CompletedTask;
                        },
                    },
                ],
                RecordInputs = false,
            }
        );

        await telemetry.OnExtractStart!(
            new ExtractStartEvent(
                "call-1",
                new Dictionary<string, object?>(),
                "mock-provider",
                "mock-model-id",
                DecisionState.From("secret invoice"),
                Fields(),
                "2026-11-18",
                null,
                null,
                2,
                null,
                OperationJson.Parse("{}")
            ),
            default
        );

        Assert.Equal(string.Empty, delivered!.State!.Text);
        Assert.Empty(delivered.Fields);
        Assert.Null(delivered.ReferenceDate);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "redacts download urls from errors sent to integrations",
        Coverage = UpstreamCoverage.Covered,
        Commit = "15bfd6b262f688cfb8281dbdb32c7dffe082337e"
    )]
    public async Task Redacts_download_urls_from_errors_sent_to_integrations()
    {
        TelemetryErrorEvent? delivered = null;
        var telemetry = TelemetryDispatcher.Create(
            new TelemetryOptions
            {
                Integrations =
                [
                    new TestTelemetry
                    {
                        OnError = (item, _) =>
                        {
                            delivered = item;
                            return Task.CompletedTask;
                        },
                    },
                ],
            }
        );
        var error = new DownloadError(
            "https://files.example.com/invoice.pdf?X-Amz-Signature=secret",
            "Download failed.",
            413,
            "Payload Too Large"
        );

        await telemetry.OnError!(new TelemetryErrorEvent("call-1", error), default);

        var download = Assert.IsType<DownloadError>(delivered!.Error);
        Assert.Equal("[redacted]", download.Url);
        Assert.Equal(413, download.StatusCode);
        Assert.DoesNotContain("secret", download.Url);
        Assert.DoesNotContain("secret", download.Message);
        Assert.DoesNotContain("secret", download.StackTrace ?? string.Empty);
        Assert.Contains("secret", error.Url);
    }
}
