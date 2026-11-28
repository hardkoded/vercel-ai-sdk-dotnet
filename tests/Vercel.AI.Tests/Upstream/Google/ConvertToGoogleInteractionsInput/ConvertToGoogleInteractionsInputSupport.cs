// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Helpers shared by the <c>convert-to-google-interactions-input.test.ts</c> ports.</summary>
internal static class ConvertToGoogleInteractionsInputSupport
{
    public const string Id =
        "packages/google/src/interactions/convert-to-google-interactions-input.test.ts::convertToGoogleInteractionsInput > ";

    public static ConvertToGoogleInteractionsInputResult Convert(params ModelMessage[] prompt)
    {
        return GoogleInteractionsInput.Convert(prompt);
    }

    /// <summary>The whole result as the upstream snapshot shows it: input, systemInstruction, warnings.</summary>
    public static JsonObject Snapshot(ConvertToGoogleInteractionsInputResult result)
    {
        var warnings = new JsonArray();
        foreach (var warning in result.Warnings)
        {
            warnings.Add(new JsonObject { ["type"] = warning.Type, ["message"] = warning.Message });
        }

        var snapshot = new JsonObject { ["input"] = result.Input.DeepClone() };
        if (result.SystemInstruction != null)
        {
            snapshot["systemInstruction"] = result.SystemInstruction;
        }

        snapshot["warnings"] = warnings;
        return snapshot;
    }
}
