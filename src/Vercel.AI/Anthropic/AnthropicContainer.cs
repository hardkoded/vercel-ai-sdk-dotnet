// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Anthropic;

/// <summary>Copies the latest Anthropic container id into the next call's provider options.</summary>
public static class AnthropicContainer
{
    /// <summary>
    /// Searches <paramref name="steps"/> from the end and returns a <c>prepareStep</c> update whose provider options set
    /// <c>anthropic.container.id</c>. Returns null when no step has a container id.
    /// Maps to <c>forwardAnthropicContainerIdFromLastStep</c>.
    /// </summary>
    public static PrepareStepUpdate? ForwardFromLastStep(IReadOnlyList<StepResult> steps)
    {
        if (steps == null)
        {
            throw new ArgumentNullException(nameof(steps));
        }

        for (var i = steps.Count - 1; i >= 0; i--)
        {
            if (steps[i].ProviderMetadata is not { ValueKind: JsonValueKind.Object } metadata
                || !metadata.TryGetProperty("anthropic", out var anthropic) || anthropic.ValueKind != JsonValueKind.Object
                || !anthropic.TryGetProperty("container", out var container) || container.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var id = AnthropicJson.String(container, "id");
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            using var options = JsonDocument.Parse("{\"container\":{\"id\":" + JsonSerializer.Serialize(id) + "}}");
            return new PrepareStepUpdate
            {
                ProviderOptions = new Dictionary<string, JsonElement> { ["anthropic"] = options.RootElement.Clone() },
            };
        }

        return null;
    }
}
