// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI;

/// <summary>Whether a call is a generate or a stream call. Maps to the <c>type</c> argument of <c>transformParams</c>.</summary>
public enum LanguageModelCallType
{
    /// <summary>A <c>doGenerate</c> call.</summary>
    Generate,

    /// <summary>A <c>doStream</c> call.</summary>
    Stream,
}

/// <summary>What <c>wrapGenerate</c> and <c>wrapStream</c> receive: the call options, the model, and both ways to continue.</summary>
public sealed class LanguageModelWrapContext
{
    private readonly Func<CancellationToken, Task<LanguageModelGenerateResult>> _doGenerate;
    private readonly Func<CancellationToken, Task<LanguageModelStreamResult>> _doStream;

    /// <summary>Creates a context.</summary>
    public LanguageModelWrapContext(
        LanguageModelCallOptions options,
        ILanguageModel model,
        Func<CancellationToken, Task<LanguageModelGenerateResult>> doGenerate,
        Func<CancellationToken, Task<LanguageModelStreamResult>> doStream
    )
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _doGenerate = doGenerate ?? throw new ArgumentNullException(nameof(doGenerate));
        _doStream = doStream ?? throw new ArgumentNullException(nameof(doStream));
    }

    /// <summary>The call options after <c>transformParams</c>. Maps to <c>params</c>.</summary>
    public LanguageModelCallOptions Options { get; }

    /// <summary>The wrapped model.</summary>
    public ILanguageModel Model { get; }

    /// <summary>Calls <c>doGenerate</c> of the wrapped model with <see cref="Options"/>.</summary>
    public Task<LanguageModelGenerateResult> DoGenerateAsync(CancellationToken cancellationToken) =>
        _doGenerate(cancellationToken);

    /// <summary>Calls <c>doStream</c> of the wrapped model with <see cref="Options"/>.</summary>
    public Task<LanguageModelStreamResult> DoStreamAsync(CancellationToken cancellationToken) =>
        _doStream(cancellationToken);
}

/// <summary>Language-model middleware. Maps to <c>LanguageModelMiddleware</c>.</summary>
public interface ILanguageModelMiddleware
{
    /// <summary>Transforms the call options before the call. Maps to <c>transformParams</c>.</summary>
    Task<LanguageModelCallOptions> TransformParamsAsync(
        LanguageModelCallOptions options,
        LanguageModelCallType type,
        ILanguageModel model,
        CancellationToken cancellationToken
    );

    /// <summary>Wraps a generate call. Maps to <c>wrapGenerate</c>.</summary>
    Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    );

    /// <summary>Wraps a stream call. Maps to <c>wrapStream</c>.</summary>
    Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    );

    /// <summary>Overrides the provider id. Null keeps the model's. Maps to <c>overrideProvider</c>.</summary>
    string? OverrideProvider(ILanguageModel model);

    /// <summary>Overrides the model id. Null keeps the model's. Maps to <c>overrideModelId</c>.</summary>
    string? OverrideModelId(ILanguageModel model);

    /// <summary>Overrides the supported URLs. Null keeps the model's. Maps to <c>overrideSupportedUrls</c>.</summary>
    IReadOnlyDictionary<string, string>? OverrideSupportedUrls(ILanguageModel model);
}

/// <summary>Passes every operation through. Override what the middleware changes.</summary>
public abstract class LanguageModelMiddleware : ILanguageModelMiddleware
{
    /// <inheritdoc />
    public virtual Task<LanguageModelCallOptions> TransformParamsAsync(
        LanguageModelCallOptions options,
        LanguageModelCallType type,
        ILanguageModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(options);

    /// <inheritdoc />
    public virtual Task<LanguageModelGenerateResult> WrapGenerateAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    ) => context.DoGenerateAsync(cancellationToken);

    /// <inheritdoc />
    public virtual Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    ) => context.DoStreamAsync(cancellationToken);

    /// <inheritdoc />
    public virtual string? OverrideProvider(ILanguageModel model) => null;

    /// <inheritdoc />
    public virtual string? OverrideModelId(ILanguageModel model) => null;

    /// <inheritdoc />
    public virtual IReadOnlyDictionary<string, string>? OverrideSupportedUrls(
        ILanguageModel model
    ) => null;
}

/// <summary>Wraps a language model with middleware. Maps to <c>wrapLanguageModel</c>.</summary>
public static class LanguageModelMiddlewareExtensions
{
    /// <summary>Wraps <paramref name="model"/> so middleware runs outside-in.</summary>
    public static ILanguageModel WrapLanguageModel(
        this ILanguageModel model,
        params ILanguageModelMiddleware[] middleware
    ) => WrapLanguageModel(model, middleware, null, null);

    /// <summary>
    /// Wraps <paramref name="model"/> so middleware runs outside-in. The first middleware transforms the input first, and the last is wrapped directly around the model.
    /// <paramref name="modelId"/> and <paramref name="providerId"/> override the ids of every wrapper.
    /// </summary>
    public static ILanguageModel WrapLanguageModel(
        this ILanguageModel model,
        IEnumerable<ILanguageModelMiddleware> middleware,
        string? modelId = null,
        string? providerId = null
    )
    {
        if (model is null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        ILanguageModel current = model;
        if (middleware != null)
        {
            foreach (var item in middleware.Reverse())
            {
                current = new MiddlewareLanguageModel(current, item, modelId, providerId);
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
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    )
    {
        var result = await context.DoGenerateAsync(cancellationToken).ConfigureAwait(false);
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
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    )
    {
        var inner = await context.DoStreamAsync(cancellationToken).ConfigureAwait(false);
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
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    )
    {
        var result = await context.DoGenerateAsync(cancellationToken).ConfigureAwait(false);
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
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    )
    {
        var inner = await context.DoStreamAsync(cancellationToken).ConfigureAwait(false);
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
    // Maps a generated content part to the stream part with the same shape.
    public static LanguageModelStreamPart ToStreamPart(GeneratedContent part) =>
        part switch
        {
            GeneratedToolCall call => new ToolCallStreamPart(
                call.ToolCallId,
                call.ToolName,
                call.ArgumentsJson,
                call.ProviderMetadata
            )
            {
                ProviderExecuted = call.ProviderExecuted,
                Dynamic = call.Dynamic,
            },
            GeneratedToolResult result => new ToolResultStreamPart(
                result.ToolCallId,
                result.ToolName,
                System.Text.Json.JsonDocument.Parse(result.ResultJson).RootElement.Clone()
            )
            {
                IsError = result.IsError,
                ProviderMetadata = result.ProviderMetadata,
            },
            GeneratedToolApprovalRequest request => new ToolApprovalRequestStreamPart(
                request.ApprovalId,
                request.ToolCallId
            ),
            GeneratedSource source => new SourceStreamPart(
                source.Id,
                source.Url,
                source.Title,
                source.ProviderMetadata
            ),
            GeneratedDocumentSource document => new DocumentSourceStreamPart(
                document.Id,
                document.MediaType,
                document.Title,
                document.Filename,
                document.ProviderMetadata
            ),
            GeneratedFile file => new FileStreamPart(file.Data, file.MediaType),
            GeneratedReasoningFile file => new ReasoningFileStreamPart(
                file.Data,
                file.MediaType,
                file.ProviderMetadata
            ),
            GeneratedCustom custom => new CustomStreamPart(custom.Kind, custom.ProviderMetadata),
            _ => throw new NotSupportedException(
                "Unsupported content part: " + part.Type
            ),
        };

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

/// <summary>Turns a non-streaming model into a stream of one call. Maps to <c>simulateStreamingMiddleware</c>.</summary>
public sealed class SimulateStreamingMiddleware : LanguageModelMiddleware
{
    /// <inheritdoc />
    public override async Task<LanguageModelStreamResult> WrapStreamAsync(
        LanguageModelWrapContext context,
        CancellationToken cancellationToken
    )
    {
        var result = await context.DoGenerateAsync(cancellationToken).ConfigureAwait(false);
        return new LanguageModelStreamResult(
            Parts(result),
            result.Request,
            new LanguageModelStreamResponse(result.ResponseHeaders)
        );
    }

    private static async IAsyncEnumerable<LanguageModelStreamPart> Parts(
        LanguageModelGenerateResult result
    )
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield return new StreamStartStreamPart(result.Warnings);
        yield return new ResponseMetadataStreamPart(
            result.ResponseId,
            result.ResponseModelId,
            result.ResponseTimestamp
        );

        var id = 0;
        foreach (var part in result.Content)
        {
            switch (part)
            {
                case GeneratedText generatedText:
                    if (generatedText.Text.Length > 0 || (generatedText.Citations?.Count ?? 0) > 0)
                    {
                        var textId = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        yield return new TextStartStreamPart(textId, generatedText.ProviderMetadata);
                        yield return new TextDeltaStreamPart(textId, generatedText.Text);
                        yield return new TextEndStreamPart(textId, generatedText.Citations);
                        id++;
                    }

                    break;
                case GeneratedReasoning reasoning:
                    var reasoningId = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    yield return new ReasoningStartStreamPart(reasoningId, reasoning.ProviderMetadata);
                    yield return new ReasoningDeltaStreamPart(reasoningId, reasoning.Text);
                    yield return new ReasoningEndStreamPart(reasoningId);
                    id++;
                    break;
                default:
                    yield return MiddlewareResults.ToStreamPart(part);
                    break;
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

/// <summary>Fills call settings the caller left unset. Maps to <c>defaultSettingsMiddleware</c>.</summary>
public sealed class DefaultSettingsMiddleware : LanguageModelMiddleware
{
    /// <summary>Creates middleware. Every setting of <paramref name="settings"/> other than the prompt is a default.</summary>
    public DefaultSettingsMiddleware(LanguageModelCallOptions settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>The default settings.</summary>
    public LanguageModelCallOptions Settings { get; }

    /// <inheritdoc />
    public override Task<LanguageModelCallOptions> TransformParamsAsync(
        LanguageModelCallOptions options,
        LanguageModelCallType type,
        ILanguageModel model,
        CancellationToken cancellationToken
    )
    {
        var merged = options.Copy();
        merged.MaxOutputTokens ??= Settings.MaxOutputTokens;
        merged.Temperature ??= Settings.Temperature;
        merged.StopSequences ??= Settings.StopSequences;
        merged.TopP ??= Settings.TopP;
        merged.TopK ??= Settings.TopK;
        merged.PresencePenalty ??= Settings.PresencePenalty;
        merged.FrequencyPenalty ??= Settings.FrequencyPenalty;
        merged.Seed ??= Settings.Seed;
        merged.Reasoning ??= Settings.Reasoning;
        merged.Tools ??= Settings.Tools;
        merged.ToolChoice ??= Settings.ToolChoice;
        if (!merged.JsonResponse && merged.JsonSchema is null)
        {
            merged.JsonResponse = Settings.JsonResponse;
            merged.JsonSchema = Settings.JsonSchema;
            merged.JsonSchemaName = Settings.JsonSchemaName;
            merged.JsonSchemaDescription = Settings.JsonSchemaDescription;
        }

        merged.Headers = MergeMaps(Settings.Headers, options.Headers);
        merged.ProviderOptions = MergeProviderOptions(
            Settings.ProviderOptions,
            options.ProviderOptions
        );
        return Task.FromResult(merged);
    }

    internal static IReadOnlyDictionary<string, T>? MergeMaps<T>(
        IReadOnlyDictionary<string, T>? defaults,
        IReadOnlyDictionary<string, T>? overrides
    )
    {
        if (defaults is null)
        {
            return overrides;
        }

        if (overrides is null)
        {
            return defaults;
        }

        var merged = new Dictionary<string, T>();
        foreach (var pair in defaults)
        {
            merged[pair.Key] = pair.Value;
        }

        foreach (var pair in overrides)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }

    internal static IReadOnlyDictionary<string, JsonElement>? MergeProviderOptions(
        IReadOnlyDictionary<string, JsonElement>? defaults,
        IReadOnlyDictionary<string, JsonElement>? overrides
    )
    {
        if (defaults is null)
        {
            return overrides;
        }

        if (overrides is null)
        {
            return defaults;
        }

        var merged = new Dictionary<string, JsonElement>();
        foreach (var pair in defaults)
        {
            merged[pair.Key] = pair.Value;
        }

        foreach (var pair in overrides)
        {
            merged[pair.Key] = merged.TryGetValue(pair.Key, out var existing)
                ? MergeObjects(existing, pair.Value)
                : pair.Value;
        }

        return merged;
    }

    // Maps to mergeObjects: objects merge recursively, any other value from the override wins.
    internal static JsonElement MergeObjects(JsonElement defaults, JsonElement overrides)
    {
        if (
            defaults.ValueKind != JsonValueKind.Object
            || overrides.ValueKind != JsonValueKind.Object
        )
        {
            return overrides;
        }

        var node = new System.Text.Json.Nodes.JsonObject();
        foreach (var property in defaults.EnumerateObject())
        {
            node[property.Name] = System.Text.Json.Nodes.JsonNode.Parse(property.Value.GetRawText());
        }

        foreach (var property in overrides.EnumerateObject())
        {
            node[property.Name] = defaults.TryGetProperty(property.Name, out var existing)
                ? System.Text.Json.Nodes.JsonNode.Parse(
                    MergeObjects(existing, property.Value).GetRawText()
                )
                : System.Text.Json.Nodes.JsonNode.Parse(property.Value.GetRawText());
        }

        using var document = JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }
}

/// <summary>Prepends default instructions when the prompt has no system message. Maps to <c>defaultInstructionsMiddleware</c>.</summary>
public sealed class DefaultInstructionsMiddleware : LanguageModelMiddleware
{
    private readonly IReadOnlyList<SystemModelMessage> _defaults;

    /// <summary>Creates middleware with one system message.</summary>
    public DefaultInstructionsMiddleware(string instructions)
        : this(new[] { new SystemModelMessage(instructions ?? string.Empty) }) { }

    /// <summary>Creates middleware with several system messages, kept in order.</summary>
    public DefaultInstructionsMiddleware(IEnumerable<SystemModelMessage> instructions)
    {
        _defaults = (instructions ?? throw new ArgumentNullException(nameof(instructions)))
            .ToList();
    }

    /// <summary>Instructions to prepend.</summary>
    public IReadOnlyList<SystemModelMessage> Instructions => _defaults;

    /// <inheritdoc />
    public override Task<LanguageModelCallOptions> TransformParamsAsync(
        LanguageModelCallOptions options,
        LanguageModelCallType type,
        ILanguageModel model,
        CancellationToken cancellationToken
    )
    {
        if (_defaults.Count == 0 || options.Prompt.Any(message => message is SystemModelMessage))
        {
            return Task.FromResult(options);
        }

        var copy = options.Copy();
        var messages = new List<ModelMessage>(_defaults);
        messages.AddRange(options.Prompt);
        copy.Prompt = messages;
        return Task.FromResult(copy);
    }
}

internal sealed class MiddlewareLanguageModel : ILanguageModel
{
    private readonly ILanguageModel _inner;
    private readonly ILanguageModelMiddleware _middleware;
    private readonly string? _modelId;
    private readonly string? _providerId;

    public MiddlewareLanguageModel(
        ILanguageModel inner,
        ILanguageModelMiddleware middleware,
        string? modelId = null,
        string? providerId = null
    )
    {
        _inner = inner;
        _middleware = middleware;
        _modelId = modelId;
        _providerId = providerId;
    }

    public string SpecificationVersion => _inner.SpecificationVersion;

    public IReadOnlyDictionary<string, string> SupportedUrls =>
        _middleware.OverrideSupportedUrls(_inner) ?? _inner.SupportedUrls;

    public string Provider => _providerId ?? _middleware.OverrideProvider(_inner) ?? _inner.Provider;

    public string ModelId => _modelId ?? _middleware.OverrideModelId(_inner) ?? _inner.ModelId;

    public async Task<LanguageModelGenerateResult> DoGenerateAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var transformed = await _middleware
            .TransformParamsAsync(options, LanguageModelCallType.Generate, _inner, cancellationToken)
            .ConfigureAwait(false);
        return await _middleware
            .WrapGenerateAsync(Context(transformed), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<LanguageModelStreamResult> DoStreamAsync(
        LanguageModelCallOptions options,
        CancellationToken cancellationToken
    )
    {
        var transformed = await _middleware
            .TransformParamsAsync(options, LanguageModelCallType.Stream, _inner, cancellationToken)
            .ConfigureAwait(false);
        return await _middleware
            .WrapStreamAsync(Context(transformed), cancellationToken)
            .ConfigureAwait(false);
    }

    private LanguageModelWrapContext Context(LanguageModelCallOptions options) =>
        new(
            options,
            _inner,
            token => _inner.DoGenerateAsync(options, token),
            token => _inner.DoStreamAsync(options, token)
        );
}
