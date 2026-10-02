// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Normalizes tool-call ids for models that reject Bedrock's generated ids.</summary>
public static class AmazonBedrockToolCallId
{
    /// <summary>Returns whether <paramref name="modelId"/> is a Mistral model on Bedrock.</summary>
    public static bool IsMistralModel(string modelId)
    {
        return modelId != null && modelId.IndexOf("mistral.", StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Mistral requires <c>^[a-zA-Z0-9]{9}$</c>. This keeps the first nine alphanumeric characters.
    /// Other models receive <paramref name="toolCallId"/> unchanged.
    /// </summary>
    public static string Normalize(string toolCallId, bool isMistral)
    {
        if (!isMistral || toolCallId == null)
        {
            return toolCallId ?? string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var character in toolCallId)
        {
            if ((character >= 'a' && character <= 'z')
                || (character >= 'A' && character <= 'Z')
                || (character >= '0' && character <= '9'))
            {
                builder.Append(character);
                if (builder.Length == 9)
                {
                    break;
                }
            }
        }

        return builder.ToString();
    }
}
