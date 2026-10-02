// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text;

namespace Vercel.AI.AmazonBedrock;

/// <summary>Mistral tool-call ids on Bedrock must be at most nine alphanumeric characters.</summary>
public static class AmazonBedrockToolIds
{
    /// <summary>True when <paramref name="modelId"/> is a Mistral model, including region-prefixed ids.</summary>
    public static bool IsMistralModel(string modelId)
    {
        return modelId != null && modelId.IndexOf("mistral.", StringComparison.Ordinal) >= 0;
    }

    /// <summary>
    /// Returns <paramref name="toolCallId"/> unchanged for other models.
    /// For Mistral, keeps the first nine letters and digits.
    /// </summary>
    public static string Normalize(string toolCallId, bool isMistral)
    {
        if (!isMistral || toolCallId == null)
        {
            return toolCallId ?? string.Empty;
        }

        var alphanumeric = new StringBuilder(toolCallId.Length);
        foreach (var character in toolCallId)
        {
            if ((character >= 'a' && character <= 'z')
                || (character >= 'A' && character <= 'Z')
                || (character >= '0' && character <= '9'))
            {
                alphanumeric.Append(character);
                if (alphanumeric.Length == 9)
                {
                    break;
                }
            }
        }

        return alphanumeric.ToString();
    }
}
