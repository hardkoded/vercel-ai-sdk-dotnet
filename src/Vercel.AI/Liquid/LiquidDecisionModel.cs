// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.OpenAICompatible;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Liquid;

/// <summary>
/// Answers questions through the Liquid Decisions API (<c>POST {base}/decisions/v1/systemone</c>). Boolean questions are sent as
/// native <c>noul</c> questions. Image and audio file parts are sent as base64 data URLs. Maps to <c>DecisionLiquidModel</c>.
/// </summary>
public sealed class DecisionLiquidModel : IDecisionCaller
{
    private static readonly IReadOnlyDictionary<string, string[]> s_supportedMediaTypes =
        new Dictionary<string, string[]>
        {
            ["image"] = ["image/jpeg", "image/png", "image/webp"],
            ["audio"] = ["audio/wav", "audio/mpeg", "audio/flac", "audio/ogg"],
        };

    private readonly LiquidProvider _provider;

    /// <summary>Creates a decision model. <paramref name="modelId"/> may be a model id this library does not know yet.</summary>
    public DecisionLiquidModel(LiquidProvider provider, string modelId)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
    }

    /// <inheritdoc />
    public string Provider => "liquid.decision";

    /// <inheritdoc />
    public string ModelId { get; }

    /// <inheritdoc />
    public string SpecificationVersion => "v4";

    /// <inheritdoc />
    public IReadOnlyList<string> SupportedQuestionTypes =>
        LiquidModelCatalog.DecisionSupportedQuestionTypes;

    /// <summary>
    /// Serializes the model id, the endpoint, and the resolved request headers (including authentication) for a workflow boundary.
    /// Maps to <c>[WORKFLOW_SERIALIZE]</c>. The HTTP client is not serialized.
    /// </summary>
    public SerializedModelOptions Serialize()
    {
        var headers = new Dictionary<string, string>();
        foreach (var pair in _provider.CreateHeadersForSerialization())
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
    public static DecisionLiquidModel Deserialize(
        SerializedModelOptions options,
        HttpClient? httpClient = null
    )
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
                            Restore(providerOptions, property.Name, property.Value.GetString()!);
                        }
                    }

                    break;
                case IDictionary<string, object?> values:
                    foreach (var pair in values)
                    {
                        if (AsString(pair.Value) is { } value)
                        {
                            Restore(providerOptions, pair.Key, value);
                        }
                    }

                    break;
            }
        }

        return new LiquidProvider(httpClient ?? new HttpClient(), providerOptions).DecisionModel(
            options.ModelId
        );
    }

    /// <summary>Sends every question in one Decisions request.</summary>
    public async Task<DecisionModelResult> DoDecideAsync(
        DecisionModelCall call,
        CancellationToken cancellationToken = default
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        if (
            call.Questions.Count == 0
            || call.Questions.Count > LiquidModelCatalog.DecisionMaxQuestions
        )
        {
            throw new InvalidArgumentException(
                "questions",
                call.Questions,
                "Liquid decisions support 1 to "
                    + LiquidModelCatalog.DecisionMaxQuestions
                    + " questions per request."
            );
        }

        var nativeQuestions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            nativeQuestions[pair.Key] = BuildQuestion(pair.Key, pair.Value);
        }

        var (nativeState, images, audio) = ConvertToLiquidDecisionInput(call.State);
        if (call.Questions.Values.Any(question => question.Type == "box") && images.Count != 1)
        {
            throw new InvalidArgumentException(
                "state",
                call.State,
                "Liquid Box questions require exactly one image in the state."
            );
        }

        var body = new JsonObject { ["model"] = ModelId, ["state"] = nativeState };
        if (images.Count > 0)
        {
            body["images"] = new JsonArray(images.Select(JsonNode? (image) => image).ToArray());
        }

        if (audio.Count > 0)
        {
            body["audio"] = new JsonArray(audio.Select(JsonNode? (clip) => clip).ToArray());
        }

        body["questions"] = nativeQuestions;

        ProviderExchangeResult response;
        try
        {
            response = await ProviderExchange
                .SendAsync(
                    _provider._httpClient,
                    HttpMethod.Post,
                    ApiKeys.Combine(_provider.Options.BaseUrl, "decisions/v1/systemone"),
                    ProviderExchange.Json(body.ToJsonString()),
                    ProviderExchange.Merge(
                        await _provider.CreateHeadersAsync(cancellationToken).ConfigureAwait(false),
                        call.Headers
                    ),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
            when (LiquidError.ErrorMessage(exception.ResponseBody) is { } message)
        {
            throw ProviderHttp.MapStatus(exception.StatusCode, message, exception.ResponseBody);
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
            );
            root = document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new ApiException("Invalid JSON response", 200, response.Body, exception);
        }

        var data = ParseResponse(root, response.Body);
        if (
            data.Answers.Count != call.Questions.Count
            || call.Questions.Keys.Any(id => !data.Answers.Any(answer => answer.Name == id))
        )
        {
            throw new InvalidResponseDataException(
                root,
                "Liquid must return exactly one answer for every question."
            );
        }

        var answers = new Dictionary<string, DecisionAnswer>();
        var confidence = new JsonObject();
        var legend = new JsonObject();
        var found = new JsonObject();
        foreach (var answer in data.Answers)
        {
            answers[answer.Name] = answer.Type switch
            {
                "noul" => new DecisionAnswer("boolean", probability: answer.Probability),
                "choice" => new DecisionAnswer(
                    "choice",
                    choice: answer.Choice,
                    probabilities: answer.Probabilities
                ),
                "score" => new DecisionAnswer(
                    "score",
                    score: answer.Score,
                    probabilities: answer.Probabilities
                ),
                // Liquid omits the box when the object is not found (found < 0.5).
                _ => answer.Box == null
                    ? new DecisionAnswer("refusal")
                    : new DecisionAnswer("box", box: answer.Box),
            };
            if (answer.Type != "noul" && answer.Confidence != null)
            {
                confidence[answer.Name] = answer.Confidence.Value;
            }

            if (answer.Type == "score" && answer.Legend != null)
            {
                legend[answer.Name] = JsonNode.Parse(answer.Legend.Value.GetRawText());
            }

            if (answer.Type == "box" && answer.Found != null)
            {
                found[answer.Name] = answer.Found.Value;
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

        if (
            call.ProviderOptions.ValueKind == JsonValueKind.Object
            && call.ProviderOptions.TryGetProperty(LiquidProvider.ProviderId, out var options)
            && options.ValueKind == JsonValueKind.Object
        )
        {
            foreach (var option in options.EnumerateObject())
            {
                warnings.Add(
                    new OperationWarning(
                        "unsupported",
                        feature: "providerOptions.liquid." + option.Name
                    )
                );
            }
        }

        var liquid = new JsonObject();
        if (data.Cost != null)
        {
            liquid["cost"] = data.Cost.Value;
        }

        liquid["confidence"] = confidence;
        liquid["legend"] = legend;
        liquid["found"] = found;
        var metadata = JsonSerializer.SerializeToElement(
            new JsonObject { [LiquidProvider.ProviderId] = liquid }
        );

        // Liquid returns full-precision probabilities, so no rounding is declared.
        return new DecisionModelResult(
            answers,
            warnings,
            data.Usage,
            null,
            metadata,
            new ProviderResponse(response.Headers, root, modelId: data.Model ?? ModelId)
        );
    }

    private static void Restore(OpenAICompatibleOptions options, string name, string value)
    {
        const string BearerPrefix = "Bearer ";
        if (
            name.Equals("authorization", StringComparison.OrdinalIgnoreCase)
            && value.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase)
        )
        {
            options.ApiKey = value.Substring(BearerPrefix.Length);
        }
        else
        {
            options.Headers[name] = value;
        }
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

    // Maps to convertToLiquidDecisionInput.
    private static (
        JsonNode? State,
        List<string> Images,
        List<string> Audio
    ) ConvertToLiquidDecisionInput(IReadOnlyList<DecisionModelStatePart> state)
    {
        var values = new List<JsonNode?>();
        var images = new List<string>();
        var audio = new List<string>();
        foreach (var part in state)
        {
            switch (part)
            {
                case DecisionModelTextPart text:
                    values.Add(JsonValue.Create(text.Text));
                    break;
                case DecisionModelJsonPart json:
                    values.Add(JsonNode.Parse(json.Value.GetRawText()));
                    break;
                case DecisionModelFilePart file:
                    var kind = MediaTypes.GetTopLevelMediaType(file.MediaType);
                    if (kind != "image" && kind != "audio")
                    {
                        throw new UnsupportedFunctionalityException(
                            "Liquid decision file media type: " + file.MediaType,
                            "'Liquid decision file media type: "
                                + file.MediaType
                                + "' functionality not supported."
                        );
                    }

                    var data = file.Data;
                    if (data.DataType != "data")
                    {
                        throw new UnsupportedFunctionalityException(
                            "Liquid decision file input: " + data.DataType,
                            "'Liquid decision file input: "
                                + data.DataType
                                + "' functionality not supported."
                        );
                    }

                    var mediaType =
                        MediaTypes.IsFullMediaType(file.MediaType) ? file.MediaType
                        : data.Bytes != null ? MediaTypes.DetectMediaType(data.Bytes, kind)
                        : MediaTypes.DetectMediaType(data.Base64 ?? string.Empty, kind);
                    if (mediaType == null || !s_supportedMediaTypes[kind].Contains(mediaType))
                    {
                        throw new UnsupportedFunctionalityException(
                            "Liquid decision "
                                + kind
                                + " media type: "
                                + (mediaType ?? file.MediaType),
                            "'Liquid decision "
                                + kind
                                + " media type: "
                                + (mediaType ?? file.MediaType)
                                + "' functionality not supported."
                        );
                    }

                    var base64 =
                        data.Bytes != null ? Convert.ToBase64String(data.Bytes) : data.Base64;

                    // Liquid reads images and audio from top-level arrays, before the state.
                    (kind == "image" ? images : audio).Add(
                        "data:" + mediaType + ";base64," + base64
                    );
                    break;
            }
        }

        if (
            images.Count > LiquidModelCatalog.DecisionMaxImages
            || audio.Count > LiquidModelCatalog.DecisionMaxAudioClips
        )
        {
            throw new InvalidArgumentException(
                "state",
                state,
                "Liquid decisions support at most "
                    + LiquidModelCatalog.DecisionMaxImages
                    + " images and "
                    + LiquidModelCatalog.DecisionMaxAudioClips
                    + " audio clips per request."
            );
        }

        // The API accepts plain text or structured JSON. A single string, object, or array is sent as is;
        // other combinations are joined as text.
        JsonNode? nativeState;
        if (values.Count == 0)
        {
            nativeState = null;
        }
        else if (
            values.Count == 1
            && (
                values[0] is JsonObject or JsonArray
                || (values[0] is JsonValue single && single.GetValueKind() == JsonValueKind.String)
            )
        )
        {
            nativeState = values[0];
        }
        else
        {
            nativeState = string.Join(
                "\n",
                values.Select(value =>
                    value is JsonValue text && text.GetValueKind() == JsonValueKind.String
                        ? text.GetValue<string>()
                        : value?.ToJsonString() ?? "null"
                )
            );
        }

        return (nativeState, images, audio);
    }

    private static JsonObject BuildQuestion(string id, DecisionQuestion question)
    {
        if (question.Instructions == null)
        {
            throw new InvalidArgumentException(
                "questions." + id + ".instructions",
                question.Instructions,
                "Liquid decision questions require instructions."
            );
        }

        switch (question.Type)
        {
            case "boolean":
                var boolean = new JsonObject
                {
                    ["type"] = "noul",
                    ["instructions"] = question.Instructions.ToNode(),
                };
                if (question.Criteria != null)
                {
                    boolean["criteria"] = question.Criteria.ToNode();
                }

                return boolean;
            case "choice":
                var count = question.Criteria?.Count ?? 0;
                if (
                    count < LiquidModelCatalog.DecisionMinChoiceOptions
                    || count > LiquidModelCatalog.DecisionMaxChoiceOptions
                )
                {
                    throw new InvalidArgumentException(
                        "questions." + id + ".criteria",
                        question.Criteria,
                        "Liquid Choice questions support "
                            + LiquidModelCatalog.DecisionMinChoiceOptions
                            + " to "
                            + LiquidModelCatalog.DecisionMaxChoiceOptions
                            + " options."
                    );
                }

                return Native("choice", question);
            case "score":
                var levels = (question.Criteria as DecisionScoreCriteria)?.Levels ?? [];
                if (
                    levels.Count < LiquidModelCatalog.DecisionMinScoreLevels
                    || levels.Count > LiquidModelCatalog.DecisionMaxScoreLevels
                    || levels.Any(level => level == null)
                )
                {
                    throw new InvalidArgumentException(
                        "questions." + id + ".criteria",
                        question.Criteria,
                        "Liquid Score questions require "
                            + LiquidModelCatalog.DecisionMinScoreLevels
                            + " to "
                            + LiquidModelCatalog.DecisionMaxScoreLevels
                            + " non-null levels."
                    );
                }

                return Native("score", question);
            default:
                // Box: the criteria are not sent.
                return new JsonObject
                {
                    ["type"] = "box",
                    ["instructions"] = question.Instructions.ToNode(),
                };
        }
    }

    private static JsonObject Native(string type, DecisionQuestion question)
    {
        return new JsonObject
        {
            ["type"] = type,
            ["instructions"] = question.Instructions?.ToNode(),
            ["criteria"] = question.Criteria?.ToNode(),
        };
    }

    // Maps to liquidDecisionResponseSchema. A response that does not fit it is an API call error.
    private static ParsedResponse ParseResponse(JsonElement root, string body)
    {
        ApiException Malformed(string reason)
        {
            return new ApiException("Type validation failed: " + reason, 200, body);
        }

        string? Text(JsonElement parent, string property)
        {
            if (
                !parent.TryGetProperty(property, out var value)
                || value.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : throw Malformed(property + " must be a string.");
        }

        double Probability(JsonElement element, string what)
        {
            if (
                element.ValueKind != JsonValueKind.Number
                || !element.TryGetDouble(out var value)
                || value < 0
                || value > 1
            )
            {
                throw Malformed(what + " must be a number from 0 to 1.");
            }

            return value;
        }

        double? NullableProbability(JsonElement parent, string property)
        {
            return
                parent.TryGetProperty(property, out var value)
                && value.ValueKind != JsonValueKind.Null
                ? Probability(value, property)
                : null;
        }

        Dictionary<string, double> Distribution(JsonElement parent)
        {
            if (
                !parent.TryGetProperty("probabilities", out var map)
                || map.ValueKind != JsonValueKind.Object
            )
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

        double? NullableNumber(JsonElement parent, string property)
        {
            if (
                !parent.TryGetProperty(property, out var value)
                || value.ValueKind == JsonValueKind.Null
            )
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number)
                ? number
                : throw Malformed(property + " must be a number.");
        }

        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("answers", out var list)
            || list.ValueKind != JsonValueKind.Object
        )
        {
            throw Malformed("answers must be an object.");
        }

        var parsed = new ParsedResponse { Model = Text(root, "model") };
        foreach (var item in list.EnumerateObject())
        {
            var element = item.Value;
            if (
                element.ValueKind != JsonValueKind.Object
                || !element.TryGetProperty("type", out var type)
                || type.ValueKind != JsonValueKind.String
            )
            {
                throw Malformed("answer type must be noul, choice, score, or box.");
            }

            var answer = new ParsedAnswer { Name = item.Name, Type = type.GetString()! };
            switch (answer.Type)
            {
                case "noul":
                    answer.Probability = element.TryGetProperty("noul", out var noul)
                        ? Probability(noul, "noul")
                        : throw Malformed("noul is required.");
                    break;
                case "choice":
                    if (
                        !element.TryGetProperty("choice", out var choice)
                        || choice.ValueKind != JsonValueKind.String
                    )
                    {
                        throw Malformed("choice must be a string.");
                    }

                    answer.Choice = choice.GetString();
                    answer.Probabilities = Distribution(element);
                    answer.Confidence = NullableProbability(element, "confidence");
                    break;
                case "score":
                    if (
                        !element.TryGetProperty("score", out var score)
                        || score.ValueKind != JsonValueKind.Number
                        || !score.TryGetDouble(out var scoreValue)
                    )
                    {
                        throw Malformed("score must be a number.");
                    }

                    answer.Score = scoreValue;
                    answer.Probabilities = Distribution(element);
                    answer.Confidence = NullableProbability(element, "confidence");
                    if (
                        element.TryGetProperty("legend", out var legend)
                        && legend.ValueKind != JsonValueKind.Null
                    )
                    {
                        answer.Legend =
                            legend.ValueKind == JsonValueKind.Object
                                ? legend
                                : throw Malformed("legend must be an object.");
                    }

                    break;
                case "box":
                    if (
                        element.TryGetProperty("box", out var box)
                        && box.ValueKind != JsonValueKind.Null
                    )
                    {
                        if (box.ValueKind != JsonValueKind.Object)
                        {
                            throw Malformed("box must be an object.");
                        }

                        double Edge(string edge) =>
                            box.TryGetProperty(edge, out var value)
                                ? Probability(value, edge)
                                : throw Malformed(edge + " is required.");

                        answer.Box = new DecisionBox(
                            Edge("top"),
                            Edge("bottom"),
                            Edge("left"),
                            Edge("right")
                        );
                    }

                    answer.Found = NullableProbability(element, "found");
                    answer.Confidence = NullableProbability(element, "confidence");
                    break;
                default:
                    throw Malformed("answer type must be noul, choice, score, or box.");
            }

            parsed.Answers.Add(answer);
        }

        if (root.TryGetProperty("usage", out var usage) && usage.ValueKind != JsonValueKind.Null)
        {
            if (usage.ValueKind != JsonValueKind.Object)
            {
                throw Malformed("usage must be an object.");
            }

            parsed.Usage = new OperationUsage(
                (int?)NullableNumber(usage, "input_tokens"),
                (int?)NullableNumber(usage, "output_tokens")
            );
            parsed.Cost = NullableNumber(usage, "cost");
        }

        return parsed;
    }

    private sealed class ParsedResponse
    {
        public string? Model { get; set; }

        public List<ParsedAnswer> Answers { get; } = [];

        public OperationUsage? Usage { get; set; }

        public double? Cost { get; set; }
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

        public DecisionBox? Box { get; set; }

        public double? Found { get; set; }
    }
}
