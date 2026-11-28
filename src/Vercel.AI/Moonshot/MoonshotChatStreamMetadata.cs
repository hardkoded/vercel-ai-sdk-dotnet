// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.Moonshot;

/// <summary>
/// Collects the response metadata Moonshot AI keeps in the provider metadata of the stream finish part:
/// <c>responseObject</c>, <c>choiceIndex</c>, <c>messageRole</c>, <c>toolCallTypes</c>, and the streamed <c>logprobs</c>.
/// Maps to the <c>doStream</c> transform of <c>moonshotai-chat-language-model.ts</c>.
/// </summary>
internal sealed class MoonshotChatStreamMetadata : OpenAICompatibleStreamMetadataExtractor
{
    private readonly SortedDictionary<int, string> _toolCallTypes = [];
    private readonly JsonArray _logprobs = [];
    private string? _responseObject;
    private int? _choiceIndex;
    private string? _messageRole;
    private bool _hasLogprobs;

    /// <inheritdoc />
    public override void ProcessChunk(JsonElement parsedChunk)
    {
        if (parsedChunk.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (
            parsedChunk.TryGetProperty("object", out var responseObject)
            && responseObject.ValueKind == JsonValueKind.String
        )
        {
            _responseObject = responseObject.GetString();
        }

        if (
            !parsedChunk.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
            || choices.GetArrayLength() == 0
        )
        {
            return;
        }

        var choice = choices[0];
        if (
            choice.TryGetProperty("index", out var index)
            && index.ValueKind == JsonValueKind.Number
        )
        {
            _choiceIndex = index.GetInt32();
        }

        if (
            choice.TryGetProperty("logprobs", out var logprobs)
            && logprobs.ValueKind == JsonValueKind.Object
            && logprobs.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.Array
        )
        {
            _hasLogprobs = true;
            foreach (var entry in content.EnumerateArray())
            {
                _logprobs.Add(JsonNode.Parse(entry.GetRawText()));
            }
        }

        if (
            !choice.TryGetProperty("delta", out var delta)
            || delta.ValueKind != JsonValueKind.Object
        )
        {
            return;
        }

        if (delta.TryGetProperty("role", out var role) && role.ValueKind == JsonValueKind.String)
        {
            _messageRole = role.GetString();
        }

        if (
            delta.TryGetProperty("tool_calls", out var toolCalls)
            && toolCalls.ValueKind == JsonValueKind.Array
        )
        {
            var position = 0;
            foreach (var toolCall in toolCalls.EnumerateArray())
            {
                var toolIndex =
                    toolCall.TryGetProperty("index", out var explicitIndex)
                    && explicitIndex.ValueKind == JsonValueKind.Number
                        ? explicitIndex.GetInt32()
                        : position;
                position++;
                if (
                    toolCall.TryGetProperty("type", out var type)
                    && type.ValueKind == JsonValueKind.String
                )
                {
                    _toolCallTypes[toolIndex] = type.GetString()!;
                }
            }
        }
    }

    /// <inheritdoc />
    public override JsonObject? BuildMetadata()
    {
        var inner = new JsonObject();
        if (_responseObject != null)
        {
            inner["responseObject"] = _responseObject;
        }

        if (_choiceIndex is { } choiceIndex)
        {
            inner["choiceIndex"] = choiceIndex;
        }

        if (_messageRole != null)
        {
            inner["messageRole"] = _messageRole;
        }

        if (_toolCallTypes.Count > 0)
        {
            inner["toolCallTypes"] = new JsonArray(
                _toolCallTypes.Values.Select(t => (JsonNode?)JsonValue.Create(t)).ToArray()
            );
        }

        if (_hasLogprobs)
        {
            inner["logprobs"] = new JsonObject { ["content"] = _logprobs.DeepClone() };
        }

        return new JsonObject { [MoonshotProvider.ProviderId] = inner };
    }
}
