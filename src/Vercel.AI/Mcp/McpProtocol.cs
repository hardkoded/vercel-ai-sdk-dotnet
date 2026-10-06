// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mcp;

/// <summary>Checks JSON-RPC 2.0 requests, notifications, results, and errors.</summary>
public static class JsonRpcMessages
{
    /// <summary>Parses one JSON-RPC message. Prototype-pollution keys are rejected.</summary>
    public static JsonElement Parse(string text)
    {
        var value = JsonParsing.Parse(text);
        Validate(value);
        return value;
    }

    /// <summary>Throws <see cref="AiSdkException"/> when <paramref name="message"/> is not a JSON-RPC 2.0 message.</summary>
    public static void Validate(JsonElement message)
    {
        if (message.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("JSON-RPC message must be an object.");
        }

        var version = ReadString(message, "jsonrpc");
        if (version != "2.0")
        {
            throw new AiSdkException("JSON-RPC version must be \"2.0\".");
        }

        var hasMethod = message.TryGetProperty("method", out var method);
        var hasResult = message.TryGetProperty("result", out _);
        var hasError = message.TryGetProperty("error", out var error);
        var hasId = message.TryGetProperty("id", out var id);
        if (hasMethod)
        {
            if (method.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(method.GetString()))
            {
                throw new AiSdkException("JSON-RPC method must be a string.");
            }

            if (hasId && !IsId(id))
            {
                throw new AiSdkException("JSON-RPC id must be a string or an integer.");
            }

            if (hasResult || hasError)
            {
                throw new AiSdkException("JSON-RPC request cannot include result or error.");
            }

            return;
        }

        if (hasResult)
        {
            if (!hasId || !IsId(id))
            {
                throw new AiSdkException("JSON-RPC result requires an id.");
            }

            if (hasError)
            {
                throw new AiSdkException("JSON-RPC result cannot include error.");
            }

            return;
        }

        if (hasError)
        {
            if (hasId && id.ValueKind != JsonValueKind.Null && !IsId(id))
            {
                throw new AiSdkException("JSON-RPC error id must be a string, an integer, or null.");
            }

            if (error.ValueKind != JsonValueKind.Object
                || !error.TryGetProperty("code", out var code)
                || code.ValueKind != JsonValueKind.Number
                || code.GetRawText().IndexOf('.') >= 0
                || !error.TryGetProperty("message", out var errorMessage)
                || errorMessage.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("JSON-RPC error must include an integer code and a message.");
            }

            return;
        }

        throw new AiSdkException("JSON-RPC message must be a request, notification, result, or error.");
    }

    private static bool IsId(JsonElement id)
    {
        if (id.ValueKind == JsonValueKind.String)
        {
            return true;
        }

        return id.ValueKind == JsonValueKind.Number
            && id.TryGetInt64(out _)
            && id.GetRawText().IndexOf('.') < 0
            && id.GetRawText().IndexOf('e') < 0
            && id.GetRawText().IndexOf('E') < 0;
    }

    private static string? ReadString(JsonElement message, string name)
    {
        if (!message.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return value.GetString();
    }
}

/// <summary>Normalizes MCP <c>tools/call</c> results, including structured-only results.</summary>
public static class CallToolResults
{
    /// <summary>A value that is not JSON, matching an omitted field.</summary>
    public static object Undefined { get; } = new object();

    /// <summary>True when <paramref name="value"/> can be JSON structured content.</summary>
    public static bool IsJsonValue(object? value)
    {
        if (ReferenceEquals(value, Undefined))
        {
            return false;
        }

        if (value is double number && (double.IsNaN(number) || double.IsInfinity(number)))
        {
            return false;
        }

        if (value is float single && (float.IsNaN(single) || float.IsInfinity(single)))
        {
            return false;
        }

        if (value is System.Numerics.BigInteger)
        {
            return false;
        }

        return true;
    }

    /// <summary>Normalizes a call-tool result.</summary>
    public static JsonElement Normalize(JsonElement result)
    {
        if (result.ValueKind != JsonValueKind.Object)
        {
            throw new AiSdkException("Call tool result must be an object.");
        }

        if (HasContent(result))
        {
            ValidateContent(result);
            return WithDefaultError(result);
        }

        if (result.TryGetProperty("structuredContent", out var structured))
        {
            var text = structured.GetRawText();
            var isError = ReadError(result);
            using (var document = JsonDocument.Parse(
                "{\"content\":[{\"type\":\"text\",\"text\":" + Quote(text) + "}],\"structuredContent\":" + text + ",\"isError\":" + (isError ? "true" : "false") + "}"))
            {
                return document.RootElement.Clone();
            }
        }

        if (result.TryGetProperty("toolResult", out var toolResult) && toolResult.ValueKind != JsonValueKind.Undefined)
        {
            return result.Clone();
        }

        throw new AiSdkException("Call tool result requires content, structuredContent, or toolResult.");
    }

    /// <summary>Normalizes a call-tool result held as a JSON node so cycles can be rejected.</summary>
    public static JsonElement Normalize(JsonNode result)
    {
        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        EnsureAcyclic(result, new HashSet<JsonNode>());
        using (var document = JsonDocument.Parse(result.ToJsonString()))
        {
            return Normalize(document.RootElement.Clone());
        }
    }

    private static bool HasContent(JsonElement result)
    {
        return result.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array;
    }

    private static void ValidateContent(JsonElement result)
    {
        foreach (var part in result.GetProperty("content").EnumerateArray())
        {
            if (part.ValueKind != JsonValueKind.Object || !part.TryGetProperty("type", out var type) || type.ValueKind != JsonValueKind.String)
            {
                throw new AiSdkException("Call tool content is malformed.");
            }

            var kind = type.GetString();
            if (kind == "text")
            {
                if (!part.TryGetProperty("text", out var text) || text.ValueKind != JsonValueKind.String)
                {
                    throw new AiSdkException("Text content requires a string text field.");
                }
            }
            else if (kind == "image")
            {
                if (!part.TryGetProperty("mimeType", out var mime) || mime.ValueKind != JsonValueKind.String
                    || !part.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.String
                    || !IsBase64(data.GetString() ?? string.Empty))
                {
                    throw new AiSdkException("Image content requires base64 data and a mime type.");
                }
            }
            else if (kind == "resource")
            {
                if (!part.TryGetProperty("resource", out var resource) || resource.ValueKind != JsonValueKind.Object)
                {
                    throw new AiSdkException("Resource content requires a resource object.");
                }
            }
            else if (kind == "resource_link")
            {
                if (!part.TryGetProperty("uri", out var uri) || uri.ValueKind != JsonValueKind.String
                    || !part.TryGetProperty("name", out var name) || name.ValueKind != JsonValueKind.String)
                {
                    throw new AiSdkException("Resource link content requires a uri and a name.");
                }
            }
        }
    }

    private static JsonElement WithDefaultError(JsonElement result)
    {
        if (result.TryGetProperty("isError", out _))
        {
            return result.Clone();
        }

        var raw = result.GetRawText();
        if (raw.Length == 0 || raw[raw.Length - 1] != '}')
        {
            throw new AiSdkException("Call tool result must be a JSON object.");
        }

        using (var document = JsonDocument.Parse(raw.Substring(0, raw.Length - 1) + ",\"isError\":false}"))
        {
            return document.RootElement.Clone();
        }
    }

    private static bool ReadError(JsonElement result)
    {
        return result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True;
    }

    private static void EnsureAcyclic(JsonNode? node, HashSet<JsonNode> seen)
    {
        if (node is null || node is JsonValue)
        {
            return;
        }

        if (!seen.Add(node))
        {
            throw new AiSdkException("Structured content contains a cycle.");
        }

        if (node is JsonObject obj)
        {
            foreach (var property in obj)
            {
                EnsureAcyclic(property.Value, seen);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                EnsureAcyclic(item, seen);
            }
        }

        seen.Remove(node);
    }

    private static bool IsBase64(string value)
    {
        if (value.Length == 0 || value.Length % 4 != 0)
        {
            return false;
        }

        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string Quote(string rawJson)
    {
        return System.Text.Json.JsonSerializer.Serialize(rawJson);
    }
}

/// <summary>Reads <c>x-mcp-header</c> annotations and encodes <c>Mcp-Param-*</c> headers.</summary>
public static class McpToolHeaders
{
    private static readonly Regex HttpToken = new Regex("^[!#$%&'*+\\-.^_`|~0-9A-Za-z]+$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Base64Sentinel = new Regex("^=\\?base64\\?.*\\?=$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>One header bound to a tool argument path.</summary>
    public sealed class Binding
    {
        /// <summary>Creates a binding.</summary>
        public Binding(string headerName, IReadOnlyList<string> path, string valueType)
        {
            HeaderName = headerName;
            Path = path;
            ValueType = valueType;
        }

        /// <summary>HTTP header token from <c>x-mcp-header</c>.</summary>
        public string HeaderName { get; }

        /// <summary>Property path from the input object.</summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary><c>string</c>, <c>integer</c>, or <c>boolean</c>.</summary>
        public string ValueType { get; }
    }

    /// <summary>Result of reading header bindings.</summary>
    public sealed class BindingResult
    {
        private BindingResult(bool success, IReadOnlyList<Binding> bindings, string? error)
        {
            Success = success;
            Bindings = bindings;
            Error = error;
        }

        /// <summary>True when every annotation was valid.</summary>
        public bool Success { get; }

        /// <summary>Bindings in visit order.</summary>
        public IReadOnlyList<Binding> Bindings { get; }

        /// <summary>Error text when <see cref="Success"/> is false.</summary>
        public string? Error { get; }

        /// <summary>Creates a success result.</summary>
        public static BindingResult Ok(IReadOnlyList<Binding> bindings)
        {
            return new BindingResult(true, bindings, null);
        }

        /// <summary>Creates a failure result.</summary>
        public static BindingResult Fail(string error)
        {
            return new BindingResult(false, Array.Empty<Binding>(), error);
        }
    }

    /// <summary>Encodes a header value, using the base64 sentinel when the text is not plain ASCII.</summary>
    public static string EncodeValue(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var plain = true;
        foreach (var character in value)
        {
            var code = (int)character;
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

        return "=?base64?" + ByteEncoding.ToBase64(System.Text.Encoding.UTF8.GetBytes(value)) + "?=";
    }

    /// <summary>Collects statically reachable <c>x-mcp-header</c> annotations.</summary>
    public static BindingResult GetBindings(JsonElement inputSchema)
    {
        if (inputSchema.ValueKind != JsonValueKind.Object)
        {
            return BindingResult.Fail("inputSchema must be a JSON Schema object");
        }

        var bindings = new List<Binding>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        string? error = null;
        Visit(inputSchema, new List<string>(), true, bindings, names, ref error);
        return error == null ? BindingResult.Ok(bindings) : BindingResult.Fail(error);
    }

    /// <summary>Builds <c>Mcp-Param-*</c> headers from tool arguments.</summary>
    public static IReadOnlyDictionary<string, string> CreateHeaders(IReadOnlyList<Binding> bindings, JsonElement arguments)
    {
        if (bindings is null)
        {
            throw new ArgumentNullException(nameof(bindings));
        }

        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var binding in bindings)
        {
            if (!TryGet(arguments, binding.Path, out var value) || value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            {
                continue;
            }

            if (!Matches(binding.ValueType, value))
            {
                throw new InvalidOperationException("Tool argument \"" + string.Join(".", binding.Path) + "\" does not match its x-mcp-header type");
            }

            var text = binding.ValueType == "string" ? value.GetString() ?? string.Empty : value.ValueKind == JsonValueKind.True ? "true" : value.ValueKind == JsonValueKind.False ? "false" : value.GetRawText();
            headers["Mcp-Param-" + binding.HeaderName] = EncodeValue(text);
        }

        return headers;
    }

    private static void Visit(JsonElement value, List<string> path, bool staticallyReachable, List<Binding> bindings, HashSet<string> names, ref string? error)
    {
        if (error != null || value.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (value.TryGetProperty("x-mcp-header", out var headerName))
        {
            if (!staticallyReachable || path.Count == 0)
            {
                error = "x-mcp-header is not on a statically reachable property";
                return;
            }

            var token = headerName.ValueKind == JsonValueKind.String ? headerName.GetString() : null;
            if (string.IsNullOrEmpty(token) || !HttpToken.IsMatch(token!))
            {
                error = "x-mcp-header must be a non-empty HTTP token";
                return;
            }

            if (!names.Add(token!.ToLowerInvariant()))
            {
                error = "x-mcp-header value \"" + token + "\" is not unique";
                return;
            }

            var valueType = value.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String ? type.GetString() : null;
            if (valueType != "boolean" && valueType != "integer" && valueType != "string")
            {
                error = "x-mcp-header can only annotate boolean, integer, or string properties";
                return;
            }

            bindings.Add(new Binding(token!, path.ToArray(), valueType!));
        }

        foreach (var property in value.EnumerateObject())
        {
            if (property.NameEquals("x-mcp-header"))
            {
                continue;
            }

            if (property.NameEquals("properties") && property.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var child in property.Value.EnumerateObject())
                {
                    path.Add(child.Name);
                    Visit(child.Value, path, staticallyReachable, bindings, names, ref error);
                    path.RemoveAt(path.Count - 1);
                }
            }
            else
            {
                Visit(property.Value, path, false, bindings, names, ref error);
            }
        }
    }

    private static bool TryGet(JsonElement value, IReadOnlyList<string> path, out JsonElement current)
    {
        current = value;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Matches(string valueType, JsonElement value)
    {
        if (valueType == "string")
        {
            return value.ValueKind == JsonValueKind.String;
        }

        if (valueType == "boolean")
        {
            return value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False;
        }

        if (valueType == "integer")
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var integer))
            {
                return false;
            }

            var text = value.GetRawText();
            return text.IndexOf('.') < 0 && text.IndexOf('e') < 0 && text.IndexOf('E') < 0
                && integer <= 9007199254740991L && integer >= -9007199254740991L;
        }

        return false;
    }
}

/// <summary>Newline-delimited JSON used by the upstream stdio transport.</summary>
public static class McpNdjson
{
    /// <summary>Serializes one message and appends a newline.</summary>
    public static string Serialize(JsonElement message)
    {
        return message.GetRawText() + "\n";
    }

    /// <summary>Writes one newline-delimited message. A null stream means the transport is not connected.</summary>
    public static void Write(Stream? output, JsonElement message)
    {
        if (output is null)
        {
            throw new MCPClientError("StdioClientTransport not connected");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(Serialize(message));
        output.Write(bytes, 0, bytes.Length);
    }

    /// <summary>Buffer that reassembles newline-delimited messages split across chunks.</summary>
    public sealed class Buffer
    {
        private readonly List<byte> _bytes = new List<byte>();

        /// <summary>Appends the next stdout chunk.</summary>
        public void Append(byte[] chunk)
        {
            if (chunk is null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            _bytes.AddRange(chunk);
        }

        /// <summary>Returns the next line without its newline, or null when the line is incomplete.</summary>
        public string? ReadLine()
        {
            var newline = -1;
            for (var index = 0; index < _bytes.Count; index++)
            {
                if (_bytes[index] == (byte)'\n')
                {
                    newline = index;
                    break;
                }
            }

            if (newline < 0)
            {
                return null;
            }

            var text = System.Text.Encoding.UTF8.GetString(_bytes.ToArray(), 0, newline);
            _bytes.RemoveRange(0, newline + 1);
            return text;
        }

        /// <summary>Drops buffered bytes.</summary>
        public void Clear()
        {
            _bytes.Clear();
        }
    }
}

/// <summary>SHA-256 fingerprint of an MCP app resource.</summary>
public static class McpAppFingerprint
{
    /// <summary>Stable base64url SHA-256 of html, csp, and permissions. Object keys are sorted.</summary>
    public static string Fingerprint(string html, JsonElement? csp, JsonElement? permissions)
    {
        var canonical = Canonical(html, csp, permissions);
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonical));
            return ToBase64Url(hash);
        }
    }

    /// <summary>True when the current fingerprint differs from the baseline.</summary>
    public static bool DetectDrift(string current, string baseline)
    {
        return !string.Equals(current, baseline, StringComparison.Ordinal);
    }

    private static string Canonical(string html, JsonElement? csp, JsonElement? permissions)
    {
        return "{\"csp\":" + CanonicalValue(csp) + ",\"html\":" + System.Text.Json.JsonSerializer.Serialize(html) + ",\"permissions\":" + CanonicalValue(permissions) + "}";
    }

    private static string CanonicalValue(JsonElement? value)
    {
        if (value is null || value.Value.ValueKind == JsonValueKind.Null || value.Value.ValueKind == JsonValueKind.Undefined)
        {
            return "null";
        }

        return CanonicalNode(value.Value);
    }

    private static string CanonicalNode(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                var names = new List<string>();
                foreach (var property in value.EnumerateObject())
                {
                    names.Add(property.Name);
                }

                names.Sort(StringComparer.Ordinal);
                var fields = new List<string>(names.Count);
                foreach (var name in names)
                {
                    fields.Add(System.Text.Json.JsonSerializer.Serialize(name) + ":" + CanonicalNode(value.GetProperty(name)));
                }

                return "{" + string.Join(",", fields) + "}";
            case JsonValueKind.Array:
                var items = new List<string>();
                foreach (var item in value.EnumerateArray())
                {
                    items.Add(CanonicalNode(item));
                }

                return "[" + string.Join(",", items) + "]";
            default:
                return value.GetRawText();
        }
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}

/// <summary>OAuth resource URL checks from the MCP authorization helper.</summary>
public static class OAuthResources
{
    /// <summary>Removes the fragment. The result uses the absolute URI form.</summary>
    public static string ResourceUrlFromServerUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Server URL must be absolute.", nameof(url));
        }

        return uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.PathAndQuery, UriFormat.UriEscaped);
    }

    /// <summary>
    /// True when the requested resource has the same origin and its path is under the configured path.
    /// A path matches itself and its children, not a different prefix such as <c>/mcpxxxx</c> for <c>/mcp</c>.
    /// </summary>
    public static bool CheckResourceAllowed(string requestedResource, string configuredResource)
    {
        if (!Uri.TryCreate(requestedResource, UriKind.Absolute, out var requested)
            || !Uri.TryCreate(configuredResource, UriKind.Absolute, out var configured))
        {
            return false;
        }

        if (!string.Equals(Origin(requested), Origin(configured), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (requested.AbsolutePath.Length < configured.AbsolutePath.Length)
        {
            return false;
        }

        var requestedPath = requested.AbsolutePath.EndsWith("/", StringComparison.Ordinal) ? requested.AbsolutePath : requested.AbsolutePath + "/";
        var configuredPath = configured.AbsolutePath.EndsWith("/", StringComparison.Ordinal) ? configured.AbsolutePath : configured.AbsolutePath + "/";
        return requestedPath.StartsWith(configuredPath, StringComparison.Ordinal);
    }

    private static string Origin(Uri uri)
    {
        var port = uri.IsDefaultPort ? string.Empty : ":" + uri.Port.ToString(CultureInfo.InvariantCulture);
        return uri.Scheme + "://" + uri.Host + port;
    }
}

/// <summary>Environment passed to an MCP stdio child. The caller's dictionary is not modified.</summary>
public static class StdioEnvironment
{
    private static readonly string[] UnixKeys = { "HOME", "LOGNAME", "PATH", "SHELL", "TERM", "USER" };
    private static readonly string[] WindowsKeys =
    {
        "APPDATA", "HOMEDRIVE", "HOMEPATH", "LOCALAPPDATA", "PATH", "PROCESSOR_ARCHITECTURE", "SYSTEMDRIVE", "SYSTEMROOT", "TEMP", "USERNAME", "USERPROFILE",
    };

    /// <summary>Copies <paramref name="custom"/> and fills inherited process variables that do not start with <c>()</c>.</summary>
    public static Dictionary<string, string> GetEnvironment(IReadOnlyDictionary<string, string>? custom, bool? windows = null)
    {
        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        if (custom != null)
        {
            foreach (var pair in custom)
            {
                environment[pair.Key] = pair.Value;
            }
        }

        var windowsPlatform = windows ?? System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        foreach (var key in windowsPlatform ? WindowsKeys : UnixKeys)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (value == null || value.StartsWith("()", StringComparison.Ordinal))
            {
                continue;
            }

            environment[key] = value;
        }

        return environment;
    }

    /// <summary>Rejects command and argument line breaks on Windows.</summary>
    public static void ValidateCommand(string command, IReadOnlyList<string>? arguments, bool windows)
    {
        if (!windows)
        {
            return;
        }

        if (HasLineBreak(command) || HasLineBreak(arguments))
        {
            throw new ArgumentException("Stdio MCP commands and arguments must not contain line breaks on Windows.");
        }
    }

    private static bool HasLineBreak(string? value)
    {
        return value != null && (value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0);
    }

    private static bool HasLineBreak(IReadOnlyList<string>? values)
    {
        if (values == null)
        {
            return false;
        }

        foreach (var value in values)
        {
            if (HasLineBreak(value))
            {
                return true;
            }
        }

        return false;
    }
}
