// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Vercel.AI.Operations;

namespace Vercel.AI.ScaleDown;

/// <summary>Maps to <c>convertScaleDownExtractionSource</c>.</summary>
internal static class ConvertScaleDownExtractionSource
{
    private static readonly JsonSerializerOptions Compact = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    internal static ExtractionSource? Convert(
        ScaleDownEntity entity,
        IReadOnlyList<DecisionModelStatePart> state,
        string? ocrText
    )
    {
        if (entity.Start == null || entity.End == null || entity.End < entity.Start)
        {
            return null;
        }

        var entityStart = entity.Start.Value;
        var entityEnd = entity.End.Value;
        var encoding = new UTF8Encoding(false);
        if (encoding.GetByteCount(entity.Text) != entityEnd - entityStart)
        {
            return null;
        }

        var offset = 0;
        int? filePart = null;
        for (var partIndex = 0; partIndex < state.Count; partIndex++)
        {
            var part = state[partIndex];
            if (part is DecisionModelFilePart)
            {
                filePart = partIndex;
                continue;
            }

            var text = part is DecisionModelTextPart textPart
                ? textPart.Text
                : JsonSerializer.Serialize(((DecisionModelJsonPart)part).Value, Compact);
            var bytes = encoding.GetBytes(text);
            if (entityStart >= offset && entityEnd <= offset + bytes.Length)
            {
                if (part is not DecisionModelTextPart)
                {
                    return null;
                }

                // ScaleDown reports UTF-8 byte offsets; the SDK uses UTF-16 code units.
                var start = encoding.GetString(bytes, 0, entityStart - offset).Length;
                var end = encoding.GetString(bytes, 0, entityEnd - offset).Length;
                if (
                    start <= end
                    && end <= text.Length
                    && string.Equals(
                        text.Substring(start, end - start),
                        entity.Text,
                        StringComparison.Ordinal
                    )
                    && encoding.GetByteCount(text.Substring(0, start)) == entityStart - offset
                )
                {
                    return new ExtractionSource("text", partIndex, start, end);
                }

                return null;
            }

            offset += bytes.Length + 1; // Parts are joined with a newline.
        }

        // OCR text is appended after the supplied text. Only cite the file when the
        // span lies beyond that text and the OCR result contains the extracted value.
        if (
            filePart != null
            && entityStart >= Math.Max(0, offset - 1)
            && ocrText != null
            && ocrText.Contains(entity.Text)
        )
        {
            return new ExtractionSource("file", filePart.Value);
        }

        return null;
    }
}
