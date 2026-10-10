// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;

namespace Vercel.AI.Util;

/// <summary>Stable codes for deprecation warnings.</summary>
public static class Deprecations
{
    // These codes are stable identifiers. Do not rename or reuse an existing code.
    private static readonly Dictionary<string, string> DeprecationCodes = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["generateObject"] = "AISDK_DEP_GENERATE_OBJECT",
        ["streamObject"] = "AISDK_DEP_STREAM_OBJECT",
        ["experimental_generateSpeech"] = "AISDK_DEP_EXPERIMENTAL_GENERATE_SPEECH",
        ["experimental_transcribe"] = "AISDK_DEP_EXPERIMENTAL_TRANSCRIBE",
        ["\"image\" content part"] = "AISDK_DEP_IMAGE_CONTENT_PART",
        ["\"tool-result\" content of type \"file-data\""] = "AISDK_DEP_TOOL_RESULT_FILE_DATA",
        ["\"tool-result\" content of type \"file-url\""] = "AISDK_DEP_TOOL_RESULT_FILE_URL",
        ["\"tool-result\" content of type \"file-id\""] = "AISDK_DEP_TOOL_RESULT_FILE_ID",
        ["\"tool-result\" content of type \"file-reference\""] = "AISDK_DEP_TOOL_RESULT_FILE_REFERENCE",
        ["\"tool-result\" content of type \"image-data\""] = "AISDK_DEP_TOOL_RESULT_IMAGE_DATA",
        ["\"tool-result\" content of type \"image-url\""] = "AISDK_DEP_TOOL_RESULT_IMAGE_URL",
        ["\"tool-result\" content of type \"image-file-id\""] = "AISDK_DEP_TOOL_RESULT_IMAGE_FILE_ID",
        ["\"tool-result\" content of type \"image-file-reference\""] = "AISDK_DEP_TOOL_RESULT_IMAGE_FILE_REFERENCE",
        ["rawInput in output-error UI message parts"] = "AISDK_DEP_UI_MESSAGE_RAW_INPUT",
    };

    /// <summary>
    /// Returns the stable code for a deprecated <paramref name="setting"/>. A <paramref name="provider"/>
    /// scopes the code to that provider, but not to a model or message.
    /// </summary>
    public static string GetDeprecationCode(string setting, string? provider)
    {
        if (provider == null)
        {
            return DeprecationCodes.TryGetValue(setting, out var code) ? code : "AISDK_DEP_SETTING_" + EncodeCodePart(setting);
        }

        return "AISDK_DEP_PROVIDER_" + EncodeCodePart(provider) + "__" + EncodeCodePart(setting);
    }

    // Escapes every UTF-16 code unit outside A-Z, a-z, and 0-9 (including "_"), so "__" only separates parts.
    private static string EncodeCodePart(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if ((character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'))
            {
                builder.Append(character);
            }
            else
            {
                builder.Append('_').Append(((int)character).ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }
}
