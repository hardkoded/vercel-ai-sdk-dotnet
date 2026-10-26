---
title: "ToolSearch"
description: "Creates the search tool. Maps to toolSearch."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearch"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearch"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class ToolSearch
```


Creates the search tool. Maps to `toolSearch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L119)

## Fields

<a id="Vercel_AI_Operations_ToolSearch_Description"></a>

### Description


```csharp
public const string Description = "Search for tools by keywords in their names and descriptions. Returns up to five matching tools. Matches become available on the next model step, after this execution finishes. Wait for their tool definitions before calling the discovered tools. If no tools match, try different keywords."
```


Description copied onto the search tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L122)

## Methods

<a id="Vercel_AI_Operations_ToolSearch_Create_"></a>
<a id="Vercel_AI_Operations_ToolSearch_Create"></a>

### Create()


```csharp
public static SearchableTool Create()
```


Creates an unbound search tool. Its execute method throws until a generation binds it.

**Returns:** [SearchableTool](/api/Vercel.AI.Operations/SearchableTool/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L125)

<a id="Vercel_AI_Operations_ToolSearch_IsSearch_"></a>
<a id="Vercel_AI_Operations_ToolSearch_IsSearch_Vercel_AI_Operations_SearchableTool_"></a>

### IsSearch(SearchableTool?)


```csharp
public static bool IsSearch(SearchableTool? tool)
```


True when the tool carries the search marker.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tool` | [SearchableTool](/api/Vercel.AI.Operations/SearchableTool/) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L135)
