// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.GenerateText;
using Vercel.AI.Operations;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Util;

namespace Vercel.AI.GenerateObject;

/// <summary>The result of validating a partial or final value. Maps to <c>ValidationResult</c>.</summary>
internal readonly struct ObjectValidationResult<TValue>
{
    private ObjectValidationResult(bool success, TValue? value, Exception? error)
    {
        Success = success;
        Value = value;
        Error = error;
    }

    public bool Success { get; }

    public TValue? Value { get; }

    public Exception? Error { get; }

    public static ObjectValidationResult<TValue> Ok(TValue value) => new(true, value, null);

    public static ObjectValidationResult<TValue> Fail(Exception error) =>
        new(false, default, error);
}

/// <summary>A validated partial output and the text delta that goes with it.</summary>
internal readonly record struct PartialObjectResult(JsonElement Partial, string TextDelta);

/// <summary>Maps to <c>OutputStrategy</c> of <c>output-strategy.ts</c>.</summary>
internal abstract class ObjectOutputStrategy
{
    /// <summary>Maps to <c>type</c>.</summary>
    public abstract StreamObjectOutput Type { get; }

    /// <summary>Maps to <c>jsonSchema</c>. Null for no-schema output.</summary>
    public abstract JsonElement? JsonSchema { get; }

    /// <summary>Maps to <c>createArrayStreamParser</c>. Only the array strategy has one.</summary>
    public virtual ArrayOutputStreamParser? CreateArrayStreamParser() => null;

    /// <summary>Maps to <c>validatePartialResult</c>.</summary>
    public abstract ObjectValidationResult<PartialObjectResult> ValidatePartialResult(
        JsonElement value,
        string textDelta,
        JsonElement? latestObject,
        bool isFirstDelta,
        bool isFinalDelta
    );

    /// <summary>Maps to <c>validateFinalResult</c>.</summary>
    public abstract ObjectValidationResult<JsonElement> ValidateFinalResult(
        JsonElement? value,
        string text,
        LanguageModelResponseMetadata response,
        LanguageModelUsage usage,
        FinishReason finishReason
    );

    /// <summary>Maps to <c>createElementStream</c>. Returns the elements of one partial array not yet published.</summary>
    public abstract IReadOnlyList<JsonElement> NewElements(JsonElement partial, ref int published);

    /// <summary>Maps to <c>getOutputStrategy</c>.</summary>
    public static ObjectOutputStrategy Create(
        StreamObjectOutput output,
        JsonElement? schema,
        IReadOnlyList<string>? enumValues
    )
    {
        return output switch
        {
            StreamObjectOutput.Object => new ObjectOutputStrategyImpl(schema!.Value),
            StreamObjectOutput.Array => new ArrayOutputStrategyImpl(schema!.Value),
            StreamObjectOutput.Enum => new EnumOutputStrategyImpl(enumValues!),
            StreamObjectOutput.NoSchema => NoSchemaOutputStrategyImpl.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(output)),
        };
    }

    protected static StepResponse AsStepResponse(LanguageModelResponseMetadata response) =>
        response as StepResponse
        ?? new StepResponse(response.Id, response.Timestamp, response.ModelId, response.Headers);

    protected static JsonNode SchemaNode(JsonElement schema) =>
        JsonNode.Parse(schema.GetRawText())!;

    private sealed class NoSchemaOutputStrategyImpl : ObjectOutputStrategy
    {
        public static readonly NoSchemaOutputStrategyImpl Instance = new();

        public override StreamObjectOutput Type => StreamObjectOutput.NoSchema;

        public override JsonElement? JsonSchema => null;

        public override ObjectValidationResult<PartialObjectResult> ValidatePartialResult(
            JsonElement value,
            string textDelta,
            JsonElement? latestObject,
            bool isFirstDelta,
            bool isFinalDelta
        ) => ObjectValidationResult<PartialObjectResult>.Ok(new(value, textDelta));

        public override ObjectValidationResult<JsonElement> ValidateFinalResult(
            JsonElement? value,
            string text,
            LanguageModelResponseMetadata response,
            LanguageModelUsage usage,
            FinishReason finishReason
        ) =>
            value == null
                ? ObjectValidationResult<JsonElement>.Fail(
                    new NoObjectGeneratedException(
                        "No object generated: response did not match schema.",
                        null,
                        text,
                        AsStepResponse(response),
                        usage,
                        finishReason
                    )
                )
                : ObjectValidationResult<JsonElement>.Ok(value.Value);

        public override IReadOnlyList<JsonElement> NewElements(
            JsonElement partial,
            ref int published
        ) =>
            throw new UnsupportedFunctionalityException(
                "element streams in no-schema mode",
                "'element streams in no-schema mode' functionality not supported."
            );
    }

    private sealed class ObjectOutputStrategyImpl(JsonElement schema) : ObjectOutputStrategy
    {
        public override StreamObjectOutput Type => StreamObjectOutput.Object;

        public override JsonElement? JsonSchema => schema;

        public override ObjectValidationResult<PartialObjectResult> ValidatePartialResult(
            JsonElement value,
            string textDelta,
            JsonElement? latestObject,
            bool isFirstDelta,
            bool isFinalDelta
        ) =>
            // Note: currently no validation of partial results:
            ObjectValidationResult<PartialObjectResult>.Ok(new(value, textDelta));

        public override ObjectValidationResult<JsonElement> ValidateFinalResult(
            JsonElement? value,
            string text,
            LanguageModelResponseMetadata response,
            LanguageModelUsage usage,
            FinishReason finishReason
        )
        {
            if (value == null)
            {
                return ObjectValidationResult<JsonElement>.Fail(
                    new ProviderUtils.TypeValidationException("Schema validation failed.", null)
                );
            }

            var result = JsonParsing.SafeValidateTypes(value.Value, SchemaNode(schema));
            return result.Success
                ? ObjectValidationResult<JsonElement>.Ok(result.Value!.Value)
                : ObjectValidationResult<JsonElement>.Fail(result.Error!);
        }

        public override IReadOnlyList<JsonElement> NewElements(
            JsonElement partial,
            ref int published
        ) =>
            throw new UnsupportedFunctionalityException(
                "element streams in object mode",
                "'element streams in object mode' functionality not supported."
            );
    }

    private sealed class ArrayOutputStrategyImpl(JsonElement schema) : ObjectOutputStrategy
    {
        private readonly JsonNode _schemaNode = SchemaNode(schema);

        public override StreamObjectOutput Type => StreamObjectOutput.Array;

        public override ArrayOutputStreamParser CreateArrayStreamParser() =>
            ArrayOutputStream.CreateArrayOutputStreamParser(_schemaNode, false);

        // wrap in object that contains array of elements, since most LLMs will not
        // be able to generate an array directly:
        public override JsonElement? JsonSchema => OutputStrategies.WrapArrayOutputSchema(schema);

        public override ObjectValidationResult<PartialObjectResult> ValidatePartialResult(
            JsonElement value,
            string textDelta,
            JsonElement? latestObject,
            bool isFirstDelta,
            bool isFinalDelta
        )
        {
            // check that the value is an object that contains an array of elements:
            if (
                value.ValueKind != JsonValueKind.Object
                || !value.TryGetProperty("elements", out var elements)
                || elements.ValueKind != JsonValueKind.Array
            )
            {
                return ObjectValidationResult<PartialObjectResult>.Fail(
                    new ProviderUtils.TypeValidationException(
                        "value must be an object that contains an array of elements",
                        value
                    )
                );
            }

            var input = elements.EnumerateArray().ToArray();
            var result = new List<JsonElement>();
            for (var i = 0; i < input.Length; i++)
            {
                var validated = JsonParsing.SafeValidateTypes(input[i], _schemaNode);

                // special treatment for last processed element:
                // ignore parse or validation failures, since they indicate that the
                // last element is incomplete and should not be included in the result,
                // unless it is the final delta
                if (i == input.Length - 1 && !isFinalDelta)
                {
                    continue;
                }

                if (!validated.Success)
                {
                    return ObjectValidationResult<PartialObjectResult>.Fail(validated.Error!);
                }

                result.Add(validated.Value!.Value);
            }

            // calculate delta:
            var publishedElementCount = latestObject is { ValueKind: JsonValueKind.Array } latest
                ? latest.GetArrayLength()
                : 0;
            var delta = new StringBuilder();
            if (isFirstDelta)
            {
                delta.Append('[');
            }

            if (publishedElementCount > 0)
            {
                delta.Append(',');
            }

            delta.Append(
                string.Join(",", result.Skip(publishedElementCount).Select(e => e.GetRawText()))
            );
            if (isFinalDelta)
            {
                delta.Append(']');
            }

            return ObjectValidationResult<PartialObjectResult>.Ok(
                new(JsonSerializer.SerializeToElement(result), delta.ToString())
            );
        }

        public override ObjectValidationResult<JsonElement> ValidateFinalResult(
            JsonElement? value,
            string text,
            LanguageModelResponseMetadata response,
            LanguageModelUsage usage,
            FinishReason finishReason
        )
        {
            if (
                value is not { ValueKind: JsonValueKind.Object } root
                || !root.TryGetProperty("elements", out var elements)
                || elements.ValueKind != JsonValueKind.Array
            )
            {
                return ObjectValidationResult<JsonElement>.Fail(
                    new ProviderUtils.TypeValidationException(
                        "value must be an object that contains an array of elements",
                        value
                    )
                );
            }

            var result = new List<JsonElement>();

            // check that each element in the array is of the correct type:
            foreach (var element in elements.EnumerateArray())
            {
                var validated = JsonParsing.SafeValidateTypes(element, _schemaNode);
                if (!validated.Success)
                {
                    return ObjectValidationResult<JsonElement>.Fail(validated.Error!);
                }

                result.Add(validated.Value!.Value);
            }

            return ObjectValidationResult<JsonElement>.Ok(
                JsonSerializer.SerializeToElement(result)
            );
        }

        public override IReadOnlyList<JsonElement> NewElements(
            JsonElement partial,
            ref int published
        )
        {
            // publish new elements one by one:
            var result = new List<JsonElement>();
            var array = partial.EnumerateArray().ToArray();
            for (; published < array.Length; published++)
            {
                result.Add(array[published]);
            }

            return result;
        }
    }

    private sealed class EnumOutputStrategyImpl(IReadOnlyList<string> enumValues)
        : ObjectOutputStrategy
    {
        public override StreamObjectOutput Type => StreamObjectOutput.Enum;

        // wrap in object that contains result, since most LLMs will not
        // be able to generate an enum value directly:
        public override JsonElement? JsonSchema =>
            JsonSerializer.SerializeToElement(
                new JsonObject
                {
                    ["$schema"] = "http://json-schema.org/draft-07/schema#",
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["result"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["enum"] = new JsonArray(
                                enumValues.Select(v => (JsonNode?)JsonValue.Create(v)).ToArray()
                            ),
                        },
                    },
                    ["required"] = new JsonArray("result"),
                    ["additionalProperties"] = false,
                }
            );

        public override ObjectValidationResult<JsonElement> ValidateFinalResult(
            JsonElement? value,
            string text,
            LanguageModelResponseMetadata response,
            LanguageModelUsage usage,
            FinishReason finishReason
        )
        {
            if (
                value is not { ValueKind: JsonValueKind.Object } root
                || !root.TryGetProperty("result", out var result)
                || result.ValueKind != JsonValueKind.String
            )
            {
                return ObjectValidationResult<JsonElement>.Fail(
                    new ProviderUtils.TypeValidationException(
                        "value must be an object that contains a string in the \"result\" property.",
                        value
                    )
                );
            }

            return enumValues.Contains(result.GetString()!)
                ? ObjectValidationResult<JsonElement>.Ok(result.Clone())
                : ObjectValidationResult<JsonElement>.Fail(
                    new ProviderUtils.TypeValidationException(
                        "value must be a string in the enum",
                        value
                    )
                );
        }

        public override ObjectValidationResult<PartialObjectResult> ValidatePartialResult(
            JsonElement value,
            string textDelta,
            JsonElement? latestObject,
            bool isFirstDelta,
            bool isFinalDelta
        )
        {
            if (
                value.ValueKind != JsonValueKind.Object
                || !value.TryGetProperty("result", out var resultElement)
                || resultElement.ValueKind != JsonValueKind.String
            )
            {
                return ObjectValidationResult<PartialObjectResult>.Fail(
                    new ProviderUtils.TypeValidationException(
                        "value must be an object that contains a string in the \"result\" property.",
                        value
                    )
                );
            }

            var result = resultElement.GetString()!;
            var possibleEnumValues = enumValues
                .Where(v => v.StartsWith(result, StringComparison.Ordinal))
                .ToArray();
            if (result.Length == 0 || possibleEnumValues.Length == 0)
            {
                return ObjectValidationResult<PartialObjectResult>.Fail(
                    new ProviderUtils.TypeValidationException(
                        "value must be a string in the enum",
                        value
                    )
                );
            }

            return ObjectValidationResult<PartialObjectResult>.Ok(
                new(
                    JsonSerializer.SerializeToElement(
                        possibleEnumValues.Length > 1 ? result : possibleEnumValues[0]
                    ),
                    textDelta
                )
            );
        }

        public override IReadOnlyList<JsonElement> NewElements(
            JsonElement partial,
            ref int published
        ) =>
            // no streaming in enum mode
            throw new UnsupportedFunctionalityException(
                "element streams in enum mode",
                "'element streams in enum mode' functionality not supported."
            );
    }
}
