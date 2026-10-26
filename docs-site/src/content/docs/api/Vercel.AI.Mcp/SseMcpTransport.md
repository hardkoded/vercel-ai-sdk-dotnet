---
title: "SseMcpTransport"
description: "MCP over the legacy HTTP+SSE transport. StartAsync(CancellationToken) opens a GET event stream, waits for the endpoint event, then CancellationToken) posts J..."
editUrl: false
prev: false
next: false
sidebar:
  label: "SseMcpTransport"
  hidden: true
---

<a id="Vercel_AI_Mcp_SseMcpTransport"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class SseMcpTransport : IMcpTransport, IDisposable
```


MCP over the legacy HTTP+SSE transport. [StartAsync(CancellationToken)](#Vercel_AI_Mcp_SseMcpTransport_StartAsync_System_Threading_CancellationToken_) opens a `GET` event stream, waits for the
`endpoint` event, then `CancellationToken)` posts JSON-RPC messages to that endpoint. Responses arrive on the stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IMcpTransport](/api/Vercel.AI.Mcp/IMcpTransport/), [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L18)

## Constructors

<a id="Vercel_AI_Mcp_SseMcpTransport__ctor_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport__ctor_System_Net_Http_HttpClient_System_Uri_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `SseMcpTransport(HttpClient, Uri, IReadOnlyDictionary<string, string>?)`


```csharp
public SseMcpTransport(HttpClient httpClient, Uri url, IReadOnlyDictionary<string, string>? headers = null)
```


Creates a transport for the SSE endpoint at &lt;code class="paramref"&gt;url&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `httpClient` | [HttpClient](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) |  |
| `url` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L35)

## Fields

<a id="Vercel_AI_Mcp_SseMcpTransport_LatestLegacyProtocolVersion"></a>

### LatestLegacyProtocolVersion


```csharp
public const string LatestLegacyProtocolVersion = "2024-11-05"
```


Protocol version sent in `mcp-protocol-version` until [ProtocolVersion](#Vercel_AI_Mcp_SseMcpTransport_ProtocolVersion) is set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L21)

## Properties

<a id="Vercel_AI_Mcp_SseMcpTransport_OnClose_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_OnClose"></a>

### OnClose


```csharp
public Action? OnClose { get; set; }
```


Called when the stream closes after [CloseAsync()](#Vercel_AI_Mcp_SseMcpTransport_CloseAsync).

**Value:** [Action](https://learn.microsoft.com/dotnet/api/system.action)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L43)

<a id="Vercel_AI_Mcp_SseMcpTransport_OnError_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_OnError"></a>

### OnError


```csharp
public Action<Exception>? OnError { get; set; }
```


Called with each transport error, such as a failed connection or an unparsable message.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L46)

<a id="Vercel_AI_Mcp_SseMcpTransport_OnMessage_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_OnMessage"></a>

### OnMessage


```csharp
public Action<JsonElement>? OnMessage { get; set; }
```


Called with each JSON-RPC message the server sends.

**Value:** [Action&lt;JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L49)

<a id="Vercel_AI_Mcp_SseMcpTransport_ProtocolVersion_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_ProtocolVersion"></a>

### ProtocolVersion


```csharp
public string? ProtocolVersion { get; set; }
```


Negotiated protocol version. Defaults to [LatestLegacyProtocolVersion](#Vercel_AI_Mcp_SseMcpTransport_LatestLegacyProtocolVersion) when unset.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L52)

## Methods

<a id="Vercel_AI_Mcp_SseMcpTransport_CallAsync_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_CallAsync_System_String_System_Text_Json_JsonElement_System_Threading_CancellationToken_"></a>

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L156)

<a id="Vercel_AI_Mcp_SseMcpTransport_CloseAsync_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_CloseAsync"></a>

### CloseAsync()


```csharp
public Task CloseAsync()
```


Closes the event stream and fails pending calls.

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L197)

<a id="Vercel_AI_Mcp_SseMcpTransport_DeserializeMessage_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_DeserializeMessage_System_String_"></a>

### DeserializeMessage(string)


```csharp
public static JsonElement DeserializeMessage(string line)
```


Parses one JSON-RPC message. Prototype-pollution keys are rejected.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `line` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L231)

<a id="Vercel_AI_Mcp_SseMcpTransport_Dispose_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_Dispose"></a>

### Dispose()


```csharp
public void Dispose()
```


Closes the transport.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L221)

<a id="Vercel_AI_Mcp_SseMcpTransport_SendAsync_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_SendAsync_System_Text_Json_JsonElement_System_Threading_CancellationToken_"></a>

### SendAsync(JsonElement, CancellationToken)


```csharp
public Task SendAsync(JsonElement message, CancellationToken cancellationToken = default)
```


Posts one JSON-RPC message to the endpoint the server announced.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L115)

<a id="Vercel_AI_Mcp_SseMcpTransport_StartAsync_"></a>
<a id="Vercel_AI_Mcp_SseMcpTransport_StartAsync_System_Threading_CancellationToken_"></a>

### StartAsync(CancellationToken)


```csharp
public Task StartAsync(CancellationToken cancellationToken = default)
```


Opens the event stream and completes once the server has sent its `endpoint` event.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/SseMcpTransport.cs#L55)
