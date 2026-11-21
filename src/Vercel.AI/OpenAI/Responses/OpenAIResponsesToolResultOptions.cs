// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.OpenAI;

/// <summary>
/// The <c>openai</c> options of an ordinary tool result in Responses history. Maps to
/// <c>OpenAIResponsesToolResultOptions</c>.
/// </summary>
public sealed class OpenAIResponsesToolResultOptions
{
    /// <summary>Tool definitions to load immediately after this ordinary tool result.</summary>
    public IReadOnlyList<OpenAIResponsesAdditionalTool>? AdditionalTools { get; set; }

    /// <summary>
    /// Returns the options as provider metadata under <paramref name="providerOptionsName"/> (<c>openai</c> by
    /// default), ready for <c>ToolResultPart.ProviderMetadata</c>.
    /// </summary>
    public JsonElement ToProviderMetadata(string providerOptionsName = "openai")
    {
        var json = new JsonObject();
        if (AdditionalTools != null)
        {
            json["additionalTools"] = OpenAIResponsesAdditionalToolList.ToJson(AdditionalTools);
        }

        return JsonSerializer.SerializeToElement(new JsonObject { [providerOptionsName] = json });
    }
}

internal static class OpenAIResponsesAdditionalToolList
{
    internal static JsonArray ToJson(IReadOnlyList<OpenAIResponsesAdditionalTool> tools)
    {
        var array = new JsonArray();
        foreach (var tool in tools)
        {
            array.Add(tool.ToJson());
        }

        return array;
    }
}
