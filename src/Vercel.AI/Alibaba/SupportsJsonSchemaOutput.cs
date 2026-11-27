// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Alibaba;

/// <summary>Maps to <c>supports-json-schema-output.ts</c>.</summary>
internal static class SupportsJsonSchemaOutputChecker
{
    // Model families with JSON schema structured output. Every row of upstream's alibaba-model-catalog.ts that sets
    // jsonSchema belongs to one of them, and upstream uses this list for model IDs the catalog does not list.
    private static readonly string[] JsonSchemaModelPrefixes =
    [
        "qwen3.7-plus",
        "qwen3.7-flash",
        "qwen3.7-max",
        "qwen3.8-max",
        "qwen3.8-flash",
    ];

    /// <summary>True when the model supports JSON schema structured output. Maps to <c>supportsJsonSchemaOutput</c>.</summary>
    public static bool SupportsJsonSchemaOutput(string modelId)
    {
        foreach (var prefix in JsonSchemaModelPrefixes)
        {
            if (modelId == prefix || modelId.StartsWith(prefix + "-", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
