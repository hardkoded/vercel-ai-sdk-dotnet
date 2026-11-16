// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.AspNetCore;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.Logger;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ToUiMessageStream;

/// <summary>Port of <c>to-ui-message-stream.test.ts</c> &gt; <c>toUIMessageStream</c> &gt; <c>tool error text mismatch warning</c>.</summary>
[Collection("LogWarnings")]
public sealed class ToolErrorTextMismatchWarningTests
{
    private const string Prefix =
        "packages/ai/src/ui-message-stream/to-ui-message-stream.test.ts::toUIMessageStream > tool error text mismatch warning::";
    private const string Commit = "ceeee29f9a7b7ca8c12a9270a542581bd760bdc7";

    private static readonly StartStepPart StartStep = new();

    private static readonly TextStreamPart TestStep = new StepFinishPart(
        new StepResult("", null, [], [], FinishReason.Stop, new LanguageModelUsage(0, 0, 0), []),
        new LanguageModelResponseMetadata(
            "id-0",
            DateTimeOffset.FromUnixTimeMilliseconds(0),
            "mock-model-id"
        )
    );

    private static readonly ToolCallPart ToolCall = new(
        new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"valid\"}") { Dynamic = true }
    );

    private static readonly ToolErrorPart ToolError = new(
        new ExecutedTool("call-1", "test-tool", "{}", true)
        {
            Dynamic = true,
            Error = new JsError("execution failed"),
            ModelOutput = "Error: execution failed",
        }
    );

    private static readonly TextStreamPart[] Reasoning =
    [
        new ReasoningStartPart("r1"),
        new ReasoningDeltaPart("r1", "thinking"),
        new ReasoningEndPart("r1"),
    ];

    // the tool call fails in the first step and the model reasons about the tool error in the second step
    private static readonly TextStreamPart[] ToolErrorThenReasoning =
    [
        StartStep,
        ToolCall,
        ToolError,
        TestStep,
        StartStep,
        .. Reasoning,
        TestStep,
    ];

    [Fact]
    [UpstreamTest(
        Prefix + "warns when a masked tool error is followed by reasoning from a later step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_when_a_masked_tool_error_is_followed_by_reasoning_from_a_later_step()
    {
        var logged = await Consume(
            ToolErrorThenReasoning,
            new ToUIMessageChunkOptions { RedactToolErrors = true }
        );

        var options = Assert.Single(logged);
        var warning = Assert.IsType<OtherWarning>(Assert.Single(options.Warnings!));
        Assert.Contains(
            "The error text sent to the UI for tool call \"call-1\" differs from the error text the model received",
            warning.Message,
            StringComparison.Ordinal
        );
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns once per stream",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_once_per_stream()
    {
        var secondToolCall = new ToolCallPart(
            new GeneratedToolCall("call-2", "test-tool", "{\"value\":\"valid\"}") { Dynamic = true }
        );
        var secondToolError = new ToolErrorPart(
            new ExecutedTool("call-2", "test-tool", "{}", true)
            {
                Dynamic = true,
                Error = new JsError("execution failed"),
                ModelOutput = "Error: execution failed",
            }
        );
        var logged = await Consume(
            [
                .. ToolErrorThenReasoning,
                StartStep,
                secondToolCall,
                secondToolError,
                TestStep,
                StartStep,
                .. Reasoning,
                TestStep,
            ],
            new ToUIMessageChunkOptions { RedactToolErrors = true }
        );

        Assert.Single(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn by default because the UI receives the text the model received",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_by_default_because_the_UI_receives_the_text_the_model_received()
    {
        Assert.Empty(await Consume(ToolErrorThenReasoning));
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn when a provided onError does not redact tool errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_when_a_provided_onError_does_not_redact_tool_errors()
    {
        var logged = await Consume(
            ToolErrorThenReasoning,
            new ToUIMessageChunkOptions
            {
                OnError = error => error.Message,
                RedactToolErrors = false,
            }
        );

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns when onError returns a text that differs from the text the model received",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_when_onError_returns_a_text_that_differs_from_the_text_the_model_received()
    {
        var logged = await Consume(
            ToolErrorThenReasoning,
            new ToUIMessageChunkOptions { OnError = error => error.Message }
        );

        Assert.Single(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn when onError returns the text the model received",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_when_onError_returns_the_text_the_model_received()
    {
        var logged = await Consume(
            ToolErrorThenReasoning,
            new ToUIMessageChunkOptions { OnError = error => "Error: " + error.Message }
        );

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn when reasoning is not sent",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_when_reasoning_is_not_sent()
    {
        var logged = await Consume(
            ToolErrorThenReasoning,
            new ToUIMessageChunkOptions { SendReasoning = false }
        );

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn when the reasoning precedes the tool error",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_when_the_reasoning_precedes_the_tool_error()
    {
        var logged = await Consume([
            StartStep,
            .. Reasoning,
            ToolCall,
            ToolError,
            TestStep,
            StartStep,
            new TextStartPart("t1"),
            new TextDeltaPart("t1", "The tool failed."),
            new TextEndPart("t1"),
            TestStep,
        ]);

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn when the reasoning follows the tool error in the same step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_when_the_reasoning_follows_the_tool_error_in_the_same_step()
    {
        var logged = await Consume([StartStep, ToolCall, ToolError, .. Reasoning, TestStep]);

        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns when a masked tool error precedes the first step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_when_a_masked_tool_error_precedes_the_first_step()
    {
        // approved tool calls run before the first step of the next call
        var logged = await Consume(
            [ToolError, StartStep, .. Reasoning, TestStep],
            new ToUIMessageChunkOptions { RedactToolErrors = true }
        );

        Assert.Single(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "warns when a masked invalid tool call is followed by reasoning from a later step",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Warns_when_a_masked_invalid_tool_call_is_followed_by_reasoning_from_a_later_step()
    {
        var invalidCall = new ToolCallPart(
            new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"valid\"}")
            {
                Dynamic = true,
                Invalid = true,
                Error = new JsError("invalid tool input"),
            }
        );
        var invalidError = new ToolErrorPart(
            new ExecutedTool("call-1", "test-tool", "{}", true)
            {
                Dynamic = true,
                ModelOutput = "Error: invalid tool input",
            }
        );
        var logged = await Consume(
            [StartStep, invalidCall, invalidError, TestStep, StartStep, .. Reasoning, TestStep],
            new ToUIMessageChunkOptions { RedactToolErrors = true }
        );

        Assert.Single(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not fail the stream when the tool error cannot be serialized",
        Coverage = UpstreamCoverage.Partial,
        Note = "The unserializable error is an exception whose Message throws, because a .NET tool error is an exception and has no circular value.",
        Commit = Commit
    )]
    public async Task Does_not_fail_the_stream_when_the_tool_error_cannot_be_serialized()
    {
        var unserializable = new ToolErrorPart(
            new ExecutedTool("call-1", "test-tool", "{}", true)
            {
                Dynamic = true,
                Error = new ToUiMessageChunk.ErrorTextOfFailedToolCallsTests.UnserializableError(),
            }
        );
        var logged = new List<LogWarningsOptions>();
        using var recorder = new LogWarningsRecorder((Action<LogWarningsOptions>)logged.Add);
        var chunks = await ToUIMessageStreamTests.Convert([
            StartStep,
            ToolCall,
            unserializable,
            TestStep,
            StartStep,
            .. Reasoning,
            TestStep,
        ]);

        Assert.Contains(chunks, chunk =>
            (string?)chunk?["type"] == "tool-output-error"
            && (string?)chunk["errorText"] == "An error occurred."
        );
        Assert.Empty(logged);
    }

    [Fact]
    [UpstreamTest(
        Prefix + "does not warn for provider-executed tool errors",
        Coverage = UpstreamCoverage.Covered,
        Commit = Commit
    )]
    public async Task Does_not_warn_for_provider_executed_tool_errors()
    {
        var providerCall = new ToolCallPart(
            new GeneratedToolCall("call-1", "test-tool", "{\"value\":\"valid\"}")
            {
                Dynamic = true,
                ProviderExecuted = true,
            }
        );
        var providerError = new ToolErrorPart(
            new ExecutedTool("call-1", "test-tool", "\"provider error\"", true)
            {
                Dynamic = true,
                ProviderExecuted = true,
            }
        );
        var logged = await Consume([
            StartStep,
            providerCall,
            providerError,
            TestStep,
            StartStep,
            .. Reasoning,
            TestStep,
        ]);

        Assert.Empty(logged);
    }

    private static async Task<List<LogWarningsOptions>> Consume(
        TextStreamPart[] parts,
        ToUIMessageChunkOptions? options = null
    )
    {
        var logged = new List<LogWarningsOptions>();
        using var recorder = new LogWarningsRecorder((Action<LogWarningsOptions>)logged.Add);
        await ToUIMessageStreamTests.Convert(parts, options);
        return logged;
    }
}
