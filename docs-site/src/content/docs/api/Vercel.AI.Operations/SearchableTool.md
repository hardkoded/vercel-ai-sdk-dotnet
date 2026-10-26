---
title: "SearchableTool"
description: "A tool the search step can hide until it is discovered."
editUrl: false
prev: false
next: false
sidebar:
  label: "SearchableTool"
  hidden: true
---

<a id="Vercel_AI_Operations_SearchableTool"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SearchableTool
```


A tool the search step can hide until it is discovered.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L17)

## Constructors

<a id="Vercel_AI_Operations_SearchableTool__ctor_"></a>
<a id="Vercel_AI_Operations_SearchableTool__ctor_System_String_"></a>

### SearchableTool(string?)


```csharp
public SearchableTool(string? description = null)
```


Creates a tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L20)

## Properties

<a id="Vercel_AI_Operations_SearchableTool_Caller_"></a>
<a id="Vercel_AI_Operations_SearchableTool_Caller"></a>

### Caller


```csharp
public ToolCallerBinding? Caller { get; set; }
```


Local or provider caller bound to this tool.

**Value:** [ToolCallerBinding](/api/Vercel.AI.Operations/ToolCallerBinding/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L41)

<a id="Vercel_AI_Operations_SearchableTool_DeferLoading_"></a>
<a id="Vercel_AI_Operations_SearchableTool_DeferLoading"></a>

### DeferLoading


```csharp
public bool DeferLoading { get; set; }
```


True when the tool stays hidden until search discovers it.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L29)

<a id="Vercel_AI_Operations_SearchableTool_Describe_"></a>
<a id="Vercel_AI_Operations_SearchableTool_Describe"></a>

### Describe


```csharp
public Func<IReadOnlyDictionary<string, object?>, string?>? Describe { get; set; }
```


Description resolved from the current tool context.

**Value:** [Func&lt;IReadOnlyDictionary&lt;string, object&gt;, string&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L35)

<a id="Vercel_AI_Operations_SearchableTool_Description_"></a>
<a id="Vercel_AI_Operations_SearchableTool_Description"></a>

### Description


```csharp
public string? Description { get; set; }
```


Static description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L32)

<a id="Vercel_AI_Operations_SearchableTool_Execute_"></a>
<a id="Vercel_AI_Operations_SearchableTool_Execute"></a>

### Execute


```csharp
public Func<string, IReadOnlyDictionary<string, object?>?, Task<ToolSearchMatchList>>? Execute { get; set; }
```


Bound search implementation. The unbound search tool throws.

**Value:** [Func&lt;string, IReadOnlyDictionary&lt;string, object&gt;, Task&lt;ToolSearchMatchList&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L38)

<a id="Vercel_AI_Operations_SearchableTool_InputSchemaJson_"></a>
<a id="Vercel_AI_Operations_SearchableTool_InputSchemaJson"></a>

### InputSchemaJson


```csharp
public string? InputSchemaJson { get; set; }
```


Input schema JSON, kept stable across steps.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L44)

<a id="Vercel_AI_Operations_SearchableTool_Invoke_"></a>
<a id="Vercel_AI_Operations_SearchableTool_Invoke"></a>

### Invoke


```csharp
public Func<string, CancellationToken, Task<string>>? Invoke { get; set; }
```


Executes a discovered tool.

**Value:** [Func&lt;string, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L47)

<a id="Vercel_AI_Operations_SearchableTool_IsSearch_"></a>
<a id="Vercel_AI_Operations_SearchableTool_IsSearch"></a>

### IsSearch


```csharp
public bool IsSearch { get; set; }
```


True when this tool is the search tool. Copying the tool keeps the flag.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L26)

## Methods

<a id="Vercel_AI_Operations_SearchableTool_Copy_"></a>
<a id="Vercel_AI_Operations_SearchableTool_Copy"></a>

### Copy()


```csharp
public SearchableTool Copy()
```


Copies the search marker and the other fields.

**Returns:** [SearchableTool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L50)
