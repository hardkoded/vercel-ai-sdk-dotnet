// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.ProviderUtils;

/// <summary>
/// Helpers for providers that cannot combine tools with native structured output. They add an internal function tool whose
/// arguments are the structured output, then turn that tool call back into text.
/// </summary>
public static class JsonResponseTools
{
    private const string DefaultDescription = "Respond with a JSON object.";

    /// <summary>
    /// Rewrites a language-model stream so the JSON response tool <paramref name="toolName"/> produces text.
    /// Maps to <c>convertJsonResponseToolStream</c>.
    /// </summary>
    public static async IAsyncEnumerable<LanguageModelStreamPart> ConvertJsonResponseToolStream(
        IAsyncEnumerable<LanguageModelStreamPart> source,
        string toolName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var responseToolIds = new HashSet<string>(StringComparer.Ordinal);
        var responseToolsWithDeltas = new HashSet<string>(StringComparer.Ordinal);
        var hasResponseTool = false;
        var hasApplicationTool = false;

        await foreach (var part in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            switch (part)
            {
                case TextStartStreamPart:
                case TextDeltaStreamPart:
                case TextEndStreamPart:
                    // The response tool supplies the output. Omit conversational text.
                    continue;
                case ToolInputStartStreamPart start when start.ToolName == toolName:
                    responseToolIds.Add(start.Id);
                    yield return new TextStartStreamPart(start.Id, start.ProviderMetadata);
                    continue;
                case ToolInputDeltaStreamPart delta when responseToolIds.Contains(delta.Id):
                    if (delta.Delta.Length > 0)
                    {
                        responseToolsWithDeltas.Add(delta.Id);
                    }

                    yield return new TextDeltaStreamPart(delta.Id, delta.Delta, delta.ProviderMetadata);
                    continue;
                case ToolInputEndStreamPart end when responseToolIds.Contains(end.Id):
                    continue;
                case ToolCallStreamPart call when call.ToolName == toolName:
                    if (!responseToolsWithDeltas.Contains(call.ToolCallId))
                    {
                        yield return new TextDeltaStreamPart(call.ToolCallId, call.ArgumentsJson, call.ProviderMetadata);
                    }

                    yield return new TextEndStreamPart(call.ToolCallId, null, call.ProviderMetadata);
                    hasResponseTool = true;
                    continue;
                case ToolCallStreamPart call:
                    if (!call.ProviderExecuted)
                    {
                        hasApplicationTool = true;
                    }

                    break;
                case FinishStreamPart finish when hasResponseTool && !hasApplicationTool && finish.FinishReason == FinishReason.ToolCalls:
                    yield return new FinishStreamPart(FinishReason.Stop, finish.Usage, finish.RawFinishReason, finish.ProviderMetadata);
                    continue;
            }

            yield return part;
        }
    }

    /// <summary>
    /// Builds the response tool. The name is <c>json</c>, or <c>json_1</c>, <c>json_2</c>, and so on when a caller tool already uses it.
    /// The description is the schema description, or a default.
    /// </summary>
    internal static LanguageModelTool CreateTool(IReadOnlyList<LanguageModelTool> tools, JsonElement schema)
    {
        var name = "json";
        for (var suffix = 1; tools.Any(tool => tool.Name == name); suffix++)
        {
            name = "json_" + suffix.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var description = schema.ValueKind == JsonValueKind.Object
            && schema.TryGetProperty("description", out var text)
            && text.ValueKind == JsonValueKind.String
            && !string.IsNullOrEmpty(text.GetString())
                ? text.GetString()
                : DefaultDescription;
        return new LanguageModelTool(name, description, schema);
    }

    /// <summary>A named tool stays that tool. <c>none</c> selects the response tool. Anything else becomes <c>required</c>.</summary>
    internal static ToolChoice ChoiceFor(ToolChoice? toolChoice, string responseToolName)
    {
        if (toolChoice is ToolChoice.NamedChoice)
        {
            return toolChoice;
        }

        return toolChoice?.Type == "none" ? ToolChoice.Tool(responseToolName) : ToolChoice.Required;
    }
}
