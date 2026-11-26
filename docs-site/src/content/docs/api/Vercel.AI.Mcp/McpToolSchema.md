---
title: "McpToolSchema"
description: "The schemas of one MCP tool. Maps to an entry of the schemas option of client.tools()."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpToolSchema"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpToolSchema"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpToolSchema
```


The schemas of one MCP tool. Maps to an entry of the `schemas` option of `client.tools()`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L82)

## Constructors

<a id="Vercel_AI_Mcp_McpToolSchema__ctor_"></a>
<a id="Vercel_AI_Mcp_McpToolSchema__ctor_System_String_"></a>

### McpToolSchema(string)


```csharp
public McpToolSchema(string inputSchema)
```


Creates the schemas with the JSON Schema of the tool input.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L85)

## Properties

<a id="Vercel_AI_Mcp_McpToolSchema_InputSchema_"></a>
<a id="Vercel_AI_Mcp_McpToolSchema_InputSchema"></a>

### InputSchema


```csharp
public string InputSchema { get; }
```


JSON Schema of the tool input.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L91)

<a id="Vercel_AI_Mcp_McpToolSchema_OutputSchema_"></a>
<a id="Vercel_AI_Mcp_McpToolSchema_OutputSchema"></a>

### OutputSchema


```csharp
public string? OutputSchema { get; set; }
```


JSON Schema of the tool output. When set, the tool returns the `structuredContent` of the result, or the JSON
in its first text content, validated against it. Without it the tool returns the call-tool result.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpClient.cs#L97)
