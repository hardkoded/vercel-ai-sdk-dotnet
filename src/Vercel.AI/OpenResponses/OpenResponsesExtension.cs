// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.OpenResponses;

/// <summary>Input of <see cref="OpenResponsesExtension.EncodeTool"/> and <see cref="OpenResponsesExtension.EncodeToolChoice"/>.</summary>
public sealed class OpenResponsesToolContext
{
    /// <summary>Creates the context.</summary>
    public OpenResponsesToolContext(string name, JsonElement args)
    {
        Name = name;
        Args = args;
    }

    /// <summary>Tool name.</summary>
    public string Name { get; }

    /// <summary>Provider-tool arguments, a JSON object.</summary>
    public JsonElement Args { get; }
}

/// <summary>Whether an item is decoded from a generate response or from a stream.</summary>
public enum OpenResponsesItemMode
{
    /// <summary>A <c>DoGenerateAsync</c> response.</summary>
    Generate,

    /// <summary>A <c>DoStreamAsync</c> stream.</summary>
    Stream,
}

/// <summary>Input of <see cref="OpenResponsesExtension.DecodeItem"/>.</summary>
public sealed class OpenResponsesDecodeItemContext
{
    /// <summary>Creates the context.</summary>
    public OpenResponsesDecodeItemContext(JsonObject item, OpenResponsesItemMode mode)
    {
        Item = item;
        Mode = mode;
    }

    /// <summary>The completed item. It has a namespaced <c>type</c>, a string <c>id</c>, and a string <c>status</c>.</summary>
    public JsonObject Item { get; }

    /// <summary>Where the item came from.</summary>
    public OpenResponsesItemMode Mode { get; }
}

/// <summary>Input of <see cref="OpenResponsesExtension.EncodeInputItem"/>.</summary>
public sealed class OpenResponsesEncodeInputItemContext
{
    /// <summary>Creates the context.</summary>
    public OpenResponsesEncodeInputItemContext(AssistantContentPart part, LanguageModelTool tool)
    {
        Part = part;
        Tool = tool;
    }

    /// <summary>The history part: an <see cref="AssistantToolCallPart"/> or an <see cref="AssistantToolResultPart"/>.</summary>
    public AssistantContentPart Part { get; }

    /// <summary>The provider-defined tool the part belongs to.</summary>
    public LanguageModelTool Tool { get; }
}

/// <summary>Input of <see cref="OpenResponsesExtension.DecodeEvent"/>.</summary>
public sealed class OpenResponsesDecodeEventContext
{
    /// <summary>Creates the context.</summary>
    public OpenResponsesDecodeEventContext(JsonObject @event, IDictionary<string, object?> state)
    {
        Event = @event;
        State = state;
    }

    /// <summary>The event. It has a namespaced <c>type</c> and a numeric <c>sequence_number</c>.</summary>
    public JsonObject Event { get; }

    /// <summary>State that lasts for one response stream.</summary>
    public IDictionary<string, object?> State { get; }
}

/// <summary>
/// One Open Responses extension. Tool, item, and event capabilities are registered independently.
/// </summary>
public sealed class OpenResponsesExtension
{
    /// <summary>Extension id in <c>implementor.extension</c> form.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Namespaced tool type. Required together with <see cref="EncodeTool"/>.</summary>
    public string? ToolType { get; set; }

    /// <summary>Namespaced item types. Required together with <see cref="DecodeItem"/>.</summary>
    public IReadOnlyList<string>? ItemTypes { get; set; }

    /// <summary>Namespaced event types. Required together with <see cref="DecodeEvent"/>.</summary>
    public IReadOnlyList<string>? EventTypes { get; set; }

    /// <summary>Encodes a provider tool. Return null when its arguments cannot be encoded. The model adds <see cref="ToolType"/> and omits a tool choice that selects the omitted tool. Present or absent together with <see cref="ToolType"/>.</summary>
    public Func<OpenResponsesToolContext, JsonObject?>? EncodeTool { get; set; }

    /// <summary>Encodes a specific tool choice. The model adds <see cref="ToolType"/>. The default is an object holding only <see cref="ToolType"/>. Requires a tool encoder.</summary>
    public Func<OpenResponsesToolContext, JsonObject?>? EncodeToolChoice { get; set; }

    /// <summary>Decodes a completed item into content parts. The model adds a <c>open-responses.extension-replay</c> custom part holding the original item. Present or absent together with <see cref="ItemTypes"/>.</summary>
    public Func<OpenResponsesDecodeItemContext, IReadOnlyList<GeneratedContent>?>? DecodeItem { get; set; }

    /// <summary>Encodes a history part when no original wire item is available. Every returned item must use one of <see cref="ItemTypes"/>. Requires an item decoder.</summary>
    public Func<OpenResponsesEncodeInputItemContext, IReadOnlyList<JsonObject>?>? EncodeInputItem { get; set; }

    /// <summary>Decodes a streaming event into stream parts. Present or absent together with <see cref="EventTypes"/>.</summary>
    public Func<OpenResponsesDecodeEventContext, IReadOnlyList<LanguageModelStreamPart>?>? DecodeEvent { get; set; }
}

/// <summary>Indexes extensions by id and by wire type.</summary>
public sealed class OpenResponsesExtensionRegistry
{
    /// <summary>Creates an empty registry.</summary>
    public OpenResponsesExtensionRegistry()
    {
        ByEventType = new Dictionary<string, OpenResponsesExtension>(StringComparer.Ordinal);
        ByExtensionId = new Dictionary<string, OpenResponsesExtension>(StringComparer.Ordinal);
        ByItemType = new Dictionary<string, OpenResponsesExtension>(StringComparer.Ordinal);
        ByProviderToolId = new Dictionary<string, OpenResponsesExtension>(StringComparer.Ordinal);
        ByToolType = new Dictionary<string, OpenResponsesExtension>(StringComparer.Ordinal);
    }

    /// <summary>Event type to the extension that decodes it.</summary>
    public IDictionary<string, OpenResponsesExtension> ByEventType { get; }

    /// <summary>Extension id to the extension.</summary>
    public IDictionary<string, OpenResponsesExtension> ByExtensionId { get; }

    /// <summary>Item type to the extension that decodes it.</summary>
    public IDictionary<string, OpenResponsesExtension> ByItemType { get; }

    /// <summary>Provider-tool id to the extension that encodes it.</summary>
    public IDictionary<string, OpenResponsesExtension> ByProviderToolId { get; }

    /// <summary>Tool type to the extension that encodes it.</summary>
    public IDictionary<string, OpenResponsesExtension> ByToolType { get; }
}

/// <summary>Builds an <see cref="OpenResponsesExtensionRegistry"/>.</summary>
public static class OpenResponsesExtensions
{
    /// <summary>Indexes <paramref name="extensions"/> and rejects incomplete or colliding registrations.</summary>
    public static OpenResponsesExtensionRegistry CreateRegistry(IReadOnlyList<OpenResponsesExtension>? extensions)
    {
        var registry = new OpenResponsesExtensionRegistry();
        if (extensions == null)
        {
            return registry;
        }

        foreach (var extension in extensions)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            var separator = extension.Id == null ? -1 : extension.Id.IndexOf('.');
            if (separator <= 0)
            {
                throw new InvalidOperationException(
                    "Open Responses extension ID " + extension.Id + " must use <implementor>.<extension> format.");
            }

            var ns = extension.Id!.Substring(0, separator);
            Register(registry.ByExtensionId, extension.Id, extension, "id");

            var hasToolType = extension.ToolType != null;
            var hasToolEncoder = extension.EncodeTool != null;
            if (hasToolType != hasToolEncoder)
            {
                throw new InvalidOperationException(
                    "Open Responses extension " + extension.Id + " must provide toolType and encodeTool together.");
            }

            if (extension.EncodeToolChoice != null && !hasToolEncoder)
            {
                throw new InvalidOperationException(
                    "Open Responses extension " + extension.Id + " cannot provide encodeToolChoice without toolType and encodeTool.");
            }

            if (hasToolType && hasToolEncoder)
            {
                AssertNamespaced(extension.Id, ns, extension.ToolType!, "toolType");
                Register(registry.ByProviderToolId, extension.Id, extension, "provider-tool id");
                Register(registry.ByToolType, extension.ToolType!, extension, "toolType");
            }

            var hasItemTypes = extension.ItemTypes != null;
            var hasItemDecoder = extension.DecodeItem != null;
            if (hasItemTypes != hasItemDecoder)
            {
                throw new InvalidOperationException(
                    "Open Responses extension " + extension.Id + " must provide itemTypes and decodeItem together.");
            }

            if (extension.EncodeInputItem != null && !hasItemDecoder)
            {
                throw new InvalidOperationException(
                    "Open Responses extension " + extension.Id + " cannot provide encodeInputItem without itemTypes and decodeItem.");
            }

            if (hasItemTypes && hasItemDecoder)
            {
                if (extension.ItemTypes!.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Open Responses extension " + extension.Id + " must register at least one item type.");
                }

                foreach (var itemType in extension.ItemTypes)
                {
                    AssertNamespaced(extension.Id, ns, itemType, "itemTypes");
                    Register(registry.ByItemType, itemType, extension, "item type");
                }
            }

            var hasEventTypes = extension.EventTypes != null;
            var hasEventDecoder = extension.DecodeEvent != null;
            if (hasEventTypes != hasEventDecoder)
            {
                throw new InvalidOperationException(
                    "Open Responses extension " + extension.Id + " must provide eventTypes and decodeEvent together.");
            }

            if (hasEventTypes && hasEventDecoder)
            {
                if (extension.EventTypes!.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Open Responses extension " + extension.Id + " must register at least one event type.");
                }

                foreach (var eventType in extension.EventTypes)
                {
                    AssertNamespaced(extension.Id, ns, eventType, "eventTypes");
                    Register(registry.ByEventType, eventType, extension, "event type");
                }
            }

            if (!hasToolEncoder && !hasItemDecoder && !hasEventDecoder)
            {
                throw new InvalidOperationException(
                    "Open Responses extension " + extension.Id + " must register a tool, item, or event capability.");
            }
        }

        return registry;
    }

    private static void AssertNamespaced(string extensionId, string ns, string type, string field)
    {
        var colon = type == null ? -1 : type.IndexOf(':');
        if (colon < 0 || type!.Substring(0, colon) != ns)
        {
            throw new InvalidOperationException(
                "Open Responses extension " + extensionId + " has invalid " + field + " value " + type + ". Extension wire types must use the " + ns + ": namespace.");
        }
    }

    private static void Register(IDictionary<string, OpenResponsesExtension> map, string key, OpenResponsesExtension extension, string field)
    {
        OpenResponsesExtension? existing;
        if (map.TryGetValue(key, out existing) && existing != null)
        {
            throw new InvalidOperationException(
                "Open Responses extension " + extension.Id + " cannot register " + field + " " + key + " because it is already registered by " + existing.Id + ".");
        }

        map[key] = extension;
    }
}
