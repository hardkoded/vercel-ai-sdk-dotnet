// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;

namespace Vercel.AI.Tests;

/// <summary>Builds typed decision values from plain CLR trees, the way the upstream tests write them.</summary>
internal static class DecisionFixtures
{
    public static DecisionInput In(object? value)
    {
        return value is string text ? DecisionInput.From(text) : DecisionInput.FromJson(JsonSerializer.SerializeToElement(value));
    }

    private static DecisionInput? Optional(object? value)
    {
        return value == null ? null : In(value);
    }

    public static DecisionCriteria? Criteria(string type, object? criteria)
    {
        switch (type)
        {
            case "choice" when criteria is IDictionary<string, object?> options:
                return new DecisionChoiceCriteria(options.ToDictionary(pair => pair.Key, pair => Optional(pair.Value), StringComparer.Ordinal));
            case "score" when criteria is IEnumerable<object?> levels:
                return new DecisionScoreCriteria(levels.Select(Optional).ToList());
            case "boolean" when criteria is IDictionary<string, object?> descriptions:
                var hasTrue = descriptions.TryGetValue("true", out var whenTrue);
                var hasFalse = descriptions.TryGetValue("false", out var whenFalse);
                return (hasTrue, hasFalse) switch
                {
                    (true, true) => new DecisionBooleanCriteria { True = Optional(whenTrue), False = Optional(whenFalse) },
                    (true, false) => new DecisionBooleanCriteria { True = Optional(whenTrue) },
                    (false, true) => new DecisionBooleanCriteria { False = Optional(whenFalse) },
                    _ => new DecisionBooleanCriteria(),
                };
            default:
                return null;
        }
    }

    public static DecisionQuestion Q(string type, object? instructions, object? criteria = null)
    {
        return new DecisionQuestion(type, Optional(instructions), Criteria(type, criteria));
    }

#pragma warning disable CS0618
    public static EvaluationQuestion EQ(string type, object? instructions, object? criteria = null)
    {
        return new EvaluationQuestion(type, Optional(instructions), Criteria(type, criteria));
    }
#pragma warning restore CS0618

    /// <summary>Public state: a string, a JSON object, or a list of part dictionaries.</summary>
    public static DecisionState S(object? state)
    {
        switch (state)
        {
            case string text:
                return text;
            case IDictionary<string, object?> json:
                return DecisionState.FromJson(JsonSerializer.SerializeToElement(json));
            case IEnumerable<object?> parts:
                return DecisionState.From(parts.Select(Part).ToList());
            default:
                throw new InvalidArgumentException("state", state, DecisionState.InvalidMessage);
        }
    }

    private static DecisionStatePart Part(object? item)
    {
        var part = (IDictionary<string, object?>)item!;
        switch ((string)part["type"]!)
        {
            case "text":
                return new DecisionTextPart((string)part["text"]!);
            case "json":
                return new DecisionJsonPart(JsonSerializer.SerializeToElement(part["value"]));
            default:
                var data = part["data"] switch
                {
                    FilePartInput input => input,
                    byte[] bytes => FilePartInput.FromBytes(bytes),
                    Uri uri => FilePartInput.FromUrl(uri),
                    string text => FilePartInput.FromString(text),
                    _ => throw new InvalidOperationException("Unsupported file data."),
                };
                return new DecisionFilePart((string)part["mediaType"]!, data)
                {
                    FileName = part.TryGetValue("filename", out var name) ? name as string : null,
                };
        }
    }

    /// <summary>Model state from part dictionaries. A file part's <c>data</c> is <c>{ type, data | url | reference | text }</c>.</summary>
    public static IReadOnlyList<DecisionModelStatePart> P(IEnumerable<object?> parts)
    {
        return parts.Select(item =>
        {
            var part = (IDictionary<string, object?>)item!;
            switch ((string)part["type"]!)
            {
                case "text":
                    return (DecisionModelStatePart)new DecisionModelTextPart((string)part["text"]!);
                case "json":
                    return new DecisionModelJsonPart(JsonSerializer.SerializeToElement(part["value"]));
                default:
                    var data = (IDictionary<string, object?>)part["data"]!;
                    var mediaType = (string)part["mediaType"]!;
                    var file = (string)data["type"]! switch
                    {
                        "url" => new LanguageModelFilePart("url", mediaType) { Url = new Uri(data["url"]!.ToString()!) },
                        "reference" => new LanguageModelFilePart("reference", mediaType) { Reference = JsonSerializer.SerializeToElement(data["reference"]).EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetString()!, StringComparer.Ordinal) },
                        "text" => new LanguageModelFilePart("text", mediaType) { Text = (string)data["text"]! },
                        _ => data["data"] is byte[] bytes
                            ? new LanguageModelFilePart("data", mediaType) { Bytes = bytes }
                            : new LanguageModelFilePart("data", mediaType) { Base64 = (string)data["data"]! },
                    };
                    return new DecisionModelFilePart(mediaType, file)
                    {
                        FileName = part.TryGetValue("filename", out var name) ? name as string : null,
                        ProviderOptions = part.TryGetValue("providerOptions", out var options) && options != null
                            ? JsonSerializer.SerializeToElement(options).EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal)
                            : null,
                    };
            }
        }).ToList();
    }

    /// <summary>The state parts as plain dictionaries, in the shape <see cref="P"/> reads.</summary>
    public static List<object?> Canon(IReadOnlyList<DecisionModelStatePart>? parts)
    {
        return parts!.Select(item =>
        {
            switch (item)
            {
                case DecisionModelTextPart text:
                    return (object?)new Dictionary<string, object?> { ["type"] = "text", ["text"] = text.Text };
                case DecisionModelJsonPart json:
                    return new Dictionary<string, object?> { ["type"] = "json", ["value"] = json.Value };
                default:
                    var file = (DecisionModelFilePart)item;
                    var data = new Dictionary<string, object?> { ["type"] = file.Data.DataType };
                    switch (file.Data.DataType)
                    {
                        case "url":
                            data["url"] = file.Data.Url!.ToString();
                            break;
                        case "reference":
                            data["reference"] = file.Data.Reference;
                            break;
                        case "text":
                            data["text"] = file.Data.Text;
                            break;
                        default:
                            data["data"] = (object?)file.Data.Bytes ?? file.Data.Base64;
                            break;
                    }

                    var result = new Dictionary<string, object?> { ["type"] = "file", ["mediaType"] = file.MediaType, ["data"] = data };
                    if (file.FileName != null)
                    {
                        result["filename"] = file.FileName;
                    }

                    if (file.ProviderOptions != null)
                    {
                        result["providerOptions"] = file.ProviderOptions;
                    }

                    return result;
            }
        }).ToList();
    }

    /// <summary>JSON text of a CLR tree, for structural comparison.</summary>
    public static string J(object? value)
    {
        return JsonSerializer.Serialize(value);
    }
}
