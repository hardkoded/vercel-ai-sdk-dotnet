// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Provider;

namespace Vercel.AI.Mcp;

/// <summary>An MCP call failed. Carries the JSON-RPC and HTTP details when the transport has them.</summary>
public class MCPClientError : AiSdkException
{
    /// <summary>Creates the error with optional MCP error metadata.</summary>
    public MCPClientError(string message, int? code = null, int? statusCode = null, string? url = null, string? responseBody = null, JsonElement? data = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Url = url;
        ResponseBody = responseBody;
        Data = data;
    }

    /// <summary>JSON-RPC <c>error.code</c>. This is not the HTTP status.</summary>
    public int? Code { get; }

    /// <summary>HTTP status, when the failure came from an HTTP transport.</summary>
    public int? StatusCode { get; }

    /// <summary>MCP endpoint URL.</summary>
    public string? Url { get; }

    /// <summary>Raw response text.</summary>
    public string? ResponseBody { get; }

    /// <summary>JSON-RPC <c>error.data</c>, when the server sent it.</summary>
    public new JsonElement? Data { get; }

    /// <summary>Returns whether <paramref name="error"/> is an <see cref="MCPClientError"/>.</summary>
    public static bool IsInstance(Exception? error)
    {
        return error is MCPClientError;
    }
}
