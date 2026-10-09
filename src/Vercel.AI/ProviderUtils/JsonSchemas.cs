// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Vercel.AI.ProviderUtils;

/// <summary>Builds JSON Schema documents from explicit shapes. This is not a Zod converter.</summary>
public static class JsonSchemas
{
    private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Empty object schema used when a caller does not supply one.</summary>
    public static JsonObject EmptyObject()
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject(),
            ["additionalProperties"] = false,
        };
    }

    /// <summary>Schema that accepts any value.</summary>
    public static JsonObject Anything()
    {
        return new JsonObject();
    }

    /// <summary>JSON Schema <c>not</c> of an empty schema, used for an explicit undefined alternative.</summary>
    public static JsonObject NotEmpty()
    {
        return new JsonObject
        {
            ["not"] = new JsonObject(),
        };
    }

    /// <summary>String schema.</summary>
    public static JsonObject String(
        int? minLength = null,
        int? maxLength = null,
        string? format = null,
        string? pattern = null,
        string? contentEncoding = null,
        JsonArray? allOf = null,
        JsonArray? anyOf = null)
    {
        var schema = new JsonObject { ["type"] = "string" };
        Set(schema, "minLength", minLength);
        Set(schema, "maxLength", maxLength);
        Set(schema, "format", format);
        Set(schema, "pattern", pattern);
        Set(schema, "contentEncoding", contentEncoding);
        if (allOf != null)
        {
            schema["allOf"] = allOf;
        }

        if (anyOf != null)
        {
            schema["anyOf"] = anyOf;
        }

        return schema;
    }

    /// <summary>Number schema.</summary>
    public static JsonObject Number(
        double? minimum = null,
        double? maximum = null,
        double? exclusiveMinimum = null,
        double? exclusiveMaximum = null,
        double? multipleOf = null)
    {
        var schema = new JsonObject { ["type"] = "number" };
        Set(schema, "minimum", minimum);
        Set(schema, "maximum", maximum);
        Set(schema, "exclusiveMinimum", exclusiveMinimum);
        Set(schema, "exclusiveMaximum", exclusiveMaximum);
        Set(schema, "multipleOf", multipleOf);
        return schema;
    }

    /// <summary>Integer schema.</summary>
    public static JsonObject Integer(double? minimum = null, double? maximum = null, string? format = null)
    {
        var schema = new JsonObject { ["type"] = "integer" };
        Set(schema, "format", format);
        Set(schema, "minimum", minimum);
        Set(schema, "maximum", maximum);
        return schema;
    }

    /// <summary>Boolean schema.</summary>
    public static JsonObject Boolean()
    {
        return new JsonObject { ["type"] = "boolean" };
    }

    /// <summary>Null schema.</summary>
    public static JsonObject Null()
    {
        return new JsonObject { ["type"] = "null" };
    }

    /// <summary>Array schema. <paramref name="items"/> may be a schema or a tuple of schemas.</summary>
    public static JsonObject Array(JsonNode? items = null, int? minItems = null, int? maxItems = null, JsonNode? additionalItems = null)
    {
        var schema = new JsonObject { ["type"] = "array" };
        if (items != null)
        {
            schema["items"] = items;
        }

        Set(schema, "minItems", minItems);
        Set(schema, "maxItems", maxItems);
        if (additionalItems != null)
        {
            schema["additionalItems"] = additionalItems;
        }

        return schema;
    }

    /// <summary>
    /// Object schema. Pass <paramref name="additionalProperties"/> for a schema, or
    /// <paramref name="additionalPropertiesFlag"/> for true or false. Omit both to leave the keyword unset.
    /// </summary>
    public static JsonObject Object(
        IReadOnlyList<KeyValuePair<string, JsonNode>>? properties = null,
        IReadOnlyList<string>? required = null,
        JsonNode? additionalProperties = null,
        bool? additionalPropertiesFlag = null,
        JsonNode? defaultValue = null)
    {
        var schema = new JsonObject { ["type"] = "object" };
        if (properties != null)
        {
            var map = new JsonObject();
            foreach (var property in properties)
            {
                map[property.Key] = property.Value;
            }

            schema["properties"] = map;
        }

        if (required != null)
        {
            var names = new JsonArray();
            foreach (var name in required)
            {
                names.Add(name);
            }

            schema["required"] = names;
        }

        if (additionalProperties != null)
        {
            schema["additionalProperties"] = additionalProperties;
        }
        else if (additionalPropertiesFlag is bool flag)
        {
            schema["additionalProperties"] = flag;
        }

        if (defaultValue != null)
        {
            schema["default"] = defaultValue;
        }

        return schema;
    }

    /// <summary>Enum schema. <paramref name="type"/> may be a string or an array of type names.</summary>
    public static JsonObject Enum(JsonNode type, JsonArray values)
    {
        return new JsonObject
        {
            ["type"] = type,
            ["enum"] = values,
        };
    }

    /// <summary>A single literal.</summary>
    public static JsonObject Literal(string type, JsonNode constant, string? description = null)
    {
        var schema = new JsonObject
        {
            ["type"] = type,
            ["const"] = constant,
        };
        Set(schema, "description", description);
        return schema;
    }

    /// <summary><c>anyOf</c> schema.</summary>
    public static JsonObject AnyOf(params JsonNode[] schemas)
    {
        var array = new JsonArray();
        foreach (var schema in schemas)
        {
            array.Add(schema);
        }

        return new JsonObject { ["anyOf"] = array };
    }

    /// <summary>Union of primitive type names.</summary>
    public static JsonObject PrimitiveUnion(params string[] types)
    {
        var array = new JsonArray();
        foreach (var type in types)
        {
            array.Add(type);
        }

        return new JsonObject { ["type"] = array };
    }

    /// <summary>Object whose values match <paramref name="additionalProperties"/>.</summary>
    public static JsonObject Record(JsonNode additionalProperties, JsonNode? propertyNames = null)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = additionalProperties,
        };
        if (propertyNames != null)
        {
            schema["propertyNames"] = propertyNames;
        }

        return schema;
    }

    /// <summary>Date-time string.</summary>
    public static JsonObject DateTimeString()
    {
        return String(format: "date-time");
    }

    /// <summary>Copies <paramref name="schema"/> and sets <c>default</c>.</summary>
    public static JsonObject WithDefault(JsonObject schema, JsonNode defaultValue)
    {
        var copy = (JsonObject)schema.DeepClone();
        copy["default"] = defaultValue;
        return copy;
    }

    /// <summary>Escapes a literal for a JSON Schema pattern.</summary>
    public static string EscapePattern(string literal)
    {
        var builder = new StringBuilder(literal.Length);
        foreach (var character in literal)
        {
            if ("\\^$.|?*+()[]{}".IndexOf(character) >= 0)
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    /// <summary>Pattern that anchors <paramref name="literal"/> at the start.</summary>
    public static string StartsWithPattern(string literal)
    {
        return "^" + EscapePattern(literal);
    }

    /// <summary>Pattern that anchors <paramref name="literal"/> at the end.</summary>
    public static string EndsWithPattern(string literal)
    {
        return EscapePattern(literal) + "$";
    }

    /// <summary>Pattern that contains <paramref name="literal"/>.</summary>
    public static string IncludesPattern(string literal)
    {
        return EscapePattern(literal);
    }

    /// <summary>Serializes a schema the way <c>JSON.stringify</c> does: no extra whitespace, insertion order.</summary>
    public static string Stringify(JsonNode? node)
    {
        if (node is null)
        {
            return "null";
        }

        return RelaxNonAscii(node.ToJsonString(Compact));
    }

    /// <summary>
    /// Turns <c>\u</c> escapes above ASCII back into characters. System.Text.Json writes emoji as surrogate escapes
    /// even when the encoder allows them, and JSON.stringify leaves those characters in place.
    /// </summary>
    private static string RelaxNonAscii(string json)
    {
        var builder = new StringBuilder(json.Length);
        for (var i = 0; i < json.Length; i++)
        {
            if (json[i] == '\\' && i + 5 < json.Length && json[i + 1] == 'u' && TryHex(json, i + 2, out var code))
            {
                if (code >= 0xD800 && code <= 0xDBFF
                    && i + 11 < json.Length
                    && json[i + 6] == '\\'
                    && json[i + 7] == 'u'
                    && TryHex(json, i + 8, out var low)
                    && low >= 0xDC00
                    && low <= 0xDFFF)
                {
                    var rune = ((code - 0xD800) << 10) + (low - 0xDC00) + 0x10000;
                    builder.Append(char.ConvertFromUtf32(rune));
                    i += 11;
                    continue;
                }

                if (code >= 0x80)
                {
                    builder.Append((char)code);
                    i += 5;
                    continue;
                }
            }

            builder.Append(json[i]);
        }

        return builder.ToString();
    }

    private static bool TryHex(string text, int start, out int value)
    {
        value = 0;
        for (var i = 0; i < 4; i++)
        {
            var digit = text[start + i];
            int nibble;
            if (digit >= '0' && digit <= '9')
            {
                nibble = digit - '0';
            }
            else if (digit >= 'a' && digit <= 'f')
            {
                nibble = digit - 'a' + 10;
            }
            else if (digit >= 'A' && digit <= 'F')
            {
                nibble = digit - 'A' + 10;
            }
            else
            {
                return false;
            }

            value = (value << 4) + nibble;
        }

        return true;
    }

    /// <summary>
    /// Sets <c>additionalProperties</c> to false on object schemas, including objects nested in
    /// combinators, items, and definitions. A schema-valued <c>additionalProperties</c> is kept and visited.
    /// </summary>
    public static JsonObject AddAdditionalProperties(JsonObject schema)
    {
        if (schema is null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (IsObjectType(schema))
        {
            if (schema["additionalProperties"] is JsonObject additional)
            {
                AddAdditionalProperties(additional);
            }
            else
            {
                schema["additionalProperties"] = false;
            }

            if (schema["properties"] is JsonObject properties)
            {
                foreach (var property in properties)
                {
                    if (property.Value is JsonObject child)
                    {
                        AddAdditionalProperties(child);
                    }
                }
            }
        }

        VisitList(schema, "items");
        VisitList(schema, "anyOf");
        VisitList(schema, "allOf");
        VisitList(schema, "oneOf");
        if (schema["definitions"] is JsonObject definitions)
        {
            foreach (var property in definitions)
            {
                if (property.Value is JsonObject child)
                {
                    AddAdditionalProperties(child);
                }
            }
        }

        return schema;
    }

    private static void VisitList(JsonObject schema, string name)
    {
        if (schema[name] is JsonObject child)
        {
            AddAdditionalProperties(child);
            return;
        }

        if (schema[name] is not JsonArray array)
        {
            return;
        }

        foreach (var item in array)
        {
            if (item is JsonObject childSchema)
            {
                AddAdditionalProperties(childSchema);
            }
        }
    }

    private static bool IsObjectType(JsonObject schema)
    {
        if (schema["type"] is JsonValue type && type.TryGetValue<string>(out var name))
        {
            return name == "object";
        }

        if (schema["type"] is JsonArray types)
        {
            foreach (var item in types)
            {
                if (item is JsonValue value && value.TryGetValue<string>(out var typeName) && typeName == "object")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void Set(JsonObject schema, string name, int? value)
    {
        if (value is int number)
        {
            schema[name] = number;
        }
    }

    private static void Set(JsonObject schema, string name, double? value)
    {
        if (value is not double number)
        {
            return;
        }

        if (number == Math.Floor(number) && number <= long.MaxValue && number >= long.MinValue)
        {
            var whole = (long)number;
            if (whole <= int.MaxValue && whole >= int.MinValue)
            {
                schema[name] = (int)whole;
            }
            else
            {
                schema[name] = whole;
            }

            return;
        }

        schema[name] = number;
    }

    private static void Set(JsonObject schema, string name, string? value)
    {
        if (value != null)
        {
            schema[name] = value;
        }
    }
}

/// <summary>Appends a JSON schema instruction to a prompt or to the first system message.</summary>
public static class JsonInstructions
{
    private const string SchemaPrefix = "JSON schema:";
    private const string SchemaSuffix = "You MUST answer with a JSON object that matches the JSON schema above.";
    private const string GenericSuffix = "You MUST answer with JSON.";

    /// <summary>One prompt message. <see cref="Content"/> is a string for system text, or any other JSON value.</summary>
    public sealed class PromptMessage
    {
        /// <summary>Creates a message.</summary>
        public PromptMessage(string role, object? content)
        {
            Role = role ?? string.Empty;
            Content = content;
        }

        /// <summary>Message role.</summary>
        public string Role { get; }

        /// <summary>Message content.</summary>
        public object? Content { get; }
    }

    /// <summary>Joins an optional prompt with a JSON schema instruction.</summary>
    public static string Inject(string? prompt, JsonNode? schema, string? schemaPrefix = null, string? schemaSuffix = null, bool prefixSpecified = false, bool suffixSpecified = false)
    {
        var prefix = prefixSpecified ? schemaPrefix : schema != null ? SchemaPrefix : null;
        var suffix = suffixSpecified ? schemaSuffix : schema != null ? SchemaSuffix : GenericSuffix;
        var lines = new List<string>();
        if (!string.IsNullOrEmpty(prompt))
        {
            lines.Add(prompt!);
            lines.Add(string.Empty);
        }

        if (prefix != null)
        {
            lines.Add(prefix);
        }

        if (schema != null)
        {
            lines.Add(JsonSchemas.Stringify(schema));
        }

        if (suffix != null)
        {
            lines.Add(suffix);
        }

        return string.Join("\n", lines);
    }

    /// <summary>Writes the instruction into the first system message, or inserts one.</summary>
    public static IReadOnlyList<PromptMessage> InjectIntoMessages(
        IReadOnlyList<PromptMessage> messages,
        JsonNode? schema = null,
        string? schemaPrefix = null,
        string? schemaSuffix = null,
        bool prefixSpecified = false,
        bool suffixSpecified = false)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        var hasSystem = messages.Count > 0 && messages[0].Role == "system";
        var systemContent = hasSystem && messages[0].Content is string text ? text : string.Empty;
        var system = new PromptMessage(
            "system",
            Inject(systemContent, schema, schemaPrefix, schemaSuffix, prefixSpecified, suffixSpecified));
        var result = new List<PromptMessage> { system };
        var start = hasSystem ? 1 : 0;
        for (var index = start; index < messages.Count; index++)
        {
            result.Add(messages[index]);
        }

        return result;
    }
}

/// <summary>Reads JSON and Server-Sent Event bodies, including response headers.</summary>
public static class JsonStreams
{
    /// <summary>Lower-cases response and content header names and joins repeated values with a comma.</summary>
    public static IReadOnlyDictionary<string, string> ExtractResponseHeaders(HttpResponseMessage response)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        Copy(headers, response.Headers);
        if (response.Content != null)
        {
            Copy(headers, response.Content.Headers);
        }

        return headers;
    }

    /// <summary>A parsed JSON body and the headers that came with it.</summary>
    public sealed class JsonBody
    {
        /// <summary>Creates a body.</summary>
        public JsonBody(JsonElement value, JsonElement rawValue, IReadOnlyDictionary<string, string> headers)
        {
            Value = value;
            RawValue = rawValue;
            Headers = headers;
        }

        /// <summary>Value after the schema projection.</summary>
        public JsonElement Value { get; }

        /// <summary>Parsed JSON before projection.</summary>
        public JsonElement RawValue { get; }

        /// <summary>Response headers.</summary>
        public IReadOnlyDictionary<string, string> Headers { get; }
    }

    /// <summary>Reads a JSON response. Unknown object properties are removed when the schema says so.</summary>
    public static async Task<JsonBody> ReadJsonAsync(HttpResponseMessage response, JsonNode? schema, CancellationToken cancellationToken = default)
    {
        if (response is null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var text = response.Content == null
            ? string.Empty
            : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var parsed = JsonParsing.SafeParse(text, schema);
        if (!parsed.Success || parsed.Value is null || parsed.RawValue is null)
        {
            throw parsed.Error ?? new JsonParseException(text, new JsonException("Invalid JSON response"));
        }

        return new JsonBody(parsed.Value.Value, parsed.RawValue.Value, ExtractResponseHeaders(response));
    }

    /// <summary>Default largest JSON Lines row, in UTF-8 bytes.</summary>
    public const int DefaultMaxLineBytes = 64 * 1024 * 1024;

    /// <summary>
    /// Yields one JSON value per non-empty line. Lines may be split across reads.
    /// A final line without a newline is still yielded. Early disposal cancels <paramref name="onCancel"/>.
    /// A row longer than <paramref name="maxLineBytes"/> UTF-8 bytes (the newline excluded) throws a
    /// <see cref="Util.DownloadError"/> for <paramref name="url"/> and cancels the body.
    /// </summary>
    public static async IAsyncEnumerable<JsonElement> ReadJsonLinesAsync(
        Stream? stream,
        JsonNode? schema = null,
        Action? onCancel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        int maxLineBytes = DefaultMaxLineBytes,
        string url = "")
    {
        if (maxLineBytes <= 0)
        {
            throw new Util.InvalidArgumentError("maxLineBytes", maxLineBytes, "maxLineBytes must be a positive safe integer.");
        }

        if (stream is null)
        {
            throw new InvalidOperationException("Empty response body");
        }

        var finished = false;
        try
        {
            var line = new MemoryStream();
            var chunk = new byte[4096];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false);
                if (count == 0)
                {
                    finished = true;
                    break;
                }

                var start = 0;
                while (start < count)
                {
                    var newline = Array.IndexOf(chunk, (byte)'\n', start, count - start);
                    var end = newline < 0 ? count : newline;
                    if (line.Length + (end - start) > maxLineBytes)
                    {
                        finished = true;
                        try
                        {
                            onCancel?.Invoke();
                        }
                        catch (Exception)
                        {
                            // The size error is the one the caller needs to see.
                        }

                        throw new Util.DownloadError(url, "JSON Lines response exceeded maximum line size of " + maxLineBytes + " bytes.");
                    }

                    line.Write(chunk, start, end - start);
                    if (newline < 0)
                    {
                        break;
                    }

                    var text = DecodeLine(line);
                    if (text != null)
                    {
                        yield return JsonParsing.Parse(text, schema);
                    }

                    start = newline + 1;
                }
            }

            var tail = DecodeLine(line);
            if (tail != null)
            {
                yield return JsonParsing.Parse(tail, schema);
            }
        }
        finally
        {
            if (!finished && onCancel != null)
            {
                onCancel();
            }
        }
    }

    /// <summary>
    /// Parses a Server-Sent Event stream into JSON values. <c>data: [DONE]</c> is ignored.
    /// Events are validated with <paramref name="schema"/> when it is supplied.
    /// </summary>
    public static async IAsyncEnumerable<JsonParseResult> ReadJsonEventsAsync(
        Stream stream,
        JsonNode? schema = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var parser = new EventSourceParser();
        var pending = new Queue<EventSourceMessage>();
        using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
        {
            var chunk = new char[256];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = await reader.ReadAsync(chunk, 0, chunk.Length).ConfigureAwait(false);
                if (count == 0)
                {
                    break;
                }

                parser.Feed(new string(chunk, 0, count), message => pending.Enqueue(message));
                while (pending.Count > 0)
                {
                    var message = pending.Dequeue();
                    if (message.Data == "[DONE]")
                    {
                        continue;
                    }

                    yield return JsonParsing.SafeParse(message.Data, schema);
                }
            }
        }
    }

    private static string? DecodeLine(MemoryStream line)
    {
        var bytes = line.GetBuffer();
        var length = (int)line.Length;
        line.SetLength(0);
        var offset = length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
        if (length > offset && bytes[length - 1] == (byte)'\r')
        {
            length--;
        }

        var text = Encoding.UTF8.GetString(bytes, offset, length - offset);
        return text.Trim().Length > 0 ? text : null;
    }

    private static void Copy(Dictionary<string, string> target, System.Net.Http.Headers.HttpHeaders headers)
    {
        foreach (var header in headers)
        {
            target[header.Key.ToLowerInvariant()] = string.Join(", ", header.Value);
        }
    }
}
