---
title: "ToolSearchMatchList"
description: "Tools returned by one search."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchMatchList"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchMatchList"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ToolSearchMatchList
```


Tools returned by one search.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L89)

## Constructors

<a id="Vercel_AI_Operations_ToolSearchMatchList__ctor_"></a>
<a id="Vercel_AI_Operations_ToolSearchMatchList__ctor_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_ToolSearchMatch__"></a>

### `ToolSearchMatchList(IReadOnlyList<ToolSearchMatch>)`


```csharp
public ToolSearchMatchList(IReadOnlyList<ToolSearchMatch> tools)
```


Creates a match list.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyList&lt;ToolSearchMatch&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L92)

## Properties

<a id="Vercel_AI_Operations_ToolSearchMatchList_Tools_"></a>
<a id="Vercel_AI_Operations_ToolSearchMatchList_Tools"></a>

### Tools


```csharp
public IReadOnlyList<ToolSearchMatch> Tools { get; }
```


Matches, at most five.

**Value:** [IReadOnlyList&lt;ToolSearchMatch&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L98)
