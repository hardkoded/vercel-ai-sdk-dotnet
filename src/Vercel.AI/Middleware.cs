// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using Vercel.AI.Util;

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

/// <summary>Splits delimited reasoning, such as <c>&lt;think&gt;...&lt;/think&gt;</c>, out of text. Maps to <c>extractReasoningMiddleware</c>.</summary>
public sealed class ExtractReasoningMiddleware : LanguageModelMiddleware
{
    private readonly string _opening;
    private readonly string _closing;
    private readonly string _separator;
    private readonly bool _startWithReasoning;
    private readonly Regex _reasoning;

    /// <summary>Creates middleware for the XML tags <c>&lt;tagName&gt;</c> and <c>&lt;/tagName&gt;</c>.</summary>
    /// <param name="tagName">Tag name without angle brackets.</param>
    /// <param name="separator">Joins reasoning blocks, and the text around them.</param>
    /// <param name="startWithReasoning">Prepends the opening tag when the model omits it.</param>
    public ExtractReasoningMiddleware(string tagName = "think", string separator = "\n", bool startWithReasoning = false)
        : this(("<" + tagName + ">", "</" + tagName + ">"), separator, startWithReasoning)
    {
    }

    /// <summary>Creates middleware for literal delimiters, such as Gemma's thought channel.</summary>
    /// <param name="tagName">Literal opening and closing delimiters.</param>
    /// <param name="separator">Joins reasoning blocks, and the text around them.</param>
    /// <param name="startWithReasoning">Prepends the opening delimiter when the model omits it.</param>
    public ExtractReasoningMiddleware((string Opening, string Closing) tagName, string separator = "\n", bool startWithReasoning = false)
    {
        if (string.IsNullOrEmpty(tagName.Opening) || string.IsNullOrEmpty(tagName.Closing))
        {
            throw new InvalidArgumentError(nameof(tagName), tagName, "Reasoning delimiters must not be empty.");
        }

        _opening = tagName.Opening;
        _closing = tagName.Closing;
        _separator = separator;
        _startWithReasoning = startWithReasoning;
        _reasoning = new Regex(Regex.Escape(_opening) + "(.*?)" + Regex.Escape(_closing), RegexOptions.Singleline);
    }

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
            if (part is not GeneratedText generatedText)
            {
                content.Add(part);
                continue;
            }

            var text = _startWithReasoning ? _opening + generatedText.Text : generatedText.Text;
            var matches = _reasoning.Matches(text);
            if (matches.Count == 0)
            {
                content.Add(part);
                continue;
            }

            var reasoning = new string[matches.Count];
            var remaining = text;
            for (var i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                reasoning[i] = match.Groups[1].Value;
                var before = remaining.Substring(0, match.Index);
                var after = remaining.Substring(match.Index + match.Length);
                remaining = before + (before.Length > 0 && after.Length > 0 ? _separator : string.Empty) + after;
            }

            content.Add(new GeneratedReasoning(string.Join(_separator, reasoning)));
            content.Add(new GeneratedText(remaining));
        }

        return MiddlewareResults.CopyResult(content, result);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<LanguageModelStreamPart> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, IAsyncEnumerable<LanguageModelStreamPart>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var extractions = new Dictionary<string, StreamExtraction>();

        // Holds text-start until text is published, so it never comes before reasoning-start.
        var delayedTextStarts = new Dictionary<string, TextStartStreamPart>();
        var reasoningIds = 0;
        var output = new List<LanguageModelStreamPart>();
        await foreach (var part in next(options, cancellationToken).ConfigureAwait(false))
        {
            switch (part)
            {
                case TextStartStreamPart start:
                    delayedTextStarts[start.Id] = start;
                    break;
                case TextDeltaStreamPart delta:
                    if (!extractions.TryGetValue(delta.Id, out var extraction))
                    {
                        extraction = new StreamExtraction(delta.Id, _startWithReasoning);
                        extractions[delta.Id] = extraction;
                    }

                    extraction.Buffer += delta.Delta;
                    Extract(extraction);
                    break;
                case TextEndStreamPart end:
                    if (extractions.TryGetValue(end.Id, out var ended))
                    {
                        Publish(ended, ended.Buffer);
                        ended.Buffer = string.Empty;
                    }

                    FlushTextStart(end.Id);
                    output.Add(end);
                    break;
                default:
                    output.Add(part);
                    break;
            }

            foreach (var item in output)
            {
                yield return item;
            }

            output.Clear();
        }

        void Extract(StreamExtraction extraction)
        {
            while (true)
            {
                var tag = extraction.IsReasoning ? _closing : _opening;
                var startIndex = Collections.GetPotentialStartIndex(extraction.Buffer, tag);
                if (startIndex is null)
                {
                    Publish(extraction, extraction.Buffer);
                    extraction.Buffer = string.Empty;
                    return;
                }

                Publish(extraction, extraction.Buffer.Substring(0, startIndex.Value));
                if (startIndex.Value + tag.Length > extraction.Buffer.Length)
                {
                    // The buffer ends with part of a delimiter. Wait for more text.
                    extraction.Buffer = extraction.Buffer.Substring(startIndex.Value);
                    return;
                }

                extraction.Buffer = extraction.Buffer.Substring(startIndex.Value + tag.Length);
                if (extraction.IsReasoning)
                {
                    // An empty reasoning block published no delta, so it still needs its start.
                    if (extraction.IsFirstReasoning)
                    {
                        output.Add(new ReasoningStartStreamPart(ReasoningId(extraction)));
                    }

                    output.Add(new ReasoningEndStreamPart(ReasoningId(extraction)));
                    extraction.ReasoningId = null;
                }

                extraction.IsReasoning = !extraction.IsReasoning;
                extraction.AfterSwitch = true;
            }
        }

        void Publish(StreamExtraction extraction, string text)
        {
            if (text.Length == 0)
            {
                return;
            }

            var isFirst = extraction.IsReasoning ? extraction.IsFirstReasoning : extraction.IsFirstText;
            var prefix = extraction.AfterSwitch && !isFirst ? _separator : string.Empty;
            if (extraction.IsReasoning)
            {
                if (extraction.AfterSwitch || extraction.IsFirstReasoning)
                {
                    output.Add(new ReasoningStartStreamPart(ReasoningId(extraction)));
                }

                output.Add(new ReasoningDeltaStreamPart(ReasoningId(extraction), prefix + text));
                extraction.IsFirstReasoning = false;
            }
            else
            {
                FlushTextStart(extraction.TextId);
                output.Add(new TextDeltaStreamPart(extraction.TextId, prefix + text));
                extraction.IsFirstText = false;
            }

            extraction.AfterSwitch = false;
        }

        string ReasoningId(StreamExtraction extraction)
        {
            return extraction.ReasoningId ??= "reasoning-" + reasoningIds++;
        }

        void FlushTextStart(string id)
        {
            if (delayedTextStarts.TryGetValue(id, out var start))
            {
                output.Add(start);
                delayedTextStarts.Remove(id);
            }
        }
    }

    private sealed class StreamExtraction
    {
        public StreamExtraction(string textId, bool isReasoning)
        {
            TextId = textId;
            IsReasoning = isReasoning;
        }

        public string TextId { get; }

        public bool IsReasoning { get; set; }

        public bool IsFirstReasoning { get; set; } = true;

        public bool IsFirstText { get; set; } = true;

        public bool AfterSwitch { get; set; }

        public string Buffer { get; set; } = string.Empty;

        public string? ReasoningId { get; set; }
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

