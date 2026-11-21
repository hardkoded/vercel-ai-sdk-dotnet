// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAI;

/// <summary>
/// The <c>openai</c> options of a system message in Responses history. Maps to
/// <c>OpenAIResponsesSystemMessageOptions</c>.
/// </summary>
public sealed class OpenAIResponsesSystemMessageOptions
{
    /// <summary>
    /// Emits a configuration update at this position in history: <c>none</c>, <c>low</c>, <c>medium</c>,
    /// <c>high</c>, <c>xhigh</c> or <c>max</c>. Requires empty system message content.
    /// </summary>
    public string? ReasoningEffortUpdate { get; set; }

    /// <summary>
    /// Replaces the response format at this position in history (a <c>text</c> or <c>json_schema</c> format).
    /// Requires empty system message content.
    /// </summary>
    public JsonElement? TextFormatUpdate { get; set; }

    /// <summary>Tool definitions to load at this position in history. Requires empty system message content.</summary>
    public IReadOnlyList<OpenAIResponsesAdditionalTool>? AdditionalTools { get; set; }

    /// <summary>
    /// Returns the options as provider options under <paramref name="providerOptionsName"/> (<c>openai</c> by
    /// default), ready for <c>SystemModelMessage.ProviderOptions</c>.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement> ToProviderOptions(
        string providerOptionsName = "openai"
    )
    {
        var json = new JsonObject();
        if (ReasoningEffortUpdate != null)
        {
            json["reasoningEffortUpdate"] = ReasoningEffortUpdate;
        }

        if (TextFormatUpdate is { } textFormat)
        {
            json["textFormatUpdate"] = JsonNode.Parse(textFormat.GetRawText());
        }

        if (AdditionalTools != null)
        {
            json["additionalTools"] = OpenAIResponsesAdditionalToolList.ToJson(AdditionalTools);
        }

        return new Dictionary<string, JsonElement>
        {
            [providerOptionsName] = JsonSerializer.SerializeToElement(json),
        };
    }
}
