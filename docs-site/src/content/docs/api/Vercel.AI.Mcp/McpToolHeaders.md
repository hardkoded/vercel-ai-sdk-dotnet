---
title: "McpToolHeaders"
description: "Reads x-mcp-header annotations and encodes Mcp-Param- headers."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpToolHeaders"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpToolHeaders"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public static class McpToolHeaders
```


Reads `x-mcp-header` annotations and encodes `Mcp-Param-*` headers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L334)

## Methods

<a id="Vercel_AI_Mcp_McpToolHeaders_CreateHeaders_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_CreateHeaders_System_Collections_Generic_IReadOnlyList_Vercel_AI_Mcp_McpToolHeaders_Binding__System_Text_Json_JsonElement_"></a>

### `CreateHeaders(IReadOnlyList<Binding>, JsonElement)`


```csharp
public static IReadOnlyDictionary<string, string> CreateHeaders(IReadOnlyList<McpToolHeaders.Binding> bindings, JsonElement arguments)
```


Builds `Mcp-Param-*` headers from tool arguments.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bindings` | [IReadOnlyList&lt;McpToolHeaders.Binding&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `arguments` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L435)

<a id="Vercel_AI_Mcp_McpToolHeaders_EncodeValue_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_EncodeValue_System_String_"></a>

### EncodeValue(string)


```csharp
public static string EncodeValue(string value)
```


Encodes a header value, using the base64 sentinel when the text is not plain ASCII.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `value` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L393)

<a id="Vercel_AI_Mcp_McpToolHeaders_GetBindings_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_GetBindings_System_Text_Json_JsonElement_"></a>

### GetBindings(JsonElement)


```csharp
public static McpToolHeaders.BindingResult GetBindings(JsonElement inputSchema)
```


Collects statically reachable `x-mcp-header` annotations.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `inputSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [McpToolHeaders.BindingResult](/api/Vercel.AI.Mcp/McpToolHeaders.BindingResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L420)
