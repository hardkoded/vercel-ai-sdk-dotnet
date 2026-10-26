---
title: "StdioMcpTransport"
description: "MCP over a child process using newline-delimited JSON-RPC messages."
editUrl: false
prev: false
next: false
sidebar:
  label: "StdioMcpTransport"
  hidden: true
---

<a id="Vercel_AI_Mcp_StdioMcpTransport"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class StdioMcpTransport : IMcpTransport, IDisposable
```


MCP over a child process using newline-delimited JSON-RPC messages.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/), [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L208)

## Properties

<a id="Vercel_AI_Mcp_StdioMcpTransport_ProcessId_"></a>
<a id="Vercel_AI_Mcp_StdioMcpTransport_ProcessId"></a>

### ProcessId


```csharp
public int ProcessId { get; }
```


Child process id.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L231)

<a id="Vercel_AI_Mcp_StdioMcpTransport_StandardError_"></a>
<a id="Vercel_AI_Mcp_StdioMcpTransport_StandardError"></a>

### StandardError


```csharp
public StreamReader? StandardError { get; }
```


The child's stderr, when [RedirectStandardError](/api/Vercel.AI.Mcp/StdioMcpOptions/#Vercel_AI_Mcp_StdioMcpOptions_RedirectStandardError) is true.

**Value:** [StreamReader](https://learn.microsoft.com/dotnet/api/system.io.streamreader)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L234)

## Methods

<a id="Vercel_AI_Mcp_StdioMcpTransport_CallAsync_"></a>
<a id="Vercel_AI_Mcp_StdioMcpTransport_CallAsync_System_String_System_Text_Json_JsonElement_System_Threading_CancellationToken_"></a>

### CallAsync(string, JsonElement, CancellationToken)


```csharp
public Task<JsonElement> CallAsync(string method, JsonElement parameters, CancellationToken cancellationToken)
```


Calls &lt;code class="paramref"&gt;method&lt;/code&gt; and returns the JSON-RPC result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `method` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `parameters` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L324)

<a id="Vercel_AI_Mcp_StdioMcpTransport_CreateStartInfo_"></a>
<a id="Vercel_AI_Mcp_StdioMcpTransport_CreateStartInfo_Vercel_AI_Mcp_StdioMcpOptions_"></a>

### CreateStartInfo(StdioMcpOptions)


```csharp
public static ProcessStartInfo CreateStartInfo(StdioMcpOptions options)
```


Builds the process start info. The environment is replaced by `Boolean%7d)`,
so the child does not inherit every variable of this process.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StdioMcpOptions](/api/Vercel.AI.Mcp/StdioMcpOptions/) |  |

**Returns:** [ProcessStartInfo](https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L263)

<a id="Vercel_AI_Mcp_StdioMcpTransport_Dispose_"></a>
<a id="Vercel_AI_Mcp_StdioMcpTransport_Dispose"></a>

### Dispose()


```csharp
public void Dispose()
```


Kills the child process, if one was started.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L370)

<a id="Vercel_AI_Mcp_StdioMcpTransport_Start_"></a>
<a id="Vercel_AI_Mcp_StdioMcpTransport_Start_System_String_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `Start(string, IReadOnlyList<string>?)`


```csharp
public static StdioMcpTransport Start(string fileName, IReadOnlyList<string>? arguments = null)
```


Starts &lt;code class="paramref"&gt;fileName&lt;/code&gt; and speaks MCP on its stdin and stdout.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `arguments` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [StdioMcpTransport](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L237)

<a id="Vercel_AI_Mcp_StdioMcpTransport_Start_Vercel_AI_Mcp_StdioMcpOptions_"></a>

### Start(StdioMcpOptions)


```csharp
public static StdioMcpTransport Start(StdioMcpOptions options)
```


Starts the server described by &lt;code class="paramref"&gt;options&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [StdioMcpOptions](/api/Vercel.AI.Mcp/StdioMcpOptions/) |  |

**Returns:** [StdioMcpTransport](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L243)
