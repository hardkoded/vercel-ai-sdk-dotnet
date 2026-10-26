---
title: "McpToolHeaders.BindingResult"
description: "Result of reading header bindings."
editUrl: false
prev: false
next: false
sidebar:
  label: "McpToolHeaders.BindingResult"
  hidden: true
---

<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult"></a>

Namespace: [Vercel.AI.Mcp](/api/Vercel.AI.Mcp/)


```csharp
public sealed class McpToolHeaders.BindingResult
```


Result of reading header bindings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L361)

## Properties

<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Bindings_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Bindings"></a>

### Bindings


```csharp
public IReadOnlyList<McpToolHeaders.Binding> Bindings { get; }
```


Bindings in visit order.

**Value:** [IReadOnlyList&lt;McpToolHeaders.Binding&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L374)

<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Error_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Error"></a>

### Error


```csharp
public string? Error { get; }
```


Error text when [Success](#Vercel_AI_Mcp_McpToolHeaders_BindingResult_Success) is false.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L377)

<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Success_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Success"></a>

### Success


```csharp
public bool Success { get; }
```


True when every annotation was valid.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L371)

## Methods

<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Fail_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Fail_System_String_"></a>

### Fail(string)


```csharp
public static McpToolHeaders.BindingResult Fail(string error)
```


Creates a failure result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [McpToolHeaders.BindingResult](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L386)

<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Ok_"></a>
<a id="Vercel_AI_Mcp_McpToolHeaders_BindingResult_Ok_System_Collections_Generic_IReadOnlyList_Vercel_AI_Mcp_McpToolHeaders_Binding__"></a>

### `Ok(IReadOnlyList<Binding>)`


```csharp
public static McpToolHeaders.BindingResult Ok(IReadOnlyList<McpToolHeaders.Binding> bindings)
```


Creates a success result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `bindings` | [IReadOnlyList&lt;McpToolHeaders.Binding&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [McpToolHeaders.BindingResult](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Mcp/McpProtocol.cs#L380)
