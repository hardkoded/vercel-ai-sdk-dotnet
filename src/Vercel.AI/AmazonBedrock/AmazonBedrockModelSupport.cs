// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Which Bedrock model ids take the Anthropic request fields.</summary>
internal static class AmazonBedrockModelSupport
{
    // Bedrock validates against its own copy of the Messages schema, which rejects
    // `output_config.format` and tool `strict` for the newest Claude models.
    private static readonly string[] ModelsWithoutStrictToolSupport =
    {
        "claude-opus-4-7",
        "claude-opus-4-8",
        "claude-opus-5",
        "claude-fable-5",
        "claude-sonnet-5",
        "claude-haiku-5-5",
    };

    // Native structured output is unreliable for additional models even though their strict tool
    // support remains available.
    private static readonly string[] ModelsWithoutReliableNativeStructuredOutput =
    {
        "claude-opus-4-7",
        "claude-opus-4-8",
        "claude-opus-5",
        "claude-fable-5",
        "claude-sonnet-5",
        "claude-haiku-5-5",
        "claude-sonnet-4-6",
        "claude-haiku-4-5",
    };

    public static bool IsAnthropicModel(string modelId, int? reasoningBudgetTokens)
    {
        return modelId.IndexOf("anthropic", StringComparison.Ordinal) >= 0
            || (modelId.IndexOf(":application-inference-profile/", StringComparison.Ordinal) >= 0 && reasoningBudgetTokens != null);
    }

    public static bool SupportsStrictTools(string modelId) => !Matches(modelId, ModelsWithoutStrictToolSupport);

    public static bool SupportsNativeStructuredOutput(string modelId) => !Matches(modelId, ModelsWithoutReliableNativeStructuredOutput);

    private static bool Matches(string modelId, string[] models)
    {
        foreach (var model in models)
        {
            if (modelId.IndexOf(model, StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>Keeps the first complete JSON object in streamed text and drops everything around it.</summary>
internal sealed class JsonObjectTextExtractor
{
    private bool _started;
    private bool _completed;
    private int _depth;
    private bool _inString;
    private bool _escaped;

    public string Process(string text)
    {
        var result = new StringBuilder();
        foreach (var character in text)
        {
            if (_completed)
            {
                break;
            }

            if (!_started)
            {
                if (character != '{')
                {
                    continue;
                }

                _started = true;
                _depth = 1;
                result.Append(character);
                continue;
            }

            result.Append(character);
            if (_escaped)
            {
                _escaped = false;
                continue;
            }

            if (character == '\\' && _inString)
            {
                _escaped = true;
                continue;
            }

            if (character == '"')
            {
                _inString = !_inString;
                continue;
            }

            if (_inString)
            {
                continue;
            }

            if (character == '{')
            {
                _depth++;
            }
            else if (character == '}')
            {
                _depth--;
                if (_depth == 0)
                {
                    _completed = true;
                }
            }
        }

        return result.ToString();
    }
}
