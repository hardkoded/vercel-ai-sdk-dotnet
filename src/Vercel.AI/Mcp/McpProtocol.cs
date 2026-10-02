// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Mcp;

/// <summary>MCP protocol versions this client can negotiate.</summary>
public static class McpProtocol
{
    /// <summary>Newest protocol version advertised during initialize.</summary>
    public const string Latest = "2026-07-28";

    /// <summary>Newest protocol version that still uses the legacy initialize flow.</summary>
    public const string LatestLegacy = "2025-11-25";

    /// <summary>Protocol versions the client accepts, newest first.</summary>
    public static readonly IReadOnlyList<string> Supported = new[]
    {
        Latest,
        LatestLegacy,
        "2025-06-18",
        "2025-03-26",
        "2024-11-05",
    };

    /// <summary>Client name sent in <c>clientInfo</c>.</summary>
    public const string ClientName = "ai-sdk-mcp-client";

    /// <summary>Client version sent in <c>clientInfo</c>.</summary>
    public const string ClientVersion = "1.0.0";
}
