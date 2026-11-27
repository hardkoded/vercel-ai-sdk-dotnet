// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Alibaba;

/// <summary>Alibaba embedding provider options, read from <c>providerOptions.alibaba</c>. Maps to <c>AlibabaEmbeddingModelOptions</c>.</summary>
public sealed class AlibabaEmbeddingModelOptions
{
    /// <summary>
    /// Differentiates query text from document text for asymmetric retrieval tasks: <c>query</c> or <c>document</c>.
    /// Defaults to <c>document</c>.
    /// </summary>
    public string? TextType { get; init; }

    /// <summary>
    /// The dimension of the output embedding vectors. Defaults to 1024.
    /// <c>text-embedding-v4</c> also supports 1536 and 2048 dimensions.
    /// </summary>
    public int? Dimension { get; init; }

    /// <summary>
    /// The output vector type: <c>dense</c>, <c>sparse</c> or <c>dense&amp;sparse</c>. Defaults to <c>dense</c>.
    /// Sparse-only output is not supported by the embedding interface, which requires dense number arrays.
    /// </summary>
    public string? OutputType { get; init; }

    /// <summary>Parses <c>providerOptions.alibaba</c>. Maps to <c>parseProviderOptions</c> with <c>alibabaEmbeddingModelOptions</c>.</summary>
    /// <exception cref="InvalidArgumentException">An option has the wrong shape.</exception>
    public static AlibabaEmbeddingModelOptions? Parse(JsonElement? providerOptions)
    {
        if (
            providerOptions is not { ValueKind: JsonValueKind.Object } options
            || !options.TryGetProperty(AlibabaProvider.ProviderId, out var value)
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
                "invalid alibaba provider options: " + error
            );
        }

        return new AlibabaEmbeddingModelOptions
        {
            TextType = Read(value, "textType")?.GetString(),
            Dimension = Read(value, "dimension")?.GetInt32(),
            OutputType = Read(value, "outputType")?.GetString(),
        };
    }

    private static Dictionary<string, OptionValidator> Shape()
    {
        return new Dictionary<string, OptionValidator>(StringComparer.Ordinal)
        {
            ["textType"] = ProviderOptionShape.Enum("query", "document"),
            ["dimension"] = ProviderOptionShape.Number(integer: true),
            ["outputType"] = ProviderOptionShape.Enum("dense", "sparse", "dense&sparse"),
        };
    }

    private static JsonElement? Read(JsonElement value, string name)
    {
        return
            value.TryGetProperty(name, out var property)
            && property.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
            ? property
            : null;
    }
}
