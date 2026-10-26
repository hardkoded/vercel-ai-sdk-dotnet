---
title: "ToolSearchMatch"
description: "One discovered tool. Schemas are not included."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchMatch"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchMatch"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolSearchMatch
```


One discovered tool. Schemas are not included.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L102)

## Constructors

<a id="Vercel_AI_Operations_ToolSearchMatch__ctor_"></a>
<a id="Vercel_AI_Operations_ToolSearchMatch__ctor_System_String_System_String_"></a>

### ToolSearchMatch(string, string?)


```csharp
public ToolSearchMatch(string name, string? description)
```


Creates a match.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L105)

## Properties

<a id="Vercel_AI_Operations_ToolSearchMatch_Description_"></a>
<a id="Vercel_AI_Operations_ToolSearchMatch_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Resolved description, omitted when the tool has none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L115)

<a id="Vercel_AI_Operations_ToolSearchMatch_Name_"></a>
<a id="Vercel_AI_Operations_ToolSearchMatch_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L112)
