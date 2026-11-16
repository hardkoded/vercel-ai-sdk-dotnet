// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;

namespace Vercel.AI.OpenAI;

/// <summary>A server event the Live data channel forwards to the client.</summary>
public sealed class OpenAIRealtimeModelLiveServerEventSelector
{
    /// <summary>Event type.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Required for <c>response.event</c> and forbidden for other event types.</summary>
    public string? ResponseEvent { get; set; }
}

/// <summary>Data channel permissions of a Live WebRTC client.</summary>
public sealed class OpenAIRealtimeModelLiveDataChannel
{
    /// <summary>True for <c>allowedClientEvents: "all"</c>.</summary>
    public bool AllowAllClientEvents { get; set; }

    /// <summary>The client event types the data channel accepts.</summary>
    public IReadOnlyList<string>? AllowedClientEvents { get; set; }

    /// <summary>True for <c>allowedServerEvents: "all"</c>.</summary>
    public bool AllowAllServerEvents { get; set; }

    /// <summary>The server events the data channel forwards.</summary>
    public IReadOnlyList<OpenAIRealtimeModelLiveServerEventSelector>? AllowedServerEvents { get; set; }
}

/// <summary>
/// Experimental Live options under <c>sessionConfig.providerOptions.openai</c>. Maps to
/// <c>OpenAIRealtimeModelLiveOptions</c>. <see cref="ToJsonObject"/> builds the provider options value.
/// </summary>
public sealed class OpenAIRealtimeModelLiveOptions
{
    /// <summary>Client permissions; valid for WebRTC startup only. Maps to <c>client.dataChannel</c>.</summary>
    public OpenAIRealtimeModelLiveDataChannel? Client { get; set; }

    /// <summary>True for <c>delegation: { type: 'client' }</c>.</summary>
    public bool ClientDelegation { get; set; }

    /// <summary>True for an explicit <c>delegation: null</c>.</summary>
    public bool DisableDelegation { get; set; }

    /// <summary>Startup history messages (at most 128).</summary>
    public IReadOnlyList<JsonObject>? Input { get; set; }

    /// <summary>Whether the provider stores the session.</summary>
    public bool? Store { get; set; }

    /// <summary>Custom voice id. Maps to <c>voice: { id }</c>.</summary>
    public string? VoiceId { get; set; }

    /// <summary>Builds the JSON value for <c>providerOptions.openai</c>.</summary>
    public JsonObject ToJsonObject()
    {
        var result = new JsonObject();
        if (Client != null)
        {
            var channel = new JsonObject();
            if (Client.AllowAllClientEvents)
            {
                channel["allowedClientEvents"] = "all";
            }
            else if (Client.AllowedClientEvents != null)
            {
                channel["allowedClientEvents"] = new JsonArray(Client.AllowedClientEvents.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray());
            }

            if (Client.AllowAllServerEvents)
            {
                channel["allowedServerEvents"] = "all";
            }
            else if (Client.AllowedServerEvents != null)
            {
                var selectors = new JsonArray();
                foreach (var selector in Client.AllowedServerEvents)
                {
                    var node = new JsonObject { ["type"] = selector.Type };
                    if (selector.ResponseEvent != null)
                    {
                        node["responseEvent"] = selector.ResponseEvent;
                    }

                    selectors.Add(node);
                }

                channel["allowedServerEvents"] = selectors;
            }

            result["client"] = new JsonObject { ["dataChannel"] = channel };
        }

        if (ClientDelegation)
        {
            result["delegation"] = new JsonObject { ["type"] = "client" };
        }
        else if (DisableDelegation)
        {
            result["delegation"] = null;
        }

        if (Input != null)
        {
            result["input"] = new JsonArray(Input.Select(item => (JsonNode?)item.DeepClone()).ToArray());
        }

        if (Store != null)
        {
            result["store"] = Store;
        }

        if (VoiceId != null)
        {
            result["voice"] = new JsonObject { ["id"] = VoiceId };
        }

        return result;
    }

    /// <summary>
    /// Validates and reads <c>providerOptions.openai</c>. Maps to <c>openaiRealtimeModelLiveOptionsSchema.parse</c>:
    /// unknown keys and wrong shapes are rejected.
    /// </summary>
    internal static OpenAIRealtimeModelLiveOptions Parse(JsonNode? node)
    {
        var options = new OpenAIRealtimeModelLiveOptions();
        if (node == null)
        {
            return options;
        }

        var root = AsObject(node, "providerOptions.openai");
        StrictKeys(root, "providerOptions.openai", "client", "delegation", "input", "store", "voice");

        if (root.TryGetPropertyValue("client", out var client) && client != null)
        {
            var clientObject = AsObject(client, "client");
            StrictKeys(clientObject, "client", "dataChannel");
            if (!clientObject.TryGetPropertyValue("dataChannel", out var dataChannel) || dataChannel == null)
            {
                throw Invalid("client.dataChannel", "is required.");
            }

            options.Client = ParseDataChannel(AsObject(dataChannel, "client.dataChannel"));
        }

        if (root.TryGetPropertyValue("delegation", out var delegation))
        {
            if (delegation == null)
            {
                options.DisableDelegation = true;
            }
            else
            {
                var delegationObject = AsObject(delegation, "delegation");
                StrictKeys(delegationObject, "delegation", "type");
                if (!IsString(delegationObject["type"], "client"))
                {
                    throw Invalid("delegation.type", "must be 'client'.");
                }

                options.ClientDelegation = true;
            }
        }

        if (root.TryGetPropertyValue("input", out var input) && input != null)
        {
            options.Input = ParseInput(input);
        }

        if (root.TryGetPropertyValue("store", out var store) && store != null)
        {
            if (store is not JsonValue storeValue || !storeValue.TryGetValue<bool>(out var storeFlag))
            {
                throw Invalid("store", "must be a boolean.");
            }

            options.Store = storeFlag;
        }

        if (root.TryGetPropertyValue("voice", out var voice) && voice != null)
        {
            var voiceObject = AsObject(voice, "voice");
            StrictKeys(voiceObject, "voice", "id");
            if (voiceObject["id"] is not JsonValue id || !id.TryGetValue<string>(out var voiceId) || voiceId.Length == 0)
            {
                throw Invalid("voice.id", "must be a non-empty string.");
            }

            options.VoiceId = voiceId;
        }

        return options;
    }

    private static OpenAIRealtimeModelLiveDataChannel ParseDataChannel(JsonObject channel)
    {
        StrictKeys(channel, "client.dataChannel", "allowedClientEvents", "allowedServerEvents");
        var result = new OpenAIRealtimeModelLiveDataChannel();
        if (channel.TryGetPropertyValue("allowedClientEvents", out var clientEvents) && clientEvents != null)
        {
            if (IsString(clientEvents, "all"))
            {
                result.AllowAllClientEvents = true;
            }
            else
            {
                result.AllowedClientEvents = ParseStrings(clientEvents, "client.dataChannel.allowedClientEvents");
            }
        }

        if (channel.TryGetPropertyValue("allowedServerEvents", out var serverEvents) && serverEvents != null)
        {
            if (IsString(serverEvents, "all"))
            {
                result.AllowAllServerEvents = true;
            }
            else
            {
                var selectors = new List<OpenAIRealtimeModelLiveServerEventSelector>();
                if (serverEvents is not JsonArray array)
                {
                    throw Invalid("client.dataChannel.allowedServerEvents", "must be 'all' or an array.");
                }

                foreach (var entry in array)
                {
                    var selectorObject = AsObject(entry, "client.dataChannel.allowedServerEvents[]");
                    StrictKeys(selectorObject, "client.dataChannel.allowedServerEvents[]", "type", "responseEvent");
                    var type = RequiredString(selectorObject, "type", "client.dataChannel.allowedServerEvents[].type");
                    var responseEvent = selectorObject.TryGetPropertyValue("responseEvent", out var responseNode) && responseNode != null
                        ? RequiredString(selectorObject, "responseEvent", "client.dataChannel.allowedServerEvents[].responseEvent")
                        : null;
                    if ((type == "response.event") != (responseEvent != null))
                    {
                        throw Invalid(
                            "client.dataChannel.allowedServerEvents[]",
                            "responseEvent is required for response.event and forbidden for other event types.");
                    }

                    selectors.Add(new OpenAIRealtimeModelLiveServerEventSelector { Type = type, ResponseEvent = responseEvent });
                }

                result.AllowedServerEvents = selectors;
            }
        }

        return result;
    }

    private static List<JsonObject> ParseInput(JsonNode input)
    {
        if (input is not JsonArray array)
        {
            throw Invalid("input", "must be an array.");
        }

        if (array.Count > 128)
        {
            throw Invalid("input", "must contain at most 128 items.");
        }

        var items = new List<JsonObject>();
        foreach (var entry in array)
        {
            var item = AsObject(entry, "input[]");
            StrictKeys(item, "input[]", "type", "role", "content");
            if (!IsString(item["type"], "message"))
            {
                throw Invalid("input[].type", "must be 'message'.");
            }

            var role = RequiredString(item, "role", "input[].role");
            string[] contentTypes;
            if (role == "developer" || role == "user")
            {
                contentTypes = ["input_text"];
            }
            else if (role == "assistant")
            {
                contentTypes = ["text", "output_text"];
            }
            else
            {
                throw Invalid("input[].role", "must be 'developer', 'user' or 'assistant'.");
            }

            if (item["content"] is not JsonArray content || content.Count != 1)
            {
                throw Invalid("input[].content", "must be an array with exactly one part.");
            }

            var part = AsObject(content[0], "input[].content[]");
            StrictKeys(part, "input[].content[]", "type", "text");
            var partType = RequiredString(part, "type", "input[].content[].type");
            if (Array.IndexOf(contentTypes, partType) < 0)
            {
                throw Invalid("input[].content[].type", "is not valid for role '" + role + "'.");
            }

            RequiredString(part, "text", "input[].content[].text", allowEmpty: true);
            items.Add((JsonObject)item.DeepClone());
        }

        return items;
    }

    private static List<string> ParseStrings(JsonNode node, string path)
    {
        if (node is not JsonArray array)
        {
            throw Invalid(path, "must be 'all' or an array of strings.");
        }

        var values = new List<string>();
        foreach (var entry in array)
        {
            if (entry is not JsonValue value || !value.TryGetValue<string>(out var text))
            {
                throw Invalid(path, "must contain only strings.");
            }

            values.Add(text);
        }

        return values;
    }

    private static bool IsString(JsonNode? node, string expected)
    {
        return node is JsonValue value && value.TryGetValue<string>(out var text) && text == expected;
    }

    private static string RequiredString(JsonObject parent, string key, string path, bool allowEmpty = false)
    {
        if (!parent.TryGetPropertyValue(key, out var node)
            || node is not JsonValue value
            || !value.TryGetValue<string>(out var text)
            || (!allowEmpty && text.Length == 0))
        {
            throw Invalid(path, "must be a string.");
        }

        return text;
    }

    private static JsonObject AsObject(JsonNode? node, string path)
    {
        return node as JsonObject ?? throw Invalid(path, "must be an object.");
    }

    private static void StrictKeys(JsonObject value, string path, params string[] allowed)
    {
        foreach (var pair in value)
        {
            if (Array.IndexOf(allowed, pair.Key) < 0)
            {
                throw Invalid(path, "has unrecognized key '" + pair.Key + "'.");
            }
        }
    }

    private static InvalidArgumentException Invalid(string path, string message)
    {
        return new InvalidArgumentException("providerOptions.openai", path, path + " " + message);
    }
}
