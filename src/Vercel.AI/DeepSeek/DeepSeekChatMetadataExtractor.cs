// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;

namespace Vercel.AI.DeepSeek;

/// <summary>
/// Builds the DeepSeek chat provider metadata: prompt cache tokens, response object, choice index, message role,
/// tool call types, logprobs and system fingerprint. Maps to the <c>providerMetadata</c> of <c>DeepSeekChatLanguageModel</c>.
/// </summary>
internal sealed class DeepSeekChatMetadataExtractor : OpenAICompatibleMetadataExtractor
{
    private readonly string _providerOptionsName;

    public DeepSeekChatMetadataExtractor(string providerOptionsName)
    {
        _providerOptionsName = providerOptionsName;
    }

    public override JsonObject? ExtractMetadata(JsonElement parsedBody)
    {
        var stream = new StreamExtractor(_providerOptionsName);
        stream.ProcessChunk(parsedBody);
        return stream.BuildMetadata();
    }

    public override OpenAICompatibleStreamMetadataExtractor CreateStreamExtractor()
    {
        return new StreamExtractor(_providerOptionsName);
    }

    private sealed class StreamExtractor(string providerOptionsName)
        : OpenAICompatibleStreamMetadataExtractor
    {
        private readonly SortedDictionary<int, string> _toolCallTypes = [];
        private readonly JsonArray _contentLogprobs = [];
        private readonly JsonArray _reasoningLogprobs = [];
        private JsonElement? _usage;
        private string? _responseObject;
        private string? _systemFingerprint;
        private int? _choiceIndex;
        private string? _messageRole;

        public override void ProcessChunk(JsonElement parsedChunk)
        {
            if (parsedChunk.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            if (
                parsedChunk.TryGetProperty("usage", out var usage)
                && usage.ValueKind == JsonValueKind.Object
            )
            {
                _usage = usage.Clone();
            }

            _responseObject = String(parsedChunk, "object") ?? _responseObject;
            _systemFingerprint = String(parsedChunk, "system_fingerprint") ?? _systemFingerprint;

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
            )
            {
                Append(logprobs, "content", _contentLogprobs);
                Append(logprobs, "reasoning_content", _reasoningLogprobs);
            }

            // A response has a message, a stream chunk has a delta.
            foreach (var name in new[] { "message", "delta" })
            {
                if (
                    !choice.TryGetProperty(name, out var message)
                    || message.ValueKind != JsonValueKind.Object
                )
                {
                    continue;
                }

                _messageRole = String(message, "role") ?? _messageRole;
                if (
                    message.TryGetProperty("tool_calls", out var toolCalls)
                    && toolCalls.ValueKind == JsonValueKind.Array
                )
                {
                    var position = 0;
                    foreach (var toolCall in toolCalls.EnumerateArray())
                    {
                        var toolIndex =
                            toolCall.TryGetProperty("index", out var i)
                            && i.ValueKind == JsonValueKind.Number
                                ? i.GetInt32()
                                : position;
                        if (String(toolCall, "type") is { } type)
                        {
                            _toolCallTypes[toolIndex] = type;
                        }

                        position++;
                    }
                }
            }
        }

        public override JsonObject? BuildMetadata()
        {
            var inner = new JsonObject();
            if (_usage is { } usage)
            {
                Copy(usage, "prompt_cache_hit_tokens", inner, "promptCacheHitTokens");
                Copy(usage, "prompt_cache_miss_tokens", inner, "promptCacheMissTokens");
            }

            if (_responseObject != null)
            {
                inner["responseObject"] = _responseObject;
            }

            if (_choiceIndex is { } index)
            {
                inner["choiceIndex"] = index;
            }

            if (_messageRole != null)
            {
                inner["messageRole"] = _messageRole;
            }

            if (_toolCallTypes.Count > 0)
            {
                inner["toolCallTypes"] = new JsonArray(
                    _toolCallTypes
                        .Values.Select(type => (JsonNode?)JsonValue.Create(type))
                        .ToArray()
                );
            }

            if (_contentLogprobs.Count > 0 || _reasoningLogprobs.Count > 0)
            {
                var logprobs = new JsonObject();
                if (_contentLogprobs.Count > 0)
                {
                    logprobs["content"] = _contentLogprobs.DeepClone();
                }

                if (_reasoningLogprobs.Count > 0)
                {
                    logprobs["reasoning_content"] = _reasoningLogprobs.DeepClone();
                }

                inner["logprobs"] = logprobs;
            }

            if (_systemFingerprint != null)
            {
                inner["systemFingerprint"] = _systemFingerprint;
            }

            return new JsonObject { [providerOptionsName] = inner };
        }

        private static string? String(JsonElement element, string name)
        {
            return
                element.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static void Copy(
            JsonElement source,
            string name,
            JsonObject target,
            string targetName
        )
        {
            if (
                source.TryGetProperty(name, out var value)
                && value.ValueKind == JsonValueKind.Number
            )
            {
                target[targetName] = JsonNode.Parse(value.GetRawText());
            }
        }

        private static void Append(JsonElement logprobs, string name, JsonArray target)
        {
            if (
                logprobs.TryGetProperty(name, out var items)
                && items.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var item in items.EnumerateArray())
                {
                    target.Add(JsonNode.Parse(item.GetRawText()));
                }
            }
        }
    }
}
