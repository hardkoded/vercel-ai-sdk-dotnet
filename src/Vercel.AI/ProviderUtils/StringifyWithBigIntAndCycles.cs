// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.ProviderUtils;

/// <summary>Maps to <c>stringify-with-bigint-and-cycles.ts</c>.</summary>
public static class StringifyWithBigIntAndCyclesUtil
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>
    /// Serializes a value as JSON, representing <see cref="BigInteger"/> values as decimal strings and
    /// ancestor cycles as <c>"[Circular]"</c>. Reused objects are serialized each time. Maps to <c>stringifyWithBigIntAndCycles</c>.
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
}
