// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Mistral embedding provider options. Maps to <c>MistralEmbeddingModelOptions</c>.</summary>
public sealed class MistralEmbeddingModelOptions
{
    /// <summary>Additional metadata to attach to the embedding request.</summary>
    public IReadOnlyDictionary<string, JsonNode?>? Metadata { get; init; }

    /// <summary>The dimension of the output embeddings when supported by the model.</summary>
    public int? OutputDimension { get; init; }

    /// <summary>The data type of the output embeddings when supported by the model: <c>float</c>, <c>int8</c>, <c>uint8</c>, <c>binary</c> or <c>ubinary</c>.</summary>
    public string? OutputDtype { get; init; }

    /// <summary>Parses <c>providerOptions.mistral</c>. Maps to <c>parseProviderOptions</c> with <c>mistralEmbeddingModelOptions</c>.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape or is out of range.</exception>
    public static MistralEmbeddingModelOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue(MistralProvider.ProviderId, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        var error = ProviderOptionShape.Validate(value, Shape(), false);
        if (error != null)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                value.GetRawText(),
                "invalid mistral provider options: " + error
            );
        }

        Dictionary<string, JsonNode?>? metadata = null;
        if (value.TryGetProperty("metadata", out var metadataValue))
        {
            metadata = new Dictionary<string, JsonNode?>(StringComparer.Ordinal);
            foreach (var entry in metadataValue.EnumerateObject())
            {
                metadata[entry.Name] = JsonNode.Parse(entry.Value.GetRawText());
            }
        }

        return new MistralEmbeddingModelOptions
        {
            Metadata = metadata,
            OutputDimension = value.TryGetProperty("outputDimension", out var dimension)
                ? dimension.GetInt32()
                : null,
            OutputDtype = value.TryGetProperty("outputDtype", out var dtype)
                ? dtype.GetString()
                : null,
        };
    }

    internal static Dictionary<string, OptionValidator> Shape()
    {
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["metadata"] = value =>
                value.ValueKind == JsonValueKind.Object ? null : "expected object",
            ["outputDimension"] = ProviderOptionShape.Number(min: 1, integer: true),
            ["outputDtype"] = ProviderOptionShape.Enum(
                "float",
                "int8",
                "uint8",
                "binary",
                "ubinary"
            ),
        };
    }
}
