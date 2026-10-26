---
title: "CallToolResults"
description: "Normalizes MCP tools/call results, including structured-only results."
editUrl: false
prev: false
next: false
sidebar:
  label: "CallToolResults"
  hidden: true
---

<a id="Vercel_AI_Mcp_CallToolResults"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class CallToolResults
```


Normalizes MCP `tools/call` results, including structured-only results.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L127)

## Properties

<a id="Vercel_AI_Mcp_CallToolResults_Undefined_"></a>
<a id="Vercel_AI_Mcp_CallToolResults_Undefined"></a>

### Undefined


```csharp
public static object Undefined { get; }
```


A value that is not JSON, matching an omitted field.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L130)

## Methods

<a id="Vercel_AI_Mcp_CallToolResults_IsJsonValue_"></a>
<a id="Vercel_AI_Mcp_CallToolResults_IsJsonValue_System_Object_"></a>

### IsJsonValue(object?)


```csharp
public static bool IsJsonValue(object? value)
```


True when &lt;code class="paramref"&gt;value&lt;/code&gt; can be JSON structured content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L133)

<a id="Vercel_AI_Mcp_CallToolResults_Normalize_"></a>
<a id="Vercel_AI_Mcp_CallToolResults_Normalize_System_Text_Json_JsonElement_"></a>

### Normalize(JsonElement)


```csharp
public static JsonElement Normalize(JsonElement result)
```


Normalizes a call-tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L159)

<a id="Vercel_AI_Mcp_CallToolResults_Normalize_System_Text_Json_Nodes_JsonNode_"></a>

### Normalize(JsonNode)


```csharp
public static JsonElement Normalize(JsonNode result)
```


Normalizes a call-tool result held as a JSON node so cycles can be rejected.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `result` | [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L192)
