// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Gateway;
using Vercel.AI.Provider;
using Vercel.AI.Testing;
using Simulate = global::Vercel.AI.SimulateStreamingMiddleware;

namespace Vercel.AI.Tests.Upstream.Middleware.SimulateStreamingMiddleware;

/// <summary>Port of <c>simulate-streaming-middleware.test.ts</c> &gt; <c>simulateStreamingMiddleware</c>.</summary>
public sealed class SimulateStreamingMiddlewareTests
{
    private const string Id =
        "packages/ai/src/middleware/simulate-streaming-middleware.test.ts::simulateStreamingMiddleware::";

    private const string SnapshotNote =
        "Asserts the part types, ids, text, finish reasons, usage and provider metadata. The inline snapshot's generated response id, frozen timestamp and performance metrics are not asserted.";

    private static readonly LanguageModelUsage TestUsage = new(
        inputTokens: 5,
        outputTokens: 10,
        totalTokens: 15,
        cacheReadTokens: 0,
        cacheWriteTokens: 0,
        reasoningTokens: 3,
        noCacheInputTokens: 5,
        textTokens: 10
    );

    private static AiClient Client() =>
        new(GatewayProvider.Create(new GatewayOptions { ApiKey = "test" }));

    private static LanguageModelGenerateResult Generate(
        IReadOnlyList<GeneratedContent> content,
        FinishReason finishReason = FinishReason.Stop,
        string? rawFinishReason = "stop",
        IReadOnlyList<CallWarning>? warnings = null,
        JsonElement? providerMetadata = null
    ) =>
        new(
            content,
            finishReason,
            TestUsage,
            rawFinishReason,
            warnings ?? [],
            providerMetadata: providerMetadata
        );

    private static StreamTextResult Stream(
        LanguageModelGenerateResult generated,
        IReadOnlyList<Tool>? tools = null
    ) =>
        Client()
            .StreamTextAsync(
                new StreamTextOptions
                {
                    Model = new TestLanguageModel("mock-model-id")
                    {
                        OnGenerate = _ => generated,
                    }.WrapLanguageModel(new Simulate()),
                    Prompt = "Test prompt",
                    Tools = tools,
                }
            );

    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static string? Raw(JsonElement? element) => element?.GetRawText();

    private static async Task<List<TextStreamPart>> Collect(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    // Describes the parts the way the upstream inline snapshots list them, minus the fields noted in SnapshotNote.
    private static List<string> Describe(IEnumerable<TextStreamPart> parts)
    {
        var lines = new List<string>();
        foreach (var part in parts)
        {
            lines.Add(
                part switch
                {
                    StartPart => "start",
                    StartStepPart start => $"start-step warnings={start.Warnings.Count}",
                    TextStartPart p => $"text-start {p.Id} {Raw(p.ProviderMetadata)}".TrimEnd(),
                    TextDeltaPart p => $"text-delta {p.Id} {p.Text}",
                    TextEndPart p => $"text-end {p.Id}",
                    ReasoningStartPart p =>
                        $"reasoning-start {p.Id} {Raw(p.ProviderMetadata)}".TrimEnd(),
                    ReasoningDeltaPart p => $"reasoning-delta {p.Id} {p.Text}",
                    ReasoningEndPart p => $"reasoning-end {p.Id}",
                    ToolCallPart p =>
                        $"tool-call {p.ToolCall.ToolCallId} {p.ToolCall.ToolName} {p.ToolCall.ArgumentsJson}",
                    StepFinishPart p =>
                        $"finish-step {p.Step.FinishReason} raw={p.RawFinishReason} model={p.Response.ModelId} provider={Raw(p.ProviderMetadata)}",
                    FinishPart p => $"finish {p.FinishReason}",
                    _ => part.Type,
                }
            );
        }

        return lines;
    }

    private static void AssertUsage(LanguageModelUsage usage)
    {
        Assert.Equal(5, usage.InputTokens);
        Assert.Equal(5, usage.NoCacheInputTokens);
        Assert.Equal(0, usage.CacheReadTokens);
        Assert.Equal(0, usage.CacheWriteTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Equal(3, usage.ReasoningTokens);
        Assert.Equal(15, usage.TotalTokens);
    }

    private static void AssertFinishUsage(IEnumerable<TextStreamPart> parts)
    {
        AssertUsage(parts.OfType<StepFinishPart>().Single().Step.Usage);
        AssertUsage(parts.OfType<FinishPart>().Single().Usage);
    }

    [Fact]
    [UpstreamTest(
        Id + "should simulate streaming with text response",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
    )]
    public async Task Should_simulate_streaming_with_text_response()
    {
        var parts = await Collect(Stream(Generate([new GeneratedText("This is a test response")])));

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "text-start 0",
                "text-delta 0 This is a test response",
                "text-end 0",
                "finish-step Stop raw=stop model=mock-model-id provider=",
                "finish Stop",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should simulate streaming with reasoning as string",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
    )]
    public async Task Should_simulate_streaming_with_reasoning_as_string()
    {
        var parts = await Collect(
            Stream(
                Generate([
                    new GeneratedReasoning("This is the reasoning process"),
                    new GeneratedText("This is a test response"),
                ])
            )
        );

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "reasoning-start 0",
                "reasoning-delta 0 This is the reasoning process",
                "reasoning-end 0",
                "text-start 1",
                "text-delta 1 This is a test response",
                "text-end 1",
                "finish-step Stop raw=stop model=mock-model-id provider=",
                "finish Stop",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should simulate streaming with reasoning as array of text objects",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
    )]
    public async Task Should_simulate_streaming_with_reasoning_as_array_of_text_objects()
    {
        var parts = await Collect(
            Stream(
                Generate([
                    new GeneratedText("This is a test response"),
                    new GeneratedReasoning("First reasoning step"),
                    new GeneratedReasoning("Second reasoning step"),
                    new GeneratedReasoning("", Json("{\"testProvider\":{\"signature\":\"abc\"}}")),
                ])
            )
        );

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "text-start 0",
                "text-delta 0 This is a test response",
                "text-end 0",
                "reasoning-start 1",
                "reasoning-delta 1 First reasoning step",
                "reasoning-end 1",
                "reasoning-start 2",
                "reasoning-delta 2 Second reasoning step",
                "reasoning-end 2",
                "reasoning-start 3 {\"testProvider\":{\"signature\":\"abc\"}}",
                "reasoning-delta 3 ",
                "reasoning-end 3",
                "finish-step Stop raw=stop model=mock-model-id provider=",
                "finish Stop",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should simulate streaming with reasoning as array of mixed objects",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
    )]
    public async Task Should_simulate_streaming_with_reasoning_as_array_of_mixed_objects()
    {
        var parts = await Collect(
            Stream(
                Generate([
                    new GeneratedReasoning("First reasoning step"),
                    new GeneratedReasoning(
                        "data",
                        Json("{\"testProvider\":{\"isRedacted\":true}}")
                    ),
                    new GeneratedText("This is a test response"),
                ])
            )
        );

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "reasoning-start 0",
                "reasoning-delta 0 First reasoning step",
                "reasoning-end 0",
                "reasoning-start 1 {\"testProvider\":{\"isRedacted\":true}}",
                "reasoning-delta 1 data",
                "reasoning-end 1",
                "text-start 2",
                "text-delta 2 This is a test response",
                "text-end 2",
                "finish-step Stop raw=stop model=mock-model-id provider=",
                "finish Stop",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should simulate streaming with tool calls",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
            + " The finish part's stoppedBy ('tool-without-execute') has no counterpart on the C# finish part."
    )]
    public async Task Should_simulate_streaming_with_tool_calls()
    {
        var parts = await Collect(
            Stream(
                Generate(
                    [
                        new GeneratedText("This is a test response"),
                        new GeneratedToolCall("tool-1", "calculator", "{\"expression\": \"2+2\"}"),
                        new GeneratedToolCall("tool-2", "weather", "{\"location\": \"New York\"}"),
                    ],
                    FinishReason.ToolCalls,
                    rawFinishReason: null
                ),
                [
                    Tool.Function("calculator", null, "{\"type\":\"object\"}", null),
                    Tool.Function("weather", null, "{\"type\":\"object\"}", null),
                ]
            )
        );

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "text-start 0",
                "text-delta 0 This is a test response",
                "text-end 0",
                "tool-call tool-1 calculator {\"expression\": \"2+2\"}",
                "tool-call tool-2 weather {\"location\": \"New York\"}",
                "finish-step ToolCalls raw= model=mock-model-id provider=",
                "finish ToolCalls",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve additional metadata in the response",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
    )]
    public async Task Should_preserve_additional_metadata_in_the_response()
    {
        var parts = await Collect(
            Stream(
                Generate(
                    [new GeneratedText("This is a test response")],
                    providerMetadata: Json("{\"custom\":{\"key\":\"value\"}}")
                )
            )
        );

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "text-start 0",
                "text-delta 0 This is a test response",
                "text-end 0",
                "finish-step Stop raw=stop model=mock-model-id provider={\"custom\":{\"key\":\"value\"}}",
                "finish Stop",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should handle empty text response",
        Coverage = UpstreamCoverage.Partial,
        Note = SnapshotNote
    )]
    public async Task Should_handle_empty_text_response()
    {
        var parts = await Collect(Stream(Generate([new GeneratedText("")])));

        Assert.Equal(
            [
                "start",
                "start-step warnings=0",
                "finish-step Stop raw=stop model=mock-model-id provider=",
                "finish Stop",
            ],
            Describe(parts)
        );
        AssertFinishUsage(parts);
    }

    [Fact]
    [UpstreamTest(
        Id + "should pass through warnings from the model",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_pass_through_warnings_from_the_model()
    {
        var result = Stream(
            Generate(
                [new GeneratedText("This is a test response")],
                warnings: [new CallWarning("other", "Test warning")]
            )
        );

        await result.ConsumeStreamAsync();

        var warning = Assert.Single(await result.Warnings);
        Assert.Equal("other", warning.Type);
        Assert.Equal("Test warning", warning.Message);
    }

    [Fact]
    [UpstreamTest(
        Id + "should preserve provider metadata from generated text",
        Coverage = UpstreamCoverage.Covered
    )]
    public async Task Should_preserve_provider_metadata_from_generated_text()
    {
        var providerMetadata = Json("{\"testProvider\":{\"signature\":\"test-signature\"}}");
        var result = Stream(
            Generate([
                new GeneratedText("This is a test response", providerMetadata: providerMetadata),
            ])
        );

        var content = Assert.Single(await result.Content);
        var text = Assert.IsType<GeneratedText>(content);
        Assert.Equal("This is a test response", text.Text);
        Assert.Equal(providerMetadata.GetRawText(), Raw(text.ProviderMetadata));

        var message = Assert.IsType<AssistantModelMessage>(
            Assert.Single(await result.ResponseMessages)
        );
        var part = Assert.IsType<AssistantTextPart>(Assert.Single(message.Parts));
        Assert.Equal("This is a test response", part.Text);
        var providerOptions = Assert.Single(part.ProviderOptions!);
        Assert.Equal("testProvider", providerOptions.Key);
        Assert.Equal("{\"signature\":\"test-signature\"}", providerOptions.Value.GetRawText());
    }
}
