// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway;
using Vercel.AI.Provider;

namespace Vercel.AI.Prompt;

/// <summary>Replaces a Gateway authentication error with a message that says how to authenticate. Maps to <c>prompt/wrap-gateway-error.ts</c>.</summary>
public static class GatewayErrorWrappers
{
    private const string MoreInfoUrl = "https://ai-sdk.dev/unauthenticated-ai-gateway";

    /// <summary>
    /// Returns <paramref name="error"/> unchanged unless it is a <see cref="GatewayAuthenticationError"/>.
    /// Maps to <c>wrapGatewayError</c>.
    /// </summary>
    public static Exception WrapGatewayError(Exception error)
    {
        if (!GatewayAuthenticationError.IsInstance(error))
        {
            return error;
        }

        if (IsProductionEnvironment())
        {
            return new AiSdkException(
                "Unauthenticated. Configure AI_GATEWAY_API_KEY or use a provider module. Learn more: "
                    + MoreInfoUrl
            );
        }

        return new Exception(
            "\u001b[1m\u001b[31mUnauthenticated request to AI Gateway.\u001b[0m\n\n"
                + "To authenticate, set the \u001b[33mAI_GATEWAY_API_KEY\u001b[0m environment variable with your API key.\n\n"
                + "Alternatively, you can use a provider module instead of the AI Gateway.\n\n"
                + "Learn more: \u001b[34m"
                + MoreInfoUrl
                + "\u001b[0m\n\n"
        );
    }

    // Upstream reads NODE_ENV. The .NET hosts name the same setting ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT.
    private static bool IsProductionEnvironment()
    {
        foreach (var name in (string[])["NODE_ENV", "ASPNETCORE_ENVIRONMENT", "DOTNET_ENVIRONMENT"])
        {
            if (
                string.Equals(
                    Environment.GetEnvironmentVariable(name),
                    "production",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }
        }

        return false;
    }
}
