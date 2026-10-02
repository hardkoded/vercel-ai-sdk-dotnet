// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>Wraps <c>doGenerate</c> and <c>doStream</c>. Maps to language-model middleware.</summary>
public interface ILanguageModelMiddleware
{
    /// <summary>Wraps a generate call.</summary>
    Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken);

    /// <summary>Wraps a stream call.</summary>
    IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
        CancellationToken cancellationToken);
}

/// <summary>Passes both operations through.</summary>
public abstract class LanguageModelMiddleware : ILanguageModelMiddleware
{
    /// <inheritdoc />
    public virtual Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken)
    {
        return next(options, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var part in next(options, cancellationToken).ConfigureAwait(false))
        {
            yield return part;
        }
    }
}

/// <summary>Wraps a language model with middleware. Maps to <c>wrapLanguageModel</c>.</summary>
public static class LanguageModelMiddlewareExtensions
{
    /// <summary>Wraps <paramref name="model"/> so middleware runs outside-in.</summary>
    public static ILanguageModel WrapLanguageModel(this ILanguageModel model, params ILanguageModelMiddleware[] middleware)
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        ILanguageModel current = model;
        if (middleware != null)
        {
            for (var i = middleware.Length - 1; i >= 0; i--)
            {
                current = new MiddlewareLanguageModel(current, middleware[i]);
            }
        }

        return current;
    }
}

/// <summary>Splits an XML tag out of text into reasoning. Maps to <c>extractReasoningMiddleware</c>.</summary>
public sealed class ExtractReasoningMiddleware : LanguageModelMiddleware
{
    private readonly Regex _tags;
    private readonly string _openingTag;
    private readonly string _closingTag;

    /// <summary>Extracts <c>&lt;think&gt;</c> tags, joining sections with a newline.</summary>
    public ExtractReasoningMiddleware()
        : this("think", "\n", false)
    {
    }

    /// <summary>Extracts <c>&lt;tagName&gt;</c> sections.</summary>
    /// <param name="tagName">XML tag that wraps reasoning.</param>
    /// <param name="separator">Text inserted between extracted sections.</param>
    /// <param name="startWithReasoning">Treat the text as already inside the reasoning tag.</param>
    public ExtractReasoningMiddleware(string tagName, string separator, bool startWithReasoning)
    {
        TagName = string.IsNullOrEmpty(tagName) ? "think" : tagName;
        Separator = separator ?? "\n";
        StartWithReasoning = startWithReasoning;
        _openingTag = "<" + TagName + ">";
        _closingTag = "</" + TagName + ">";
        _tags = new Regex(Regex.Escape(_openingTag) + "(.*?)" + Regex.Escape(_closingTag), RegexOptions.Singleline | RegexOptions.Compiled);
    }

    /// <summary>Tag extracted from the text.</summary>
    public string TagName { get; }

    /// <summary>Separator between reasoning sections and between text sections.</summary>
    public string Separator { get; }

    /// <summary>Whether generation starts inside the reasoning tag.</summary>
    public bool StartWithReasoning { get; }

    /// <inheritdoc />
    public override async Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken)
    {
        var result = await next(options, cancellationToken).ConfigureAwait(false);
        var content = new List<GeneratedContent>();
        foreach (var part in result.Content)
        {
            if (part is GeneratedText text)
            {
                var source = StartWithReasoning ? _openingTag + text.Text : text.Text;
                var matches = _tags.Matches(source);
                if (matches.Count == 0)
                {
                    content.Add(part);
                    continue;
                }

                var reasoning = new string[matches.Count];
                for (var i = 0; i < matches.Count; i++)
                {
                    reasoning[i] = matches[i].Groups[1].Value;
                }

                var textWithoutReasoning = source;
                for (var i = matches.Count - 1; i >= 0; i--)
                {
                    var match = matches[i];
                    var before = textWithoutReasoning.Substring(0, match.Index);
                    var after = textWithoutReasoning.Substring(match.Index + match.Length);
                    var glue = before.Length > 0 && after.Length > 0 ? Separator : string.Empty;
                    textWithoutReasoning = before + glue + after;
                }

                content.Add(new GeneratedReasoning(string.Join(Separator, reasoning)));
                content.Add(new GeneratedText(textWithoutReasoning.Trim()));
                continue;
            }

            content.Add(part);
        }

        return MiddlewareResults.CopyResult(content, result);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var extractions = new Dictionary<string, ReasoningExtraction>(StringComparer.Ordinal);
        var delayedStarts = new Dictionary<string, TextStartStreamPart>(StringComparer.Ordinal);
        var reasoningCounter = 0;
        await foreach (var part in next(options, cancellationToken).ConfigureAwait(false))
        {
            foreach (var emitted in Transform(part, extractions, delayedStarts, ref reasoningCounter))
            {
                yield return emitted;
            }
        }
    }

    private List<LanguageModelStreamPart> Transform(
        LanguageModelStreamPart part,
        Dictionary<string, ReasoningExtraction> extractions,
        Dictionary<string, TextStartStreamPart> delayedStarts,
        ref int reasoningCounter)
    {
        var emitted = new List<LanguageModelStreamPart>();
        if (part is TextStartStreamPart start)
        {
            delayedStarts[start.Id] = start;
            return emitted;
        }

        if (part is TextEndStreamPart end && delayedStarts.ContainsKey(end.Id))
        {
            emitted.Add(delayedStarts[end.Id]);
            delayedStarts.Remove(end.Id);
        }

        if (!(part is TextDeltaStreamPart delta))
        {
            emitted.Add(part);
            return emitted;
        }

        if (!extractions.TryGetValue(delta.Id, out var active))
        {
            active = new ReasoningExtraction(delta.Id, StartWithReasoning);
            extractions[delta.Id] = active;
        }

        active.Buffer += delta.Delta;
        while (true)
        {
            var nextTag = active.IsReasoning ? _closingTag : _openingTag;
            var startIndex = PotentialStart(active.Buffer, nextTag);
            if (startIndex == null)
            {
                Publish(emitted, delayedStarts, active, active.Buffer, ref reasoningCounter);
                active.Buffer = string.Empty;
                break;
            }

            Publish(emitted, delayedStarts, active, active.Buffer.Substring(0, startIndex.Value), ref reasoningCounter);
            var foundFullMatch = startIndex.Value + nextTag.Length <= active.Buffer.Length;
            if (foundFullMatch)
            {
                active.Buffer = active.Buffer.Substring(startIndex.Value + nextTag.Length);
                if (active.IsReasoning)
                {
                    if (active.IsFirstReasoning)
                    {
                        emitted.Add(new ReasoningStartStreamPart(ReasoningId(active, ref reasoningCounter)));
                    }

                    emitted.Add(new ReasoningEndStreamPart(ReasoningId(active, ref reasoningCounter)));
                    active.ReasoningId = null;
                }

                active.IsReasoning = !active.IsReasoning;
                active.AfterSwitch = true;
            }
            else
            {
                active.Buffer = active.Buffer.Substring(startIndex.Value);
                break;
            }
        }

        return emitted;
    }

    private void Publish(
        List<LanguageModelStreamPart> emitted,
        Dictionary<string, TextStartStreamPart> delayedStarts,
        ReasoningExtraction active,
        string text,
        ref int reasoningCounter)
    {
        if (text.Length == 0)
        {
            return;
        }

        var prefix = active.AfterSwitch && (active.IsReasoning ? !active.IsFirstReasoning : !active.IsFirstText)
            ? Separator
            : string.Empty;
        if (active.IsReasoning && (active.AfterSwitch || active.IsFirstReasoning))
        {
            emitted.Add(new ReasoningStartStreamPart(ReasoningId(active, ref reasoningCounter)));
        }

        if (active.IsReasoning)
        {
            emitted.Add(new ReasoningDeltaStreamPart(ReasoningId(active, ref reasoningCounter), prefix + text));
        }
        else
        {
            if (delayedStarts.TryGetValue(active.TextId, out var delayed))
            {
                emitted.Add(delayed);
                delayedStarts.Remove(active.TextId);
            }

            emitted.Add(new TextDeltaStreamPart(active.TextId, prefix + text));
        }

        active.AfterSwitch = false;
        if (active.IsReasoning)
        {
            active.IsFirstReasoning = false;
        }
        else
        {
            active.IsFirstText = false;
        }
    }

    private static string ReasoningId(ReasoningExtraction active, ref int reasoningCounter)
    {
        if (active.ReasoningId == null)
        {
            active.ReasoningId = "reasoning-" + reasoningCounter.ToString();
            reasoningCounter++;
        }

        return active.ReasoningId;
    }

    private static int? PotentialStart(string text, string searched)
    {
        if (searched.Length == 0)
        {
            return null;
        }

        var direct = text.IndexOf(searched, StringComparison.Ordinal);
        if (direct >= 0)
        {
            return direct;
        }

        for (var i = text.Length - 1; i >= 0; i--)
        {
            if (searched.StartsWith(text.Substring(i), StringComparison.Ordinal))
            {
                return i;
            }
        }

        return null;
    }

    private sealed class ReasoningExtraction
    {
        public ReasoningExtraction(string textId, bool isReasoning)
        {
            TextId = textId;
            IsReasoning = isReasoning;
        }

        public bool IsFirstReasoning { get; set; } = true;

        public bool IsFirstText { get; set; } = true;

        public bool AfterSwitch { get; set; }

        public bool IsReasoning { get; set; }

        public string Buffer { get; set; } = string.Empty;

        public string? ReasoningId { get; set; }

        public string TextId { get; }
    }
}

/// <summary>Strips markdown JSON fences. Maps to <c>extractJsonMiddleware</c>.</summary>
public sealed class ExtractJsonMiddleware : LanguageModelMiddleware
{
    /// <inheritdoc />
    public override async Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken)
    {
        var result = await next(options, cancellationToken).ConfigureAwait(false);
        var content = new List<GeneratedContent>();
        foreach (var part in result.Content)
        {
            if (part is GeneratedText text)
            {
                content.Add(new GeneratedText(StripFence(text.Text)));
            }
            else
            {
                content.Add(part);
            }
        }

        return MiddlewareResults.CopyResult(content, result);
    }

    internal static string StripFence(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```"))
        {
            return trimmed;
        }

        var firstLine = trimmed.IndexOf('\n');
        if (firstLine < 0)
        {
            return trimmed;
        }

        var body = trimmed.Substring(firstLine + 1);
        if (body.EndsWith("```"))
        {
            body = body.Substring(0, body.Length - 3);
        }

        return body.Trim();
    }
}

internal static class MiddlewareResults
{
    public static LanguageModelGenerateResult CopyResult(IReadOnlyList<GeneratedContent> content, LanguageModelGenerateResult result)
    {
        return new LanguageModelGenerateResult(
            content,
            result.FinishReason,
            result.Usage,
            result.RawFinishReason,
            result.Warnings,
            result.ResponseId,
            result.ProviderMetadata,
            result.RawResponse,
            result.ResponseModelId,
            result.ResponseTimestamp,
            result.ResponseHeaders);
    }
}

/// <summary>Turns a non-streaming model into text deltas. Maps to <c>simulateStreamingMiddleware</c>.</summary>
public sealed class SimulateStreamingMiddleware : LanguageModelMiddleware
{
    /// <summary>Generates once and yields the text as a single delta.</summary>
    public async IAsyncEnumerable<LanguageModelStreamPart> Simulate(
        ILanguageModel model,
        LanguageModelCallOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await model.DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(result.Text))
        {
            yield return new TextDeltaStreamPart("text", result.Text);
        }

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason, result.ProviderMetadata);
    }
}

/// <summary>Fills sampling settings when the caller left them unset. Maps to <c>defaultSettingsMiddleware</c>.</summary>
public sealed class DefaultSettingsMiddleware : LanguageModelMiddleware
{
    /// <summary>Creates middleware with default sampling settings.</summary>
    public DefaultSettingsMiddleware(double? temperature = null, int? maxOutputTokens = null)
    {
        Temperature = temperature;
        MaxOutputTokens = maxOutputTokens;
    }

    /// <summary>Default temperature.</summary>
    public double? Temperature { get; }

    /// <summary>Default max output tokens.</summary>
    public int? MaxOutputTokens { get; }

    /// <inheritdoc />
    public override Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken)
    {
        Apply(options);
        return next(options, cancellationToken);
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
        CancellationToken cancellationToken)
    {
        Apply(options);
        return next(options, cancellationToken);
    }

    private void Apply(LanguageModelCallOptions options)
    {
        options.Temperature ??= Temperature;
        options.MaxOutputTokens ??= MaxOutputTokens;
    }
}

/// <summary>Prepends system instructions when the prompt has none. Maps to <c>defaultInstructionsMiddleware</c>.</summary>
public sealed class DefaultInstructionsMiddleware : LanguageModelMiddleware
{
    /// <summary>Creates middleware.</summary>
    public DefaultInstructionsMiddleware(string instructions)
    {
        Instructions = instructions ?? string.Empty;
    }

    /// <summary>Instructions to prepend.</summary>
    public string Instructions { get; }

    /// <inheritdoc />
    public override Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken)
    {
        Apply(options);
        return next(options, cancellationToken);
    }

    private static bool HasSystem(IReadOnlyList<ModelMessage> prompt)
    {
        foreach (var message in prompt)
        {
            if (message is SystemModelMessage)
            {
                return true;
            }
        }

        return false;
    }

    private void Apply(LanguageModelCallOptions options)
    {
        if (HasSystem(options.Prompt) || string.IsNullOrEmpty(Instructions))
        {
            return;
        }

        var messages = new List<ModelMessage> { new SystemModelMessage(Instructions) };
        messages.AddRange(options.Prompt);
        options.Prompt = messages;
    }
}

internal sealed class MiddlewareLanguageModel : ILanguageModel
{
    private readonly ILanguageModel _inner;
    private readonly ILanguageModelMiddleware _middleware;

    public MiddlewareLanguageModel(ILanguageModel inner, ILanguageModelMiddleware middleware)
    {
        _inner = inner;
        _middleware = middleware;
    }

    public string SpecificationVersion => _inner.SpecificationVersion;

    public string Provider => _inner.Provider;

    public string ModelId => _inner.ModelId;

    public Task<LanguageModelGenerateResult> DoGenerateAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        return _middleware.WrapGenerateAsync(options, _inner.DoGenerateAsync, cancellationToken);
    }

    public IAsyncEnumerable<LanguageModelStreamPart> DoStreamAsync(LanguageModelCallOptions options, CancellationToken cancellationToken)
    {
        if (_middleware is SimulateStreamingMiddleware simulate)
        {
            return simulate.Simulate(_inner, options, cancellationToken);
        }

        return _middleware.WrapStreamAsync(options, _inner.DoStreamAsync, cancellationToken);
    }
}

