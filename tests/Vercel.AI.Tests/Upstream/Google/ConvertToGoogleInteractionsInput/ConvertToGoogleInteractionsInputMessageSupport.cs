// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.Google;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests.Upstream.Google.ConvertToGoogleInteractionsInput;

/// <summary>Message builders shared by the assistant, tool-result, and compaction ports of <c>convert-to-google-interactions-input.test.ts</c>.</summary>
internal static class ConvertToGoogleInteractionsInputMessageSupport
{
    public static AssistantModelMessage Assistant(params AssistantContentPart[] parts)
    {
        return new AssistantModelMessage(parts);
    }

    public static AssistantToolCallPart ToolCall(
        string id,
        string name,
        string argumentsJson,
        string? providerOptionsJson = null
    )
    {
        return new AssistantToolCallPart(
            new GeneratedToolCall(id, name, argumentsJson),
            providerOptionsJson == null ? null : GoogleUpstream.ProviderOptions(providerOptionsJson)
        );
    }

    public static AssistantTextPart Text(string text, string? providerOptionsJson = null)
    {
        return new AssistantTextPart(
            text,
            providerOptionsJson == null ? null : GoogleUpstream.ProviderOptions(providerOptionsJson)
        );
    }

    public static AssistantReasoningPart Reasoning(string text, string providerOptionsJson)
    {
        return new AssistantReasoningPart(
            text,
            GoogleUpstream.ProviderOptions(providerOptionsJson)
        );
    }

    public static ConvertToGoogleInteractionsInputResult Convert(
        string? previousInteractionId,
        bool? store,
        params ModelMessage[] prompt
    )
    {
        return GoogleInteractionsInput.Convert(prompt, previousInteractionId, store);
    }

    public static JsonObject? FindStep(ConvertToGoogleInteractionsInputResult result, string type)
    {
        foreach (var step in result.Input)
        {
            if ((string?)step!["type"] == type)
            {
                return step.AsObject();
            }
        }

        return null;
    }

    public static List<string?> StepTypes(ConvertToGoogleInteractionsInputResult result)
    {
        return result.Input.Select(step => (string?)step!["type"]).ToList();
    }

    public static JsonArray Warnings(ConvertToGoogleInteractionsInputResult result)
    {
        var warnings = new JsonArray();
        foreach (var warning in result.Warnings)
        {
            warnings.Add(new JsonObject { ["type"] = warning.Type, ["message"] = warning.Message });
        }

        return warnings;
    }
}
