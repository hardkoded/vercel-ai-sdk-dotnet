// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Prompt;

namespace Vercel.AI.OpenTelemetry;

/// <summary>
/// Maps to <c>stringify-for-telemetry.ts</c>. It also holds the <c>JSON.stringify</c> equivalents the port needs for decision and
/// extraction values, which are plain objects upstream.
/// </summary>
internal static class StringifyForTelemetry
{
    /// <summary>Serializes decision files like prompt files. Maps to <c>stringifyDecisionStateForTelemetry</c>.</summary>
    public static string StringifyDecisionStateForTelemetry(DecisionState state)
    {
        if (state.Parts is { } parts)
        {
            var array = new JsonArray();
            foreach (var part in parts)
            {
                array.Add(
                    part switch
                    {
                        DecisionTextPart text => new JsonObject
                        {
                            ["type"] = "text",
                            ["text"] = text.Text,
                        },
                        DecisionFilePart file => FileNode(
                            file.MediaType,
                            file.FileName,
                            SerializeFileData(file.Data)
                        ),
                        DecisionJsonPart json => new JsonObject
                        {
                            ["type"] = "json",
                            ["value"] = JsonNode.Parse(json.Value.GetRawText()),
                        },
                        _ => new JsonObject { ["type"] = part.Type },
                    }
                );
            }

            return array.ToJsonString();
        }

        if (state.Json is { } json2)
        {
            return json2.GetRawText();
        }

        return JsonSerializer.Serialize(state.Text);
    }

    /// <summary>Serializes the state a model call receives. Maps to <c>stringifyDecisionStateForTelemetry</c> on model parts.</summary>
    public static string StringifyDecisionModelState(IReadOnlyList<DecisionModelStatePart> parts)
    {
        var array = new JsonArray();
        foreach (var part in parts)
        {
            array.Add(
                part switch
                {
                    DecisionModelTextPart text => new JsonObject
                    {
                        ["type"] = "text",
                        ["text"] = text.Text,
                    },
                    DecisionModelFilePart file => FileNode(
                        file.MediaType,
                        file.FileName,
                        SerializeFileData(file.Data)
                    ),
                    DecisionModelJsonPart json => new JsonObject
                    {
                        ["type"] = "json",
                        ["value"] = JsonNode.Parse(json.Value.GetRawText()),
                    },
                    _ => new JsonObject { ["type"] = part.Type },
                }
            );
        }

        return array.ToJsonString();
    }

    /// <summary>Serializes decision questions, the equivalent of <c>JSON.stringify(event.questions)</c>.</summary>
    public static string StringifyQuestions(IReadOnlyDictionary<string, DecisionQuestion> questions)
    {
        var result = new JsonObject();
        foreach (var pair in questions)
        {
            var question = pair.Value;
            var node = new JsonObject { ["type"] = question.Type };
            if (question.HasInstructions)
            {
                node["instructions"] = question.Instructions is null
                    ? null
                    : JsonNode.Parse(question.Instructions.Value.GetRawText());
            }

            if (question.HasCriteria)
            {
                node["criteria"] = question.Criteria?.ToNode();
            }

            result[pair.Key] = node;
        }

        return result.ToJsonString();
    }

    /// <summary>Serializes decision answers, the equivalent of <c>JSON.stringify(event.answers)</c>.</summary>
    public static string StringifyDecisionAnswers(
        IReadOnlyDictionary<string, DecisionAnswer> answers
    )
    {
        var result = new JsonObject();
        foreach (var pair in answers)
        {
            var answer = pair.Value;
            var node = new JsonObject { ["type"] = answer.Type };
            if (answer.Choice is not null)
            {
                node["choice"] = answer.Choice;
            }

            if (answer.Score is not null)
            {
                node["score"] = answer.Score;
            }

            if (answer.Probability is not null)
            {
                node["probability"] = answer.Probability;
            }

            if (answer.Probabilities is not null)
            {
                var probabilities = new JsonObject();
                foreach (var entry in answer.Probabilities)
                {
                    probabilities[entry.Key] = entry.Value;
                }

                node["probabilities"] = probabilities;
            }

            if (answer.Box is { } box)
            {
                node["box"] = new JsonObject
                {
                    ["top"] = box.Top,
                    ["bottom"] = box.Bottom,
                    ["left"] = box.Left,
                    ["right"] = box.Right,
                };
            }

            result[pair.Key] = node;
        }

        return result.ToJsonString();
    }

    /// <summary>Serializes extraction fields, the equivalent of <c>JSON.stringify(event.fields)</c>.</summary>
    public static string StringifyFields(IReadOnlyDictionary<string, ExtractionField> fields)
    {
        var result = new JsonObject();
        foreach (var pair in fields)
        {
            var field = pair.Value;
            var node = new JsonObject { ["type"] = field.Type };
            if (field.Instructions is not null)
            {
                node["instructions"] = JsonNode.Parse(field.Instructions.Value.GetRawText());
            }

            if (field.Unit is not null)
            {
                node["unit"] = field.Unit;
            }

            if (field.Items is not null)
            {
                node["items"] = field.Items;
            }

            if (field.Integer is not null)
            {
                node["integer"] = field.Integer;
            }

            if (field.Decimals is not null)
            {
                node["decimals"] = field.Decimals;
            }

            result[pair.Key] = node;
        }

        return result.ToJsonString();
    }

    /// <summary>Serializes extraction answers, the equivalent of <c>JSON.stringify(event.answers)</c>.</summary>
    public static string StringifyExtractionAnswers(
        IReadOnlyDictionary<string, ExtractionAnswer> answers
    )
    {
        var result = new JsonObject();
        foreach (var pair in answers)
        {
            var answer = pair.Value;
            var node = new JsonObject { ["type"] = answer.Type };
            if (answer.Type != "refusal")
            {
                node["value"] =
                    answer.TextItems is not null
                        ? new JsonArray([.. answer.TextItems.Select(item => (JsonNode?)item)])
                    : answer.NumberItems is not null
                        ? new JsonArray([.. answer.NumberItems.Select(item => (JsonNode?)item)])
                    : answer.Text is not null ? answer.Text
                    : answer.Number is not null ? answer.Number
                    : null;
                if (answer.GuessText is not null)
                {
                    node["guess"] = answer.GuessText;
                }
                else if (answer.GuessNumber is not null)
                {
                    node["guess"] = answer.GuessNumber;
                }

                if (answer.PNull is not null)
                {
                    node["pNull"] = answer.PNull;
                }

                if (answer.Confidence is not null)
                {
                    node["confidence"] = answer.Confidence;
                }

                if (answer.Source is { } source)
                {
                    node["source"] = SourceNode(source);
                }

                if (answer.Items is { } items)
                {
                    var array = new JsonArray();
                    foreach (var item in items)
                    {
                        var itemNode = new JsonObject();
                        if (item.Confidence is not null)
                        {
                            itemNode["confidence"] = item.Confidence;
                        }

                        if (item.Source is { } itemSource)
                        {
                            itemNode["source"] = SourceNode(itemSource);
                        }

                        array.Add(itemNode);
                    }

                    node["items"] = array;
                }
            }

            result[pair.Key] = node;
        }

        return result.ToJsonString();
    }

    private static JsonObject SourceNode(ExtractionSource source)
    {
        var node = new JsonObject { ["type"] = source.Type, ["part"] = source.Part };
        if (source.Start is not null)
        {
            node["start"] = source.Start;
        }

        if (source.End is not null)
        {
            node["end"] = source.End;
        }

        return node;
    }

    private static JsonObject FileNode(string mediaType, string? fileName, JsonNode? data)
    {
        var node = new JsonObject { ["type"] = "file", ["mediaType"] = mediaType };
        if (fileName is not null)
        {
            node["filename"] = fileName;
        }

        node["data"] = data;
        return node;
    }

    /// <summary>Maps to <c>serializeFileData</c>.</summary>
    private static JsonNode? SerializeFileData(FilePartInput data)
    {
        if (data.Bytes is { } bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        if (data.Url is { } url)
        {
            return data.UrlText ?? url.ToString();
        }

        if (data.Reference is { } reference)
        {
            var node = new JsonObject();
            foreach (var pair in reference)
            {
                node[pair.Key] = pair.Value;
            }

            return node;
        }

        return data.Text ?? data.TextData;
    }

    private static JsonNode? SerializeFileData(LanguageModelFilePart data)
    {
        if (data.Bytes is { } bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        if (data.Base64 is { } base64)
        {
            return base64;
        }

        if (data.Url is { } url)
        {
            return data.OriginalUrl ?? url.ToString();
        }

        if (data.Reference is { } reference)
        {
            var node = new JsonObject();
            foreach (var pair in reference)
            {
                node[pair.Key] = pair.Value;
            }

            return node;
        }

        return data.Text;
    }
}
