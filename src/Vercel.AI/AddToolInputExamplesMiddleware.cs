// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI;

/// <summary>
/// Appends the input examples of function tools to their descriptions. It is for providers that do not support <c>inputExamples</c> natively.
/// Maps to <c>addToolInputExamplesMiddleware</c>.
/// </summary>
public sealed class AddToolInputExamplesMiddleware : LanguageModelMiddleware
{
    private readonly string _prefix;
    private readonly Func<ToolInputExample, int, string> _format;
    private readonly bool _remove;

    /// <summary>Creates the middleware.</summary>
    /// <param name="prefix">Prepended before the examples. Default <c>Input Examples:</c>.</param>
    /// <param name="format">Formats one example from the example and its index. Default: the compact JSON of the input.</param>
    /// <param name="remove">Whether to remove <c>InputExamples</c> from the tool after adding them to the description. Default true.</param>
    public AddToolInputExamplesMiddleware(
        string prefix = "Input Examples:",
        Func<ToolInputExample, int, string>? format = null,
        bool remove = true
    )
    {
        _prefix = prefix;
        _format = format ?? DefaultFormatExample;
        _remove = remove;
    }

    /// <inheritdoc />
    public override Task<LanguageModelCallOptions> TransformParamsAsync(
        LanguageModelCallOptions options,
        LanguageModelCallType type,
        ILanguageModel model,
        CancellationToken cancellationToken
    )
    {
        if (options.Tools is not { Count: > 0 } tools)
        {
            return Task.FromResult(options);
        }

        var transformed = new List<LanguageModelTool>(tools.Count);
        foreach (var tool in tools)
        {
            if (tool.IsProviderDefined || tool.InputExamples is not { Count: > 0 } examples)
            {
                transformed.Add(tool);
                continue;
            }

            var formatted = string.Join("\n", examples.Select((example, i) => _format(example, i)));
            var section = _prefix + "\n" + formatted;
            var description = string.IsNullOrEmpty(tool.Description)
                ? section
                : tool.Description + "\n\n" + section;
            transformed.Add(
                new LanguageModelTool(tool.Name, description, tool.InputSchema, tool.Strict)
                {
                    OutputSchema = tool.OutputSchema,
                    ProviderOptions = tool.ProviderOptions,
                    InputExamples = _remove ? null : tool.InputExamples,
                }
            );
        }

        var copy = options.Copy();
        copy.Tools = transformed;
        return Task.FromResult(copy);
    }

    private static string DefaultFormatExample(ToolInputExample example, int index) =>
        JsonSerializer.Serialize(example.Input);
}
