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
#pragma warning disable CS0618
public sealed class GatewayDecisionModel : IEvaluationDecisionCaller
#pragma warning restore CS0618
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
    public IReadOnlyList<string> SupportedQuestionTypes { get; } =
        new[] { "choice", "score", "boolean" };

    /// <summary>Deprecated. Use <see cref="DoDecideAsync(DecisionModelCall, CancellationToken)"/>.</summary>
    [Obsolete("Use DoDecideAsync instead.")]
    public async Task<EvaluationModelResult> DoEvaluateAsync(
        EvaluationModelCall call,
        CancellationToken cancellationToken
    )
    {
        return EvaluationModelResult.From(
            await DoDecideAsync(call, cancellationToken).ConfigureAwait(false)
        );
    }

    /// <summary>Sends the state and every question in one request.</summary>
    public async Task<DecisionModelResult> DoDecideAsync(
        DecisionModelCall call,
        CancellationToken cancellationToken = default
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        GatewayDecisionProviderOptions.Validate(call.ProviderOptions);
        var questions = new JsonObject();
        foreach (var pair in call.Questions)
        {
            var question = new JsonObject { ["type"] = pair.Value.Type };
            if (pair.Value.HasInstructions)
            {
                question["instructions"] = pair.Value.Instructions?.ToNode();
            }

            if (pair.Value.Criteria != null)
            {
                question["criteria"] = pair.Value.Criteria.ToNode();
            }

            questions[pair.Key] = question;
        }

        var body = new JsonObject
        {
            ["stateParts"] = ToGatewayStateParts.Convert(call.State, "decision"),
            ["questions"] = questions,
        };
        if (call.MaxOutputTokens != null)
        {
            body["maxOutputTokens"] = call.MaxOutputTokens;
        }

        if (call.Reasoning != null)
        {
            body["reasoning"] = call.Reasoning;
        }

        if (call.ProviderOptions.ValueKind == JsonValueKind.Object)
        {
            body["providerOptions"] = JsonNode.Parse(call.ProviderOptions.GetRawText());
        }

        var headers = new Dictionary<string, string?>();
        foreach (var pair in call.Headers)
        {
            headers[pair.Key] = pair.Value;
        }

        ProviderTextResponse response;
        var requestHeaders = await _provider
            .HeadersAsync(
                "ai-decision-model-specification-version",
                "4",
                "ai-model-id",
                ModelId,
                null,
                headers,
                cancellationToken
            )
            .ConfigureAwait(false);
        try
        {
            response = await _provider
                .Http.SendJsonStringAsync(
                    HttpMethod.Post,
                    _provider.Route("decision-model"),
                    body.ToJsonString(),
                    requestHeaders,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }
        catch (ApiConnectionException exception)
        {
            throw GatewayProvider.MapFailure(exception, requestHeaders, body.ToJsonString());
        }

        using var document = JsonDocument.Parse(
            string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
        );
        var root = document.RootElement.Clone();
        var answers = new Dictionary<string, DecisionAnswer>();
        foreach (var answer in root.GetProperty("answers").EnumerateObject())
        {
            answers[answer.Name] = ParseAnswer(answer.Value);
        }

        DecisionRounding? rounding = null;
        if (
            root.TryGetProperty("rounding", out var roundingElement)
            && roundingElement.ValueKind == JsonValueKind.Object
        )
        {
            rounding = new DecisionRounding(
                Integer(roundingElement, "probabilityDecimals"),
                Integer(roundingElement, "scoreDecimals")
            );
        }

        OperationUsage? usage = null;
        if (
            root.TryGetProperty("usage", out var usageElement)
            && usageElement.ValueKind == JsonValueKind.Object
        )
        {
            usage = new OperationUsage(
                Integer(usageElement, "inputTokens"),
                Integer(usageElement, "outputTokens")
            );
        }

        var warnings = new List<OperationWarning>();
        if (
            root.TryGetProperty("warnings", out var warningList)
            && warningList.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var warning in warningList.EnumerateArray())
            {
                warnings.Add(
                    new OperationWarning(
                        warning.GetProperty("type").GetString()!,
                        message: Text(warning, "message"),
                        feature: Text(warning, "feature"),
                        details: Text(warning, "details"),
                        setting: Text(warning, "setting")
                    )
                );
            }
        }

        JsonElement? providerMetadata = null;
        if (
            root.TryGetProperty("providerMetadata", out var metadata)
            && metadata.ValueKind == JsonValueKind.Object
        )
        {
            providerMetadata = metadata;
        }

        return new DecisionModelResult(
            answers,
            warnings,
            usage,
            rounding,
            providerMetadata,
            new ProviderResponse(response.Headers, root, modelId: Text(root, "model") ?? ModelId)
        );
    }

    private static DecisionAnswer ParseAnswer(JsonElement answer)
    {
        var type = answer.GetProperty("type").GetString()!;
        IReadOnlyDictionary<string, double>? probabilities = null;
        if (
            answer.TryGetProperty("probabilities", out var map)
            && map.ValueKind == JsonValueKind.Object
        )
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
                return new DecisionAnswer(
                    type,
                    choice: answer.GetProperty("choice").GetString(),
                    probabilities: probabilities
                );
            case "score":
                return new DecisionAnswer(
                    type,
                    score: answer.GetProperty("score").GetDouble(),
                    probabilities: probabilities
                );
            case "boolean":
                return new DecisionAnswer(
                    type,
                    probability: answer.GetProperty("probability").GetDouble()
                );
            case "refusal":
                return new DecisionAnswer(type);
            default:
                throw new InvalidResponseDataException(
                    answer,
                    "Unknown answer type \"" + type + "\"."
                );
        }
    }

    private static int? Integer(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetInt32(out var number)
            ? number
            : null;
    }

    private static string? Text(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
