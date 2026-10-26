---
title: "MCPClientError"
description: "An MCP call failed. Carries the JSON-RPC and HTTP details when the transport has them."
editUrl: false
prev: false
next: false
sidebar:
  label: "MCPClientError"
  hidden: true
---

<a id="Vercel_AI_Mcp_MCPClientError"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public class MCPClientError : AiSdkException, ISerializable
```


An MCP call failed. Carries the JSON-RPC and HTTP details when the transport has them.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L11)

## Constructors

<a id="Vercel_AI_Mcp_MCPClientError__ctor_"></a>
<a id="Vercel_AI_Mcp_MCPClientError__ctor_System_String_System_Nullable_System_Int32__System_Nullable_System_Int32__System_String_System_String_System_Nullable_System_Text_Json_JsonElement__System_Exception_"></a>

### MCPClientError(string, int?, int?, string?, string?, JsonElement?, Exception?)


```csharp
public MCPClientError(string message, int? code = null, int? statusCode = null, string? url = null, string? responseBody = null, JsonElement? data = null, Exception? cause = null)
```


Creates the error with optional MCP error metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `statusCode` | [int?](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cause` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L14)

## Properties

<a id="Vercel_AI_Mcp_MCPClientError_Code_"></a>
<a id="Vercel_AI_Mcp_MCPClientError_Code"></a>

### Code


```csharp
public int? Code { get; }
```


JSON-RPC `error.code`. This is not the HTTP status.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L25)

<a id="Vercel_AI_Mcp_MCPClientError_Data_"></a>
<a id="Vercel_AI_Mcp_MCPClientError_Data"></a>

### Data


```csharp
public JsonElement? Data { get; }
```


JSON-RPC `error.data`, when the server sent it.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L37)

<a id="Vercel_AI_Mcp_MCPClientError_ResponseBody_"></a>
<a id="Vercel_AI_Mcp_MCPClientError_ResponseBody"></a>

### ResponseBody


```csharp
public string? ResponseBody { get; }
```


Raw response text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L34)

<a id="Vercel_AI_Mcp_MCPClientError_StatusCode_"></a>
<a id="Vercel_AI_Mcp_MCPClientError_StatusCode"></a>

### StatusCode


```csharp
public int? StatusCode { get; }
```


HTTP status, when the failure came from an HTTP transport.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L28)

<a id="Vercel_AI_Mcp_MCPClientError_Url_"></a>
<a id="Vercel_AI_Mcp_MCPClientError_Url"></a>

### Url


```csharp
public string? Url { get; }
```


MCP endpoint URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L31)

## Methods

<a id="Vercel_AI_Mcp_MCPClientError_IsInstance_"></a>
<a id="Vercel_AI_Mcp_MCPClientError_IsInstance_System_Exception_"></a>

### IsInstance(Exception?)


```csharp
public static bool IsInstance(Exception? error)
```


Returns whether &lt;code class="paramref"&gt;error&lt;/code&gt; is an [MCPClientError](./).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/MCPClientError.cs#L40)
