// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Prompt;
using Vercel.AI.Provider;
using Vercel.AI.ProviderUtils;

namespace Vercel.AI.Mcp;

/// <summary>Sends one JSON-RPC call and returns the result object.</summary>
public interface IMcpTransport
{
    /// <summary>Calls <paramref name="method"/> and returns the JSON-RPC result.</summary>
    Task<JsonElement> CallAsync(
        string method,
        JsonElement parameters,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// A transport that can probe the server with <c>server/discover</c> before legacy initialization.
/// Maps to the <c>supportsProtocolVersionDiscovery</c> and <c>waitsForProtocolVersionDiscovery</c> members of upstream's
/// <c>MCPTransport</c>. It is a separate interface because <see cref="IMcpTransport"/> cannot gain members on netstandard2.0.
/// </summary>
public interface IMcpProtocolVersionDiscoveryTransport : IMcpTransport
{
    /// <summary>True when the client may probe with <c>server/discover</c>.</summary>
    bool SupportsProtocolVersionDiscovery { get; }

    /// <summary>True when the client waits for the discovery response, with no timeout and no fallback after a network error.</summary>
    bool WaitsForProtocolVersionDiscovery { get; }

    /// <summary>
    /// The protocol version the transport announces, for example in the <c>mcp-protocol-version</c> header.
    /// The client sets it when discovery or initialization negotiates a version. Maps to <c>MCPTransport.protocolVersion</c>.
    /// </summary>
    string? ProtocolVersion { get; set; }
}

/// <summary>Options of <see cref="McpClient.ConnectAsync(IMcpTransport, McpClientOptions, CancellationToken)"/>. Maps to <c>MCPClientConfig</c>.</summary>
public sealed class McpClientOptions
{
    /// <summary>
    /// Whether transports that support stateless protocol discovery probe with <c>server/discover</c> before falling back
    /// to legacy initialization. Disable it for legacy servers that require <c>initialize</c> to be the first request. Default true.
    /// </summary>
    public bool ProtocolVersionDiscovery { get; set; } = true;

    /// <summary>
    /// Explicit schemas per tool name. Only the listed tools are returned, as typed tools. Null discovers every tool.
    /// Maps to the <c>schemas</c> option of <c>client.tools()</c>.
    /// </summary>
    public IReadOnlyDictionary<string, McpToolSchema>? Schemas { get; set; }
}

/// <summary>The schemas of one MCP tool. Maps to an entry of the <c>schemas</c> option of <c>client.tools()</c>.</summary>
public sealed class McpToolSchema
{
    /// <summary>Creates the schemas with the JSON Schema of the tool input.</summary>
    public McpToolSchema(string inputSchema)
    {
        InputSchema = inputSchema ?? throw new ArgumentNullException(nameof(inputSchema));
    }

    /// <summary>JSON Schema of the tool input.</summary>
    public string InputSchema { get; }

    /// <summary>
    /// JSON Schema of the tool output. When set, the tool returns the <c>structuredContent</c> of the result, or the JSON
    /// in its first text content, validated against it. Without it the tool returns the call-tool result.
    /// </summary>
    public string? OutputSchema { get; set; }
}

/// <summary>Lists MCP tools and adapts them to <see cref="Tool"/>.</summary>
public static class McpClient
{
    /// <summary>Latest MCP protocol version. Maps to <c>LATEST_PROTOCOL_VERSION</c>.</summary>
    public const string LatestProtocolVersion = "2026-07-28";

    /// <summary>Latest legacy MCP protocol version. Maps to <c>LATEST_LEGACY_PROTOCOL_VERSION</c>.</summary>
    public const string LatestLegacyProtocolVersion = "2025-11-25";

    private const int DefaultProtocolDiscoveryTimeoutMilliseconds = 1000;

    private static readonly string[] SupportedProtocolVersions =
    [
        LatestProtocolVersion,
        LatestLegacyProtocolVersion,
        "2025-06-18",
        "2025-03-26",
        "2024-11-05",
    ];

    /// <summary>Calls <c>initialize</c>, then <c>tools/list</c>.</summary>
    public static Task<IReadOnlyList<Tool>> ConnectAsync(
        IMcpTransport transport,
        CancellationToken cancellationToken = default
    ) => ConnectAsync(transport, new McpClientOptions(), cancellationToken);

    /// <summary>
    /// Probes with <c>server/discover</c> when <see cref="McpClientOptions.ProtocolVersionDiscovery"/> and the transport allow it,
    /// otherwise calls <c>initialize</c>. Then calls <c>tools/list</c>.
    /// </summary>
    public static async Task<IReadOnlyList<Tool>> ConnectAsync(
        IMcpTransport transport,
        McpClientOptions options,
        CancellationToken cancellationToken = default
    )
    {
        if (transport is null)
        {
            throw new ArgumentNullException(nameof(transport));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (
            options.ProtocolVersionDiscovery
            && transport
                is IMcpProtocolVersionDiscoveryTransport
                {
                    SupportsProtocolVersionDiscovery: true
                } discoverable
            && await TryProtocolDiscoveryAsync(discoverable, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            return await ListToolsAsync(transport, options.Schemas, cancellationToken)
                .ConfigureAwait(false);
        }

        if (transport is IMcpProtocolVersionDiscoveryTransport versioned)
        {
            versioned.ProtocolVersion = LatestLegacyProtocolVersion;
        }

        using var init = JsonDocument.Parse(
            "{\"protocolVersion\":\"2024-11-05\",\"capabilities\":{},\"clientInfo\":{\"name\":\"Vercel.AI.Mcp\",\"version\":\"0.1.0\"}}"
        );
        await transport
            .CallAsync("initialize", init.RootElement, cancellationToken)
            .ConfigureAwait(false);
        return await ListToolsAsync(transport, options.Schemas, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<bool> TryProtocolDiscoveryAsync(
        IMcpProtocolVersionDiscoveryTransport transport,
        CancellationToken cancellationToken
    )
    {
        transport.ProtocolVersion = LatestProtocolVersion;
        using var timeout = transport.WaitsForProtocolVersionDiscovery
            ? null
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout?.CancelAfter(DefaultProtocolDiscoveryTimeoutMilliseconds);
        try
        {
            using var empty = JsonDocument.Parse("{}");
            JsonElement result;
            try
            {
                result = await transport
                    .CallAsync(
                        "server/discover",
                        empty.RootElement,
                        timeout?.Token ?? cancellationToken
                    )
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
                when (timeout is { IsCancellationRequested: true }
                    && !cancellationToken.IsCancellationRequested
                )
            {
                throw new MCPClientError(
                    "Request timed out after " + DefaultProtocolDiscoveryTimeoutMilliseconds + "ms"
                );
            }

            var supported =
                result.TryGetProperty("supportedVersions", out var versions)
                && versions.ValueKind == JsonValueKind.Array
                    ? versions.EnumerateArray().Select(v => v.GetString()).ToList()
                    : throw new MCPClientError("Server sent invalid server/discover result.");
            if (!supported.Contains(LatestProtocolVersion))
            {
                throw new MCPClientError(
                    "Server does not support the requested protocol version: "
                        + LatestProtocolVersion
                );
            }

            return true;
        }
        catch (MCPClientError error) when (error.Code is -32020 or -32021 or -32022)
        {
            if (
                error.Code == -32022
                && TryGetSupportedVersions(error.Data, out var supportedVersions)
            )
            {
                if (
                    !supportedVersions.Contains(LatestProtocolVersion)
                    && supportedVersions.Any(version =>
                        string.CompareOrdinal(version, LatestLegacyProtocolVersion) <= 0
                        && SupportedProtocolVersions.Contains(version)
                    )
                )
                {
                    throw new MCPClientError(
                        "MCP server rejected protocol version "
                            + LatestProtocolVersion
                            + " during discovery. Server-supported versions: "
                            + string.Join(", ", supportedVersions)
                            + ". The client does not automatically switch to legacy initialization after this error. "
                            + "If this is a known legacy server, set ProtocolVersionDiscovery to false to use initialize.",
                        code: error.Code,
                        statusCode: error.StatusCode,
                        url: error.Url,
                        responseBody: error.ResponseBody,
                        data: error.Data,
                        cause: error
                    );
                }
            }

            throw;
        }
        catch (Exception exception)
            when (transport.WaitsForProtocolVersionDiscovery && exception is not MCPClientError)
        {
            throw;
        }
        catch (MCPClientError)
        {
            return false;
        }
    }

    private static bool TryGetSupportedVersions(JsonElement? data, out List<string> versions)
    {
        versions = [];
        if (
            data is { ValueKind: JsonValueKind.Object } element
            && element.TryGetProperty("supported", out var supported)
            && supported.ValueKind == JsonValueKind.Array
            && supported.EnumerateArray().All(v => v.ValueKind == JsonValueKind.String)
        )
        {
            versions = supported.EnumerateArray().Select(v => v.GetString()!).ToList();
            return true;
        }

        return false;
    }

    /// <summary>Maps <c>tools/list</c> onto SDK tools. Execution calls <c>tools/call</c>.</summary>
    public static Task<IReadOnlyList<Tool>> ListToolsAsync(
        IMcpTransport transport,
        CancellationToken cancellationToken = default
    ) => ListToolsAsync(transport, schemas: null, cancellationToken);

    /// <summary>
    /// Maps <c>tools/list</c> onto SDK tools. With <paramref name="schemas"/>, only the listed tools are returned,
    /// typed by their schemas. Maps to <c>client.tools({ schemas })</c>.
    /// </summary>
    public static async Task<IReadOnlyList<Tool>> ListToolsAsync(
        IMcpTransport transport,
        IReadOnlyDictionary<string, McpToolSchema>? schemas,
        CancellationToken cancellationToken = default
    )
    {
        if (transport is null)
        {
            throw new ArgumentNullException(nameof(transport));
        }

        using var empty = JsonDocument.Parse("{}");
        var listed = await transport
            .CallAsync("tools/list", empty.RootElement, cancellationToken)
            .ConfigureAwait(false);
        var tools = new List<Tool>();
        if (
            !listed.TryGetProperty("tools", out var array)
            || array.ValueKind != JsonValueKind.Array
        )
        {
            return tools;
        }

        foreach (var item in array.EnumerateArray())
        {
            var name = item.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString() ?? "tool"
                : "tool";
            var description = item.TryGetProperty("description", out var descriptionElement)
                ? descriptionElement.GetString()
                : null;
            var schema = item.TryGetProperty("inputSchema", out var schemaElement)
                ? schemaElement.GetRawText()
                : "{}";
            var toolName = name;
            JsonNode? outputSchema = null;
            if (schemas is not null)
            {
                if (!schemas.TryGetValue(name, out var toolSchema))
                {
                    continue;
                }

                schema = toolSchema.InputSchema;
                outputSchema = toolSchema.OutputSchema is null
                    ? null
                    : JsonNode.Parse(toolSchema.OutputSchema);
            }

            var tool = Tool.Function(toolName, description, schema, (arguments, token) =>
                CallToolAsync(
                    transport,
                    toolName,
                    arguments,
                    schemas is not null,
                    outputSchema,
                    token
                )
            );
            if (outputSchema is not null)
            {
                using var outputDocument = JsonDocument.Parse(outputSchema.ToJsonString());
                tool.OutputSchema = outputDocument.RootElement.Clone();
            }

            tool.ToModelOutput = McpToModelOutput;
            tools.Add(tool);
        }

        return tools;
    }

    private static async Task<string> CallToolAsync(
        IMcpTransport transport,
        string name,
        JsonElement arguments,
        bool returnsErrorResult,
        JsonNode? outputSchema,
        CancellationToken cancellationToken
    )
    {
        var parameters = new JsonObject
        {
            ["name"] = name,
            ["arguments"] = JsonNode.Parse(arguments.GetRawText()),
        };
        using var document = JsonDocument.Parse(parameters.ToJsonString());
        var result = await transport
            .CallAsync("tools/call", document.RootElement, cancellationToken)
            .ConfigureAwait(false);
        var normalized = CallToolResults.Normalize(result);
        if (
            normalized.TryGetProperty("isError", out var isError)
            && isError.ValueKind == JsonValueKind.True
        )
        {
            // With explicit schemas, upstream's execute returns the error result instead of throwing.
            if (returnsErrorResult)
            {
                return normalized.GetRawText();
            }

            throw new AiSdkException(ReadText(normalized));
        }

        if (outputSchema is not null)
        {
            return ExtractStructuredContent(normalized, outputSchema, name).GetRawText();
        }

        // Upstream's execute returns the call-tool result itself. The port's execute returns its JSON text.
        return normalized.GetRawText();
    }

    /// <summary>Extracts and validates <c>structuredContent</c> from a tool result. Maps to <c>extractStructuredContent</c>.</summary>
    private static JsonElement ExtractStructuredContent(
        JsonElement result,
        JsonNode outputSchema,
        string toolName
    )
    {
        if (
            result.TryGetProperty("structuredContent", out var structured)
            && structured.ValueKind != JsonValueKind.Null
            && structured.ValueKind != JsonValueKind.Undefined
        )
        {
            var validation = JsonParsing.SafeValidateTypes(structured, outputSchema);
            if (!validation.Success)
            {
                throw new MCPClientError(
                    "Tool \""
                        + toolName
                        + "\" returned structuredContent that does not match the expected outputSchema",
                    cause: validation.Error
                );
            }

            return validation.Value!.Value;
        }

        // Fallback
        if (
            result.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var part in content.EnumerateArray())
            {
                if (
                    part.ValueKind == JsonValueKind.Object
                    && part.TryGetProperty("type", out var type)
                    && type.ValueKind == JsonValueKind.String
                    && type.GetString() == "text"
                )
                {
                    if (
                        !part.TryGetProperty("text", out var text)
                        || text.ValueKind != JsonValueKind.String
                    )
                    {
                        break;
                    }

                    var parsed = JsonParsing.SafeParse(text.GetString()!, outputSchema);
                    if (!parsed.Success)
                    {
                        throw new MCPClientError(
                            "Tool \""
                                + toolName
                                + "\" returned content that does not match the expected outputSchema",
                            cause: parsed.Error
                        );
                    }

                    return parsed.Value!.Value;
                }
            }
        }

        throw new MCPClientError(
            "Tool \"" + toolName + "\" did not return structuredContent or parseable text content"
        );
    }

    /// <summary>Converts a tool output to the result sent to the model. Maps to <c>mcpToModelOutput</c>.</summary>
    private static ToolModelOutput McpToModelOutput(ToolModelOutputCall call)
    {
        var json = call.Output switch
        {
            null => "null",
            string text => text,
            JsonElement element => element.GetRawText(),
            var other => JsonSerializer.Serialize(other),
        };
        if (string.IsNullOrWhiteSpace(json))
        {
            json = "null";
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException) when (call.Output is string plain)
        {
            // A wrapper that returns a plain string, not JSON text, returns a string upstream.
            return new TextToolModelOutput(plain);
        }

        using var parsed = document;
        var root = document.RootElement;

        // The output is JSON text, so a JSON string is a string output upstream.
        if (root.ValueKind == JsonValueKind.String)
        {
            return new TextToolModelOutput(root.GetString()!);
        }

        if (
            root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("content", out var content)
            || content.ValueKind != JsonValueKind.Array
        )
        {
            return new JsonToolModelOutput(root.Clone());
        }

        var parts = new List<ToolModelContentPart>();
        foreach (var part in content.EnumerateArray())
        {
            JsonElement type = default;
            var hasType =
                part.ValueKind == JsonValueKind.Object
                && part.TryGetProperty("type", out type)
                && type.ValueKind == JsonValueKind.String;
            if (
                part.ValueKind == JsonValueKind.Object
                && hasType
                && type.GetString() == "text"
                && part.TryGetProperty("text", out var textValue)
                && textValue.ValueKind == JsonValueKind.String
            )
            {
                parts.Add(new ToolModelContentPart("text", textValue.GetString()!));
            }
            else if (
                part.ValueKind == JsonValueKind.Object
                && hasType
                && type.GetString() == "image"
                && part.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.String
                && part.TryGetProperty("mimeType", out var mimeType)
                && mimeType.ValueKind == JsonValueKind.String
            )
            {
                parts.Add(new FileToolModelContentPart(mimeType.GetString()!, data.GetString()!));
            }
            else
            {
                parts.Add(new ToolModelContentPart("text", part.GetRawText()));
            }
        }

        return new ContentToolModelOutput(parts);
    }

    private static string ReadText(JsonElement result)
    {
        if (
            result.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.Array
        )
        {
            var builder = new StringBuilder();
            foreach (var part in content.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text))
                {
                    builder.Append(text.GetString());
                }
            }

            if (builder.Length > 0)
            {
                return builder.ToString();
            }
        }

        return result.GetRawText();
    }
}

/// <summary>How to start an MCP stdio server. Mirrors the upstream <c>StdioConfig</c>.</summary>
public sealed class StdioMcpOptions
{
    /// <summary>Creates options for <paramref name="command"/>.</summary>
    public StdioMcpOptions(string command)
    {
        Command = command ?? throw new ArgumentNullException(nameof(command));
    }

    /// <summary>Executable to start.</summary>
    public string Command { get; }

    /// <summary>Command arguments.</summary>
    public IReadOnlyList<string>? Arguments { get; set; }

    /// <summary>Extra environment variables. The child gets these plus a small set of safe inherited variables.</summary>
    public IReadOnlyDictionary<string, string>? Environment { get; set; }

    /// <summary>Working directory. Null uses the current directory.</summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>True pipes stderr to <see cref="StdioMcpTransport.StandardError"/>. False lets the child inherit it.</summary>
    public bool RedirectStandardError { get; set; }

    /// <summary>
    /// True runs the command through a shell (<c>/bin/sh -c</c>, or <c>cmd.exe /d /s /c</c> on Windows), so command shims
    /// such as the Windows <c>npx</c> work. Arguments are joined with spaces and are not quoted. Mirrors the upstream <c>shell</c> option.
    /// </summary>
    public bool Shell { get; set; }

    /// <summary>The shell executable to use. Setting it implies <see cref="Shell"/>. Mirrors the upstream <c>shell</c> string.</summary>
    public string? ShellPath { get; set; }
}

/// <summary>MCP over a child process using newline-delimited JSON-RPC messages.</summary>
public sealed class StdioMcpTransport : IMcpTransport, IDisposable
{
    private readonly Process? _process;
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly McpNdjson.Buffer _buffer = new();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _id;

    private StdioMcpTransport(Process process)
        : this(process.StandardInput.BaseStream, process.StandardOutput.BaseStream)
    {
        _process = process;
    }

    /// <summary>Speaks MCP over <paramref name="input"/> (the server's stdin) and <paramref name="output"/> (its stdout).</summary>
    internal StdioMcpTransport(Stream input, Stream output)
    {
        _input = input;
        _output = output;
    }

    /// <summary>Child process id.</summary>
    public int ProcessId => _process?.Id ?? 0;

    /// <summary>The child's stderr, when <see cref="StdioMcpOptions.RedirectStandardError"/> is true.</summary>
    public StreamReader? StandardError =>
        _process?.StartInfo.RedirectStandardError == true ? _process.StandardError : null;

    /// <summary>Starts <paramref name="fileName"/> and speaks MCP on its stdin and stdout.</summary>
    public static StdioMcpTransport Start(string fileName, IReadOnlyList<string>? arguments = null)
    {
        return Start(new StdioMcpOptions(fileName) { Arguments = arguments });
    }

    /// <summary>Starts the server described by <paramref name="options"/>.</summary>
    public static StdioMcpTransport Start(StdioMcpOptions options)
    {
        var start = CreateStartInfo(options);
        Process? process;
        try
        {
            process = Process.Start(start);
        }
        catch (Win32Exception exception)
        {
            throw new AiSdkException(
                "Failed to start MCP process: " + exception.Message,
                exception
            );
        }

        return new StdioMcpTransport(
            process ?? throw new AiSdkException("Failed to start MCP process.")
        );
    }

    /// <summary>
    /// Builds the process start info. The environment is replaced by <see cref="StdioEnvironment.GetEnvironment"/>,
    /// so the child does not inherit every variable of this process.
    /// </summary>
    public static ProcessStartInfo CreateStartInfo(StdioMcpOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        StdioEnvironment.ValidateCommand(options.Command, options.Arguments, windows);
        var useShell = options.Shell || !string.IsNullOrEmpty(options.ShellPath);
        var start = new ProcessStartInfo
        {
            FileName = options.Command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = options.RedirectStandardError,
            UseShellExecute = false,
            WorkingDirectory = options.WorkingDirectory ?? string.Empty,
        };
        if (useShell)
        {
            var commandLine = options.Command;
            if (options.Arguments is { Count: > 0 } shellArguments)
            {
                commandLine += " " + string.Join(" ", shellArguments);
            }

            if (windows)
            {
                start.FileName = string.IsNullOrEmpty(options.ShellPath)
                    ? System.Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe"
                    : options.ShellPath!;
                start.Arguments = "/d /s /c \"" + commandLine + "\"";
            }
            else
            {
                start.FileName = string.IsNullOrEmpty(options.ShellPath)
                    ? "/bin/sh"
                    : options.ShellPath!;
                start.Arguments =
                    "-c \"" + commandLine.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            }
        }
        else if (options.Arguments is { Count: > 0 } arguments)
        {
            var quoted = new string[arguments.Count];
            for (var index = 0; index < arguments.Count; index++)
            {
                quoted[index] = "\"" + arguments[index].Replace("\"", "\\\"") + "\"";
            }

            start.Arguments = string.Join(" ", quoted);
        }

        start.Environment.Clear();
        foreach (var pair in StdioEnvironment.GetEnvironment(options.Environment, windows))
        {
            start.Environment[pair.Key] = pair.Value;
        }

        return start;
    }

    /// <inheritdoc />
    public async Task<JsonElement> CallAsync(
        string method,
        JsonElement parameters,
        CancellationToken cancellationToken
    )
    {
        var id = Interlocked.Increment(ref _id);
        var request = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = method,
            ["params"] = JsonNode.Parse(parameters.GetRawText()),
        };
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using (var document = JsonDocument.Parse(request.ToJsonString()))
            {
                await McpNdjson
                    .WriteAsync(_input, document.RootElement, cancellationToken)
                    .ConfigureAwait(false);
            }

            await _input.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Servers may send notifications and requests before the response, so skip other messages.
            while (true)
            {
                var line = await ReadLineAsync(cancellationToken).ConfigureAwait(false)
                    ?? throw new MCPClientError("MCP process closed the stream.");
                if (line.Trim().Length == 0)
                {
                    continue;
                }

                var message = JsonRpcMessages.Parse(line);
                if (
                    message.TryGetProperty("id", out var responseId)
                    && responseId.ValueKind == JsonValueKind.Number
                    && responseId.GetInt32() == id
                    && !message.TryGetProperty("method", out _)
                )
                {
                    return Unwrap(message);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Kills the child process, if one was started.</summary>
    public void Dispose()
    {
        if (_process != null)
        {
            if (!_process.HasExited)
            {
                _process.Kill();
            }

            _process.Dispose();
        }

        _gate.Dispose();
    }

    internal static JsonElement Unwrap(JsonElement root)
    {
        if (root.TryGetProperty("error", out _))
        {
            throw CreateError(root);
        }

        if (!root.TryGetProperty("result", out var result))
        {
            throw new MCPClientError("MCP response did not include a result.");
        }

        return result.Clone();
    }

    internal static MCPClientError CreateError(
        JsonElement root,
        int? statusCode = null,
        string? url = null,
        string? responseBody = null
    )
    {
        var error = root.GetProperty("error");
        var message =
            error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var text)
                ? text.GetString()
                : error.ToString();
        int? code =
            error.ValueKind == JsonValueKind.Object
            && error.TryGetProperty("code", out var codeElement)
            && codeElement.TryGetInt32(out var value)
                ? value
                : null;
        JsonElement? data =
            error.ValueKind == JsonValueKind.Object
            && error.TryGetProperty("data", out var dataElement)
                ? dataElement.Clone()
                : null;
        return new MCPClientError(
            message ?? "MCP call failed.",
            code: code,
            statusCode: statusCode,
            url: url,
            responseBody: responseBody,
            data: data
        );
    }

    private async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        var chunk = new byte[4096];
        while (true)
        {
            if (_buffer.ReadLine() is { } line)
            {
                return line;
            }

            var count = await _output
                .ReadAsync(chunk, 0, chunk.Length, cancellationToken)
                .ConfigureAwait(false);
            if (count == 0)
            {
                return null;
            }

            var bytes = new byte[count];
            Array.Copy(chunk, bytes, count);
            _buffer.Append(bytes);
        }
    }
}

/// <summary>MCP over Streamable HTTP. Posts JSON-RPC and accepts a JSON body or an SSE <c>data:</c> payload.</summary>
public sealed class HttpMcpTransport : IMcpProtocolVersionDiscoveryTransport
{
    private readonly HttpClient _httpClient;
    private readonly Uri _endpoint;
    private int _id;

    /// <summary>Creates a transport that posts to <paramref name="endpoint"/>.</summary>
    public HttpMcpTransport(HttpClient httpClient, Uri endpoint)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    /// <inheritdoc />
    public bool SupportsProtocolVersionDiscovery => true;

    /// <inheritdoc />
    public bool WaitsForProtocolVersionDiscovery => true;

    /// <inheritdoc />
    public string? ProtocolVersion { get; set; } = McpClient.LatestLegacyProtocolVersion;

    /// <inheritdoc />
    public async Task<JsonElement> CallAsync(
        string method,
        JsonElement parameters,
        CancellationToken cancellationToken
    )
    {
        var id = Interlocked.Increment(ref _id);
        var request = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = method,
            ["params"] = JsonNode.Parse(parameters.GetRawText()),
        };
        using var message = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        message.Headers.Accept.ParseAdd("application/json");
        message.Headers.Accept.ParseAdd("text/event-stream");
        message.Headers.TryAddWithoutValidation(
            "mcp-protocol-version",
            ProtocolVersion ?? McpClient.LatestLegacyProtocolVersion
        );
        using var response = await _httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        var isEventStream =
            response.IsSuccessStatusCode
            && response.Content.Headers.ContentType?.MediaType?.IndexOf(
                "event-stream",
                StringComparison.OrdinalIgnoreCase
            ) >= 0;
        if (isEventStream)
        {
            // Servers may keep the stream open and send notifications first, so read events until the response for this id.
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await foreach (
                var data in SseParser.ReadDataAsync(stream, cancellationToken).ConfigureAwait(false)
            )
            {
                using var sseEvent = JsonDocument.Parse(data);
                var root = sseEvent.RootElement;
                if (
                    root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("id", out var responseId)
                    && responseId.ValueKind == JsonValueKind.Number
                    && responseId.TryGetInt32(out var responseNumber)
                    && responseNumber == id
                    && !root.TryGetProperty("method", out _)
                )
                {
                    return StdioMcpTransport.Unwrap(root);
                }
            }

            throw new MCPClientError(
                "MCP HTTP stream ended without a response.",
                url: _endpoint.ToString()
            );
        }

        var body = await CancellableReads
            .ReadAsStringAsync(response.Content, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            if (TryParseJsonRpcError(body, out var jsonRpcError))
            {
                using (jsonRpcError)
                {
                    throw StdioMcpTransport.CreateError(
                        jsonRpcError.RootElement,
                        (int)response.StatusCode,
                        _endpoint.AbsoluteUri,
                        body
                    );
                }
            }

            throw new MCPClientError(
                "MCP HTTP call failed with status " + (int)response.StatusCode + ".",
                statusCode: (int)response.StatusCode,
                url: _endpoint.ToString(),
                responseBody: body
            );
        }

        using var document = JsonDocument.Parse(body);
        return StdioMcpTransport.Unwrap(document.RootElement);
    }

    private static bool TryParseJsonRpcError(string body, out JsonDocument document)
    {
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            document = null!;
            return false;
        }

        if (
            document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty("error", out _)
        )
        {
            return true;
        }

        document.Dispose();
        return false;
    }
}
