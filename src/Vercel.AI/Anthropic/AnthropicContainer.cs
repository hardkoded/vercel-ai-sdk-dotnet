// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.Anthropic;

/// <summary>Copies the latest Anthropic container id into the next call's provider options.</summary>
public static class AnthropicContainer
{
    /// <summary>
    /// Searches <paramref name="steps"/> from the end and returns provider options that set
    /// <c>anthropic.container.id</c>. Returns null when no step has a container id.
    /// </summary>
    public static JsonObject? ForwardFromLastStep(JsonElement steps)
    {
        if (steps.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        for (var i = steps.GetArrayLength() - 1; i >= 0; i--)
        {
            var step = steps[i];
            if (!step.TryGetProperty("providerMetadata", out var metadata) || metadata.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!metadata.TryGetProperty("anthropic", out var anthropic) || anthropic.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!anthropic.TryGetProperty("container", out var container) || container.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var id = AnthropicJson.String(container, "id");
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            return new JsonObject
            {
                ["providerOptions"] = new JsonObject
                {
                    ["anthropic"] = new JsonObject
                    {
                        ["container"] = new JsonObject { ["id"] = id },
                    },
                },
            };
        }

        return null;
    }
}
