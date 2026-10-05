// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Provider;

namespace Vercel.AI.Mcp;

/// <summary>Sends one JSON-RPC call and returns the result object.</summary>
public interface IMcpTransport
{
    /// <summary>Calls <paramref name="method"/> and returns the JSON-RPC result.</summary>
    Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken);
}

/// <summary>Lists MCP tools and adapts them to <see cref="Tool"/>.</summary>
public static class McpClient
{
    /// <summary>Calls <c>initialize</c>, then <c>tools/list</c>.</summary>
    public static async Task<IReadOnlyList<Tool>> ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
    {
        if (transport is null)
        {
            throw new ArgumentNullException(nameof(transport));
        }

        using var init = JsonDocument.Parse("{\"protocolVersion\":\"2024-11-05\",\"capabilities\":{},\"clientInfo\":{\"name\":\"Vercel.AI.Mcp\",\"version\":\"0.1.0\"}}");
        await transport.CallAsync("initialize", init.RootElement, cancellationToken).ConfigureAwait(false);
        return await ListToolsAsync(transport, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Maps <c>tools/list</c> onto SDK tools. Execution calls <c>tools/call</c>.</summary>
    public static async Task<IReadOnlyList<Tool>> ListToolsAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
    {
        if (transport is null)
        {
            throw new ArgumentNullException(nameof(transport));
        }

        using var empty = JsonDocument.Parse("{}");
        var listed = await transport.CallAsync("tools/list", empty.RootElement, cancellationToken).ConfigureAwait(false);
        var tools = new List<Tool>();
        if (!listed.TryGetProperty("tools", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return tools;
        }

        foreach (var item in array.EnumerateArray())
        {
            var name = item.TryGetProperty("name", out var nameElement) ? nameElement.GetString() ?? "tool" : "tool";
            var description = item.TryGetProperty("description", out var descriptionElement) ? descriptionElement.GetString() : null;
            var schema = item.TryGetProperty("inputSchema", out var schemaElement) ? schemaElement.GetRawText() : "{}";
            var toolName = name;
            tools.Add(Tool.Function(toolName, description, schema, (arguments, token) => CallToolAsync(transport, toolName, arguments, token)));
        }

        return tools;
    }

    private static async Task<string> CallToolAsync(IMcpTransport transport, string name, JsonElement arguments, CancellationToken cancellationToken)
    {
        var parameters = new JsonObject
        {
            ["name"] = name,
            ["arguments"] = JsonNode.Parse(arguments.GetRawText()),
        };
        using var document = JsonDocument.Parse(parameters.ToJsonString());
        var result = await transport.CallAsync("tools/call", document.RootElement, cancellationToken).ConfigureAwait(false);
        var normalized = CallToolResults.Normalize(result);
        if (normalized.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
        {
            throw new AiSdkException(ReadText(normalized));
        }

        return ReadText(normalized);
    }

    private static string ReadText(JsonElement result)
    {
        if (result.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
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
    public StreamReader? StandardError => _process?.StartInfo.RedirectStandardError == true ? _process.StandardError : null;

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
            throw new AiSdkException("Failed to start MCP process: " + exception.Message, exception);
        }

        return new StdioMcpTransport(process ?? throw new AiSdkException("Failed to start MCP process."));
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
        var start = new ProcessStartInfo
        {
            FileName = options.Command,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = options.RedirectStandardError,
            UseShellExecute = false,
            WorkingDirectory = options.WorkingDirectory ?? string.Empty,
        };
        if (options.Arguments is { Count: > 0 } arguments)
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
    public async Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
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
                McpNdjson.Write(_input, document.RootElement);
            }

            await _input.FlushAsync(cancellationToken).ConfigureAwait(false);

            // Servers may send notifications and requests before the response, so skip other messages.
            while (true)
            {
                var line = await ReadLineAsync(cancellationToken).ConfigureAwait(false) ?? throw new MCPClientError("MCP process closed the stream.");
                if (line.Trim().Length == 0)
                {
                    continue;
                }

                var message = JsonRpcMessages.Parse(line);
                if (message.TryGetProperty("id", out var responseId)
                    && responseId.ValueKind == JsonValueKind.Number
                    && responseId.GetInt32() == id
                    && !message.TryGetProperty("method", out _))
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
        if (root.TryGetProperty("error", out var error))
        {
            var message = error.TryGetProperty("message", out var text) ? text.GetString() : error.ToString();
            int? code = error.TryGetProperty("code", out var codeElement) && codeElement.TryGetInt32(out var value) ? value : null;
            JsonElement? data = error.TryGetProperty("data", out var dataElement) ? dataElement.Clone() : null;
            throw new MCPClientError(message ?? "MCP call failed.", code: code, data: data);
        }

        if (!root.TryGetProperty("result", out var result))
        {
            throw new MCPClientError("MCP response did not include a result.");
        }

        return result.Clone();
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

            var count = await _output.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false);
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
public sealed class HttpMcpTransport : IMcpTransport
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
    public async Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
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
        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new MCPClientError(
                "MCP HTTP call failed with status " + (int)response.StatusCode + ".",
                statusCode: (int)response.StatusCode,
                url: _endpoint.ToString(),
                responseBody: body);
        }

        var json = ExtractJson(body, response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(json);
        return StdioMcpTransport.Unwrap(document.RootElement);
    }

    private static string ExtractJson(string body, string? mediaType)
    {
        if (mediaType != null && mediaType.ToUpperInvariant().Contains("EVENT-STREAM"))
        {
            foreach (var line in body.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.Length >= 5 && string.CompareOrdinal(trimmed, 0, "data:", 0, 5) == 0)
                {
                    return trimmed.Substring(5).Trim();
                }
            }
        }

        return body;
    }
}
