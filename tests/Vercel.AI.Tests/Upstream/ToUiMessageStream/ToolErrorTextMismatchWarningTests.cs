// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText;
using Vercel.AI.Tests.Upstream.Logger;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToUiMessageStream;

/// <summary>Port of <c>to-ui-message-stream.test.ts</c> &gt; <c>toUIMessageStream</c> &gt; <c>tool error text mismatch warning</c>.</summary>
public sealed class ToolErrorTextMismatchWarningTests
{
    private const string Prefix = "packages/ai/src/ui-message-stream/to-ui-message-stream.test.ts::toUIMessageStream > tool error text mismatch warning::";
    private const string Commit = "ceeee29f9a7b7ca8c12a9270a542581bd760bdc7";
    private const string PartialNote = "Runs through streamText with a scripted model instead of feeding hand-built stream parts, because the .NET writer takes a StreamTextResult. The onError argument is the formatted error string, not the error object.";

    [Fact]
    [UpstreamTest(Prefix + "warns when a masked tool error is followed by reasoning from a later step", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Warns_when_a_masked_tool_error_is_followed_by_reasoning_from_a_later_step()
    {
        var logged = await Consume(ToolErrorThenReasoning(), redactToolErrors: true);

        var options = Assert.Single(logged);
        var warning = Assert.IsType<OtherWarning>(Assert.Single(options.Warnings!));
        Assert.Contains(
            "The error text sent to the UI for tool call \"call-1\" differs from the error text the model received",
            warning.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns once per stream", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Warns_once_per_stream()
    {
        var logged = await Consume(
            [
                ToolCallStep("call-1"),
                ToolCallStep("call-2"),
                ReasoningStep(),
                ReasoningStep(),
            ],
            redactToolErrors: true,
            stepCount: 4);

        Assert.Single(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not warn by default because the UI receives the text the model received", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Does_not_warn_by_default_because_the_UI_receives_the_text_the_model_received()
    {
        var logged = await Consume(ToolErrorThenReasoning());

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not warn when a provided onError does not redact tool errors", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Does_not_warn_when_a_provided_onError_does_not_redact_tool_errors()
    {
        var logged = await Consume(ToolErrorThenReasoning(), onError: message => message, redactToolErrors: false);

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns when onError returns a text that differs from the text the model received", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Warns_when_onError_returns_a_text_that_differs_from_the_text_the_model_received()
    {
        var logged = await Consume(ToolErrorThenReasoning(), onError: message => "masked " + message);

        Assert.Single(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not warn when onError returns the text the model received", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Does_not_warn_when_onError_returns_the_text_the_model_received()
    {
        var logged = await Consume(ToolErrorThenReasoning(), onError: message => "Error: " + message);

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not warn when the reasoning precedes the tool error", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Does_not_warn_when_the_reasoning_precedes_the_tool_error()
    {
        var logged = await Consume(
            [
                [
                    new ReasoningStartStreamPart("r1"),
                    new ReasoningDeltaStreamPart("r1", "thinking"),
                    new ReasoningEndStreamPart("r1"),
                    new ToolCallStreamPart("call-1", "test-tool", "{\"value\":\"valid\"}"),
                    Finish("tool-calls"),
                ],
                [
                    new TextDeltaStreamPart("t1", "The tool failed."),
                    Finish("stop"),
                ],
            ],
            redactToolErrors: true);

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "does not warn when the reasoning follows the tool error in the same step", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Does_not_warn_when_the_reasoning_follows_the_tool_error_in_the_same_step()
    {
        var logged = await Consume(
            [
                [
                    new ToolCallStreamPart("call-1", "test-tool", "{\"value\":\"valid\"}"),
                    new ReasoningStartStreamPart("r1"),
                    new ReasoningDeltaStreamPart("r1", "thinking"),
                    new ReasoningEndStreamPart("r1"),
                    Finish("tool-calls"),
                ],
            ],
            redactToolErrors: true);

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(Prefix + "warns when a masked invalid tool call is followed by reasoning from a later step", Coverage = UpstreamCoverage.Partial, Note = PartialNote, Commit = Commit)]
    public async Task Warns_when_a_masked_invalid_tool_call_is_followed_by_reasoning_from_a_later_step()
    {
        var logged = await Consume(
            [
                [new ToolCallStreamPart("call-1", "unknown-tool", "{\"value\":\"valid\"}"), Finish("tool-calls")],
                ReasoningStep(),
            ],
            redactToolErrors: true);

        Assert.Single(logged);
    }

    private static LanguageModelStreamPart[][] ToolErrorThenReasoning() => [ToolCallStep("call-1"), ReasoningStep()];

    private static LanguageModelStreamPart[] ToolCallStep(string toolCallId) =>
        [new ToolCallStreamPart(toolCallId, "test-tool", "{\"value\":\"valid\"}"), Finish("tool-calls")];

    private static LanguageModelStreamPart[] ReasoningStep() =>
        [
            new ReasoningStartStreamPart("r1"),
            new ReasoningDeltaStreamPart("r1", "thinking"),
            new ReasoningEndStreamPart("r1"),
            Finish("stop"),
        ];

    private static FinishStreamPart Finish(string raw) =>
        new(raw == "stop" ? FinishReason.Stop : FinishReason.ToolCalls, new LanguageModelUsage(3, 10, 13, noCacheInputTokens: 3, textTokens: 10), raw);

    private static async Task<List<LogWarningsOptions>> Consume(
        LanguageModelStreamPart[][] steps,
        Func<string, string>? onError = null,
        bool? redactToolErrors = null,
        int stepCount = 2)
    {
        var logged = new List<LogWarningsOptions>();
        using var recorder = new LogWarningsRecorder((Action<LogWarningsOptions>)logged.Add);
        var tool = Tool.Function(
            "test-tool",
            null,
            "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}",
            (_, _) => throw new JsError("execution failed"));
        var result = new AiClient(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" })).StreamTextAsync(new StreamTextOptions
        {
            Model = new ScriptedModel(steps),
            Prompt = "test-input",
            Tools = [tool],
            StopWhen = StopWhen.IsStepCount(stepCount),
            NowMs = () => 0,
        });

        await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(result, onError, redactToolErrors);
        return logged;
    }

    private sealed class ScriptedModel(LanguageModelStreamPart[][] steps) : ILanguageModel
    {
        private int _calls;

        public string SpecificationVersion => "V4";

        public string Provider => "test";

        public string ModelId => "scripted";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
        {
            var index = Interlocked.Increment(ref _calls) - 1;
            return Task.FromResult(new LanguageModelStreamResult(Parts(steps[index])));
        }

        private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(LanguageModelStreamPart[] parts)
        {
            foreach (var part in parts)
            {
                yield return part;
                await Task.Yield();
            }
        }
    }
}
