// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Vercel.AI.ProviderUtils;

/// <summary>Maps to <c>stringify-with-bigint-and-cycles.ts</c>.</summary>
public static class StringifyWithBigIntAndCyclesUtil
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        // A null property of a POCO maps to a JavaScript undefined property, which JSON.stringify omits.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new BigIntegerConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
        },
    };

    /// <summary>
    /// Serializes a value as JSON, representing <see cref="BigInteger"/> values as decimal strings and
    /// ancestor cycles as <c>"[Circular]"</c>. Reused objects are serialized each time. A <see cref="JsonConverter"/>
    /// is the .NET mapping of <c>toJSON</c>: it can return a <see cref="BigInteger"/>, and an exception it throws is not hidden.
    /// Dates are written as ISO 8601 UTC with milliseconds, as <c>Date.prototype.toJSON</c> does, and a <see langword="null"/>
    /// POCO property is omitted, as an <c>undefined</c> property is. Maps to <c>stringifyWithBigIntAndCycles</c>.
    /// </summary>
    public static string StringifyWithBigIntAndCycles(object? value)
    {
        var node = ToNode(value, []);
        return node is null ? "null" : node.ToJsonString(s_options);
    }

    private static JsonNode? ToNode(object? value, List<object> ancestors)
    {
        switch (value)
        {
            case null:
                return null;
            case JsonNode json:
                return json.DeepClone();
            case JsonElement element:
                return JsonNode.Parse(element.GetRawText());
            case BigInteger big:
                return JsonValue.Create(big.ToString(CultureInfo.InvariantCulture));
            case string or bool or char:
                return JsonSerializer.SerializeToNode(value, s_options);
            case IDictionary or IEnumerable:
                break;
            default:
                return JsonSerializer.SerializeToNode(value, value.GetType(), s_options);
        }

        // Only objects on the current ancestor path are circular references.
        if (ancestors.Any(ancestor => ReferenceEquals(ancestor, value)))
        {
            return JsonValue.Create("[Circular]");
        }

        ancestors.Add(value);
        try
        {
            if (value is IDictionary dictionary)
            {
                var obj = new JsonObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    obj[Convert.ToString(entry.Key, CultureInfo.InvariantCulture)!] = ToNode(
                        entry.Value,
                        ancestors
                    );
                }

                return obj;
            }

            var array = new JsonArray();
            foreach (var item in (IEnumerable)value)
            {
                array.Add(ToNode(item, ancestors));
            }

            return array;
        }
        finally
        {
            ancestors.RemoveAt(ancestors.Count - 1);
        }
    }

    private sealed class BigIntegerConverter : JsonConverter<BigInteger>
    {
        public override BigInteger Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => BigInteger.Parse(reader.GetString()!, CultureInfo.InvariantCulture);

        public override void Write(
            Utf8JsonWriter writer,
            BigInteger value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }

    private sealed class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => reader.GetDateTime();

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(ToIsoString(value.ToUniversalTime()));
    }

    private sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => reader.GetDateTimeOffset();

        public override void Write(
            Utf8JsonWriter writer,
            DateTimeOffset value,
            JsonSerializerOptions options
        ) => writer.WriteStringValue(ToIsoString(value.UtcDateTime));
    }

    private static string ToIsoString(DateTime utc) =>
        utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
