---
title: "McpClient"
description: "Lists MCP tools and adapts them to Tool."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpClient"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpClient"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class McpClient
```


Lists MCP tools and adapts them to [Tool](/api/Vercel.AI/Tool/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L25)

## Methods

<a id="Vercel_AI_Mcp_McpClient_ConnectAsync_"></a>
<a id="Vercel_AI_Mcp_McpClient_ConnectAsync_Vercel_AI_Mcp_IMcpTransport_System_Threading_CancellationToken_"></a>

### ConnectAsync(IMcpTransport, CancellationToken)


```csharp
public static Task<IReadOnlyList<Tool>> ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
```


Calls `initialize`, then `tools/list`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transport` | [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L28)

<a id="Vercel_AI_Mcp_McpClient_ListToolsAsync_"></a>
<a id="Vercel_AI_Mcp_McpClient_ListToolsAsync_Vercel_AI_Mcp_IMcpTransport_System_Threading_CancellationToken_"></a>

### ListToolsAsync(IMcpTransport, CancellationToken)


```csharp
public static Task<IReadOnlyList<Tool>> ListToolsAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
```


Maps `tools/list` onto SDK tools. Execution calls `tools/call`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transport` | [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L41)
