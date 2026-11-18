// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.Tests.Upstream.GenerateText.GenerateText;

namespace Vercel.AI.Tests.Upstream.GenerateText.StreamText;

// Shared by the ports of `streamText > tool execution approval`.
internal static class ToolExecutionApprovalStreamSupport
{
    public static readonly LanguageModelUsage Usage = new(
        3,
        10,
        13,
        noCacheInputTokens: 3,
        textTokens: 10
    );

    // mockId({ prefix: 'id' }): the stream draws the response id first, so the approval ids start at id-1.
    public static Func<string> StreamMockId(string prefix)
    {
        var counter = -1;
        return () => prefix + "-" + Interlocked.Increment(ref counter);
    }

    // A model whose n-th call streams the parts of the n-th script entry (the last entry repeats). It records the prompts.
    public sealed class ScriptedModel(Func<int, IReadOnlyList<LanguageModelStreamPart>> script)
        : ILanguageModel
    {
        public List<IReadOnlyList<ModelMessage>> Prompts { get; } = [];

        public IReadOnlyDictionary<string, string> SupportedUrls =>
            Vercel.AI.ProviderUtils.UrlSupport.None;

        public string SpecificationVersion => "V4";

        public string Provider => "mock-provider";

        public string ModelId => "mock-model-id";

        public Task<LanguageModelGenerateResult> DoGenerateAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        ) => throw new NotSupportedException();

        public Task<LanguageModelStreamResult> DoStreamAsync(
            LanguageModelCallOptions options,
            CancellationToken cancellationToken
        )
        {
            var step = Prompts.Count;
            Prompts.Add(options.Prompt);
            return Task.FromResult(new LanguageModelStreamResult(Replay(script(step))));
        }

        private static async IAsyncEnumerable<LanguageModelStreamPart> Replay(
            IReadOnlyList<LanguageModelStreamPart> parts
        )
        {
            foreach (var part in parts)
            {
                yield return part;
                await Task.Yield();
            }
        }
    }

    private static ResponseMetadataStreamPart Metadata() =>
        new("id-0", "mock-model-id", DateTimeOffset.FromUnixTimeMilliseconds(0));

    public static LanguageModelStreamPart[] ToolCallsStep(
        params (string Id, string Input)[] calls
    ) =>
        new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            Metadata(),
        }
            .Concat(
                calls.Select(call =>
                    (LanguageModelStreamPart)new ToolCallStreamPart(call.Id, "tool1", call.Input)
                )
            )
            .Append(new FinishStreamPart(FinishReason.ToolCalls, Usage, null))
            .ToArray();

    public static LanguageModelStreamPart[] TextStep(string text = "Hello, world!") =>
        new LanguageModelStreamPart[]
        {
            new StreamStartStreamPart(Array.Empty<CallWarning>()),
            Metadata(),
            new TextStartStreamPart("1"),
            new TextDeltaStreamPart("1", text),
            new TextEndStreamPart("1"),
            new FinishStreamPart(FinishReason.Stop, Usage, "stop"),
        };

    public static async Task<List<TextStreamPart>> ReadAsync(StreamTextResult result)
    {
        var parts = new List<TextStreamPart>();
        await foreach (var part in result.Stream())
        {
            parts.Add(part);
        }

        return parts;
    }

    public static string[] Types(IEnumerable<TextStreamPart> parts) =>
        parts.Select(part => part.Type).ToArray();

    public static async Task<IReadOnlyList<GeneratedContent>> ContentAsync(
        StreamTextResult result
    ) => (await result.Steps).Last().Content;

    public static void AssertUsage(LanguageModelUsage usage)
    {
        Assert.Equal(3, usage.InputTokens);
        Assert.Equal(10, usage.OutputTokens);
        Assert.Equal(13, usage.TotalTokens);
        Assert.Equal(3, usage.NoCacheInputTokens);
        Assert.Equal(10, usage.TextTokens);
        Assert.Null(usage.CacheReadTokens);
        Assert.Null(usage.CacheWriteTokens);
        Assert.Null(usage.ReasoningTokens);
    }

    // The finish-step and finish parts that close the stream.
    public static void AssertStepAndFinish(
        IReadOnlyList<TextStreamPart> parts,
        int finishStepIndex,
        FinishReason reason,
        string? rawFinishReason,
        Action<IReadOnlyDictionary<string, double>>? assertToolExecutionMs = null
    )
    {
        var finishStep = Assert.IsType<StepFinishPart>(parts[finishStepIndex]);
        Assert.Equal(reason, finishStep.Step.FinishReason);
        Assert.Equal(rawFinishReason, finishStep.RawFinishReason);
        Assert.Null(finishStep.ProviderMetadata);
        Assert.Null(finishStep.Response.Headers);
        Assert.Equal("id-0", finishStep.Response.Id);
        Assert.Equal("mock-model-id", finishStep.Response.ModelId);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(0), finishStep.Response.Timestamp);
        Assert.Equal(0, finishStep.Performance.EffectiveOutputTokensPerSecond);
        Assert.Equal(0, finishStep.Performance.EffectiveTotalTokensPerSecond);
        Assert.Equal(0, finishStep.Performance.InputTokensPerSecond);
        Assert.Equal(0, finishStep.Performance.OutputTokensPerSecond);
        Assert.Equal(0, finishStep.Performance.ResponseTimeMs);
        Assert.Equal(0, finishStep.Performance.StepTimeMs);
        Assert.Equal(0, finishStep.Performance.TimeToFirstOutputMs);
        if (assertToolExecutionMs == null)
        {
            Assert.Empty(finishStep.Performance.ToolExecutionMs);
        }
        else
        {
            assertToolExecutionMs(finishStep.Performance.ToolExecutionMs);
        }

        AssertUsage(finishStep.Step.Usage);
        var finish = Assert.IsType<FinishPart>(parts[finishStepIndex + 1]);
        Assert.Equal(reason, finish.FinishReason);
        AssertUsage(finish.Usage);
        Assert.Equal(finishStepIndex + 2, parts.Count);
    }

    public static void AssertStartStep(TextStreamPart part)
    {
        var startStep = Assert.IsType<StartStepPart>(part);
        Assert.Null(startStep.Request.Body);
        Assert.Null(startStep.Request.Messages);
        Assert.Empty(startStep.Warnings);
    }

    public static async Task<JsonArray> UiChunksAsync(StreamTextResult result) =>
        await SingleProviderExecutedToolCallAndResultTests.ReadUiChunks(result);

    public static void AssertJson(string expected, JsonArray actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), actual), actual.ToJsonString());

    public static void AssertInput(string expectedCompact, JsonElement input) =>
        Assert.Equal(expectedCompact, ToolExecutionApprovalSupport.Compact(input.GetRawText()));
}
