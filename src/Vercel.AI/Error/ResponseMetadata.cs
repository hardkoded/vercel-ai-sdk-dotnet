// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;

namespace Vercel.AI.Error;

/// <summary>Response metadata for a language-model call, without the generated messages.</summary>
public sealed class LanguageModelResponseMetadata
{
    /// <summary>Creates response metadata.</summary>
    public LanguageModelResponseMetadata(
        string id,
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string>? headers = null,
        object? body = null)
    {
        Id = id;
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
        Body = body;
    }

    /// <summary>Provider response id.</summary>
    public string Id { get; }

    /// <summary>When the provider started the response.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id that produced the response.</summary>
    public string ModelId { get; }

    /// <summary>HTTP response headers, when the provider sent them.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Response body, when the provider exposed one.</summary>
    public object? Body { get; }
}

/// <summary>Response metadata shared by image, speech, transcription, translation, and video calls.</summary>
public sealed class MediaResponseMetadata
{
    /// <summary>Creates response metadata.</summary>
    public MediaResponseMetadata(
        DateTimeOffset timestamp,
        string modelId,
        IReadOnlyDictionary<string, string>? headers = null,
        object? body = null,
        object? providerMetadata = null)
    {
        Timestamp = timestamp;
        ModelId = modelId;
        Headers = headers;
        Body = body;
        ProviderMetadata = providerMetadata;
    }

    /// <summary>When the provider started the response.</summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>Model id that produced the response.</summary>
    public string ModelId { get; }

    /// <summary>HTTP response headers, when the provider sent them.</summary>
    public IReadOnlyDictionary<string, string>? Headers { get; }

    /// <summary>Response body, when the provider exposed one.</summary>
    public object? Body { get; }

    /// <summary>Provider-specific metadata for this call.</summary>
    public object? ProviderMetadata { get; }
}

/// <summary>One underlying image-model call recorded on <see cref="NoImageGeneratedException"/>.</summary>
public sealed class ImageModelCall
{
    /// <summary>Creates a call record.</summary>
    public ImageModelCall(
        IReadOnlyList<GeneratedImage>? images,
        MediaResponseMetadata response,
        IReadOnlyList<CallWarning>? warnings = null,
        object? providerMetadata = null,
        object? usage = null)
    {
        Images = images ?? Array.Empty<GeneratedImage>();
        Response = response;
        Warnings = warnings ?? Array.Empty<CallWarning>();
        ProviderMetadata = providerMetadata;
        Usage = usage;
    }

    /// <summary>Images returned by this call.</summary>
    public IReadOnlyList<GeneratedImage> Images { get; }

    /// <summary>Response metadata for this call.</summary>
    public MediaResponseMetadata Response { get; }

    /// <summary>Warnings reported for this call.</summary>
    public IReadOnlyList<CallWarning> Warnings { get; }

    /// <summary>Provider-specific metadata for this call.</summary>
    public object? ProviderMetadata { get; }

    /// <summary>Usage reported for this call, when the provider sent it.</summary>
    public object? Usage { get; }
}
