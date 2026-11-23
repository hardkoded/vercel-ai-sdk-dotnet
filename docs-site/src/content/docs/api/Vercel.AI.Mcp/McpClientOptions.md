---
title: "McpClientOptions"
description: "Options of CancellationToken). Maps to MCPClientConfig."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpClientOptions"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpClientOptions"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpClientOptions
```


Options of `CancellationToken)`. Maps to `MCPClientConfig`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L43)

## Properties

<a id="Vercel_AI_Mcp_McpClientOptions_ProtocolVersionDiscovery_"></a>
<a id="Vercel_AI_Mcp_McpClientOptions_ProtocolVersionDiscovery"></a>

### ProtocolVersionDiscovery


```csharp
public bool ProtocolVersionDiscovery { get; set; }
```


Whether transports that support stateless protocol discovery probe with `server/discover` before falling back
to legacy initialization. Disable it for legacy servers that require `initialize` to be the first request. Default true.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L49)
