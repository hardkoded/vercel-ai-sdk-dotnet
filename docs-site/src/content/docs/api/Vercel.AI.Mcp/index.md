---
title: "Vercel.AI.Mcp"
description: "Model Context Protocol client and transports."
editUrl: false
sidebar:
  label: "Vercel.AI.Mcp"
---

[API reference](/api/)

Model Context Protocol client and transports.

## Classes

| Type | Summary |
| --- | --- |
| [CallToolResults](/api/Vercel.AI.Mcp/CallToolResults/) | Normalizes MCP tools/call results, including structured-only results. |
| [HttpMcpTransport](/api/Vercel.AI.Mcp/HttpMcpTransport/) | MCP over Streamable HTTP. Posts JSON-RPC and accepts a JSON body or an SSE data: payload. |
| [JsonRpcMessages](/api/Vercel.AI.Mcp/JsonRpcMessages/) | Checks JSON-RPC 2.0 requests, notifications, results, and errors. |
| [McpAppFingerprint](/api/Vercel.AI.Mcp/McpAppFingerprint/) | SHA-256 fingerprint of an MCP app resource. |
| [McpClient](/api/Vercel.AI.Mcp/McpClient/) | Lists MCP tools and adapts them to Tool. |
| [MCPClientError](/api/Vercel.AI.Mcp/MCPClientError/) | An MCP call failed. Carries the JSON-RPC and HTTP details when the transport has them. |
| [McpNdjson](/api/Vercel.AI.Mcp/McpNdjson/) | Newline-delimited JSON used by the upstream stdio transport. |
| [McpNdjson.Buffer](/api/Vercel.AI.Mcp/McpNdjson.Buffer/) | Buffer that reassembles newline-delimited messages split across chunks. |
| [McpToolHeaders](/api/Vercel.AI.Mcp/McpToolHeaders/) | Reads x-mcp-header annotations and encodes Mcp-Param- headers. |
| [McpToolHeaders.Binding](/api/Vercel.AI.Mcp/McpToolHeaders.Binding/) | One header bound to a tool argument path. |
| [McpToolHeaders.BindingResult](/api/Vercel.AI.Mcp/McpToolHeaders.BindingResult/) | Result of reading header bindings. |
| [OAuthResources](/api/Vercel.AI.Mcp/OAuthResources/) | OAuth resource URL checks from the MCP authorization helper. |
| [SseMcpTransport](/api/Vercel.AI.Mcp/SseMcpTransport/) | MCP over the legacy HTTP+SSE transport. StartAsync(CancellationToken) opens a GET event stream, waits for the endpoint event, then CancellationToken) posts JSON-RPC messages to that endpoint. Responses arrive on the stream. |
| [StdioEnvironment](/api/Vercel.AI.Mcp/StdioEnvironment/) | Environment passed to an MCP stdio child. The caller's dictionary is not modified. |
| [StdioMcpOptions](/api/Vercel.AI.Mcp/StdioMcpOptions/) | How to start an MCP stdio server. Mirrors the upstream StdioConfig. |
| [StdioMcpTransport](/api/Vercel.AI.Mcp/StdioMcpTransport/) | MCP over a child process using newline-delimited JSON-RPC messages. |

## Interfaces

| Type | Summary |
| --- | --- |
| [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) | Sends one JSON-RPC call and returns the result object. |
