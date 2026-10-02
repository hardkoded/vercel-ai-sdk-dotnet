// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;

namespace Vercel.AI.ProviderUtils;

/// <summary>Result of validating one value.</summary>
public sealed class SchemaValidationResult
{
    private SchemaValidationResult(bool success, JsonNode? value, object? error)
    {
        Success = success;
        Value = value;
        Error = error;
    }

    /// <summary>True when validation succeeded.</summary>
    public bool Success { get; }

    /// <summary>Validated value.</summary>
    public JsonNode? Value { get; }

    /// <summary>Failure cause.</summary>
    public object? Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static SchemaValidationResult Ok(JsonNode? value)
    {
        return new SchemaValidationResult(true, value, null);
    }

    /// <summary>Creates a failed result.</summary>
    public static SchemaValidationResult Fail(object? error)
    {
        return new SchemaValidationResult(false, null, error);
    }
}

/// <summary>A schema that can validate a JSON value. Maps to <c>FlexibleSchema</c> without a Zod runtime.</summary>
public sealed class FlexibleSchema
{
    /// <summary>Creates a schema.</summary>
    public FlexibleSchema(Func<JsonNode?, SchemaValidationResult> validate)
    {
        Validate = validate ?? throw new ArgumentNullException(nameof(validate));
    }

    /// <summary>Validation function.</summary>
    public Func<JsonNode?, SchemaValidationResult> Validate { get; }
}

/// <summary>Standard Schema result used by <see cref="StandardSchema"/>.</summary>
public sealed class StandardSchemaResult
{
    /// <summary>True when <see cref="Value"/> should be used.</summary>
    public bool HasValue { get; set; }

    /// <summary>Validated value.</summary>
    public JsonNode? Value { get; set; }

    /// <summary>Validation issues.</summary>
    public IReadOnlyList<object>? Issues { get; set; }
}

/// <summary>Minimal Standard Schema adapter. Maps to the <c>~standard</c> vendor shape.</summary>
public sealed class StandardSchema
{
    /// <summary>Creates a schema.</summary>
    public StandardSchema(string vendor, Func<object?, StandardSchemaResult> validate)
    {
        Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
        Validate = validate ?? throw new ArgumentNullException(nameof(validate));
    }

    /// <summary>Schema vendor name.</summary>
    public string Vendor { get; }

    /// <summary>Validation function.</summary>
    public Func<object?, StandardSchemaResult> Validate { get; }
}

/// <summary>Result of <c>safeParseJSON</c> or <c>safeValidateTypes</c>.</summary>
public sealed class ParseResult
{
    /// <summary>True when parsing and validation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Parsed or validated value.</summary>
    public JsonNode? Value { get; set; }

    /// <summary>Value before schema transformation. Null when parsing failed.</summary>
    public JsonNode? RawValue { get; set; }

    /// <summary>True when <see cref="RawValue"/> was set, including an explicit JSON null.</summary>
    public bool HasRawValue { get; set; }

    /// <summary>Parse or validation error.</summary>
    public SdkError? Error { get; set; }
}

/// <summary>Validates values with a schema. Maps to <c>validateTypes</c> and <c>safeValidateTypes</c>.</summary>
public static class TypeValidation
{
    /// <summary>Validates <paramref name="value"/> and throws <see cref="TypeValidationError"/> on failure.</summary>
    public static JsonNode? ValidateTypes(object? value, object schema, TypeValidationContext? context = null)
    {
        var result = SafeValidateTypes(value, schema, context);
        if (!result.Success)
        {
            throw result.Error ?? TypeValidationError.Wrap(value, new Exception("Type validation failed"), context);
        }

        return result.Value;
    }

    /// <summary>Validates <paramref name="value"/> and returns a result instead of throwing.</summary>
    public static ParseResult SafeValidateTypes(object? value, object schema, TypeValidationContext? context = null)
    {
        var node = value as JsonNode;
        try
        {
            var actual = AsSchema(schema);
            if (actual.Validate == null)
            {
                return new ParseResult { Success = true, Value = node, RawValue = node, HasRawValue = true };
            }

            var validated = actual.Validate(node);
            if (validated.Success)
            {
                return new ParseResult { Success = true, Value = validated.Value, RawValue = node, HasRawValue = true };
            }

            return new ParseResult
            {
                Success = false,
                Error = TypeValidationError.Wrap(value, validated.Error, context),
                RawValue = node,
                HasRawValue = true,
            };
        }
        catch (Exception exception)
        {
            return new ParseResult
            {
                Success = false,
                Error = TypeValidationError.Wrap(value, exception, context),
                RawValue = node,
                HasRawValue = true,
            };
        }
    }

    /// <summary>Adapts a <see cref="FlexibleSchema"/> or <see cref="StandardSchema"/>.</summary>
    public static FlexibleSchema AsSchema(object schema)
    {
        if (schema is null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (schema is FlexibleSchema flexible)
        {
            return flexible;
        }

        if (schema is StandardSchema standard)
        {
            return new FlexibleSchema(value =>
            {
                var result = standard.Validate(value);
                if (result.HasValue)
                {
                    return SchemaValidationResult.Ok(result.Value);
                }

                return SchemaValidationResult.Fail(new TypeValidationError(value, result.Issues));
            });
        }

        throw new ArgumentException("Schema must be a FlexibleSchema or StandardSchema.", nameof(schema));
    }
}

/// <summary>Parses JSON text. Maps to <c>parseJSON</c>, <c>safeParseJSON</c>, and <c>isParsableJson</c>.</summary>
public static class JsonParsing
{
    /// <summary>Parses JSON. When <paramref name="schema"/> is set, the value is validated.</summary>
    public static JsonNode? ParseJson(string text, object? schema = null)
    {
        JsonNode? value;
        try
        {
            value = SecureJson.SecureJsonParse(text);
        }
        catch (Exception exception) when (exception is not JSONParseError && exception is not TypeValidationError)
        {
            throw new JSONParseError(text, exception);
        }

        if (schema is null)
        {
            return value;
        }

        try
        {
            return TypeValidation.ValidateTypes(value, schema);
        }
        catch (Exception exception) when (exception is not JSONParseError && exception is not TypeValidationError)
        {
            throw new JSONParseError(text, exception);
        }
    }

    /// <summary>Parses JSON and returns a result instead of throwing for invalid text.</summary>
    public static ParseResult SafeParseJson(string text, object? schema = null)
    {
        try
        {
            var value = SecureJson.SecureJsonParse(text);
            if (schema is null)
            {
                return new ParseResult { Success = true, Value = value, RawValue = value, HasRawValue = true };
            }

            var validated = TypeValidation.SafeValidateTypes(value, schema);
            return validated;
        }
        catch (Exception exception)
        {
            return new ParseResult
            {
                Success = false,
                Error = exception is JSONParseError parse ? parse : new JSONParseError(text, exception),
                HasRawValue = false,
            };
        }
    }

    /// <summary>True when <paramref name="input"/> can be parsed by <see cref="SecureJson.SecureJsonParse"/>.</summary>
    public static bool IsParsableJson(string input)
    {
        try
        {
            SecureJson.SecureJsonParse(input);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
