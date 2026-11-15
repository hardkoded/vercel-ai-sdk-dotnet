// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.RegularExpressions;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Which Bedrock model ids take the Anthropic request fields.</summary>
internal static class AmazonBedrockModelSupport
{
    // Bedrock's Messages schema differs from Anthropic's. Each Claude model declares its support
    // here instead of a separate allowlist.
    private static readonly Dictionary<string, (bool Strict, bool Native)> ClaudeStructuredOutputCapabilities = new(StringComparer.Ordinal)
    {
        ["claude-opus-4"] = StrictToolsAndNativeOutput,
        ["claude-sonnet-4"] = StrictToolsAndNativeOutput,
        ["claude-opus-4-1"] = StrictToolsAndNativeOutput,
        ["claude-opus-4-5"] = StrictToolsAndNativeOutput,
        ["claude-sonnet-4-5"] = StrictToolsAndNativeOutput,
        ["claude-haiku-4-5"] = StrictToolsOnly,
        ["claude-opus-4-6"] = StrictToolsAndNativeOutput,
        ["claude-sonnet-4-6"] = StrictToolsOnly,
        ["claude-opus-4-7"] = NoStrictToolsOrNativeOutput,
        ["claude-opus-4-8"] = NoStrictToolsOrNativeOutput,
        ["claude-opus-5"] = StrictToolsAndNativeOutput,
        ["claude-sonnet-5"] = StrictToolsAndNativeOutput,
        ["claude-opus-5-5"] = StrictToolsAndNativeOutput,
        ["claude-sonnet-5-5"] = StrictToolsAndNativeOutput,
        ["claude-haiku-5-5"] = StrictToolsAndNativeOutput,
        ["claude-fable-5"] = NoStrictToolsOrNativeOutput,
        ["claude-fable-5-1"] = NoStrictToolsOrNativeOutput,
        ["claude-fable-5-2"] = NoStrictToolsOrNativeOutput,
    };

    private static readonly Regex LegacyClaude = new(
        @"claude-(?:instant|v?2|3)(?=[-.:]|\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    // Claude 4 minor-version ids historically accept additional suffixes.
    private static readonly Regex Claude4MinorVersion = new(
        @"claude-[a-z]+-4-\d{1,2}(?=[-.:]|\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    // Other aliases must end at a version, release date, or Bedrock revision.
    private static readonly Regex ClaudeAlias = new(
        @"(claude-[a-z]+-\d+(?:-\d{1,2})?)(?:-\d{8})?(?:-v\d+)?(?=[:.]|\z)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static (bool Strict, bool Native) StrictToolsAndNativeOutput => (true, true);

    private static (bool Strict, bool Native) StrictToolsOnly => (true, false);

    private static (bool Strict, bool Native) NoStrictToolsOrNativeOutput => (false, false);

    public static bool IsAnthropicModel(string modelId, int? reasoningBudgetTokens)
    {
        return modelId.IndexOf("anthropic", StringComparison.Ordinal) >= 0
            || (modelId.IndexOf(":application-inference-profile/", StringComparison.Ordinal) >= 0 && reasoningBudgetTokens != null);
    }

    public static bool SupportsStrictTools(string modelId) => Resolve(modelId).Strict;

    public static bool SupportsNativeStructuredOutput(string modelId) => Resolve(modelId).Native;

    private static (bool Strict, bool Native) Resolve(string modelId)
    {
        // Non-Claude models, opaque inference profiles and legacy Claude ids keep the request fallback.
        if (string.IsNullOrEmpty(modelId)
            || modelId.IndexOf("claude-", StringComparison.Ordinal) < 0
            || LegacyClaude.IsMatch(modelId))
        {
            return StrictToolsAndNativeOutput;
        }

        var canonical = Claude4MinorVersion.Match(modelId) is { Success: true } minor
            ? minor.Value
            : ClaudeAlias.Match(modelId) is { Success: true } alias ? alias.Groups[1].Value : null;

        // Unknown Claude models get neither strict tools nor native output.
        return canonical != null && ClaudeStructuredOutputCapabilities.TryGetValue(canonical, out var capabilities)
            ? capabilities
            : NoStrictToolsOrNativeOutput;
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
