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
    /// Searches <paramref name="steps"/> from the end and returns a <see cref="PrepareStepUpdate"/> whose provider options set
    /// <c>anthropic.container.id</c>. Returns null when no step has a container id. Return it from <c>PrepareStep</c>.
    /// </summary>
    public static PrepareStepUpdate? ForwardFromLastStep(IReadOnlyList<StepResult> steps)
    {
        if (steps == null)
        {
            return null;
        }

        for (var i = steps.Count - 1; i >= 0; i--)
        {
            if (steps[i].ProviderMetadata is not { ValueKind: JsonValueKind.Object } metadata)
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

            var options = new JsonObject { ["container"] = new JsonObject { ["id"] = id } };
            return new PrepareStepUpdate
            {
                ProviderOptions = new Dictionary<string, JsonElement>
                {
                    ["anthropic"] = JsonSerializer.SerializeToElement(options),
                },
            };
        }

        return null;
    }
}
