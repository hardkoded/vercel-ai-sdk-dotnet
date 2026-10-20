// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Perplexity;

/// <summary>
/// Answers questions through the Perplexity Decisions API (<c>POST {base}/v1/decisions</c>). Boolean questions are sent as
/// native <c>noul</c> questions. Probabilities and scores keep full precision. Provider options are not supported.
/// </summary>
public sealed class PerplexityDecisionModel : IDecisionCaller
{
    private const int MaxQuestions = 128;
    private const int MaxChoices = 255;
    private const int MaxScoreLevels = 10;

    private readonly PerplexityProvider _provider;

    /// <summary>Creates a decision model.</summary>
    public PerplexityDecisionModel(PerplexityProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => "perplexity.decision";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice", "score", "boolean" };

    /// <summary>
    /// Serializes the model id, the endpoint, and the resolved request headers (including authentication) for a workflow boundary.
    /// Maps to <c>[WORKFLOW_SERIALIZE]</c>. The HTTP client is not serialized.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var headers = new Dictionary<string, string>();
        foreach (var pair in _provider.CreateHeaders())
        {
            if (pair.Value != null)
            {
                headers[pair.Key] = pair.Value;
            }
        }

        var config = new Dictionary<string, object?>
        {
            ["provider"] = Provider,
            ["baseURL"] = _provider.Options.BaseUrl,
            ["headers"] = headers,
        };
        return ModelOptionsSerialization.SerializeModelOptions(ModelId, config);
    }

    /// <summary>
    /// Restores a model from <see cref="Serialize"/>. Maps to <c>[WORKFLOW_DESERIALIZE]</c>. Pass <paramref name="httpClient"/> to
    /// send the requests through a custom handler.
    /// </summary>
    public static PerplexityDecisionModel Deserialize(SerializedModelOptions options, HttpClient? httpClient = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var providerOptions = new OpenAICompatibleOptions();
        if (options.Config.TryGetValue("baseURL", out var baseUrl) && AsString(baseUrl) is { } url)
        {
            providerOptions.BaseUrl = url;
        }

        if (options.Config.TryGetValue("headers", out var headers))
        {
            switch (headers)
            {
                case JsonElement { ValueKind: JsonValueKind.Object } element:
                    foreach (var property in element.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            providerOptions.Headers[property.Name] = property.Value.GetString()!;
                        }
                    }

                    break;
                case IDictionary<string, object?> values:
                    foreach (var pair in values)
                    {
                        if (AsString(pair.Value) is { } value)
                        {
                            providerOptions.Headers[pair.Key] = value;
                        }
                    }

                    break;
            }
        }

        return new PerplexityProvider(httpClient ?? new HttpClient(), providerOptions).DecisionModel(options.ModelId);
    }

    /// <summary>Sends every question in one Decisions request.</summary>
    public async Task<DecisionModelResult> DoDecideAsync(DecisionModelCall call, CancellationToken cancellationToken = default)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (call.Questions.Count == 0 || call.Questions.Count > MaxQuestions)
        {
            throw new InvalidArgumentException("questions", call.Questions, "Perplexity Decisions supports 1 to 128 questions per request.");
        }

        var questions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            questions[pair.Key] = BuildQuestion(pair.Key, pair.Value);
        }

        var body = new JsonObject
        {
            ["model"] = ModelId,
            ["state"] = PerplexityDecisionState.Convert(call.State),
            ["questions"] = questions,
        };
        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var response = await _provider.PostJsonAsync(
            ApiKeys.Combine(_provider.Options.BaseUrl, "v1/decisions"),
            body.ToJsonString(),
            _provider.CreateHeaders(headers),
            cancellationToken).ConfigureAwait(false);
        using var document = ParseDocument(response.Body);
        var root = document.RootElement.Clone();
        var parsed = ParseAnswers(root, response.Body, out var modelId, out var usage);
        if (parsed.Count != call.Questions.Count || parsed.Any(answer => !call.Questions.ContainsKey(answer.Name)))
        {
            throw new InvalidResponseDataException(root, "Perplexity Decisions must return exactly one answer for every question.");
        }

        var answers = new Dictionary<string, DecisionAnswer>();
        var confidence = new JsonObject();
        var legend = new JsonObject();
        foreach (var answer in parsed)
        {
            switch (answer.Type)
            {
                case "noul":
                    answers[answer.Name] = new DecisionAnswer("boolean", probability: answer.Probability);
                    break;
                case "choice":
                    answers[answer.Name] = new DecisionAnswer("choice", choice: answer.Choice, probabilities: answer.Probabilities);
                    break;
                default:
                    answers[answer.Name] = new DecisionAnswer("score", score: answer.Score, probabilities: answer.Probabilities);
                    break;
            }

            if (answer.Type != "noul" && answer.Confidence != null)
            {
                confidence[answer.Name] = answer.Confidence.Value;
            }

            if (answer.Type == "score" && answer.Legend != null)
            {
                legend[answer.Name] = JsonNode.Parse(answer.Legend.Value.GetRawText());
            }
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
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty(PerplexityProvider.ProviderId, out var options)
            && options.ValueKind == JsonValueKind.Object)
        {
            foreach (var option in options.EnumerateObject())
            {
                warnings.Add(new OperationWarning("unsupported", feature: "providerOptions.perplexity." + option.Name));
            }
        }

        var metadata = JsonSerializer.SerializeToElement(new JsonObject
        {
            [PerplexityProvider.ProviderId] = new JsonObject { ["confidence"] = confidence, ["legend"] = legend },
        });
        string? requestId = null;
        foreach (var header in response.Headers)
        {
            if (header.Key.Equals("x-request-id", StringComparison.OrdinalIgnoreCase))
            {
                requestId = header.Value;
            }
        }

        // Perplexity returns full-precision probabilities, so no rounding is declared.
        return new DecisionModelResult(answers, warnings, usage, null, metadata, new ProviderResponse(response.Headers, root, id: requestId, modelId: modelId ?? ModelId));
    }

    private static string? AsString(object? value)
    {
        return value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null,
        };
    }

    private JsonObject BuildQuestion(string name, DecisionQuestion question)
    {
        if (name.Length == 0)
        {
            throw new InvalidArgumentException("questions", name, "Perplexity Decisions requires non-empty question names.");
        }

        switch (question.Type)
        {
            case "boolean":
                return Native("noul", question);
            case "choice":
                var count = question.Criteria?.Count ?? 0;
                if (count == 0 || count > MaxChoices)
                {
                    throw new InvalidArgumentException("questions." + name + ".criteria", question.Criteria, "Perplexity Choice questions support 1 to 255 options.");
                }

                return Native("choice", question);
            case "score":
                var levels = ((DecisionScoreCriteria)question.Criteria!).Levels;
                if (levels.Count == 0 || levels.Count > MaxScoreLevels || levels.Any(level => level == null))
                {
                    throw new InvalidArgumentException("questions." + name + ".criteria", question.Criteria, "Perplexity Score questions require 1 to 10 non-null levels.");
                }

                return Native("score", question);
            default:
                throw new DecisionUnsupportedQuestionTypeException(name, question.Type, Provider, ModelId);
        }
    }

    private static JsonObject Native(string type, DecisionQuestion question)
    {
        var native = new JsonObject { ["type"] = type };
        if (question.HasInstructions)
        {
            native["instructions"] = question.Instructions?.ToNode();
        }

        if (question.Criteria != null)
        {
            native["criteria"] = question.Criteria.ToNode();
        }

        return native;
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

    private static List<ParsedAnswer> ParseAnswers(JsonElement root, string body, out string? modelId, out OperationUsage? usage)
    {
        ApiException Malformed(string reason)
        {
            return new ApiException("Type validation failed: " + reason, 200, body);
        }

        double Probability(JsonElement element, string what)
        {
            if (element.ValueKind != JsonValueKind.Number || !element.TryGetDouble(out var value) || value < 0 || value > 1)
            {
                throw Malformed(what + " must be a number from 0 to 1.");
            }

            return value;
        }

        double? NullableProbability(JsonElement parent, string property)
        {
            return parent.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null ? Probability(value, property) : null;
        }

        Dictionary<string, double> Distribution(JsonElement parent)
        {
            if (!parent.TryGetProperty("probabilities", out var map) || map.ValueKind != JsonValueKind.Object)
            {
                throw Malformed("probabilities must be an object.");
            }

            var result = new Dictionary<string, double>();
            foreach (var item in map.EnumerateObject())
            {
                result[item.Name] = Probability(item.Value, "probabilities");
            }

            return result;
        }

        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("answers", out var list) || list.ValueKind != JsonValueKind.Object)
        {
            throw Malformed("answers must be an object.");
        }

        var answers = new List<ParsedAnswer>();
        foreach (var item in list.EnumerateObject())
        {
            var element = item.Value;
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
            {
                throw Malformed("answer type must be noul, choice, or score.");
            }

            var answer = new ParsedAnswer { Name = item.Name, Type = type.GetString()! };
            switch (answer.Type)
            {
                case "noul":
                    answer.Probability = element.TryGetProperty("noul", out var noul) ? Probability(noul, "noul") : throw Malformed("noul is required.");
                    break;
                case "choice":
                    if (!element.TryGetProperty("choice", out var choice) || choice.ValueKind != JsonValueKind.String)
                    {
                        throw Malformed("choice must be a string.");
                    }

                    answer.Choice = choice.GetString();
                    answer.Probabilities = Distribution(element);
                    answer.Confidence = NullableProbability(element, "confidence");
                    break;
                case "score":
                    if (!element.TryGetProperty("score", out var score) || score.ValueKind != JsonValueKind.Number || !score.TryGetDouble(out var scoreValue))
                    {
                        throw Malformed("score must be a number.");
                    }

                    answer.Score = scoreValue;
                    answer.Probabilities = Distribution(element);
                    answer.Confidence = NullableProbability(element, "confidence");
                    if (element.TryGetProperty("legend", out var legend) && legend.ValueKind != JsonValueKind.Null)
                    {
                        if (legend.ValueKind != JsonValueKind.Object)
                        {
                            throw Malformed("legend must be an object.");
                        }

                        answer.Legend = legend;
                    }

                    break;
                default:
                    throw Malformed("answer type must be noul, choice, or score.");
            }

            answers.Add(answer);
        }

        modelId = root.TryGetProperty("model", out var model) && model.ValueKind == JsonValueKind.String ? model.GetString() : null;
        usage = null;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            usage = new OperationUsage(Count(usageElement, "input_tokens"), Count(usageElement, "output_tokens"));
        }

        return answers;
    }

    private static int? Count(JsonElement usage, string name)
    {
        return usage.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var count) ? count : null;
    }

    private sealed class ParsedAnswer
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public double? Probability { get; set; }

        public string? Choice { get; set; }

        public double? Score { get; set; }

        public Dictionary<string, double>? Probabilities { get; set; }

        public double? Confidence { get; set; }

        public JsonElement? Legend { get; set; }
    }
}
