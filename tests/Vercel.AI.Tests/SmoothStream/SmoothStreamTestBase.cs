// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.GenerateText;
using Vercel.AI.Provider;
using Xunit;

namespace Vercel.AI.Tests.SmoothStream;

/// <summary>Shared <c>events</c>, <c>consumeStream</c> and <c>delay</c> helpers of upstream's <c>smoothStream</c> describe.</summary>
public abstract class SmoothStreamTestBase
{
    private readonly List<string> _events = [];
    private readonly List<TextStreamPart> _parts = [];

    protected static string Delay => "delay 10";

    protected IReadOnlyList<TextStreamPart> Parts => _parts;

    protected static async IAsyncEnumerable<TextStreamPart> From(params TextStreamPart[] parts)
    {
        foreach (var part in parts)
        {
            yield return part;
            await Task.Yield();
        }
    }

    protected static TextDeltaPart D(string text, string id = "1", JsonElement? metadata = null) =>
        new(id, text, metadata);

    protected static ReasoningDeltaPart R(
        string text,
        string id = "1",
        JsonElement? metadata = null
    ) => new(id, text, metadata);

    protected static string DelayOf(int? ms) =>
        $"delay {(ms is int m ? m.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null")}";

    protected static JsonElement Meta(string signature) =>
        JsonSerializer.SerializeToElement(
            new Dictionary<string, object>
            {
                ["anthropic"] = new Dictionary<string, string> { ["signature"] = signature },
            }
        );

    /// <summary>Runs the transform over <paramref name="input"/> and records every delay and output part, like <c>consumeStream</c>.</summary>
    protected async Task Run(TextStreamPart[] input, SmoothStreamOptions? options = null)
    {
        options ??= new SmoothStreamOptions();
        options.Delay ??= (ms, _) =>
        {
            _events.Add(DelayOf(ms));
            return Task.CompletedTask;
        };

        var transform = Vercel.AI.GenerateText.SmoothStream.Create(options)(
            new Vercel.AI.StreamTextTransformOptions()
        );
        await foreach (var part in transform(From(input), default))
        {
            _events.Add(Describe(part));
            _parts.Add(part);
        }
    }

    /// <summary>The recorded events without the delays.</summary>
    protected string[] Output() =>
        _events.Where(e => !e.StartsWith("delay ", StringComparison.Ordinal)).ToArray();

    protected string[] Delays() =>
        _events.Where(e => e.StartsWith("delay ", StringComparison.Ordinal)).ToArray();

    protected void Expect(params object[] expected) =>
        Assert.Equal(
            expected.Select(e => e as string ?? Describe((TextStreamPart)e)).ToArray(),
            _events.ToArray()
        );

    protected void ExpectOutput(params TextStreamPart[] expected) =>
        Assert.Equal(expected.Select(Describe).ToArray(), Output());

    protected static string Describe(TextStreamPart part) =>
        part switch
        {
            TextDeltaPart p => $"{p.Type} {p.Id} {Quote(p.Text)} {Raw(p.ProviderMetadata)}",
            ReasoningDeltaPart p => $"{p.Type} {p.Id} {Quote(p.Text)} {Raw(p.ProviderMetadata)}",
            TextStartPart p => $"{p.Type} {p.Id} {Raw(p.ProviderMetadata)}",
            TextEndPart p => $"{p.Type} {p.Id}",
            ReasoningStartPart p => $"{p.Type} {p.Id} {Raw(p.ProviderMetadata)}",
            ReasoningEndPart p => $"{p.Type} {p.Id} {Raw(p.ProviderMetadata)}",
            ToolCallPart p =>
                $"{p.Type} {p.ToolCall.ToolCallId} {p.ToolCall.ToolName} {p.ToolCall.ArgumentsJson}",
            ToolInputStartPart p => $"{p.Type} {p.Id} {p.ToolName}",
            ToolInputDeltaPart p => $"{p.Type} {p.Id} {p.Delta}",
            ToolInputEndPart p => $"{p.Type} {p.Id}",
            _ => part.Type,
        };

    private static string Quote(string text) => JsonSerializer.Serialize(text);

    private static string Raw(JsonElement? metadata) => metadata?.GetRawText() ?? "-";
}
