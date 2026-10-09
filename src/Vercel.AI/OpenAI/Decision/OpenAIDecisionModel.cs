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

namespace Vercel.AI.OpenAI;

/// <summary>Answers questions through the OpenAI Decisions API (<c>POST /decisions</c>).</summary>
public sealed class OpenAIDecisionModel : IEvaluationCaller
{
    private const int MaxSafetyIdentifierLength = 128;

    private static readonly JsonSerializerOptions Text = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly OpenAIProvider _provider;

    /// <summary>Creates a decision model.</summary>
    public OpenAIDecisionModel(OpenAIProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => _provider.Name + ".decision";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice", "score", "boolean" };

    /// <inheritdoc />
    public Task<EvaluationModelResult> DoEvaluateAsync(EvaluationModelCall call, CancellationToken cancellationToken)
    {
        return DoDecideAsync(call, cancellationToken);
    }

    /// <summary>Sends every question in one Decisions request.</summary>
    public async Task<EvaluationModelResult> DoDecideAsync(EvaluationModelCall call, CancellationToken cancellationToken = default)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        string? safetyIdentifier = null;
        var warnings = new List<OperationWarning>();
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object && call.ProviderOptions.TryGetProperty("openai", out var openai) && openai.ValueKind == JsonValueKind.Object)
        {
            foreach (var option in openai.EnumerateObject())
            {
                if (option.Name == "safetyIdentifier")
                {
                    if (option.Value.ValueKind != JsonValueKind.String || option.Value.GetString()!.Length > MaxSafetyIdentifierLength)
                    {
                        throw new InvalidArgumentException("providerOptions", option.Value.ToString(), "invalid openai provider options: safetyIdentifier must be a string of at most " + MaxSafetyIdentifierLength.ToString(CultureInfo.InvariantCulture) + " characters");
                    }

                    safetyIdentifier = option.Value.GetString();
                }
                else
                {
                    warnings.Add(new OperationWarning("unsupported", feature: "providerOptions.openai." + option.Name));
                }
            }
        }

        var body = new JsonObject { ["model"] = ModelId };
        if (safetyIdentifier != null)
        {
            body["safety_identifier"] = safetyIdentifier;
        }

        body["input"] = ToText(call.State);
        var questions = new JsonArray();
        foreach (var pair in call.Questions)
        {
            questions.Add(BuildQuestion(pair.Key, pair.Value));
        }

        body["questions"] = questions;
        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            ApiKeys.Combine(_provider.Options.BaseUrl, "decisions"),
            body.ToJsonString(Text),
            _provider.CreateOpenAIHeaders(headers),
            cancellationToken).ConfigureAwait(false);
        using var document = ParseDocument(response.Body);
        var root = document.RootElement.Clone();
        var parsed = ParseAnswers(root, response.Body);

        var answers = new Dictionary<string, EvaluationAnswer>();
        foreach (var answer in parsed)
        {
            if (answer.Type == "refusal")
            {
                if (answer.Name == null)
                {
                    throw new InvalidResponseDataException(root, "OpenAI Decisions refused an unnamed question.");
                }

                answers[answer.Name] = new EvaluationAnswer("refusal");
                continue;
            }

            if (answer.Type == "predicate")
            {
                answers[answer.Name!] = new EvaluationAnswer("boolean", probability: answer.Probability);
                continue;
            }

            var probabilities = new Dictionary<string, double>();
            foreach (var item in answer.Probabilities!)
            {
                if (probabilities.ContainsKey(item.Key))
                {
                    throw new InvalidResponseDataException(root, "Decisions returned duplicate probability values.");
                }

                probabilities[item.Key] = item.Value;
            }

            answers[answer.Name!] = answer.Type == "choice"
                ? new EvaluationAnswer("choice", choice: answer.Choice, probabilities: probabilities)
                : new EvaluationAnswer("score", score: answer.Score, probabilities: probabilities);
        }

        var names = parsed.Select(answer => answer.Name).ToArray();
        if (names.Length != call.Questions.Count || names.Distinct().Count() != names.Length || names.Any(name => name == null || !call.Questions.ContainsKey(name)))
        {
            throw new InvalidResponseDataException(root, "Decisions must return exactly one answer for every question.");
        }

        OperationUsage? usage = null;
        JsonElement? rawUsage = null;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            rawUsage = usageElement;
            usage = new OperationUsage(Count(usageElement, "input_tokens"), Count(usageElement, "output_tokens"));
        }

        var confidence = new JsonObject();
        foreach (var answer in parsed)
        {
            if ((answer.Type == "choice" || answer.Type == "score") && answer.Confidence != null)
            {
                confidence[answer.Name!] = answer.Confidence.Value;
            }
        }

        var metadata = new JsonObject();
        if (rawUsage != null)
        {
            metadata["usage"] = JsonNode.Parse(rawUsage.Value.GetRawText());
        }

        metadata["confidence"] = confidence;
        var providerMetadata = JsonSerializer.SerializeToElement(new JsonObject { ["openai"] = metadata });
        string? modelId = null;
        if (root.TryGetProperty("model", out var modelElement) && modelElement.ValueKind == JsonValueKind.String)
        {
            modelId = modelElement.GetString();
        }

        return new EvaluationModelResult(
            answers,
            warnings,
            usage,
            // The Decisions API reports probabilities and scores to two decimal places.
            new EvaluationRounding(2, 2),
            providerMetadata,
            new ProviderResponse(response.Headers, root, modelId: modelId ?? ModelId));
    }

    private static int? Count(JsonElement usage, string name)
    {
        if (usage.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var count))
        {
            return count;
        }

        return null;
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

    private static string ToText(object? value)
    {
        return value as string ?? JsonSerializer.Serialize(value, Text);
    }

    private static JsonObject BuildQuestion(string name, EvaluationQuestion question)
    {
        var instructions = ToText(question.Instructions);
        switch (question.Type)
        {
            case "boolean":
                var parts = new List<string> { instructions };
                var criteria = question.Criteria as IDictionary<string, object?>;
                if (criteria != null && criteria.TryGetValue("true", out var whenTrue) && whenTrue != null)
                {
                    parts.Add("Criteria for true:\n" + ToText(whenTrue));
                }

                if (criteria != null && criteria.TryGetValue("false", out var whenFalse) && whenFalse != null)
                {
                    parts.Add("Criteria for false:\n" + ToText(whenFalse));
                }

                return new JsonObject { ["type"] = "predicate", ["name"] = name, ["instructions"] = string.Join("\n\n", parts) };
            case "choice":
                var choices = new JsonArray();
                foreach (var option in (IDictionary<string, object?>)question.Criteria!)
                {
                    var choice = new JsonObject { ["value"] = option.Key };
                    if (option.Value != null)
                    {
                        choice["description"] = ToText(option.Value);
                    }

                    choices.Add(choice);
                }

                return new JsonObject { ["type"] = "choice", ["name"] = name, ["instructions"] = instructions, ["choices"] = choices };
            default:
                var levels = new JsonArray();
                var index = 0;
                foreach (var description in (IList<object?>)question.Criteria!)
                {
                    // Score criteria have no separate labels; indices identify each level.
                    var level = new JsonObject { ["label"] = index.ToString(CultureInfo.InvariantCulture) };
                    if (description != null)
                    {
                        level["description"] = ToText(description);
                    }

                    levels.Add(level);
                    index++;
                }

                return new JsonObject { ["type"] = "score", ["name"] = name, ["instructions"] = instructions, ["levels"] = levels };
        }
    }

    private static List<ParsedAnswer> ParseAnswers(JsonElement root, string body)
    {
        ApiException Malformed(string reason)
        {
            return new ApiException("Type validation failed: " + reason, 200, body);
        }

        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("answers", out var list) || list.ValueKind != JsonValueKind.Array)
        {
            throw Malformed("answers must be an array.");
        }

        double Probability(JsonElement element, string property)
        {
            if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Number || value.GetDouble() < 0 || value.GetDouble() > 1)
            {
                throw Malformed(property + " must be a number between 0 and 1.");
            }

            return value.GetDouble();
        }

        var result = new List<ParsedAnswer>();
        foreach (var element in list.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            {
                throw Malformed("answer type is required.");
            }

            var type = typeElement.GetString()!;
            string? name = null;
            if (element.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                name = nameElement.GetString();
            }
            else if (type != "refusal" || !element.TryGetProperty("name", out nameElement) || nameElement.ValueKind != JsonValueKind.Null)
            {
                throw Malformed("answer name must be a string.");
            }

            var answer = new ParsedAnswer { Type = type, Name = name };
            switch (type)
            {
                case "refusal":
                    break;
                case "predicate":
                    answer.Probability = Probability(element, "probability");
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
                    }

                    if (element.TryGetProperty("confidence", out var confidence) && confidence.ValueKind == JsonValueKind.Number)
                    {
                        answer.Confidence = Probability(element, "confidence");
                    }

                    if (!element.TryGetProperty("probabilities", out var probabilities) || probabilities.ValueKind != JsonValueKind.Array)
                    {
                        throw Malformed("probabilities must be an array.");
                    }

                    answer.Probabilities = new List<KeyValuePair<string, double>>();
                    foreach (var item in probabilities.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object || !item.TryGetProperty("value", out var value))
                        {
                            throw Malformed("probability value is required.");
                        }

                        string key;
                        if (type == "choice" && value.ValueKind == JsonValueKind.String)
                        {
                            key = value.GetString()!;
                        }
                        else if (type == "score" && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var level) && level >= 0)
                        {
                            key = level.ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            throw Malformed("probability value has the wrong type.");
                        }

                        answer.Probabilities.Add(new KeyValuePair<string, double>(key, Probability(item, "probability")));
                    }

                    break;
                default:
                    throw Malformed("unknown answer type.");
            }

            result.Add(answer);
        }

        return result;
    }

    private sealed class ParsedAnswer
    {
        public string Type { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Choice { get; set; }

        public double? Score { get; set; }

        public double? Probability { get; set; }

        public double? Confidence { get; set; }

        public List<KeyValuePair<string, double>>? Probabilities { get; set; }
    }
}
