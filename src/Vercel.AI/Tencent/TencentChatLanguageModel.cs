// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Tencent;

/// <summary>
/// TokenHub chat completions over the OpenAI-compatible protocol.
/// Serializes its own config because the URL is rebuilt from the base URL when the model is restored.
/// </summary>
public sealed class TencentChatLanguageModel : OpenAICompatibleLanguageModel
{
    private readonly TencentProvider _tencent;

    /// <summary>Creates a model.</summary>
    public TencentChatLanguageModel(TencentProvider provider, string modelId)
        : base(provider, modelId)
    {
        _tencent = provider;
    }

    /// <summary>
    /// Serializes the model id and config for a workflow boundary. Maps to <c>[WORKFLOW_SERIALIZE]</c>.
    /// The API key is not part of the serialized config. Pass it to <see cref="Deserialize"/>.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var headers = new Dictionary<string, object?>();
        foreach (var pair in _tencent.Options.Headers)
        {
            if (!IsSensitiveHeader(pair.Key))
            {
                headers[pair.Key] = pair.Value;
            }
        }

        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["baseURL"] = _tencent.Options.BaseUrl,
            ["headers"] = headers,
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>Restores a model from <see cref="Serialize"/>. Maps to <c>[WORKFLOW_DESERIALIZE]</c>.</summary>
    /// <param name="serialized">The serialized model id and config.</param>
    /// <param name="apiKey">API key. Defaults to the <c>TENCENT_TOKENHUB_API_KEY</c> environment variable.</param>
    /// <param name="handler">HTTP handler. Pass one from tests.</param>
    public static TencentChatLanguageModel Deserialize(SerializedModelOptions serialized, string? apiKey = null, HttpMessageHandler? handler = null)
    {
        if (serialized is null)
        {
            throw new ArgumentNullException(nameof(serialized));
        }

        var options = new TencentOptions { ApiKey = apiKey };
        if (serialized.Config.TryGetValue("baseURL", out var baseUrl))
        {
            var url = baseUrl is JsonElement { ValueKind: JsonValueKind.String } element ? element.GetString() : baseUrl as string;
            if (!string.IsNullOrEmpty(url))
            {
                options.BaseUrl = url;
            }
        }

        if (serialized.Config.TryGetValue("headers", out var headers))
        {
            if (headers is JsonElement { ValueKind: JsonValueKind.Object } json)
            {
                foreach (var property in json.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        options.Headers[property.Name] = property.Value.GetString()!;
                    }
                }
            }
            else if (headers is System.Collections.IDictionary map)
            {
                foreach (System.Collections.DictionaryEntry entry in map)
                {
                    if (entry.Value is string value)
                    {
                        options.Headers[(string)entry.Key] = value;
                    }
                }
            }
        }

        return (TencentChatLanguageModel)TencentProvider.Create(options, handler).Chat(serialized.ModelId);
    }

    private static bool IsSensitiveHeader(string name)
    {
        foreach (var marker in new[] { "authorization", "key", "token", "secret", "cookie" })
        {
            if (name.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
