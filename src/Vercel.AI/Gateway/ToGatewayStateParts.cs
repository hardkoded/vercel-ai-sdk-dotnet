// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>Converts decision and extraction state to Gateway state parts. Maps to <c>toGatewayStateParts</c>.</summary>
internal static class ToGatewayStateParts
{
    /// <summary>Converts inline file bytes to base64 and rejects file data that cannot be sent.</summary>
    /// <param name="state">The ordered state parts.</param>
    /// <param name="modelType"><c>decision</c> or <c>extraction</c>.</param>
    /// <param name="allowReferences">When true, <c>reference</c> file data passes through unchanged.</param>
    public static JsonArray Convert(
        IReadOnlyList<DecisionModelStatePart> state,
        string modelType,
        bool allowReferences = false
    )
    {
        var parts = new JsonArray();
        foreach (var part in state)
        {
            switch (part)
            {
                case DecisionModelTextPart text:
                    parts.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                    break;
                case DecisionModelJsonPart json:
                    parts.Add(
                        new JsonObject
                        {
                            ["type"] = "json",
                            ["value"] = JsonNode.Parse(json.Value.GetRawText()),
                        }
                    );
                    break;
                case DecisionModelFilePart file:
                    parts.Add(FilePart(file, modelType, allowReferences));
                    break;
            }
        }

        return parts;
    }

    private static JsonObject FilePart(
        DecisionModelFilePart file,
        string modelType,
        bool allowReferences
    )
    {
        JsonObject data;
        if (allowReferences && file.Data.DataType == "reference")
        {
            var reference = new JsonObject();
            foreach (var pair in file.Data.Reference ?? new Dictionary<string, string>())
            {
                reference[pair.Key] = pair.Value;
            }

            data = new JsonObject { ["type"] = "reference", ["reference"] = reference };
        }
        else if (file.Data.DataType != "data")
        {
            var functionality =
                "Gateway " + modelType + " file input: " + file.Data.DataType + " data";
            throw new UnsupportedFunctionalityException(
                functionality,
                "'" + functionality + "' functionality not supported."
            );
        }
        else
        {
            data = new JsonObject
            {
                ["type"] = "data",
                ["data"] = file.Data.Base64
                    ?? ByteEncoding.ToBase64(file.Data.Bytes ?? Array.Empty<byte>()),
            };
        }

        var node = new JsonObject { ["type"] = "file", ["mediaType"] = file.MediaType };
        if (file.FileName != null)
        {
            node["filename"] = file.FileName;
        }

        node["data"] = data;
        if (file.ProviderOptions != null)
        {
            var options = new JsonObject();
            foreach (var pair in file.ProviderOptions)
            {
                options[pair.Key] = JsonNode.Parse(pair.Value.GetRawText());
            }

            node["providerOptions"] = options;
        }

        return node;
    }
}
