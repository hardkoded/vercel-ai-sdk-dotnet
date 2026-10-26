---
title: "McpToolHeaders.Binding"
description: "One header bound to a tool argument path."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpToolHeaders.Binding"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpToolHeaders_Binding"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpToolHeaders.Binding
```


One header bound to a tool argument path.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L340)

## Constructors

<a id="Vercel_AI_Mcp_McpToolHeaders_Binding__ctor_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_Binding__ctor_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_String_"></a>

### `Binding(string, IReadOnlyList<string>, string)`


```csharp
public Binding(string headerName, IReadOnlyList<string> path, string valueType)
```


Creates a binding.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `headerName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `path` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `valueType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L343)

## Properties

<a id="Vercel_AI_Mcp_McpToolHeaders_Binding_HeaderName_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_Binding_HeaderName"></a>

### HeaderName


```csharp
public string HeaderName { get; }
```


HTTP header token from `x-mcp-header`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L351)

<a id="Vercel_AI_Mcp_McpToolHeaders_Binding_Path_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_Binding_Path"></a>

### Path


```csharp
public IReadOnlyList<string> Path { get; }
```


Property path from the input object.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L354)

<a id="Vercel_AI_Mcp_McpToolHeaders_Binding_ValueType_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_Binding_ValueType"></a>

### ValueType


```csharp
public string ValueType { get; }
```


`string`, `integer`, or `boolean`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L357)
