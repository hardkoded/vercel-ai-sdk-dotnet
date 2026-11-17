// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.AspNetCore;
using Vercel.AI.Ui;

namespace Vercel.AI.Tests.Upstream.HandleUiMessageStreamFinish;

/// <summary>The scaffolding of <c>handle-ui-message-stream-finish.test.ts</c>.</summary>
internal sealed class HandleUIMessageStreamFinishHarness
{
    /// <summary>Maps to <c>mockErrorHandler</c>.</summary>
    public List<Exception> Errors { get; } = [];

    /// <summary>Maps to <c>createUIMessageStream</c>.</summary>
    public static async IAsyncEnumerable<JsonObject> CreateUIMessageStream(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return (JsonObject)JsonNode.Parse(chunk)!;
            await Task.Yield();
        }
    }

    /// <summary>Maps to <c>convertReadableStreamToArray</c>.</summary>
    public static async Task<List<JsonObject>> ToArray(IAsyncEnumerable<JsonObject> stream)
    {
        var result = new List<JsonObject>();
        await foreach (var chunk in stream)
        {
            result.Add(chunk);
        }

        return result;
    }

    /// <summary>Maps to <c>expect(result).toEqual(inputChunks)</c>.</summary>
    public static void AssertChunks(
        IReadOnlyList<string> expected,
        IReadOnlyList<JsonObject> actual
    )
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.True(
                JsonNode.DeepEquals(JsonNode.Parse(expected[i]), actual[i]),
                $"Chunk {i}: expected {expected[i]}, actual {actual[i].ToJsonString()}"
            );
        }
    }

    /// <summary>Maps to <c>handleUIMessageStreamFinish</c> with the test's <c>mockErrorHandler</c>.</summary>
    public IAsyncEnumerable<JsonObject> Handle(
        IAsyncEnumerable<JsonObject> stream,
        string? messageId = null,
        IReadOnlyList<UIMessage>? originalMessages = null,
        Func<UIMessageStreamOnStepEndEvent, Task>? onStepEnd = null,
        Func<UIMessageStreamOnEndEvent, Task>? onEnd = null,
        Func<UIMessageStreamOutcome>? getOutcome = null,
        Func<UIMessageStreamOnStepEndEvent, Task>? onStepFinish = null,
        Func<UIMessageStreamOnEndEvent, Task>? onFinish = null
    ) =>
        UIMessageStreamConverter.HandleUIMessageStreamFinish(
            stream,
            messageId,
            originalMessages,
            onStepEnd,
            onEnd,
            Errors.Add,
            getOutcome,
            onStepFinish,
            onFinish
        );

    /// <summary>A message with one text part.</summary>
    public static UIMessage TextMessage(string id, UIMessageRole role, string text) =>
        new(role, [new TextUIPart(text)]) { Id = id };
}
