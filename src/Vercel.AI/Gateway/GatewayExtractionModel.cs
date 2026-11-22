// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.Gateway;

/// <summary>Extracts typed field values through the Gateway <c>/extraction-model</c> route. Maps to <c>GatewayExtractionModel</c>.</summary>
public sealed class GatewayExtractionModel : IExtractionCaller
{
    private readonly GatewayProvider _provider;

    /// <summary>Creates an extraction model.</summary>
    public GatewayExtractionModel(GatewayProvider provider, string modelId)
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

    /// <summary>Every field type. Support varies by model, so the Gateway rejects unsupported fields.</summary>
    public IReadOnlyList<string> SupportedFieldTypes { get; } = ExtractionField.Types;

    /// <summary>Sends the state and every field in one request.</summary>
    public async Task<ExtractionModelResult> DoExtractAsync(
        ExtractionModelCall call,
        CancellationToken cancellationToken = default
    )
    {
        if (call == null)
        {
            throw new ArgumentNullException(nameof(call));
        }

        var fields = new JsonObject();
        foreach (var pair in call.Fields)
        {
            fields[pair.Key] = Field(pair.Value);
        }

        var body = new JsonObject
        {
            ["stateParts"] = ToGatewayStateParts.Convert(
                call.State,
                "extraction",
                allowReferences: true
            ),
            ["fields"] = fields,
        };
        if (call.ReferenceDate != null)
        {
            body["referenceDate"] = call.ReferenceDate;
        }

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
        try
        {
            response = await _provider
                .Http.SendJsonStringAsync(
                    HttpMethod.Post,
                    _provider.Route("extraction-model"),
                    body.ToJsonString(),
                    await _provider.HeadersAsync(
                        "ai-extraction-model-specification-version",
                        "4",
                        "ai-model-id",
                        ModelId,
                        null,
                        headers,
                        cancellationToken
                    ),
                    cancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (ApiException exception)
        {
            if (GetUnsupportedField(exception) is { } unsupported)
            {
                throw new ExtractionUnsupportedFieldTypeError(
                    unsupported.FieldId,
                    unsupported.FieldType,
                    Provider,
                    ModelId
                );
            }

            throw GatewayProvider.MapFailure(exception);
        }
        catch (ApiTimeoutException exception)
        {
            throw GatewayProvider.MapFailure(exception);
        }

        JsonElement root;
        var answers = new Dictionary<string, ExtractionAnswer>();
        try
        {
            using var document = JsonDocument.Parse(
                string.IsNullOrWhiteSpace(response.Body) ? "{}" : response.Body
            );
            root = document.RootElement.Clone();
            foreach (var answer in root.GetProperty("answers").EnumerateObject())
            {
                answers[answer.Name] = ParseAnswer(answer.Value);
            }
        }
        catch (Exception exception)
            when (exception is JsonException
                || exception is KeyNotFoundException
                || exception is InvalidOperationException
                || exception is FormatException
            )
        {
            throw new InvalidResponseDataException(
                response.Body,
                "Invalid extraction model response: " + exception.Message
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
                warnings.Add(GatewayWarningSchema.Parse(warning));
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

        return new ExtractionModelResult(
            answers,
            warnings,
            usage,
            providerMetadata,
            new ProviderResponse(response.Headers, root, modelId: Text(root, "model") ?? ModelId)
        );
    }

    private static JsonObject Field(ExtractionField field)
    {
        var node = new JsonObject { ["type"] = field.Type };
        if (field.Instructions != null)
        {
            node["instructions"] = field.Instructions.ToNode();
        }

        if (field.Unit != null)
        {
            node["unit"] = field.Unit;
        }

        if (field.Integer != null)
        {
            node["integer"] = field.Integer;
        }

        if (field.Decimals != null)
        {
            node["decimals"] = field.Decimals;
        }

        if (field.Items != null)
        {
            node["items"] = field.Items;
        }

        return node;
    }

    // Maps to getUnsupportedField: reads the field from the Gateway's unsupported field type response.
    private static (string FieldId, string FieldType)? GetUnsupportedField(ApiException exception)
    {
        if (exception.ResponseBody == null)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(exception.ResponseBody);
            if (
                !document.RootElement.TryGetProperty("error", out var error)
                || error.ValueKind != JsonValueKind.Object
                || Text(error, "type") != "invalid_request_error"
                || !error.TryGetProperty("param", out var param)
                || param.ValueKind != JsonValueKind.Object
                || Text(param, "name") != "AI_ExtractionUnsupportedFieldTypeError"
                || Text(param, "fieldId") is not { } fieldId
                || Text(param, "fieldType") is not { } fieldType
            )
            {
                return null;
            }

            return (fieldId, fieldType);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ExtractionAnswer ParseAnswer(JsonElement answer)
    {
        var type = answer.GetProperty("type").GetString()!;
        switch (type)
        {
            case "refusal":
                return new ExtractionAnswer(type);
            case "text":
            case "date":
            case "time":
            case "datetime":
                return new ExtractionAnswer(
                    type,
                    text: Text(answer, "value"),
                    guessText: Text(answer, "guess"),
                    pNull: Number(answer, "pNull"),
                    confidence: Number(answer, "confidence"),
                    source: Source(answer)
                );
            case "number":
            case "duration":
                return new ExtractionAnswer(
                    type,
                    number: Number(answer, "value"),
                    guessNumber: Number(answer, "guess"),
                    pNull: Number(answer, "pNull"),
                    confidence: Number(answer, "confidence"),
                    source: Source(answer)
                );
            case "list":
                return ParseList(answer);
            default:
                throw new InvalidResponseDataException(
                    answer,
                    "Unknown answer type \"" + type + "\"."
                );
        }
    }

    private static ExtractionAnswer ParseList(JsonElement answer)
    {
        IReadOnlyList<string>? textItems = null;
        IReadOnlyList<double>? numberItems = null;
        if (answer.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
        {
            // An empty list carries no item type, so it is read as text items.
            var values = value.EnumerateArray().ToList();
            if (values.Count > 0 && values.All(item => item.ValueKind == JsonValueKind.Number))
            {
                numberItems = values.Select(item => item.GetDouble()).ToList();
            }
            else
            {
                textItems = values.Select(item => item.GetString()!).ToList();
            }
        }

        List<ExtractionItem>? items = null;
        if (
            answer.TryGetProperty("items", out var itemList)
            && itemList.ValueKind == JsonValueKind.Array
        )
        {
            items = itemList
                .EnumerateArray()
                .Select(item => new ExtractionItem(Number(item, "confidence"), Source(item)))
                .ToList();
        }

        return new ExtractionAnswer(
            "list",
            textItems: textItems,
            numberItems: numberItems,
            pNull: Number(answer, "pNull"),
            confidence: Number(answer, "confidence"),
            items: items
        );
    }

    private static ExtractionSource? Source(JsonElement element)
    {
        if (
            !element.TryGetProperty("source", out var source)
            || source.ValueKind != JsonValueKind.Object
        )
        {
            return null;
        }

        var type = source.GetProperty("type").GetString()!;
        var part = source.GetProperty("part").GetDouble();
        return type == "text"
            ? new ExtractionSource(
                type,
                part,
                source.GetProperty("start").GetDouble(),
                source.GetProperty("end").GetDouble()
            )
            : new ExtractionSource(type, part);
    }

    private static double? Number(JsonElement element, string name)
    {
        return
            element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;
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
