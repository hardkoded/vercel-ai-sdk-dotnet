// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.ScaleDown;

/// <summary>Maps to <c>convertToScaleDownState</c>.</summary>
internal static class ConvertToScaleDownState
{
    private static readonly JsonSerializerOptions Compact = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static readonly string[] DocumentMediaTypes = ["image/jpeg", "image/png", "image/tiff", "application/pdf"];

    internal static JsonObject Convert(IReadOnlyList<DecisionModelStatePart> state)
    {
        var text = new List<string>();
        string? document = null;
        string? documentMimeType = null;

        foreach (var part in state)
        {
            if (part is not DecisionModelFilePart file)
            {
                text.Add(part is DecisionModelTextPart textPart ? textPart.Text : JsonSerializer.Serialize(((DecisionModelJsonPart)part).Value, Compact));
                continue;
            }

            if (document != null || file.Data.DataType != "data" || !DocumentMediaTypes.Contains(file.MediaType))
            {
                throw new UnsupportedFunctionalityException(
                    "ScaleDown supports one JPEG, PNG, TIFF, or PDF document.",
                    "'ScaleDown supports one JPEG, PNG, TIFF, or PDF document.' functionality not supported.");
            }

            document = file.Data.Bytes != null ? System.Convert.ToBase64String(file.Data.Bytes) : file.Data.Base64 ?? string.Empty;
            documentMimeType = file.MediaType;
        }

        var result = new JsonObject();
        if (text.Count > 0)
        {
            result["text"] = string.Join("\n", text);
        }

        if (document != null)
        {
            result["document"] = document;
            result["document_mime_type"] = documentMimeType;
        }

        return result;
    }
}
