// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>A part of decision state as the caller passes it. Maps to <c>DecisionStatePart</c>.</summary>
public abstract class DecisionStatePart
{
    private protected DecisionStatePart(string type)
    {
        Type = type;
    }

    /// <summary><c>text</c>, <c>file</c>, or <c>json</c>.</summary>
    public string Type { get; }
}

/// <summary>A text part of decision state.</summary>
public sealed class DecisionTextPart : DecisionStatePart
{
    /// <summary>Creates a text part.</summary>
    public DecisionTextPart(string text)
        : base("text")
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>A file part of decision state. A URL is downloaded before the model call.</summary>
public sealed class DecisionFilePart : DecisionStatePart
{
    /// <summary>Creates a file part.</summary>
    public DecisionFilePart(string mediaType, FilePartInput data)
        : base("file")
    {
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <summary>Media type. A top-level type such as <c>image</c> is resolved to a full type from the data.</summary>
    public string MediaType { get; }

    /// <summary>File data.</summary>
    public FilePartInput Data { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; init; }

    /// <summary>Provider-specific options for this part.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
}

/// <summary>A JSON part of decision state.</summary>
public sealed class DecisionJsonPart : DecisionStatePart
{
    /// <summary>Creates a JSON part. Any JSON value is valid.</summary>
    public DecisionJsonPart(JsonElement value)
        : base("json")
    {
        if (value.ValueKind == JsonValueKind.Undefined)
        {
            throw new ArgumentException("The JSON value must not be undefined.", nameof(value));
        }

        Value = value.Clone();
    }

    /// <summary>The JSON value.</summary>
    public JsonElement Value { get; }
}

/// <summary>A part of decision state as the model receives it. Maps to <c>DecisionModelV4StatePart</c>.</summary>
public abstract class DecisionModelStatePart
{
    private protected DecisionModelStatePart(string type)
    {
        Type = type;
    }

    /// <summary><c>text</c>, <c>file</c>, or <c>json</c>.</summary>
    public string Type { get; }
}

/// <summary>A text part the model receives.</summary>
public sealed class DecisionModelTextPart : DecisionModelStatePart
{
    /// <summary>Creates a text part.</summary>
    public DecisionModelTextPart(string text)
        : base("text")
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
    }

    /// <summary>Text.</summary>
    public string Text { get; }
}

/// <summary>A file part the model receives. Maps to <c>LanguageModelV4FilePart</c>.</summary>
public sealed class DecisionModelFilePart : DecisionModelStatePart
{
    /// <summary>Creates a file part.</summary>
    public DecisionModelFilePart(string mediaType, LanguageModelFilePart data)
        : base("file")
    {
        MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <summary>Media type. It is a full type after <see cref="DecisionState.PrepareDecisionStateAsync"/>.</summary>
    public string MediaType { get; }

    /// <summary>File data: inline bytes or base64, a URL, a provider reference, or text.</summary>
    public LanguageModelFilePart Data { get; }

    /// <summary>Optional file name.</summary>
    public string? FileName { get; init; }

    /// <summary>Provider-specific options for this part.</summary>
    public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
}

/// <summary>A JSON part the model receives.</summary>
public sealed class DecisionModelJsonPart : DecisionModelStatePart
{
    /// <summary>Creates a JSON part.</summary>
    public DecisionModelJsonPart(JsonElement value)
        : base("json")
    {
        Value = value.Clone();
    }

    /// <summary>The JSON value.</summary>
    public JsonElement Value { get; }
}

/// <summary>
/// Shared state of a decision: text, a JSON object, or ordered parts. Maps to <c>DecisionState</c>.
/// A JSON array is not valid state. Wrap it in an object or in a <see cref="DecisionJsonPart"/>.
/// <see cref="PrepareDecisionStateAsync"/> maps to <c>prepareDecisionState</c>.
/// </summary>
public sealed class DecisionState
{
    private DecisionState(string? text, JsonElement? json, IReadOnlyList<DecisionStatePart>? parts)
    {
        Text = text;
        Json = json;
        Parts = parts;
    }

    /// <summary>The text, when the state is a string.</summary>
    public string? Text { get; }

    /// <summary>The JSON object, when the state is an object.</summary>
    public JsonElement? Json { get; }

    /// <summary>The parts, when the state is a list of parts.</summary>
    public IReadOnlyList<DecisionStatePart>? Parts { get; }

    /// <summary>Creates state from text.</summary>
    public static DecisionState From(string text)
    {
        return new DecisionState(text ?? throw new ArgumentNullException(nameof(text)), null, null);
    }

    /// <summary>Creates state from a JSON object. Any other JSON value throws <see cref="InvalidArgumentException"/>.</summary>
    public static DecisionState FromJson(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidArgumentException("state", value.ValueKind == JsonValueKind.Undefined ? null : value.GetRawText(), InvalidMessage);
        }

        return new DecisionState(null, value.Clone(), null);
    }

    /// <summary>Creates state from ordered parts.</summary>
    public static DecisionState From(IReadOnlyList<DecisionStatePart> parts)
    {
        return new DecisionState(null, null, parts ?? throw new ArgumentNullException(nameof(parts)));
    }

    /// <summary>Converts text to state.</summary>
    public static implicit operator DecisionState(string text)
    {
        return From(text);
    }

    /// <summary>Converts an array of parts to state.</summary>
    public static implicit operator DecisionState(DecisionStatePart[] parts)
    {
        return From(parts);
    }

    internal const string InvalidMessage = "must be a string, JSON object, or array of text, file, or json parts";

    /// <summary>
    /// Turns public state into the list of parts the model receives.
    /// URL files are downloaded. File media types are resolved to a full type.
    /// </summary>
    public static async Task<IReadOnlyList<DecisionModelStatePart>> PrepareDecisionStateAsync(DecisionState state, CancellationToken cancellationToken = default)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (state.Text is { } text)
        {
            return new DecisionModelStatePart[] { new DecisionModelTextPart(text) };
        }

        if (state.Json is { } json)
        {
            return new DecisionModelStatePart[] { new DecisionModelJsonPart(json) };
        }

        var prepared = new List<DecisionModelStatePart>(state.Parts!.Count);
        foreach (var part in state.Parts)
        {
            switch (part)
            {
                case DecisionTextPart textPart:
                    prepared.Add(new DecisionModelTextPart(textPart.Text));
                    break;
                case DecisionJsonPart jsonPart:
                    prepared.Add(new DecisionModelJsonPart(jsonPart.Value));
                    break;
                case DecisionFilePart filePart:
                    prepared.Add(await ConvertFileAsync(filePart, cancellationToken).ConfigureAwait(false));
                    break;
                default:
                    throw new InvalidArgumentException("state", part, InvalidMessage);
            }
        }

        return prepared;
    }

    private static async Task<DecisionModelFilePart> ConvertFileAsync(DecisionFilePart part, CancellationToken cancellationToken)
    {
        var converted = FileParts.ConvertToLanguageModelV4FilePart(part.Data);
        var mediaType = converted.MediaType ?? part.MediaType;
        LanguageModelFilePart data;
        switch (converted.DataType)
        {
            case "data":
                mediaType = MediaTypes.ResolveFullMediaType(new LanguageModelFilePart("data", mediaType) { Bytes = converted.Bytes, Base64 = converted.Base64 });
                data = new LanguageModelFilePart("data", mediaType) { Bytes = converted.Bytes, Base64 = converted.Bytes == null ? converted.Base64 ?? string.Empty : null };
                break;
            case "url":
                var downloaded = await Download.GetAsync(converted.Url!, abortSignal: cancellationToken).ConfigureAwait(false);
                var contentType = downloaded.MediaType?.Split(';')[0].Trim();
                if (!MediaTypes.IsFullMediaType(mediaType) && contentType != null && MediaTypes.IsFullMediaType(contentType) && MediaTypes.GetTopLevelMediaType(contentType) == mediaType)
                {
                    mediaType = contentType;
                }

                mediaType = MediaTypes.ResolveFullMediaType(mediaType, downloaded.Data);
                data = new LanguageModelFilePart("data", mediaType) { Bytes = downloaded.Data };
                break;
            case "reference":
                data = new LanguageModelFilePart("reference", null) { Reference = converted.Reference };
                break;
            default:
                data = new LanguageModelFilePart("text", null) { Text = converted.Text };
                break;
        }

        return new DecisionModelFilePart(mediaType, data) { FileName = part.FileName, ProviderOptions = part.ProviderOptions };
    }
}
