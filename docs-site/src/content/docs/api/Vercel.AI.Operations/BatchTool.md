---
title: "BatchTool"
description: "A tool definition forwarded to a batch without being executed."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchTool"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchTool"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchTool
```


A tool definition forwarded to a batch without being executed.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L11)

## Constructors

<a id="Vercel_AI_Operations_BatchTool__ctor_"></a>
<a id="Vercel_AI_Operations_BatchTool__ctor_System_String_System_Text_Json_JsonElement_System_String_System_Func_System_Text_Json_JsonElement_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_Object___"></a>

### `BatchTool(string, JsonElement, string?, Func<JsonElement, CancellationToken, Task<object?>>?)`


```csharp
public BatchTool(string name, JsonElement inputSchema, string? description = null, Func<JsonElement, CancellationToken, Task<object?>>? execute = null)
```


Creates a tool definition.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `inputSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `execute` | [Func&lt;JsonElement, CancellationToken, Task&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L14)

## Properties

<a id="Vercel_AI_Operations_BatchTool_Description_"></a>
<a id="Vercel_AI_Operations_BatchTool_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L29)

<a id="Vercel_AI_Operations_BatchTool_Execute_"></a>
<a id="Vercel_AI_Operations_BatchTool_Execute"></a>

### Execute


```csharp
public Func<JsonElement, CancellationToken, Task<object?>>? Execute { get; }
```


Local execute function. Batch submission does not call it.

**Value:** [Func&lt;JsonElement, CancellationToken, Task&lt;object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L32)

<a id="Vercel_AI_Operations_BatchTool_InputSchema_"></a>
<a id="Vercel_AI_Operations_BatchTool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement InputSchema { get; }
```


JSON schema.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L26)

<a id="Vercel_AI_Operations_BatchTool_Name_"></a>
<a id="Vercel_AI_Operations_BatchTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L23)
