// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Google;

/// <summary>Answers questions through the Gemini Decisions API (<c>POST /decisions</c>). Maps to <c>DecisionGoogleModel</c>.</summary>
#pragma warning disable CS0618
public sealed class GoogleDecisionModel : IEvaluationDecisionCaller
#pragma warning restore CS0618
{
    private static readonly JsonSerializerOptions Text = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static readonly string[] MediaKinds = { "image", "audio", "video", "document" };

    private readonly GoogleProvider _provider;

    /// <summary>Creates a decision model. <paramref name="providerId"/> is the model provider id, such as <c>google.decision</c>.</summary>
    public GoogleDecisionModel(GoogleProvider provider, string modelId, string providerId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        Provider = providerId ?? throw new ArgumentNullException(nameof(providerId));
    }

    /// <inheritdoc />
    public string Provider { get; }

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice", "score", "boolean" };

    /// <summary>Deprecated. Use <see cref="DoDecideAsync(DecisionModelCall, CancellationToken)"/>.</summary>
    [Obsolete("Use DoDecideAsync instead.")]
    public async Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
    {
        return EvaluationModelResult.From(await DoDecideAsync(call, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Sends every question in one Decisions request.</summary>
    public async Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken = default)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (call.Questions.Count == 0)
        {
            throw new InvalidArgumentException("questions", call.Questions, "Google Decisions requires at least one question.");
        }

        var questions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            questions[pair.Key] = BuildQuestion(pair.Key, pair.Value, Provider, ModelId);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["input"] = BuildInput(call.State),
            ["questions"] = questions,
        };

        var headers = await _provider.HeadersAsync(cancellationToken).ConfigureAwait(false);
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "decisions"),
            body.ToJsonString(Text),
            headers,
            cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        using var document = ParseDocument(response.Body);
        var root = document.RootElement.Clone();
        var parsed = ParseResponse(root, response.Body);

        var blocked = parsed.PromptFeedback != null && parsed.PromptFeedback.Value.TryGetProperty("block_reason", out var reason) && reason.ValueKind == JsonValueKind.String;
        if (!blocked && (parsed.Answers == null || parsed.Answers.Count != call.Questions.Count || call.Questions.Keys.Any(id => !parsed.Answers.ContainsKey(id))))
        {
            throw new InvalidResponseDataException(root, "Google Decisions must return exactly one answer for every question.");
        }

        var answers = new Dictionary<string, DecisionAnswer>();
        foreach (var pair in call.Questions)
        {
            if (blocked)
            {
                answers[pair.Key] = new DecisionAnswer("refusal");
                continue;
            }

            var answer = parsed.Answers![pair.Key];
            var question = pair.Value;
            if (question.Type == "boolean" && answer.Type == "noul")
            {
                answers[pair.Key] = new DecisionAnswer("boolean", probability: answer.Probability);
            }
            else if (question.Type == "choice" && answer.Type == "choice" && ((DecisionChoiceCriteria)question.Criteria!).Options.ContainsKey(answer.Choice!))
            {
                answers[pair.Key] = new DecisionAnswer("choice", choice: answer.Choice, probabilities: answer.Probabilities);
            }
            else if (question.Type == "score" && answer.Type == "score" && answer.Score >= 0 && answer.Score <= ((DecisionScoreCriteria)question.Criteria!).Levels.Count - 1)
            {
                answers[pair.Key] = new DecisionAnswer("score", score: answer.Score, probabilities: answer.Probabilities);
            }
            else
            {
                throw new InvalidResponseDataException(root, "Google Decisions returned an invalid answer for question \"" + pair.Key + "\".");
            }
        }

        OperationUsage? usage = null;
        if (parsed.Usage != null)
        {
            usage = new OperationUsage(Count(parsed.Usage.Value, "total_input_tokens"), Count(parsed.Usage.Value, "total_output_tokens"));
        }

        var warnings = new List<OperationWarning>();
        if (call.MaxOutputTokens != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "maxOutputTokens"));
        }

        if (call.Reasoning != null)
        {
            warnings.Add(new OperationWarning("unsupported", feature: "reasoning"));
        }

        if (call.ProviderOptions.ValueKind == JsonValueKind.Object && call.ProviderOptions.TryGetProperty("google", out var google) && google.ValueKind == JsonValueKind.Object)
        {
            foreach (var option in google.EnumerateObject())
            {
                warnings.Add(new OperationWarning("unsupported", feature: "providerOptions.google." + option.Name));
            }
        }

        var metadata = new JsonObject();
        if (parsed.Usage != null)
        {
            metadata["usage"] = JsonNode.Parse(parsed.Usage.Value.GetRawText());
        }

        if (parsed.PromptFeedback != null)
        {
            metadata["promptFeedback"] = JsonNode.Parse(parsed.PromptFeedback.Value.GetRawText());
        }

        var confidence = new JsonObject();
        var legend = new JsonObject();
        foreach (var pair in parsed.Answers ?? new Dictionary<string, ParsedAnswer>())
        {
            if (pair.Value.Confidence != null)
            {
                confidence[pair.Key] = pair.Value.Confidence.Value;
            }

            if (pair.Value.Type == "score" && pair.Value.Legend != null)
            {
                legend[pair.Key] = JsonNode.Parse(pair.Value.Legend.Value.GetRawText());
            }
        }

        metadata["confidence"] = confidence;
        metadata["legend"] = legend;
        var providerMetadata = JsonSerializer.SerializeToElement(new JsonObject { ["google"] = metadata });
        return new DecisionModelResult(
            answers,
            warnings,
            usage,
            null,
            providerMetadata,
            new ProviderResponse(response.Headers, root, modelId: ModelId));
    }

    private static int? Count(JsonElement usage, string name)
    {
        return usage.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var count) ? count : null;
    }

    private static JsonDocument ParseDocument(string body)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
        }
        catch (JsonException exception)
        {
            throw new ApiException("Invalid JSON response", 200, body, exception);
        }
    }

    private static string ToText(DecisionInput input)
    {
        return input.Value.ValueKind == JsonValueKind.String ? input.Value.GetString()! : JsonSerializer.Serialize(input.Value, Text);
    }

    private static JsonArray BuildInput(IReadOnlyList<DecisionModelStatePart> state)
    {
        var input = new JsonArray();
        foreach (var part in state)
        {
            if (part is DecisionModelTextPart text)
            {
                input.Add(new JsonObject { ["type"] = "text", ["text"] = text.Text });
                continue;
            }

            if (part is DecisionModelJsonPart json)
            {
                input.Add(new JsonObject { ["type"] = "text", ["text"] = JsonSerializer.Serialize(json.Value, Text) });
                continue;
            }

            var file = (DecisionModelFilePart)part;
            var mediaType = file.MediaType;
            var kind = mediaType == "application/pdf" ? "document" : mediaType.Split('/')[0];
            if (!MediaKinds.Contains(kind))
            {
                throw new UnsupportedFunctionalityException("Google decision file media type", "'Google decision file media type: " + mediaType + "' functionality not supported.");
            }

            var dataType = file.Data.DataType;
            if (dataType != "data" && dataType != "url")
            {
                throw new UnsupportedFunctionalityException("Google decision file input", "'Google decision file input: " + dataType + "' functionality not supported.");
            }

            var item = new JsonObject { ["type"] = kind, ["mime_type"] = mediaType };
            if (dataType == "url")
            {
                item["uri"] = file.Data.Url!.ToString();
            }
            else
            {
                item["data"] = file.Data.Bytes != null ? Convert.ToBase64String(file.Data.Bytes) : file.Data.Base64 ?? string.Empty;
            }

            input.Add(item);
        }

        return input;
    }

    private static JsonObject BuildQuestion(string id, DecisionQuestion question, string provider, string modelId)
    {
        if (question.Type == "box")
        {
#pragma warning disable CS0618
            throw new EvaluationUnsupportedQuestionTypeException(id, question.Type, provider, modelId);
#pragma warning restore CS0618
        }

        if (question.Instructions == null)
        {
            throw new InvalidArgumentException("questions." + id + ".instructions", null, "Google Decisions requires instructions for every question.");
        }

        var instructions = ToText(question.Instructions);
        if (question.Type == "boolean")
        {
            var result = new JsonObject { ["type"] = "noul", ["instructions"] = instructions };
            if (question.Criteria is DecisionBooleanCriteria criteria)
            {
                var descriptions = new JsonObject();
                if (criteria.True != null)
                {
                    descriptions["true_description"] = ToText(criteria.True);
                }

                if (criteria.False != null)
                {
                    descriptions["false_description"] = ToText(criteria.False);
                }

                result["criteria"] = descriptions;
            }

            return result;
        }

        var count = question.Criteria?.Count ?? 0;
        if (count < 2 || count > 255)
        {
            throw new InvalidArgumentException("questions." + id + ".criteria", null, "Google Decisions requires 2 to 255 options or score levels.");
        }

        if (question.Type == "choice")
        {
            var options = new JsonObject();
            foreach (var option in ((DecisionChoiceCriteria)question.Criteria!).Options)
            {
                options[option.Key] = option.Value == null ? null : ToText(option.Value);
            }

            return new JsonObject { ["type"] = "choice", ["instructions"] = instructions, ["criteria"] = options };
        }

        var levels = new JsonArray();
        foreach (var level in ((DecisionScoreCriteria)question.Criteria!).Levels)
        {
            if (level == null || (level.Value.ValueKind == JsonValueKind.String && level.Value.GetString() == string.Empty))
            {
                throw new InvalidArgumentException("questions." + id + ".criteria", null, "Google Decisions requires non-empty score level descriptions.");
            }

            levels.Add(ToText(level));
        }

        return new JsonObject { ["type"] = "score", ["instructions"] = instructions, ["criteria"] = levels };
    }

    private static ParsedResponse ParseResponse(JsonElement root, string body)
    {
        ApiException Malformed(string reason)
        {
            return new ApiException("Type validation failed: " + reason, 200, body);
        }

        double Probability(JsonElement element, string property)
        {
            if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Number || value.GetDouble() < 0 || value.GetDouble() > 1)
            {
                throw Malformed(property + " must be a number between 0 and 1.");
            }

            return value.GetDouble();
        }

        static bool Present(JsonElement element, string property, out JsonElement value)
        {
            return element.TryGetProperty(property, out value) && value.ValueKind != JsonValueKind.Null;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw Malformed("response must be an object.");
        }

        var result = new ParsedResponse();
        if (Present(root, "answers", out var answers))
        {
            if (answers.ValueKind != JsonValueKind.Object)
            {
                throw Malformed("answers must be an object.");
            }

            result.Answers = new Dictionary<string, ParsedAnswer>(StringComparer.Ordinal);
            foreach (var property in answers.EnumerateObject())
            {
                var element = property.Value;
                if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
                {
                    throw Malformed("answer type is required.");
                }

                var type = typeElement.GetString()!;
                var answer = new ParsedAnswer { Type = type };
                switch (type)
                {
                    case "noul":
                        answer.Probability = Probability(element, "noul");
                        break;
                    case "choice":
                    case "score":
                        if (type == "choice")
                        {
                            if (!element.TryGetProperty("choice", out var choice) || choice.ValueKind != JsonValueKind.String)
                            {
                                throw Malformed("choice must be a string.");
                            }

                            answer.Choice = choice.GetString();
                        }
                        else
                        {
                            if (!element.TryGetProperty("score", out var score) || score.ValueKind != JsonValueKind.Number)
                            {
                                throw Malformed("score must be a number.");
                            }

                            answer.Score = score.GetDouble();
                            if (Present(element, "legend", out var legend))
                            {
                                if (legend.ValueKind != JsonValueKind.Object || legend.EnumerateObject().Any(entry => entry.Value.ValueKind != JsonValueKind.String))
                                {
                                    throw Malformed("legend must be an object of strings.");
                                }

                                answer.Legend = legend.Clone();
                            }
                        }

                        if (!element.TryGetProperty("probabilities", out var probabilities) || probabilities.ValueKind != JsonValueKind.Object)
                        {
                            throw Malformed("probabilities must be an object.");
                        }

                        answer.Probabilities = new Dictionary<string, double>(StringComparer.Ordinal);
                        foreach (var item in probabilities.EnumerateObject())
                        {
                            answer.Probabilities[item.Name] = Probability(probabilities, item.Name);
                        }

                        break;
                    default:
                        throw Malformed("unknown answer type.");
                }

                if (Present(element, "confidence", out _))
                {
                    answer.Confidence = Probability(element, "confidence");
                }

                result.Answers[property.Name] = answer;
            }
        }

        if (Present(root, "usage", out var usage))
        {
            if (usage.ValueKind != JsonValueKind.Object)
            {
                throw Malformed("usage must be an object.");
            }

            result.Usage = usage.Clone();
        }

        if (Present(root, "prompt_feedback", out var feedback))
        {
            if (feedback.ValueKind != JsonValueKind.Object)
            {
                throw Malformed("prompt_feedback must be an object.");
            }

            result.PromptFeedback = feedback.Clone();
        }

        return result;
    }

    private sealed class ParsedResponse
    {
        public Dictionary<string, ParsedAnswer>? Answers { get; set; }

        public JsonElement? Usage { get; set; }

        public JsonElement? PromptFeedback { get; set; }
    }

    private sealed class ParsedAnswer
    {
        public string Type { get; set; } = string.Empty;

        public string? Choice { get; set; }

        public double? Score { get; set; }

        public double? Probability { get; set; }

        public double? Confidence { get; set; }

        public JsonElement? Legend { get; set; }

        public Dictionary<string, double>? Probabilities { get; set; }
    }
}
