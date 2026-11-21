// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateTextSteering;

namespace Vercel.AI.Tests.Upstream.StreamText;

internal static class StreamTextTransformTestSupport
{
    // Maps to createTestModel(): "Hello", ", ", "world!" text deltas.
    public static SteeringTestModel CreateTestModel() =>
        SteeringTestModel.Stream(() =>
            new LanguageModelStreamPart[]
            {
                new StreamStartStreamPart(Array.Empty<CallWarning>()),
                new TextStartStreamPart("1"),
                new TextDeltaStreamPart("1", "Hello"),
                new TextDeltaStreamPart("1", ", "),
                new TextDeltaStreamPart("1", "world!"),
                new TextEndStreamPart("1"),
                new FinishStreamPart(FinishReason.Stop, SteeringTestSupport.Usage, "stop"),
            }
        );

    // Maps to a TransformStream whose transform maps each chunk to zero or more chunks.
    public static Func<
        IAsyncEnumerable<TextStreamPart>,
        CancellationToken,
        IAsyncEnumerable<TextStreamPart>
    > Map(Func<TextStreamPart, IEnumerable<TextStreamPart>> transform) =>
        (stream, cancellationToken) => Run(stream, transform, cancellationToken);

    private static async IAsyncEnumerable<TextStreamPart> Run(
        IAsyncEnumerable<TextStreamPart> stream,
        Func<TextStreamPart, IEnumerable<TextStreamPart>> transform,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (var part in stream.WithCancellation(cancellationToken))
        {
            foreach (var mapped in transform(part))
            {
                yield return mapped;
            }
        }
    }

    public static async Task<List<string>> TextStreamAsync(StreamTextResult result)
    {
        var chunks = new List<string>();
        await foreach (var chunk in result.TextStream())
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    // Maps to the MockLanguageModelV4 of the upstream tests: each doStream call gets its own parts, request body and response headers.
    public sealed class CallsModel(
        Func<int, LanguageModelCallOptions, CallScript> script,
        string provider = "mock-provider",
        string modelId = "mock-model-id"
    ) : ILanguageModel
    {
        private int _calls;

        public int CallCount => Volatile.Read(ref _calls);

        public List<LanguageModelCallOptions> Prompts { get; } = [];

        public IReadOnlyDictionary<string, string> SupportedUrls =>
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => provider;

        public string ModelId => modelId;

        public static CallsModel Single(
            IReadOnlyList<LanguageModelStreamPart> parts,
            string? requestBody = null,
            Dictionary<string, string>? headers = null
        ) => new((_, _) => new CallScript(parts, requestBody, headers));

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            var call = Interlocked.Increment(ref _calls);
            lock (Prompts)
            {
                Prompts.Add(options);
            }

            var scripted = script(call - 1, options);
            return Task.FromResult(
                new LanguageModelStreamResult(
                    SteeringTestModel.ToAsync(scripted.Parts, cancellationToken),
                    scripted.RequestBody == null
                        ? null
                        : new LanguageModelStreamRequest(
                            System.Text.Json.JsonSerializer.SerializeToElement(scripted.RequestBody)
                        ),
                    scripted.Headers == null
                        ? null
                        : new LanguageModelStreamResponse(scripted.Headers)
                )
            );
        }
    }

    public sealed record CallScript(
        IReadOnlyList<LanguageModelStreamPart> Parts,
        string? RequestBody = null,
        Dictionary<string, string>? Headers = null
    );

    // Maps to testUsage: 3 input tokens (3 not cached) and 10 output tokens (10 text).
    public static readonly LanguageModelUsage TestUsage = new(
        3,
        10,
        13,
        noCacheInputTokens: 3,
        textTokens: 10
    );

    // Maps to testUsage2.
    public static readonly LanguageModelUsage TestUsage2 = new(
        3,
        10,
        13,
        cacheReadTokens: 0,
        cacheWriteTokens: 0,
        reasoningTokens: 10,
        noCacheInputTokens: 3,
        textTokens: 10
    );

    // Maps to mockValues: returns each value once, then repeats the last one.
    public static Func<double> MockValues(params double[] values)
    {
        var index = -1;
        return () => values[Math.Min(Interlocked.Increment(ref index), values.Length - 1)];
    }

    // Maps to mockId({ prefix: 'id' }).
    public static Func<string> MockId(string prefix = "id")
    {
        var counter = -1;
        return () => prefix + "-" + Interlocked.Increment(ref counter);
    }

    public static FinishStreamPart Finish(
        FinishReason reason = FinishReason.Stop,
        string raw = "stop",
        System.Text.Json.JsonElement? providerMetadata = null
    ) => new(reason, TestUsage, raw, providerMetadata);

    // Maps to the DefaultStepResult inline snapshots of the transform tests: every field of a single transformed text + tool step.
    public static void AssertTransformedStep(
        StepResult step,
        string toolOutput,
        string? providerMetadataJson = null,
        string? responseHeaderValue = null
    )
    {
        Assert.Equal("test-telemetry-call-id", step.CallId);
        Assert.Equal(0, step.StepNumber);
        Assert.Equal(3, step.Content.Count);
        var text = Assert.IsType<GeneratedText>(step.Content[0]);
        Assert.Equal("HELLO, WORLD!", text.Text);
        Assert.Null(text.ProviderMetadata);
        var call = Assert.IsType<GeneratedToolCall>(step.Content[1]);
        Assert.Equal("call-1", call.ToolCallId);
        Assert.Equal("tool1", call.ToolName);
        Assert.Equal("{\"value\":\"VALUE\"}", call.ArgumentsJson);
        Assert.False(call.Dynamic);
        var result = Assert.IsType<GeneratedToolResult>(step.Content[2]);
        Assert.Equal("call-1", result.ToolCallId);
        Assert.Equal("tool1", result.ToolName);
        Assert.Equal(toolOutput, result.ResultJson);
        Assert.Equal(FinishReason.Stop, step.FinishReason);
        Assert.Equal("stop", step.RawFinishReason);
        Assert.Equal("mock-provider", step.Model.Provider);
        Assert.Equal("mock-model-id", step.Model.ModelId);
        var performance = step.Performance;
        Assert.Equal(0, performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, performance.InputTokensPerSecond);
        Assert.Equal(0, performance.OutputTokensPerSecond);
        Assert.Equal(0, performance.ResponseTimeMs);
        Assert.Equal(0, performance.StepTimeMs);
        Assert.Equal(0, performance.TimeToFirstOutputMs);
        var gaps = performance.TimeBetweenOutputChunksMs!;
        Assert.Equal(0, gaps.Avg);
        Assert.Equal(0, gaps.Max);
        Assert.Equal(0, gaps.Median);
        Assert.Equal(0, gaps.Min);
        Assert.Equal(0, gaps.P10);
        Assert.Equal(0, gaps.P90);
        var execution = Assert.Single(performance.ToolExecutionMs);
        Assert.Equal("call-1", execution.Key);
        Assert.Equal(0, execution.Value);
        Assert.Equal(providerMetadataJson, step.ProviderMetadata?.GetRawText());
        Assert.Null(step.Request.Body);
        Assert.Null(step.Request.Messages);
        Assert.Equal("id-0", step.Response.Id);
        Assert.Equal("mock-model-id", step.Response.ModelId);
        Assert.Equal(DateTimeOffset.UnixEpoch, step.Response.Timestamp);
        if (responseHeaderValue == null)
        {
            Assert.Null(step.Response.Headers);
        }
        else
        {
            var header = Assert.Single(step.Response.Headers!);
            Assert.Equal("call", header.Key);
            Assert.Equal(responseHeaderValue, header.Value);
        }

        Assert.Equal(2, step.Response.Messages.Count);
        var assistant = Assert.IsType<AssistantModelMessage>(step.Response.Messages[0]);
        Assert.Equal(2, assistant.Parts.Count);
        var assistantText = Assert.IsType<AssistantTextPart>(assistant.Parts[0]);
        Assert.Equal("HELLO, WORLD!", assistantText.Text);
        var assistantCall = Assert.IsType<AssistantToolCallPart>(assistant.Parts[1]);
        Assert.Equal("call-1", assistantCall.ToolCall.ToolCallId);
        Assert.Equal("tool1", assistantCall.ToolCall.ToolName);
        Assert.Equal("{\"value\":\"VALUE\"}", assistantCall.ToolCall.ArgumentsJson);
        var tool = Assert.IsType<ToolModelMessage>(step.Response.Messages[1]);
        Assert.Equal("call-1", tool.ToolCallId);
        Assert.Equal("tool1", tool.ToolName);
        Assert.Equal("text", tool.OutputType);
        Assert.Equal(toolOutput, tool.OutputJson);
        Assert.Empty(step.RuntimeContext);
        Assert.Empty(step.ToolsContext);
        Assert.Equal(3, step.Usage.InputTokens);
        Assert.Equal(10, step.Usage.OutputTokens);
        Assert.Equal(13, step.Usage.TotalTokens);
        Assert.Equal(3, step.Usage.NoCacheInputTokens);
        Assert.Equal(10, step.Usage.TextTokens);
        Assert.Null(step.Usage.CacheReadTokens);
        Assert.Null(step.Usage.CacheWriteTokens);
        Assert.Null(step.Usage.ReasoningTokens);
        Assert.Null(step.Usage.Raw);
        Assert.Empty(step.Warnings);
    }

    public static async Task<List<TextStreamPart>> PartsAsync(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }
}
