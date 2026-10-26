---
title: "McpNdjson"
description: "Newline-delimited JSON used by the upstream stdio transport."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpNdjson"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpNdjson"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class McpNdjson
```


Newline-delimited JSON used by the upstream stdio transport.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L566)

## Methods

<a id="Vercel_AI_Mcp_McpNdjson_Serialize_"></a>
<a id="Vercel_AI_Mcp_McpNdjson_Serialize_System_Text_Json_JsonElement_"></a>

### Serialize(JsonElement)


```csharp
public static string Serialize(JsonElement message)
```


Serializes one message and appends a newline.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L569)

<a id="Vercel_AI_Mcp_McpNdjson_Write_"></a>
<a id="Vercel_AI_Mcp_McpNdjson_Write_System_IO_Stream_System_Text_Json_JsonElement_"></a>

### Write(Stream?, JsonElement)


```csharp
public static void Write(Stream? output, JsonElement message)
```


Writes one newline-delimited message. A null stream means the transport is not connected.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `output` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `message` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L575)

<a id="Vercel_AI_Mcp_McpNdjson_WriteAsync_"></a>
<a id="Vercel_AI_Mcp_McpNdjson_WriteAsync_System_IO_Stream_System_Text_Json_JsonElement_System_Threading_CancellationToken_"></a>

### WriteAsync(Stream?, JsonElement, CancellationToken)


```csharp
public static Task WriteAsync(Stream? output, JsonElement message, CancellationToken cancellationToken = default)
```


Writes one newline-delimited message without blocking a thread when the pipe is full.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `output` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `message` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L587)
