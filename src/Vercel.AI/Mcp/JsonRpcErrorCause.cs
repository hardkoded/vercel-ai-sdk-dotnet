// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Mcp;

/// <summary>
/// Carries the JSON-RPC <c>error</c> object as the cause of an <see cref="MCPClientError"/>. Upstream sets
/// <c>cause: response.error</c> to the raw object. .NET causes must be exceptions, so the object is wrapped.
/// </summary>
internal sealed class JsonRpcErrorCause(JsonElement error) : Exception(error.GetRawText())
{
    /// <summary>The JSON-RPC <c>error</c> object as the server sent it.</summary>
    public JsonElement Error { get; } = error;
}
