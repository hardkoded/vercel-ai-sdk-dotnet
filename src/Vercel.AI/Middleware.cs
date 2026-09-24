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

/// <summary>Splits <c>&lt;think&gt;...&lt;/think&gt;</c> out of text. Maps to <c>extractReasoningMiddleware</c>.</summary>
public sealed class ExtractReasoningMiddleware : LanguageModelMiddleware
{
    private static readonly Regex Think = new("<think>([\\s\\S]*?)</think>", RegexOptions.Compiled);

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
                var match = Think.Match(text.Text);
                if (match.Success)
                {
                    content.Add(new GeneratedReasoning(match.Groups[1].Value.Trim()));
                    content.Add(new GeneratedText(Think.Replace(text.Text, string.Empty).Trim()));
                    continue;
                }
            }

            content.Add(part);
        }

        return new LanguageModelGenerateResult(content, result.FinishReason, result.Usage, result.RawFinishReason, result.Warnings, result.ResponseId);
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

        return new LanguageModelGenerateResult(content, result.FinishReason, result.Usage, result.RawFinishReason, result.Warnings, result.ResponseId);
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

        yield return new FinishStreamPart(result.FinishReason, result.Usage, result.RawFinishReason);
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

