// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Anthropic;

/// <summary>One evaluation question.</summary>
public sealed class AnthropicEvaluationQuestion
{
    /// <summary>Creates a question. <paramref name="type"/> is choice, score, or boolean.</summary>
    public AnthropicEvaluationQuestion(string name, string type, IReadOnlyList<string>? criteria)
    {
        Name = name;
        Type = type;
        Criteria = criteria ?? Array.Empty<string>();
    }

    /// <summary>Question name.</summary>
    public string Name { get; }

    /// <summary>choice, score, or boolean.</summary>
    public string Type { get; }

    /// <summary>Choice labels or score level labels, in insertion order.</summary>
    public IReadOnlyList<string> Criteria { get; }
}

/// <summary>One parsed answer.</summary>
public sealed class AnthropicEvaluationAnswer
{
    /// <summary>Creates an answer.</summary>
    public AnthropicEvaluationAnswer(string type, string? choice, double? score, double? probability)
    {
        Type = type;
        Choice = choice;
        Score = score;
        Probability = probability;
    }

    /// <summary>choice, score, or boolean.</summary>
    public string Type { get; }

    /// <summary>Choice label.</summary>
    public string? Choice { get; }

    /// <summary>Score.</summary>
    public double? Score { get; }

    /// <summary>Boolean probability.</summary>
    public double? Probability { get; }
}

/// <summary>Evaluation call result.</summary>
public sealed class AnthropicEvaluationResult
{
    /// <summary>Creates a result.</summary>
    public AnthropicEvaluationResult(IReadOnlyDictionary<string, AnthropicEvaluationAnswer> answers, LanguageModelUsage usage, IReadOnlyDictionary<string, string> headers, string provider)
    {
        Answers = answers;
        Usage = usage;
        Headers = headers;
        Provider = provider;
    }

    /// <summary>Answers keyed by question name.</summary>
    public IReadOnlyDictionary<string, AnthropicEvaluationAnswer> Answers { get; }

    /// <summary>Converted token usage.</summary>
    public LanguageModelUsage Usage { get; }

    /// <summary>Response headers.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Always <c>anthropic.evaluation</c> for the model that produced this result.</summary>
    public string Provider { get; }
}

/// <summary>Builds and scores Anthropic evaluation requests on the Messages API.</summary>
public sealed class AnthropicEvaluationModel : IEvaluationModel
{
    private readonly AnthropicProvider _provider;
    private readonly string _modelId;

    /// <summary>Creates an evaluation model.</summary>
    public AnthropicEvaluationModel(AnthropicProvider provider, string modelId)
    {
        _provider = provider;
        _modelId = modelId;
    }

    /// <inheritdoc />
    public string Provider => "anthropic.evaluation";

    /// <inheritdoc />
    public string ModelId => _modelId;

    /// <summary>Question types this model accepts.</summary>
    public IReadOnlyList<string> SupportedQuestionTypes { get; } = new[] { "choice", "score", "boolean" };

    /// <inheritdoc />
    public async Task<EvaluationResult> DoEvaluateAsync(string rubric, string candidate, CancellationToken cancellationToken)
    {
        var questions = new[] { new AnthropicEvaluationQuestion("score", "score", new[] { "low", "high" }) };
        var result = await EvaluateAsync(questions, null, cancellationToken).ConfigureAwait(false);
        AnthropicEvaluationAnswer? answer;
        if (!result.Answers.TryGetValue("score", out answer) || answer == null || answer.Score == null)
        {
            return new EvaluationResult(0, candidate);
        }

        return new EvaluationResult(answer.Score.Value, rubric);
    }

    /// <summary>Evaluates questions and maps <c>q0</c>, <c>q1</c>, … back onto the question names.</summary>
    public async Task<AnthropicEvaluationResult> EvaluateAsync(IReadOnlyList<AnthropicEvaluationQuestion> questions, LanguageModelCallOptions? options, CancellationToken cancellationToken)
    {
        var call = options ?? new LanguageModelCallOptions();
        call.Prompt = call.Prompt == null || call.Prompt.Count == 0
            ? new ModelMessage[] { new UserModelMessage("Evaluate.") }
            : call.Prompt;
        call.JsonSchema = BuildSchema(questions);
        if (call.ProviderOptions == null || !HasThinking(call))
        {
            var providerOptions = new Dictionary<string, JsonElement>();
            if (call.ProviderOptions != null)
            {
                foreach (var pair in call.ProviderOptions)
                {
                    providerOptions[pair.Key] = pair.Value;
                }
            }
            if (!providerOptions.ContainsKey("anthropic"))
            {
                using var thinking = JsonDocument.Parse("{\"thinking\":{\"type\":\"disabled\"}}");
                providerOptions["anthropic"] = thinking.RootElement.Clone();
            }
            else if (!HasThinking(call))
            {
                var node = JsonNode.Parse(providerOptions["anthropic"].GetRawText()) as JsonObject ?? new JsonObject();
                node["thinking"] = new JsonObject { ["type"] = "disabled" };
                using var document = JsonDocument.Parse(node.ToJsonString());
                providerOptions["anthropic"] = document.RootElement.Clone();
            }

            call.ProviderOptions = providerOptions;
        }

        var prepared = AnthropicMessagesRequest.Prepare(_modelId, call, false, Provider);
        if (_provider.Options.TransformRequestBody != null)
        {
            prepared = new AnthropicPreparedRequest(
                _provider.Options.TransformRequestBody(prepared.Body),
                prepared.Warnings,
                prepared.Betas,
                prepared.UsesJsonResponseTool,
                prepared.ProviderOptionsName,
                prepared.UsedCustomProviderKey);
        }

        var headers = _provider.CreateHeaders(prepared.Betas, call.Headers);
        var response = await _provider.Http.SendJsonStringAsync(
            HttpMethod.Post,
            _provider.MessagesUri(),
            prepared.Body.ToJsonString(),
            headers,
            cancellationToken).ConfigureAwait(false);
        var context = new AnthropicParseContext(prepared.UsesJsonResponseTool, prepared.ProviderOptionsName, prepared.UsedCustomProviderKey);
        var generated = AnthropicResponse.Parse(response.Body, context, response.Headers);
        if (generated.FinishReason != FinishReason.Stop)
        {
            throw new AiSdkException("Evaluation response finish reason must be stop.");
        }

        var json = generated.Text;
        if (string.IsNullOrWhiteSpace(json))
        {
            foreach (var part in generated.Content)
            {
                if (part is GeneratedText text)
                {
                    json = text.Text;
                }
            }
        }

        return new AnthropicEvaluationResult(ParseAnswers(questions, json), generated.Usage, response.Headers, Provider);
    }

    /// <summary>Builds the json schema. Choice values are <c>c0</c>, <c>c1</c>, …. Number bounds are omitted.</summary>
    public static JsonElement BuildSchema(IReadOnlyList<AnthropicEvaluationQuestion> questions)
    {
        var properties = new JsonObject();
        var required = new JsonArray();
        for (var i = 0; i < questions.Count; i++)
        {
            var key = "q" + i.ToString(CultureInfo.InvariantCulture);
            required.Add(key);
            var question = questions[i];
            if (question.Type == "choice")
            {
                var enums = new JsonArray();
                for (var c = 0; c < question.Criteria.Count; c++)
                {
                    enums.Add("c" + c.ToString(CultureInfo.InvariantCulture));
                }

                properties[key] = new JsonObject { ["type"] = "string", ["enum"] = enums };
            }
            else
            {
                properties[key] = new JsonObject { ["type"] = "number" };
            }
        }

        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false,
        };
        using var document = JsonDocument.Parse(schema.ToJsonString());
        return document.RootElement.Clone();
    }

    /// <summary>Maps encoded answers back to criteria labels and checks score and probability bounds.</summary>
    public static IReadOnlyDictionary<string, AnthropicEvaluationAnswer> ParseAnswers(IReadOnlyList<AnthropicEvaluationQuestion> questions, string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var answers = new Dictionary<string, AnthropicEvaluationAnswer>();
        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            var key = "q" + i.ToString(CultureInfo.InvariantCulture);
            if (!root.TryGetProperty(key, out var value))
            {
                throw new AiSdkException("Evaluation response is missing " + key + ".");
            }

            if (question.Type == "choice")
            {
                var code = value.GetString() ?? string.Empty;
                if (code.Length < 2 || code[0] != 'c' || !int.TryParse(code.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out var index) || index < 0 || index >= question.Criteria.Count)
                {
                    throw new AiSdkException("Evaluation choice is outside the criteria.");
                }

                answers[question.Name] = new AnthropicEvaluationAnswer("choice", question.Criteria[index], null, null);
            }
            else if (question.Type == "score")
            {
                var score = value.GetDouble();
                var max = Math.Max(0, question.Criteria.Count - 1);
                if (double.IsNaN(score) || double.IsInfinity(score) || score < 0 || score > max)
                {
                    throw new AiSdkException("Evaluation score is outside 0.." + max.ToString(CultureInfo.InvariantCulture) + ".");
                }

                answers[question.Name] = new AnthropicEvaluationAnswer("score", null, score, null);
            }
            else
            {
                var probability = value.GetDouble();
                if (double.IsNaN(probability) || probability < 0 || probability > 1)
                {
                    throw new AiSdkException("Evaluation probability must be between 0 and 1.");
                }

                answers[question.Name] = new AnthropicEvaluationAnswer("boolean", null, null, probability);
            }
        }

        return answers;
    }

    private static bool HasThinking(LanguageModelCallOptions call)
    {
        JsonElement anthropic;
        if (call.ProviderOptions == null || !call.ProviderOptions.TryGetValue("anthropic", out anthropic))
        {
            return false;
        }

        return anthropic.ValueKind == JsonValueKind.Object && anthropic.TryGetProperty("thinking", out var thinking) && thinking.ValueKind == JsonValueKind.Object;
    }
}
