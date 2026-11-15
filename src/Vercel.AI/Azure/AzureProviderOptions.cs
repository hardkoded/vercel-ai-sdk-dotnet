// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAI;

namespace Vercel.AI.Azure;

/// <summary>
/// Helpers shared by the Azure speech, transcription and image routers. Map to <c>pickAzureOptions</c>,
/// <c>pickKeys</c>, <c>withAzureOptionsForOpenAI</c> and <c>unsupportedOptionWarnings</c> in <c>azure-openai-provider.ts</c>.
/// </summary>
internal static class AzureProviderOptions
{
    /// <summary>
    /// Picks the entries of the <c>azure</c> provider options whose key is in <paramref name="keys"/>.
    /// Raw values are used so defaults are not injected. Null values are dropped.
    /// </summary>
    public static JsonObject PickAzureOptions(JsonElement? providerOptions, IEnumerable<string> keys)
    {
        return PickKeys(OpenAIJson.Child(providerOptions, "azure"), keys);
    }

    /// <summary>Picks the entries of <paramref name="options"/> whose key is in <paramref name="keys"/>. Null values are dropped.</summary>
    public static JsonObject PickKeys(JsonElement? options, IEnumerable<string> keys)
    {
        var picked = new JsonObject();
        if (options is not { ValueKind: JsonValueKind.Object } source)
        {
            return picked;
        }

        var allowed = new HashSet<string>(keys, StringComparer.Ordinal);
        foreach (var property in source.EnumerateObject())
        {
            if (allowed.Contains(property.Name) && property.Value.ValueKind != JsonValueKind.Null)
            {
                picked[property.Name] = JsonNode.Parse(property.Value.GetRawText());
            }
        }

        return picked;
    }

    /// <summary>
    /// Merges the picked Azure options into <c>openai</c> for the OpenAI model; <c>azure</c> wins.
    /// Returns <paramref name="providerOptions"/> unchanged when there is nothing to merge.
    /// </summary>
    public static JsonElement? WithAzureOptionsForOpenAI(JsonElement? providerOptions, JsonObject azureOptions)
    {
        if (azureOptions.Count == 0)
        {
            return providerOptions;
        }

        var root = providerOptions is { ValueKind: JsonValueKind.Object } existing
            ? (JsonObject)JsonNode.Parse(existing.GetRawText())!
            : new JsonObject();
        var openai = root["openai"] as JsonObject ?? new JsonObject();
        foreach (var pair in azureOptions)
        {
            openai[pair.Key] = pair.Value?.DeepClone();
        }

        root["openai"] = openai;
        return OpenAIJson.Element(root);
    }

    /// <summary>One <c>unsupported</c> warning per option, <c>providerOptions.azure.{key}</c>.</summary>
    public static List<OpenAICallWarning> UnsupportedOptionWarnings(JsonObject options, string details)
    {
        var warnings = new List<OpenAICallWarning>();
        foreach (var pair in options)
        {
            if (pair.Value != null)
            {
                warnings.Add(new OpenAICallWarning("unsupported", "providerOptions.azure." + pair.Key, details));
            }
        }

        return warnings;
    }
}
