// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Reads the response metadata Moonshot AI keeps in provider metadata: <c>responseObject</c>, <c>choiceIndex</c>,
/// <c>messageRole</c>, <c>logprobs</c>, and <c>toolCallTypes</c>. Maps to the <c>providerMetadata</c> of <c>doGenerate</c> in
/// <c>moonshotai-chat-language-model.ts</c>.
/// </summary>
internal sealed class MoonshotChatMetadata : OpenAICompatibleMetadataExtractor
{
    /// <inheritdoc />
    public override JsonObject? ExtractMetadata(JsonElement parsedBody)
    {
        var metadata = new JsonObject();
        if (
            parsedBody.ValueKind == JsonValueKind.Object
            && parsedBody.TryGetProperty("object", out var responseObject)
            && responseObject.ValueKind == JsonValueKind.String
        )
        {
            metadata["responseObject"] = responseObject.GetString();
        }

        if (
            parsedBody.ValueKind == JsonValueKind.Object
            && parsedBody.TryGetProperty("choices", out var choices)
            && choices.ValueKind == JsonValueKind.Array
            && choices.GetArrayLength() > 0
        )
        {
            var choice = choices[0];
            if (
                choice.TryGetProperty("index", out var index)
                && index.ValueKind == JsonValueKind.Number
            )
            {
                metadata["choiceIndex"] = JsonNode.Parse(index.GetRawText());
            }

            if (
                choice.TryGetProperty("logprobs", out var logprobs)
                && logprobs.ValueKind == JsonValueKind.Object
            )
            {
                metadata["logprobs"] = JsonNode.Parse(logprobs.GetRawText());
            }

            if (
                choice.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.Object
            )
            {
                if (
                    message.TryGetProperty("role", out var role)
                    && role.ValueKind == JsonValueKind.String
                )
                {
                    metadata["messageRole"] = role.GetString();
                }

                if (
                    message.TryGetProperty("tool_calls", out var toolCalls)
                    && toolCalls.ValueKind == JsonValueKind.Array
                )
                {
                    var types = new JsonArray();
                    foreach (var toolCall in toolCalls.EnumerateArray())
                    {
                        if (
                            toolCall.TryGetProperty("type", out var type)
                            && type.ValueKind == JsonValueKind.String
                        )
                        {
                            types.Add(type.GetString());
                        }
                    }

                    metadata["toolCallTypes"] = types;
                }
            }
        }

        return new JsonObject { [MoonshotProvider.ProviderId] = metadata };
    }

    /// <inheritdoc />
    public override OpenAICompatibleStreamMetadataExtractor CreateStreamExtractor()
    {
        return new MoonshotChatStreamMetadata();
    }
}
