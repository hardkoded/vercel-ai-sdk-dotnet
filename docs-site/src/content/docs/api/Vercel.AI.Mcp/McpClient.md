---
title: "McpClient"
description: "An MCP client. It connects to a server, keeps what the server announced, and adapts MCP tools to Tool."
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
public sealed class McpClient
```


An MCP client. It connects to a server, keeps what the server announced, and adapts MCP tools to [Tool](/api/Vercel.AI/Tool/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L154)

## Fields

<a id="Vercel_AI_Mcp_McpClient_LatestLegacyProtocolVersion"></a>

### LatestLegacyProtocolVersion


```csharp
public const string LatestLegacyProtocolVersion = "2025-11-25"
```


Latest legacy MCP protocol version. Maps to `LATEST_LEGACY_PROTOCOL_VERSION`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L160)

<a id="Vercel_AI_Mcp_McpClient_LatestProtocolVersion"></a>

### LatestProtocolVersion


```csharp
public const string LatestProtocolVersion = "2026-07-28"
```


Latest MCP protocol version. Maps to `LATEST_PROTOCOL_VERSION`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L157)

## Properties

<a id="Vercel_AI_Mcp_McpClient_InitializeResult_"></a>
<a id="Vercel_AI_Mcp_McpClient_InitializeResult"></a>

### InitializeResult


```csharp
public McpInitializeResult InitializeResult { get; }
```


The result of initialization or discovery. Maps to `initializeResult`.

**Value:** [McpInitializeResult](/api/Vercel.AI.Mcp/McpInitializeResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L194)

<a id="Vercel_AI_Mcp_McpClient_Instructions_"></a>
<a id="Vercel_AI_Mcp_McpClient_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; }
```


The server instructions from initialization or discovery, if any. Maps to `instructions`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L199)

<a id="Vercel_AI_Mcp_McpClient_ServerInfo_"></a>
<a id="Vercel_AI_Mcp_McpClient_ServerInfo"></a>

### ServerInfo


```csharp
public McpConfiguration ServerInfo { get; }
```


The server name and version from initialization or discovery. Maps to `serverInfo`.

**Value:** [McpConfiguration](/api/Vercel.AI.Mcp/McpConfiguration/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L191)

## Methods

<a id="Vercel_AI_Mcp_McpClient_ConnectAsync_"></a>
<a id="Vercel_AI_Mcp_McpClient_ConnectAsync_Vercel_AI_Mcp_IMcpTransport_System_Threading_CancellationToken_"></a>

### ConnectAsync(IMcpTransport, CancellationToken)


```csharp
public static Task<IReadOnlyList<Tool>> ConnectAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
```


Connects to the server and returns its tools: initializes, then calls `tools/list`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transport` | [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L202)

<a id="Vercel_AI_Mcp_McpClient_ConnectAsync_Vercel_AI_Mcp_IMcpTransport_Vercel_AI_Mcp_McpClientOptions_System_Threading_CancellationToken_"></a>

### ConnectAsync(IMcpTransport, McpClientOptions, CancellationToken)


```csharp
public static Task<IReadOnlyList<Tool>> ConnectAsync(IMcpTransport transport, McpClientOptions options, CancellationToken cancellationToken = default)
```


Probes with `server/discover` when [ProtocolVersionDiscovery](/api/Vercel.AI.Mcp/McpClientOptions/#Vercel_AI_Mcp_McpClientOptions_ProtocolVersionDiscovery) and the transport allow it,
otherwise calls `initialize`. Then calls `tools/list`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transport` | [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) |  |
| `options` | [McpClientOptions](/api/Vercel.AI.Mcp/McpClientOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L211)

<a id="Vercel_AI_Mcp_McpClient_CreateAsync_"></a>
<a id="Vercel_AI_Mcp_McpClient_CreateAsync_Vercel_AI_Mcp_IMcpTransport_Vercel_AI_Mcp_McpClientOptions_System_Threading_CancellationToken_"></a>

### CreateAsync(IMcpTransport, McpClientOptions?, CancellationToken)


```csharp
public static Task<McpClient> CreateAsync(IMcpTransport transport, McpClientOptions? options = null, CancellationToken cancellationToken = default)
```


Creates a client and initializes it: probes with `server/discover` when
[ProtocolVersionDiscovery](/api/Vercel.AI.Mcp/McpClientOptions/#Vercel_AI_Mcp_McpClientOptions_ProtocolVersionDiscovery) and the transport allow it, otherwise calls
`initialize` and sends `notifications/initialized`. Maps to `createMCPClient`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transport` | [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) |  |
| `options` | [McpClientOptions](/api/Vercel.AI.Mcp/McpClientOptions/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;McpClient&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L228)

<a id="Vercel_AI_Mcp_McpClient_ListToolsAsync_"></a>
<a id="Vercel_AI_Mcp_McpClient_ListToolsAsync_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Mcp_McpToolSchema__System_Threading_CancellationToken_"></a>

### `ListToolsAsync(IReadOnlyDictionary<string, McpToolSchema>?, CancellationToken)`


```csharp
public Task<IReadOnlyList<Tool>> ListToolsAsync(IReadOnlyDictionary<string, McpToolSchema>? schemas, CancellationToken cancellationToken = default)
```


Maps `tools/list` onto SDK tools. With &lt;code class="paramref"&gt;schemas&lt;/code&gt;, only the listed tools are returned,
typed by their schemas. Maps to `client.tools({ schemas })`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `schemas` | [IReadOnlyDictionary&lt;string, McpToolSchema&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L267)

<a id="Vercel_AI_Mcp_McpClient_ListToolsAsync_System_Threading_CancellationToken_"></a>

### ListToolsAsync(CancellationToken)


```csharp
public Task<IReadOnlyList<Tool>> ListToolsAsync(CancellationToken cancellationToken = default)
```


Maps `tools/list` onto SDK tools. Execution calls `tools/call`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L259)

<a id="Vercel_AI_Mcp_McpClient_ListToolsAsync_Vercel_AI_Mcp_IMcpTransport_System_Threading_CancellationToken_"></a>

### ListToolsAsync(IMcpTransport, CancellationToken)


```csharp
public static Task<IReadOnlyList<Tool>> ListToolsAsync(IMcpTransport transport, CancellationToken cancellationToken = default)
```


Maps `tools/list` onto SDK tools without initializing. Execution calls `tools/call`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `transport` | [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;IReadOnlyList&lt;Tool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L245)
