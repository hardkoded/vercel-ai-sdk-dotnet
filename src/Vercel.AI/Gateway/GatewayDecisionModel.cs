// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Gateway;

/// <summary>Answers questions through the Gateway <c>/decision-model</c> route.</summary>
public sealed class GatewayDecisionModel : IEvaluationCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates a decision model.</summary>
    public GatewayDecisionModel(GatewayProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => GatewayProvider.ProviderId;

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

    /// <summary>Sends the state and every question in one request.</summary>
    public async Task<EvaluationModelResult> DoDecideAsync(EvaluationModelCall call, CancellationToken cancellationToken = default)
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        GatewayDecisionProviderOptions.Validate(call.ProviderOptions);
        var questions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            var question = new JsonObject
            {
                ["type"] = pair.Value.Type,
                ["instructions"] = ToNode(pair.Value.Instructions),
            };
            if (pair.Value.Criteria != null)
            {
                question["criteria"] = ToNode(pair.Value.Criteria);
            }

            questions[pair.Key] = question;
        }

        var body = new JsonObject { ["state"] = ToNode(call.State), ["questions"] = questions };
        if (call.ProviderOptions.ValueKind == JsonValueKind.Object && call.ProviderOptions.EnumerateObject().Any())
        {
            body["providerOptions"] = JsonNode.Parse(call.ProviderOptions.GetRawText());
        }

        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        ProviderTextResponse response;
        try
        {
            response = await _provider.Http.SendJsonStringAsync(
                HttpMethod.Post,
                _provider.Route("decision-model"),
                body.ToJsonString(),
                _provider.Headers("ai-decision-model-specification-version", "4", "ai-model-id", ModelId, null, headers),
                cancellationToken).ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body);
        var root = document.RootElement.Clone();
        var answers = new Dictionary<string, EvaluationAnswer>();
        foreach (var answer in root.GetProperty("answers").EnumerateObject())
        {
            answers[answer.Name] = ParseAnswer(answer.Value);
        }

        EvaluationRounding? rounding = null;
        if (root.TryGetProperty("rounding", out var roundingElement) && roundingElement.ValueKind == JsonValueKind.Object)
        {
            rounding = new EvaluationRounding(Integer(roundingElement, "probabilityDecimals"), Integer(roundingElement, "scoreDecimals"));
        }

        OperationUsage? usage = null;
        if (root.TryGetProperty("usage", out var usageElement) && usageElement.ValueKind == JsonValueKind.Object)
        {
            usage = new OperationUsage(Integer(usageElement, "inputTokens"), Integer(usageElement, "outputTokens"));
        }

        var warnings = new List<OperationWarning>();
        if (root.TryGetProperty("warnings", out var warningList) && warningList.ValueKind == JsonValueKind.Array)
        {
            foreach (var warning in warningList.EnumerateArray())
            {
                warnings.Add(new OperationWarning(
                    warning.GetProperty("type").GetString()!,
                    message: Text(warning, "message"),
                    feature: Text(warning, "feature"),
                    details: Text(warning, "details"),
                    setting: Text(warning, "setting")));
            }
        }

        JsonElement? providerMetadata = null;
        if (root.TryGetProperty("providerMetadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object)
        {
            providerMetadata = metadata;
        }

        return new EvaluationModelResult(
            answers,
            warnings,
            usage,
            rounding,
            providerMetadata,
            new ProviderResponse(response.Headers, root, modelId: Text(root, "model") ?? ModelId));
    }

    private static EvaluationAnswer ParseAnswer(JsonElement answer)
    {
        var type = answer.GetProperty("type").GetString()!;
        IReadOnlyDictionary<string, double>? probabilities = null;
        if (answer.TryGetProperty("probabilities", out var map) && map.ValueKind == JsonValueKind.Object)
        {
            var values = new Dictionary<string, double>();
            foreach (var item in map.EnumerateObject())
            {
                values[item.Name] = item.Value.GetDouble();
            }

            probabilities = values;
        }

        switch (type)
        {
            case "choice":
                return new EvaluationAnswer(type, choice: answer.GetProperty("choice").GetString(), probabilities: probabilities);
            case "score":
                return new EvaluationAnswer(type, score: answer.GetProperty("score").GetDouble(), probabilities: probabilities);
            case "boolean":
                return new EvaluationAnswer(type, probability: answer.GetProperty("probability").GetDouble());
            case "refusal":
                return new EvaluationAnswer(type);
            default:
                throw new InvalidResponseDataException(answer, "Unknown answer type \"" + type + "\".");
        }
    }

    private static int? Integer(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
    }

    private static string? Text(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static JsonNode? ToNode(object? value)
    {
        if (value == null)
        {
            return null;
        }

        return JsonNode.Parse(JsonSerializer.Serialize(value));
    }
}

/// <summary>Validates <c>providerOptions.gateway</c> for decision requests.</summary>
internal static class GatewayDecisionProviderOptions
{
    private const int MaxDepth = 5;
    private const int MaxConditionsPerList = 20;
    private const int MaxQuestionLength = 256;

    public static void Validate(JsonElement providerOptions)
    {
        if (providerOptions.ValueKind != JsonValueKind.Object || !providerOptions.TryGetProperty("gateway", out var gateway))
        {
            return;
        }

        string? problem = gateway.ValueKind != JsonValueKind.Object ? "gateway options must be an object" : null;
        if (problem == null && gateway.TryGetProperty("models", out var models))
        {
            problem = Models(models);
        }

        if (problem != null)
        {
            throw new InvalidArgumentException("providerOptions", gateway.ToString(), "invalid gateway provider options: " + problem);
        }
    }

    private static string? Models(JsonElement models)
    {
        if (models.ValueKind != JsonValueKind.Array)
        {
            return "models must be an array";
        }

        var index = 0;
        var conditional = 0;
        foreach (var entry in models.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.String)
            {
                index++;
                continue;
            }

            if (entry.ValueKind != JsonValueKind.Object)
            {
                return "models entries must be strings or conditional fallbacks";
            }

            conditional++;
            if (conditional > 1)
            {
                return "models supports at most one conditional decision fallback";
            }

            if (index != 0)
            {
                return "a conditional decision fallback must be the first models entry";
            }

            var problem = Fallback(entry);
            if (problem != null)
            {
                return problem;
            }

            index++;
        }

        return null;
    }

    private static string? Fallback(JsonElement entry)
    {
        foreach (var property in entry.EnumerateObject())
        {
            if (property.Name != "model" && property.Name != "when")
            {
                return "unrecognized key \"" + property.Name + "\" in a conditional fallback";
            }
        }

        if (!entry.TryGetProperty("model", out var model) || model.ValueKind != JsonValueKind.String || model.GetString()!.Length == 0)
        {
            return "a conditional fallback requires a model";
        }

        return entry.TryGetProperty("when", out var when) ? Condition(when, 1) : "a conditional fallback requires a when condition";
    }

    private static string? Condition(JsonElement condition, int depth)
    {
        if (condition.ValueKind != JsonValueKind.Object)
        {
            return "a condition must be an object";
        }

        var keys = condition.EnumerateObject().Select(property => property.Name).ToArray();
        if (keys.Contains("confidenceBelow") || keys.Contains("probabilityBetween"))
        {
            return Direct(condition, keys);
        }

        if (keys.Length != 1 || (keys[0] != "any" && keys[0] != "all" && keys[0] != "atLeast"))
        {
            return "a condition must be confidenceBelow, probabilityBetween, any, all, or atLeast";
        }

        if (depth == MaxDepth)
        {
            return "conditions can be nested at most " + MaxDepth.ToString(CultureInfo.InvariantCulture) + " levels deep";
        }

        var group = condition.GetProperty(keys[0]);
        if (keys[0] != "atLeast")
        {
            return List(group, depth);
        }

        if (group.ValueKind != JsonValueKind.Object)
        {
            return "atLeast must be an object";
        }

        foreach (var property in group.EnumerateObject())
        {
            if (property.Name != "count" && property.Name != "conditions")
            {
                return "unrecognized key \"" + property.Name + "\" in atLeast";
            }
        }

        if (!group.TryGetProperty("count", out var count) || count.ValueKind != JsonValueKind.Number || !count.TryGetInt32(out var number) || number < 1)
        {
            return "atLeast count must be an integer of at least 1";
        }

        if (!group.TryGetProperty("conditions", out var conditions))
        {
            return "atLeast requires conditions";
        }

        var problem = List(conditions, depth);
        if (problem == null && number > conditions.GetArrayLength())
        {
            return "atLeast count cannot exceed the number of conditions";
        }

        return problem;
    }

    private static string? List(JsonElement list, int depth)
    {
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() < 1 || list.GetArrayLength() > MaxConditionsPerList)
        {
            return "a condition list must contain between 1 and " + MaxConditionsPerList.ToString(CultureInfo.InvariantCulture) + " conditions";
        }

        foreach (var child in list.EnumerateArray())
        {
            var problem = Condition(child, depth + 1);
            if (problem != null)
            {
                return problem;
            }
        }

        return null;
    }

    private static string? Direct(JsonElement condition, string[] keys)
    {
        var allowed = keys.Contains("confidenceBelow") ? "confidenceBelow" : "probabilityBetween";
        foreach (var key in keys)
        {
            if (key != "question" && key != allowed)
            {
                return "unrecognized key \"" + key + "\" in a condition";
            }
        }

        if (condition.TryGetProperty("question", out var question) && (question.ValueKind != JsonValueKind.String || question.GetString()!.Length < 1 || question.GetString()!.Length > MaxQuestionLength))
        {
            return "question must be a string of 1 to " + MaxQuestionLength.ToString(CultureInfo.InvariantCulture) + " characters";
        }

        var value = condition.GetProperty(allowed);
        if (allowed == "confidenceBelow")
        {
            return Probability(value) ? null : "confidenceBelow must be a number between 0 and 1";
        }

        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() != 2 || !value.EnumerateArray().All(Probability))
        {
            return "probabilityBetween must be two numbers between 0 and 1";
        }

        return value[0].GetDouble() <= value[1].GetDouble() ? null : "probabilityBetween minimum must be less than or equal to maximum";
    }

    private static bool Probability(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.Number && value.GetDouble() >= 0 && value.GetDouble() <= 1;
    }
}
