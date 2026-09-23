// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vercel.AI.Sdk.Provider;

namespace Vercel.AI.Sdk.Mcp;

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

        using var init = JsonDocument.Parse("{\"protocolVersion\":\"2024-11-05\",\"capabilities\":{},\"clientInfo\":{\"name\":\"Vercel.AI.Sdk.Mcp\",\"version\":\"0.1.0\"}}");
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
        if (result.TryGetProperty("isError", out var isError) && isError.ValueKind == JsonValueKind.True)
        {
            throw new AiSdkException(ReadText(result));
        }

        return ReadText(result);
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

/// <summary>Content-Length framing used by MCP stdio.</summary>
public static class McpFraming
{
    /// <summary>Encodes one JSON-RPC message.</summary>
    public static byte[] Encode(string json)
    {
        var body = Encoding.UTF8.GetBytes(json ?? string.Empty);
        var header = Encoding.ASCII.GetBytes("Content-Length: " + body.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\r\n\r\n");
        var message = new byte[header.Length + body.Length];
        Buffer.BlockCopy(header, 0, message, 0, header.Length);
        Buffer.BlockCopy(body, 0, message, header.Length, body.Length);
        return message;
    }

    /// <summary>Reads one framed message, or null at end of stream.</summary>
    public static async Task<string?> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        var header = new StringBuilder();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next = await ReadByteAsync(stream).ConfigureAwait(false);
            if (next < 0)
            {
                if (header.Length == 0)
                {
                    return null;
                }

                throw new AiSdkException("Incomplete MCP frame.");
            }

            header.Append((char)next);
            var text = header.ToString();
            if (text.EndsWith("\r\n\r\n") || text.EndsWith("\n\n"))
            {
                break;
            }
        }

        var length = 0;
        foreach (var line in header.ToString().Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length >= 15 && string.Compare(trimmed.Substring(0, 15), "Content-Length:", StringComparison.OrdinalIgnoreCase) == 0)
            {
                length = int.Parse(trimmed.Substring(15).Trim(), System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        var body = new byte[length];
        var read = 0;
        while (read < length)
        {
            var count = await stream.ReadAsync(body, read, length - read).ConfigureAwait(false);
            if (count == 0)
            {
                throw new AiSdkException("Incomplete MCP frame.");
            }

            read += count;
        }

        return Encoding.UTF8.GetString(body);
    }

    private static async Task<int> ReadByteAsync(Stream stream)
    {
        var buffer = new byte[1];
        var count = await stream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
        return count == 0 ? -1 : buffer[0];
    }
}

/// <summary>MCP over a child process using Content-Length frames.</summary>
public sealed class StdioMcpTransport : IMcpTransport, IDisposable
{
    private readonly Process _process;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _id;

    private StdioMcpTransport(Process process)
    {
        _process = process;
    }

    /// <summary>Starts <paramref name="fileName"/> and speaks MCP on its stdin and stdout.</summary>
    public static StdioMcpTransport Start(string fileName, IReadOnlyList<string>? arguments = null)
    {
        var start = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        if (arguments != null && arguments.Count > 0)
        {
            var quoted = new string[arguments.Count];
            for (var index = 0; index < arguments.Count; index++)
            {
                quoted[index] = "\"" + arguments[index].Replace("\"", "\\\"") + "\"";
            }

            start.Arguments = string.Join(" ", quoted);
        }

        var process = Process.Start(start) ?? throw new AiSdkException("Failed to start MCP process.");
        return new StdioMcpTransport(process);
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
            var bytes = McpFraming.Encode(request.ToJsonString());
            await _process.StandardInput.BaseStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            await _process.StandardInput.BaseStream.FlushAsync().ConfigureAwait(false);
            var payload = await McpFraming.ReadAsync(_process.StandardOutput.BaseStream, cancellationToken).ConfigureAwait(false);
            if (payload is null)
            {
                throw new AiSdkException("MCP process closed the stream.");
            }

            using var document = JsonDocument.Parse(payload);
            return Unwrap(document.RootElement);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_process.HasExited)
        {
            _process.Kill();
        }

        _process.Dispose();
        _gate.Dispose();
    }

    internal static JsonElement Unwrap(JsonElement root)
    {
        if (root.TryGetProperty("error", out var error))
        {
            var message = error.TryGetProperty("message", out var text) ? text.GetString() : error.ToString();
            throw new AiSdkException(message ?? "MCP call failed.");
        }

        if (!root.TryGetProperty("result", out var result))
        {
            throw new AiSdkException("MCP response did not include a result.");
        }

        return result.Clone();
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
            throw new AiSdkException("MCP HTTP call failed with status " + (int)response.StatusCode + ".");
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
