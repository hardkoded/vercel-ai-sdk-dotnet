// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Vercel.AI.Mcp;

/// <summary>One <c>x-mcp-header</c> binding from a tool input schema.</summary>
public sealed class McpToolHeaderBinding
{
    /// <summary>Creates a binding.</summary>
    public McpToolHeaderBinding(string headerName, IReadOnlyList<string> path, string valueType)
    {
        HeaderName = headerName ?? throw new ArgumentNullException(nameof(headerName));
        Path = path ?? throw new ArgumentNullException(nameof(path));
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
    }

    /// <summary>HTTP header token, preserving the schema's casing.</summary>
    public string HeaderName { get; }

    /// <summary>Property path from the input object to the annotated value.</summary>
    public IReadOnlyList<string> Path { get; }

    /// <summary><c>string</c>, <c>integer</c>, or <c>boolean</c>.</summary>
    public string ValueType { get; }
}

/// <summary>Bindings extracted from a tool input schema, or the reason extraction failed.</summary>
public sealed class McpToolHeaderBindingsResult
{
    private McpToolHeaderBindingsResult(bool success, IReadOnlyList<McpToolHeaderBinding> bindings, string? error)
    {
        Success = success;
        Bindings = bindings;
        Error = error;
    }

    /// <summary>True when <see cref="Bindings"/> is usable.</summary>
    public bool Success { get; }

    /// <summary>Header bindings. Empty when <see cref="Success"/> is false.</summary>
    public IReadOnlyList<McpToolHeaderBinding> Bindings { get; }

    /// <summary>Failure description. Null when <see cref="Success"/> is true.</summary>
    public string? Error { get; }

    /// <summary>Creates a successful result.</summary>
    public static McpToolHeaderBindingsResult Ok(IReadOnlyList<McpToolHeaderBinding> bindings)
    {
        return new McpToolHeaderBindingsResult(true, bindings ?? Array.Empty<McpToolHeaderBinding>(), null);
    }

    /// <summary>Creates a failed result.</summary>
    public static McpToolHeaderBindingsResult Fail(string error)
    {
        return new McpToolHeaderBindingsResult(false, Array.Empty<McpToolHeaderBinding>(), error ?? string.Empty);
    }
}

/// <summary>Reads <c>x-mcp-header</c> annotations and encodes them as <c>Mcp-Param-*</c> headers.</summary>
public static class McpHttpHeaders
{
    private static readonly Regex HttpToken = new("^[!#$%&'*+\\-.^_`|~0-9A-Za-z]+$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex Base64Sentinel = new("^=\\?base64\\?.*\\?=$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Encodes <paramref name="value"/> for an MCP parameter header.
    /// Plain ASCII that is not already a base64 sentinel is left unchanged. Everything else is wrapped as <c>=?base64?...?=</c>.
    /// </summary>
    public static string EncodeHeaderValue(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var plain = true;
        for (var index = 0; index < value.Length; index++)
        {
            var code = value[index];
            if (code != 0x09 && (code < 0x20 || code > 0x7e))
            {
                plain = false;
                break;
            }
        }

        if (plain && value.Trim() == value && !Base64Sentinel.IsMatch(value))
        {
            return value;
        }

        return "=?base64?" + Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) + "?=";
    }

    /// <summary>Walks <paramref name="inputSchema"/> for statically reachable <c>x-mcp-header</c> annotations.</summary>
    public static McpToolHeaderBindingsResult GetBindings(JsonNode? inputSchema)
    {
        if (inputSchema is not JsonObject)
        {
            return McpToolHeaderBindingsResult.Fail("inputSchema must be a JSON Schema object");
        }

        var bindings = new List<McpToolHeaderBinding>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        string? error = null;
        Visit(inputSchema, new List<string>(), true, bindings, names, ref error);
        return error == null
            ? McpToolHeaderBindingsResult.Ok(bindings)
            : McpToolHeaderBindingsResult.Fail(error);
    }

    /// <summary>Builds <c>Mcp-Param-*</c> headers from <paramref name="bindings"/> and <paramref name="args"/>.</summary>
    public static IReadOnlyDictionary<string, string> CreateHeaders(IReadOnlyList<McpToolHeaderBinding> bindings, JsonObject args)
    {
        if (bindings is null)
        {
            throw new ArgumentNullException(nameof(bindings));
        }

        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < bindings.Count; index++)
        {
            var binding = bindings[index];
            var value = ValueAt(args, binding.Path);
            if (value == null)
            {
                continue;
            }

            if (!Matches(binding.ValueType, value, out var rendered))
            {
                throw new InvalidOperationException(
                    "Tool argument \"" + string.Join(".", binding.Path) + "\" does not match its x-mcp-header type");
            }

            headers["Mcp-Param-" + binding.HeaderName] = EncodeHeaderValue(rendered);
        }

        return headers;
    }

    private static void Visit(
        JsonNode? value,
        List<string> path,
        bool staticallyReachable,
        List<McpToolHeaderBinding> bindings,
        HashSet<string> names,
        ref string? error)
    {
        if (error != null || value is not JsonObject obj)
        {
            return;
        }

        if (obj.ContainsKey("x-mcp-header"))
        {
            if (!staticallyReachable || path.Count == 0)
            {
                error = "x-mcp-header is not on a statically reachable property";
                return;
            }

            var headerNode = obj["x-mcp-header"];
            var headerName = headerNode is JsonValue headerValue && headerValue.TryGetValue<string>(out var text) ? text : null;
            if (headerName == null || !HttpToken.IsMatch(headerName))
            {
                error = "x-mcp-header must be a non-empty HTTP token";
                return;
            }

            if (!names.Add(headerName.ToLowerInvariant()))
            {
                error = "x-mcp-header value \"" + headerName + "\" is not unique";
                return;
            }

            var valueType = obj["type"] is JsonValue typeValue && typeValue.TryGetValue<string>(out var type) ? type : null;
            if (valueType != "boolean" && valueType != "integer" && valueType != "string")
            {
                error = "x-mcp-header can only annotate boolean, integer, or string properties";
                return;
            }

            bindings.Add(new McpToolHeaderBinding(headerName, path.ToArray(), valueType));
        }

        foreach (var pair in obj)
        {
            if (pair.Key == "x-mcp-header")
            {
                continue;
            }

            if (pair.Key == "properties" && pair.Value is JsonObject properties)
            {
                foreach (var property in properties)
                {
                    var next = new List<string>(path) { property.Key };
                    Visit(property.Value, next, staticallyReachable, bindings, names, ref error);
                    if (error != null)
                    {
                        return;
                    }
                }
            }
            else
            {
                Visit(pair.Value, path, false, bindings, names, ref error);
                if (error != null)
                {
                    return;
                }
            }
        }
    }

    private static JsonNode? ValueAt(JsonNode? current, IReadOnlyList<string> path)
    {
        for (var index = 0; index < path.Count; index++)
        {
            if (current is not JsonObject obj || !obj.ContainsKey(path[index]))
            {
                return null;
            }

            current = obj[path[index]];
        }

        return current;
    }

    private static bool Matches(string valueType, JsonNode value, out string rendered)
    {
        rendered = string.Empty;
        if (value is not JsonValue json)
        {
            return false;
        }

        if (valueType == "string" && json.TryGetValue<string>(out var text))
        {
            rendered = text;
            return true;
        }

        if (valueType == "boolean" && json.TryGetValue<bool>(out var flag))
        {
            rendered = flag ? "true" : "false";
            return true;
        }

        if (valueType == "integer" && TryInteger(json, out var number))
        {
            rendered = number.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        return false;
    }

    private static bool TryInteger(JsonValue value, out long number)
    {
        if (value.TryGetValue<int>(out var integer))
        {
            number = integer;
            return true;
        }

        if (value.TryGetValue<long>(out var whole) && IsSafe(whole))
        {
            number = whole;
            return true;
        }

        if (value.TryGetValue<double>(out var real) && (!double.IsNaN(real) && !double.IsInfinity(real)) && real == Math.Truncate(real) && IsSafe((long)real))
        {
            number = (long)real;
            return true;
        }

        number = 0;
        return false;
    }

    private static bool IsSafe(long number)
    {
        const long limit = 9007199254740991L;
        return number <= limit && number >= -limit;
    }
}
