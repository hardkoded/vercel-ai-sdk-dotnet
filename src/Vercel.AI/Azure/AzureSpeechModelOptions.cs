// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAI;
using Vercel.AI.Util;

namespace Vercel.AI.Azure;

/// <summary>
/// Azure speech model options, read from the <c>azure</c> provider options. They combine the OpenAI speech options and
/// the Azure Speech options. Maps to <c>AzureSpeechModelOptions</c> in <c>azure-speech-model-options.ts</c>.
/// </summary>
public sealed class AzureSpeechModelOptions
{
    /// <summary>
    /// API to use, <c>openai</c> or <c>speech</c>. Defaults to Speech for MAI-Voice models, OpenAI otherwise.
    /// </summary>
    public string? Api { get; set; }

    /// <summary>OpenAI API option for Azure OpenAI deployments. Maps to <see cref="OpenAISpeechModelOptions.Instructions"/>.</summary>
    public string? Instructions { get; set; }

    /// <summary>OpenAI API option for Azure OpenAI deployments, from 0.25 to 4.0. Maps to <see cref="OpenAISpeechModelOptions.Speed"/>.</summary>
    public double? Speed { get; set; }

    /// <summary>Speaking style for the Azure Speech API. Maps to <see cref="AzureSpeechSpeechModelOptions.Style"/>.</summary>
    public string? Style { get; set; }

    /// <summary>Intensity of <see cref="Style"/> for the Azure Speech API, from 0.01 to 2. Maps to <see cref="AzureSpeechSpeechModelOptions.StyleDegree"/>.</summary>
    public double? StyleDegree { get; set; }

    /// <summary>
    /// Returns the voice model name of an Azure Speech model, for example <c>MAI-Voice-2</c> for <c>mai-voice-2</c>,
    /// or null when the model is not a MAI-Voice model. Maps to <c>getMAIVoiceModel</c>.
    /// </summary>
    public static string? GetMAIVoiceModel(string modelId)
    {
        return AzureModelCatalog.GetAzureModelEntry(modelId)?.SpeechVoiceModel;
    }

    /// <summary>
    /// Validates the <c>azure</c> provider options against this schema and returns them, or null when there are none.
    /// Stands in for <c>parseProviderOptions</c> with <c>azureSpeechModelOptions</c>.
    /// </summary>
    /// <exception cref="InvalidArgumentError">An option is unknown or out of range.</exception>
    internal static AzureSpeechModelOptions? Parse(JsonElement? providerOptions)
    {
        if (providerOptions is not { ValueKind: JsonValueKind.Object } root
            || !root.TryGetProperty("azure", out var azure)
            || azure.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (azure.ValueKind != JsonValueKind.Object)
        {
            throw Invalid(azure, "Expected an object.");
        }

        var options = new AzureSpeechModelOptions();
        foreach (var property in azure.EnumerateObject())
        {
            var value = property.Value;
            switch (property.Name)
            {
                case "api":
                    options.Api = value.ValueKind == JsonValueKind.String && value.GetString() is "openai" or "speech"
                        ? value.GetString()
                        : throw Invalid(azure, "api: Invalid option: expected one of \"openai\"|\"speech\".");
                    break;
                case "instructions":
                    options.Instructions = value.ValueKind switch
                    {
                        JsonValueKind.Null => null,
                        JsonValueKind.String => value.GetString(),
                        _ => throw Invalid(azure, "instructions: Invalid input: expected string."),
                    };
                    break;
                case "speed":
                    options.Speed = value.ValueKind == JsonValueKind.Null ? null : Number(azure, value, "speed", 0.25, 4.0);
                    break;
                case "style":
                    options.Style = value.ValueKind == JsonValueKind.String && value.GetString()!.Length > 0
                        ? value.GetString()
                        : throw Invalid(azure, "style: Too small: expected string to have >=1 characters.");
                    break;
                case "styleDegree":
                    options.StyleDegree = Number(azure, value, "styleDegree", 0.01, 2.0);
                    break;
                default:
                    throw Invalid(azure, "Unrecognized key: \"" + property.Name + "\".");
            }
        }

        return options;
    }

    private static double Number(JsonElement azure, JsonElement value, string name, double min, double max)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out var number))
        {
            throw Invalid(azure, name + ": Invalid input: expected number.");
        }

        if (number < min || number > max)
        {
            throw Invalid(azure, name + ": Too " + (number < min ? "small" : "big") + ": expected number to be in the range " + min.ToString(System.Globalization.CultureInfo.InvariantCulture) + " to " + max.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
        }

        return number;
    }

    private static InvalidArgumentError Invalid(JsonElement value, string message)
    {
        return new InvalidArgumentError("providerOptions", value.GetRawText(), message);
    }
}
