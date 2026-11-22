// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Moonshot;

/// <summary>Moonshot AI model families. Maps to <c>MoonshotAIModelFamily</c>.</summary>
internal enum MoonshotModelFamily
{
    Unknown,
    KimiK25,
    KimiK26,
    KimiK27,
    KimiK3,
    MoonshotV1,
}

/// <summary>Moonshot AI model catalog. Maps to <c>moonshotai-model-catalog.ts</c>.</summary>
internal static class MoonshotModelCatalog
{
    /// <summary>Resolves the model family of a model id. Maps to <c>resolveModelFamily</c>.</summary>
    public static MoonshotModelFamily ResolveModelFamily(string modelId)
    {
        switch (modelId)
        {
            case "kimi-k2.5":
                return MoonshotModelFamily.KimiK25;
            case "kimi-k2.6":
                return MoonshotModelFamily.KimiK26;
            case "kimi-k2.7-code":
            case "kimi-k2.7-code-highspeed":
                return MoonshotModelFamily.KimiK27;
            case "kimi-k3":
            case "kimi-k3.1-pro":
            case "kimi-k3.1-pro-highspeed":
                return MoonshotModelFamily.KimiK3;
        }

        return modelId.StartsWith("moonshot-v1-", StringComparison.Ordinal)
            ? MoonshotModelFamily.MoonshotV1
            : MoonshotModelFamily.Unknown;
    }
}
