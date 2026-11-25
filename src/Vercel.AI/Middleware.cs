// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>Wraps <c>doGenerate</c> and <c>doStream</c>. Maps to language-model middleware.</summary>
public interface ILanguageModelMiddleware
{
    /// <summary>Wraps a generate call.</summary>
    Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken
    );

    /// <summary>Wraps a stream call.</summary>
    Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next,
        CancellationToken cancellationToken
    );
}

/// <summary>Passes both operations through.</summary>
public abstract class LanguageModelMiddleware : ILanguageModelMiddleware
{
    /// <inheritdoc />
    public virtual Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken
    )
    {
        return next(options, cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next,
        CancellationToken cancellationToken
    )
    {
        return next(options, cancellationToken);
    }
}

/// <summary>Wraps a language model with middleware. Maps to <c>wrapLanguageModel</c>.</summary>
public static class LanguageModelMiddlewareExtensions
{
    /// <summary>Wraps <paramref name="model"/> so middleware runs outside-in.</summary>
    public static ILanguageModel WrapLanguageModel(
        this ILanguageModel model,
        params ILanguageModelMiddleware[] middleware
    )
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
    public ExtractReasoningMiddleware(
        string tagName = "think",
        string separator = "\n",
        bool startWithReasoning = false
    )
        : this(("<" + tagName + ">", "</" + tagName + ">"), separator, startWithReasoning) { }

    /// <summary>Creates middleware for literal delimiters, such as Gemma's thought channel.</summary>
    /// <param name="tagName">Literal opening and closing delimiters.</param>
    /// <param name="separator">Joins reasoning blocks, and the text around them.</param>
    /// <param name="startWithReasoning">Prepends the opening delimiter when the model omits it.</param>
    public ExtractReasoningMiddleware(
        (string Opening, string Closing) tagName,
        string separator = "\n",
        bool startWithReasoning = false
    )
    {
        if (string.IsNullOrEmpty(tagName.Opening) || string.IsNullOrEmpty(tagName.Closing))
        {
            throw new InvalidArgumentError(
                nameof(tagName),
                tagName,
                "Reasoning delimiters must not be empty."
            );
        }

        _opening = tagName.Opening;
        _closing = tagName.Closing;
        _separator = separator;
        _startWithReasoning = startWithReasoning;
        _reasoning = new Regex(
            Regex.Escape(_opening) + "(.*?)" + Regex.Escape(_closing),
            RegexOptions.Singleline,
            SafeRegex.LargeInputTimeout
        );
    }

    // A match timeout counts as no reasoning block, so the text passes through unchanged.
    private bool TryMatchReasoning(string text, out MatchCollection matches)
    {
        try
        {
            matches = _reasoning.Matches(text);
            _ = matches.Count;
            return true;
        }
        catch (RegexMatchTimeoutException)
        {
            matches = null!;
            return false;
        }
    }

    /// <inheritdoc />
    public override async Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken
    )
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

            var (remaining, reasoning) = ExtractReasoning(generatedText.Text);
            if (reasoning == null)
            {
                content.Add(part);
                continue;
            }

            content.Add(new GeneratedReasoning(reasoning));
            content.Add(new GeneratedText(remaining));
        }

        var structuredOutputText =
            result.StructuredOutputText == null
                ? null
                : ExtractReasoning(result.StructuredOutputText).Text;
        return MiddlewareResults.CopyResult(content, result, structuredOutputText);
    }

    // Splits the reasoning blocks out of one text. Reasoning is null when the text has none.
    private (string Text, string? Reasoning) ExtractReasoning(string input)
    {
        var text = _startWithReasoning ? _opening + input : input;
        if (!TryMatchReasoning(text, out var matches) || matches.Count == 0)
        {
            return (input, null);
        }

        var reasoning = new string[matches.Count];
        var remaining = text;
        for (var i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            reasoning[i] = match.Groups[1].Value;
            var before = remaining.Substring(0, match.Index);
            var after = remaining.Substring(match.Index + match.Length);
            remaining =
                before
                + (before.Length > 0 && after.Length > 0 ? _separator : string.Empty)
                + after;
        }

        return (remaining, string.Join(_separator, reasoning));
    }

    /// <inheritdoc />
    public override async Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next,
        CancellationToken cancellationToken
    )
    {
        var inner = await next(options, cancellationToken).ConfigureAwait(false);
        return new LanguageModelStreamResult(
            Extract(inner.Stream, cancellationToken),
            inner.Request,
            inner.Response,
            inner
        );
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> Extract(
        IAsyncEnumerable<LanguageModelStreamPart> source,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var extractions = new Dictionary<string, StreamExtraction>();

        // Holds text-start until text is published, so it never comes before reasoning-start.
        var delayedTextStarts = new Dictionary<string, TextStartStreamPart>();
        var reasoningIds = 0;
        var output = new List<LanguageModelStreamPart>();
        await foreach (var part in source.WithCancellation(cancellationToken).ConfigureAwait(false))
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

            var isFirst = extraction.IsReasoning
                ? extraction.IsFirstReasoning
                : extraction.IsFirstText;
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
    private static readonly Regex s_prefix = new(
        @"^```(?:json)?\s*\n",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5)
    );

    private static readonly Regex s_suffix = new(
        @"\n?```\s*\z",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(5)
    );

    private readonly Func<string, string> _transform;
    private readonly bool _hasCustomTransform;

    /// <summary>Creates middleware that strips markdown code fences.</summary>
    public ExtractJsonMiddleware()
        : this(null) { }

    /// <summary>Creates middleware with an optional custom transform.</summary>
    /// <param name="transform">
    /// Custom transform applied to text content and structured output text. Replaces the default markdown fence stripping when set.
    /// </param>
    public ExtractJsonMiddleware(Func<string, string>? transform)
    {
        _transform = transform ?? StripFence;
        _hasCustomTransform = transform is not null;
    }

    /// <inheritdoc />
    public override async Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelGenerateResult>> next,
        CancellationToken cancellationToken
    )
    {
        var result = await next(options, cancellationToken).ConfigureAwait(false);
        var content = new List<GeneratedContent>();
        foreach (var part in result.Content)
        {
            if (part is GeneratedText text)
            {
                content.Add(new GeneratedText(_transform(text.Text)));
            }
            else
            {
                content.Add(part);
            }
        }

        var structuredOutputText =
            result.StructuredOutputText == null ? null : _transform(result.StructuredOutputText);
        return MiddlewareResults.CopyResult(content, result, structuredOutputText);
    }

    /// <inheritdoc />
    public override async Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next,
        CancellationToken cancellationToken
    )
    {
        var inner = await next(options, cancellationToken).ConfigureAwait(false);
        return new LanguageModelStreamResult(
            Transform(inner.Stream, cancellationToken),
            inner.Request,
            inner.Response,
            inner
        );
    }

    internal static string StripFence(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstLine = trimmed.IndexOf('\n');
        if (firstLine < 0)
        {
            return trimmed;
        }

        var body = trimmed.Substring(firstLine + 1);
        if (body.EndsWith("```", StringComparison.Ordinal))
        {
            body = body.Substring(0, body.Length - 3);
        }

        return body.Trim();
    }

    private static string StripMarkdownCodeFenceSuffix(string text) =>
        s_suffix.Replace(text, string.Empty).TrimEnd();

    private static int GetPotentialSuffixStart(string text)
    {
        var index = text.Length;
        while (index > 0 && char.IsWhiteSpace(text[index - 1]))
        {
            index--;
        }

        var backtickCount = 0;
        while (index > 0 && backtickCount < 3 && text[index - 1] == '`')
        {
            index--;
            backtickCount++;
        }

        if (backtickCount > 0)
        {
            while (index > 0 && char.IsWhiteSpace(text[index - 1]))
            {
                index--;
            }
        }

        return index;
    }

    private async IAsyncEnumerable<LanguageModelStreamPart> Transform(
        IAsyncEnumerable<LanguageModelStreamPart> source,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var textBlocks = new Dictionary<string, TextBlock>();
        await foreach (
            var chunk in source.WithCancellation(cancellationToken).ConfigureAwait(false)
        )
        {
            if (chunk is TextStartStreamPart start)
            {
                // Custom transforms need to buffer all content.
                textBlocks[start.Id] = new TextBlock(
                    start,
                    _hasCustomTransform ? BlockPhase.Buffering : BlockPhase.Prefix
                );
                continue;
            }

            if (chunk is TextDeltaStreamPart delta)
            {
                if (!textBlocks.TryGetValue(delta.Id, out var block))
                {
                    yield return chunk;
                    continue;
                }

                block.Buffer += delta.Delta;
                if (block.Phase == BlockPhase.Buffering)
                {
                    continue;
                }

                if (block.Phase == BlockPhase.Prefix)
                {
                    if (block.Buffer.Length > 0 && block.Buffer[0] != '`')
                    {
                        block.Phase = BlockPhase.Streaming;
                        yield return block.StartEvent;
                    }
                    else if (block.Buffer.StartsWith("```", StringComparison.Ordinal))
                    {
                        // Only strip the prefix once a newline shows the fence is complete.
                        if (block.Buffer.IndexOf('\n') >= 0)
                        {
                            var match = s_prefix.Match(block.Buffer);
                            if (match.Success)
                            {
                                block.Buffer = block.Buffer.Substring(match.Length);
                                block.PrefixStripped = true;
                            }

                            block.Phase = BlockPhase.Streaming;
                            yield return block.StartEvent;
                        }
                    }
                    else if (block.Buffer.Length >= 3)
                    {
                        block.Phase = BlockPhase.Streaming;
                        yield return block.StartEvent;
                    }
                }

                // Stream content while retaining anything that could still become
                // trailing whitespace or a markdown fence suffix.
                if (block.Phase == BlockPhase.Streaming)
                {
                    var suffixStart = GetPotentialSuffixStart(block.Buffer);
                    var toStream = block.Buffer.Substring(0, suffixStart);
                    block.Buffer = block.Buffer.Substring(suffixStart);
                    if (toStream.Length > 0)
                    {
                        yield return new TextDeltaStreamPart(delta.Id, toStream);
                    }
                }

                continue;
            }

            if (chunk is TextEndStreamPart end && textBlocks.TryGetValue(end.Id, out var ending))
            {
                if (ending.Phase is BlockPhase.Prefix or BlockPhase.Buffering)
                {
                    yield return ending.StartEvent;
                }

                var remaining = ending.Buffer;
                if (
                    ending.Phase == BlockPhase.Buffering
                    || ending.Phase == BlockPhase.Prefix && !ending.PrefixStripped
                )
                {
                    // Nothing has streamed yet, so the full transform is safe.
                    remaining = _transform(remaining);
                }
                else
                {
                    // Only strip the suffix. Earlier text may already have streamed,
                    // so trimming would remove valid leading whitespace.
                    remaining = StripMarkdownCodeFenceSuffix(remaining);
                }

                if (remaining.Length > 0)
                {
                    yield return new TextDeltaStreamPart(end.Id, remaining);
                }

                yield return chunk;
                textBlocks.Remove(end.Id);
                continue;
            }

            yield return chunk;
        }
    }

    private enum BlockPhase
    {
        Prefix,
        Streaming,
        Buffering,
    }

    private sealed class TextBlock(TextStartStreamPart startEvent, BlockPhase phase)
    {
        public TextStartStreamPart StartEvent { get; } = startEvent;

        public BlockPhase Phase { get; set; } = phase;

        public string Buffer { get; set; } = string.Empty;

        public bool PrefixStripped { get; set; }
    }
}

internal static class MiddlewareResults
{
    public static LanguageModelGenerateResult CopyResult(
        IReadOnlyList<GeneratedContent> content,
        LanguageModelGenerateResult result,
        string? structuredOutputText
    )
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
            result.ResponseHeaders,
            structuredOutputText,
            result.Request
        );
    }
}

/// <summary>Turns a non-streaming model into text deltas. Maps to <c>simulateStreamingMiddleware</c>.</summary>
public sealed class SimulateStreamingMiddleware : LanguageModelMiddleware
{
    /// <summary>Generates once and yields the text as a single delta.</summary>
    public async Task<LanguageModelStreamResult> Simulate(
        ILanguageModel model,
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var result = await model.DoGenerateAsync(options, cancellationToken).ConfigureAwait(false);
        return new LanguageModelStreamResult(
            Parts(result),
            response: new LanguageModelStreamResponse(result.ResponseHeaders)
        );
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(
        LanguageModelGenerateResult result
    )
    {
        await Task.CompletedTask.ConfigureAwait(false);
        var id = 0;
        foreach (var part in result.Content)
        {
            if (
                part is GeneratedText generatedText
                && (generatedText.Text.Length > 0 || (generatedText.Citations?.Count ?? 0) > 0)
            )
            {
                var textId = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                yield return new TextStartStreamPart(textId);
                if (generatedText.Text.Length > 0)
                {
                    yield return new TextDeltaStreamPart(textId, generatedText.Text);
                }

                yield return new TextEndStreamPart(textId, generatedText.Citations);
                id++;
            }
        }

        yield return new FinishStreamPart(
            result.FinishReason,
            result.Usage,
            result.RawFinishReason,
            result.ProviderMetadata
        );
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
        CancellationToken cancellationToken
    )
    {
        Apply(options);
        return next(options, cancellationToken);
    }

    /// <inheritdoc />
    public override Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelCallOptions options,
        Func<LanguageModelCallOptions, CancellationToken, Task<LanguageModelStreamResult>> next,
        CancellationToken cancellationToken
    )
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
        CancellationToken cancellationToken
    )
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

    public IReadOnlyDictionary<string, string> SupportedUrls => _inner.SupportedUrls;

    public string Provider => _inner.Provider;

    public string ModelId => _inner.ModelId;

    public Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        return _middleware.WrapGenerateAsync(options, _inner.DoGenerateAsync, cancellationToken);
    }

    public Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        if (_middleware is SimulateStreamingMiddleware simulate)
        {
            return simulate.Simulate(_inner, options, cancellationToken);
        }

        return _middleware.WrapStreamAsync(options, _inner.DoStreamAsync, cancellationToken);
    }
}
