---
title: "McpNdjson.Buffer"
description: "Buffer that reassembles newline-delimited messages split across chunks."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpNdjson.Buffer"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpNdjson_Buffer"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpNdjson.Buffer
```


Buffer that reassembles newline-delimited messages split across chunks.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L599)

## Methods

<a id="Vercel_AI_Mcp_McpNdjson_Buffer_Append_"></a>
<a id="Vercel_AI_Mcp_McpNdjson_Buffer_Append_System_Byte___"></a>

### Append(byte[])


```csharp
public void Append(byte[] chunk)
```


Appends the next stdout chunk.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `chunk` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L604)

<a id="Vercel_AI_Mcp_McpNdjson_Buffer_Clear_"></a>
<a id="Vercel_AI_Mcp_McpNdjson_Buffer_Clear"></a>

### Clear()


```csharp
public void Clear()
```


Drops buffered bytes.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L638)

<a id="Vercel_AI_Mcp_McpNdjson_Buffer_ReadLine_"></a>
<a id="Vercel_AI_Mcp_McpNdjson_Buffer_ReadLine"></a>

### ReadLine()


```csharp
public string? ReadLine()
```


Returns the next line without its newline, or null when the line is incomplete.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L615)
