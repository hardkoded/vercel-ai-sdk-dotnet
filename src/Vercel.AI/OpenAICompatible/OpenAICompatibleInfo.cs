// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.OpenAICompatible;

/// <summary>Version token used in the OpenAI-compatible user-agent suffix.</summary>
public static class OpenAICompatibleInfo
{
    /// <summary>Package version appended to <c>ai-sdk/{provider}/</c>.</summary>
    public const string Version = "0.0.0";

    /// <summary>Builds the user-agent suffix for a provider package name.</summary>
    public static string UserAgent(string provider)
    {
        var name = string.IsNullOrEmpty(provider) ? "openai-compatible" : provider;
        return "ai-sdk/" + name + "/" + Version;
    }
}
