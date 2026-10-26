---
title: "ExecutedTool"
description: "The result of executing a tool."
editUrl: false
prev: false
next: false
sidebar:
  label: "ExecutedTool"
  hidden: true
---

<a id="Vercel_AI_ExecutedTool"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ExecutedTool
```


The result of executing a tool.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L171)

## Constructors

<a id="Vercel_AI_ExecutedTool__ctor_"></a>
<a id="Vercel_AI_ExecutedTool__ctor_System_String_System_String_System_String_System_Boolean_"></a>

### ExecutedTool(string, string, string, bool)


```csharp
public ExecutedTool(string toolCallId, string toolName, string outputJson, bool isError)
```


Creates a tool result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isError` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L174)

## Properties

<a id="Vercel_AI_ExecutedTool_Dynamic_"></a>
<a id="Vercel_AI_ExecutedTool_Dynamic"></a>

### Dynamic


```csharp
public bool Dynamic { get; init; }
```


True when the tool is dynamic: a dynamic tool, a provider-executed dynamic call, or an invalid call. Maps to the result's `dynamic`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L207)

<a id="Vercel_AI_ExecutedTool_Error_"></a>
<a id="Vercel_AI_ExecutedTool_Error"></a>

### Error


```csharp
public Exception? Error { get; init; }
```


The exception the tool threw, when [IsError](#Vercel_AI_ExecutedTool_IsError) is set by a failed execution. Maps to the error part's `error`.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L195)

<a id="Vercel_AI_ExecutedTool_InputJson_"></a>
<a id="Vercel_AI_ExecutedTool_InputJson"></a>

### InputJson


```csharp
public string? InputJson { get; init; }
```


The JSON input of the tool call this result answers, after any repair and refinement. Maps to the result's `input`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L204)

<a id="Vercel_AI_ExecutedTool_IsError_"></a>
<a id="Vercel_AI_ExecutedTool_IsError"></a>

### IsError


```csharp
public bool IsError { get; }
```


Whether execution failed or was denied.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L192)

<a id="Vercel_AI_ExecutedTool_OutputJson_"></a>
<a id="Vercel_AI_ExecutedTool_OutputJson"></a>

### OutputJson


```csharp
public string OutputJson { get; }
```


JSON output.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L189)

<a id="Vercel_AI_ExecutedTool_ProviderExecuted_"></a>
<a id="Vercel_AI_ExecutedTool_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool ProviderExecuted { get; init; }
```


True when the provider ran the tool. Maps to the result's `providerExecuted`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L198)

<a id="Vercel_AI_ExecutedTool_ProviderMetadata_"></a>
<a id="Vercel_AI_ExecutedTool_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; init; }
```


Provider-specific metadata of the result, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L201)

<a id="Vercel_AI_ExecutedTool_Title_"></a>
<a id="Vercel_AI_ExecutedTool_Title"></a>

### Title


```csharp
public string? Title { get; init; }
```


Display title of the tool. Maps to the result's `title`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L210)

<a id="Vercel_AI_ExecutedTool_ToolCallId_"></a>
<a id="Vercel_AI_ExecutedTool_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L183)

<a id="Vercel_AI_ExecutedTool_ToolMetadata_"></a>
<a id="Vercel_AI_ExecutedTool_ToolMetadata"></a>

### ToolMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ToolMetadata { get; init; }
```


Metadata of the tool. It stays apart from [ProviderMetadata](#Vercel_AI_ExecutedTool_ProviderMetadata). Maps to the result's `toolMetadata`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L213)

<a id="Vercel_AI_ExecutedTool_ToolName_"></a>
<a id="Vercel_AI_ExecutedTool_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L186)
