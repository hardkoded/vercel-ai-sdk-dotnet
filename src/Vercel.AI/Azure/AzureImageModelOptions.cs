// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure image options, read from <c>providerOptions.azure</c>. Maps to <c>AzureImageModelOptions</c> and
/// <c>azureImageModelOptions</c> in <c>azure-image-model-options.ts</c>.
/// </summary>
public sealed class AzureImageModelOptions : AzureImageModelMaiOptions
{
    /// <summary>API to use: <c>openai</c> or <c>mai</c>. Defaults to MAI for MAI-Image models, OpenAI otherwise.</summary>
    public string? Api { get; set; }

    /// <summary>True when <paramref name="modelId"/> is a MAI-Image model. Maps to <c>isMAIImageModel</c>.</summary>
    public static bool IsMAIImageModel(string modelId)
    {
        return AzureModelCatalog.GetAzureModelEntry(modelId)?.MaiImage == true;
    }

    /// <summary>
    /// Reads and validates the <c>azure</c> entry of <paramref name="providerOptions"/>. Unknown keys are ignored.
    /// Maps to <c>parseProviderOptions</c> with <c>azureImageModelOptions</c>.
    /// </summary>
    internal static AzureImageModelOptions Parse(JsonElement? providerOptions)
    {
        var options = new AzureImageModelOptions();
        var azure = OpenAI.OpenAIJson.Child(providerOptions, "azure");
        if (azure is not { ValueKind: JsonValueKind.Object } source)
        {
            return options;
        }

        foreach (var property in source.EnumerateObject())
        {
            switch (property.Name)
            {
                case "api" when property.Value.ValueKind != JsonValueKind.Null:
                    var api = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : null;
                    if (api is not ("openai" or "mai"))
                    {
                        throw new InvalidArgumentException("providerOptions.azure.api", property.Value.ToString(), "api must be 'openai' or 'mai'.");
                    }

                    options.Api = api;
                    break;
                case "autoAspectRatio":
                    options.AutoAspectRatio = Flag(property);
                    break;
                case "webGrounding":
                    options.WebGrounding = Flag(property);
                    break;
            }
        }

        return options;
    }

    private static bool? Flag(JsonProperty property)
    {
        return property.Value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new InvalidArgumentException("providerOptions.azure." + property.Name, property.Value.ToString(), property.Name + " must be a boolean."),
        };
    }
}
