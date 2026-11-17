// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mistral;

/// <summary>Provider options of the Mistral Conversations API. Maps to <c>mistral-conversation-language-model-options.ts</c>.</summary>
/// <remarks>The options are the chat options, see <see cref="MistralLanguageModelChatOptions"/>.</remarks>
internal static class MistralLanguageModelConversationOptions
{
    /// <summary>Parses the <c>mistral</c> provider options. Maps to <c>parseProviderOptions</c> with <c>mistralLanguageModelConversationOptions</c>.</summary>
    public static MistralLanguageModelChatOptions? Parse(
        IReadOnlyDictionary<string, JsonElement>? providerOptions
    )
    {
        if (
            providerOptions == null
            || !providerOptions.TryGetValue("mistral", out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
        )
        {
            return null;
        }

        var error = ProviderOptionShape.Validate(
            value,
            MistralLanguageModelChatOptions.Shape(),
            false
        );
        if (error != null)
        {
            throw new InvalidArgumentException(
                "providerOptions",
                value.GetRawText(),
                "invalid mistral provider options: " + error
            );
        }

        return MistralLanguageModelChatOptions.FromJson(value);
    }
}
