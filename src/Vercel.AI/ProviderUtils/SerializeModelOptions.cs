// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using Vercel.AI.Util;

namespace Vercel.AI.ProviderUtils;

/// <summary>A serialized model id and config. Maps to the result of <c>serializeModelOptions</c>.</summary>
public sealed class SerializedModelOptions
{
    /// <summary>Creates the serialized form.</summary>
    public SerializedModelOptions(string modelId, Dictionary<string, object?> config)
    {
        ModelId = modelId ?? string.Empty;
        Config = config ?? new Dictionary<string, object?>();
    }

    /// <summary>Model id.</summary>
    public string ModelId { get; }

    /// <summary>JSON-serializable configuration.</summary>
    public Dictionary<string, object?> Config { get; }
}

/// <summary>Serializes model options for a workflow boundary. Maps to <c>serializeModelOptions</c>.</summary>
public static class ModelOptionsSerialization
{
    /// <summary>
    /// Copies JSON-serializable config values. A function-valued <c>headers</c> entry is invoked.
    /// Asynchronous headers throw <see cref="SerializationError"/>.
    /// </summary>
    public static SerializedModelOptions SerializeModelOptions(string modelId, IReadOnlyDictionary<string, object?> config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var serializable = new Dictionary<string, object?>();
        foreach (var pair in config)
        {
            if (pair.Key == "headers")
            {
                var resolved = ToSerializableHeaders(ResolveSync(pair.Value));
                if (resolved != null)
                {
                    serializable[pair.Key] = resolved;
                }
            }
            else if (ProviderValues.IsJsonSerializable(pair.Value) && pair.Value is not JsUndefined)
            {
                serializable[pair.Key] = pair.Value;
            }
        }

        return new SerializedModelOptions(modelId, serializable);
    }

    private static object? ResolveSync(object? value)
    {
        object? next = value;
        if (value is Delegate function)
        {
            next = function.DynamicInvoke();
        }

        if (next is Task)
        {
            throw new SerializationError("Cannot serialize asynchronous model options.");
        }

        return next;
    }

    private static Dictionary<string, object?>? ToSerializableHeaders(object? value)
    {
        if (value is null || value is JsUndefined || value is not IDictionary dictionary)
        {
            return null;
        }

        var copy = new Dictionary<string, object?>();
        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key is not string key || entry.Value is JsUndefined || !ProviderValues.IsJsonSerializable(entry.Value))
            {
                return null;
            }

            copy[key] = entry.Value;
        }

        return copy;
    }
}

/// <summary>A value could not be serialized. Maps to <c>SerializationError</c>.</summary>
public sealed class SerializationError : AiSdkError
{
    /// <summary>Marker for <see cref="IsInstance(object)"/>.</summary>
    public const string ErrorMarker = "vercel.ai.error.AI_SerializationError";

    /// <summary>Creates the error.</summary>
    public SerializationError(string? message = null, Exception? cause = null)
        : base("AI_SerializationError", message ?? "Failed to serialize value.", cause)
    {
        SetMarker(ErrorMarker, true);
    }

    /// <summary>Returns whether <paramref name="error"/> carries this error's marker.</summary>
    public static new bool IsInstance(object? error)
    {
        return HasMarker(error, ErrorMarker);
    }
}
