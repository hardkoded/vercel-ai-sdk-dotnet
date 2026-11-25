---
title: "IMcpProtocolVersionDiscoveryTransport"
description: "A transport that can probe the server with server/discover before legacy initialization. Maps to the supportsProtocolVersionDiscovery and waitsForProtocolVer..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IMcpProtocolVersionDiscoveryTransport"
  hidden: true
---

<a id="Vercel_AI_Mcp_IMcpProtocolVersionDiscoveryTransport"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public interface IMcpProtocolVersionDiscoveryTransport : IMcpTransport
```


A transport that can probe the server with `server/discover` before legacy initialization.
Maps to the `supportsProtocolVersionDiscovery` and `waitsForProtocolVersionDiscovery` members of upstream's
`MCPTransport`. It is a separate interface because [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) cannot gain members on netstandard2.0.

**Derived:** [HttpMcpTransport](/api/Vercel.AI.Mcp/HttpMcpTransport/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L33)
