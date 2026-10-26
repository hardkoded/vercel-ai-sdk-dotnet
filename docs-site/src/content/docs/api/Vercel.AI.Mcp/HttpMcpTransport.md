---
title: "HttpMcpTransport"
description: "MCP over Streamable HTTP. Posts JSON-RPC and accepts a JSON body or an SSE data: payload."
editUrl: false
prev: false
next: false
sidebar:
  label: "HttpMcpTransport"
  hidden: true
---

<a id="Vercel_AI_Mcp_HttpMcpTransport"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class HttpMcpTransport : IMcpTransport
```


MCP over Streamable HTTP. Posts JSON-RPC and accepts a JSON body or an SSE `data:` payload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L427)

## Constructors

<a id="Vercel_AI_Mcp_HttpMcpTransport__ctor_"></a>
<a id="Vercel_AI_Mcp_HttpMcpTransport__ctor_System_Net_Http_HttpClient_System_Uri_"></a>

### HttpMcpTransport(HttpClient, Uri)


```csharp
public HttpMcpTransport(HttpClient httpClient, Uri endpoint)
```


Creates a transport that posts to &lt;code class="paramref"&gt;endpoint&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `endpoint` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L434)

## Methods

<a id="Vercel_AI_Mcp_HttpMcpTransport_CallAsync_"></a>
<a id="Vercel_AI_Mcp_HttpMcpTransport_CallAsync_System_String_System_Text_Json_JsonElement_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L441)
