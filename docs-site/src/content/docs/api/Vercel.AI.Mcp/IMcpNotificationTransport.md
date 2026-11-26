---
title: "IMcpNotificationTransport"
description: "A transport that can send a JSON-RPC notification, which has no id and gets no response. The client sends notifications/initialized through it after initiali..."
editUrl: false
prev: false
next: false
sidebar:
  label: "IMcpNotificationTransport"
  hidden: true
---

<a id="Vercel_AI_Mcp_IMcpNotificationTransport"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public interface IMcpNotificationTransport : IMcpTransport
```


A transport that can send a JSON-RPC notification, which has no id and gets no response.
The client sends `notifications/initialized` through it after `initialize`. It is a separate interface because
[IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) cannot gain members on netstandard2.0.

**Derived:** [HttpMcpTransport](/api/Vercel.AI.Mcp/HttpMcpTransport/), [SseMcpTransport](/api/Vercel.AI.Mcp/SseMcpTransport/), [StdioMcpTransport](/api/Vercel.AI.Mcp/StdioMcpTransport/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L59)
