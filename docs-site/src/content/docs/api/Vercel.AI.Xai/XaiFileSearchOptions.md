---
title: "XaiFileSearchOptions"
description: "Arguments of String). Maps to fileSearchArgsSchema."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFileSearchOptions"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFileSearchOptions"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFileSearchOptions
```


Arguments of `String)`. Maps to `fileSearchArgsSchema`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L69)

## Constructors

<a id="Vercel_AI_Xai_XaiFileSearchOptions__ctor_"></a>
<a id="Vercel_AI_Xai_XaiFileSearchOptions__ctor_System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `XaiFileSearchOptions(IReadOnlyList<string>)`


```csharp
public XaiFileSearchOptions(IReadOnlyList<string> vectorStoreIds)
```


Creates the arguments.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `vectorStoreIds` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L72)

## Properties

<a id="Vercel_AI_Xai_XaiFileSearchOptions_MaxNumResults_"></a>
<a id="Vercel_AI_Xai_XaiFileSearchOptions_MaxNumResults"></a>

### MaxNumResults


```csharp
public double? MaxNumResults { get; init; }
```


Maximum number of results to return.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L81)

<a id="Vercel_AI_Xai_XaiFileSearchOptions_VectorStoreIds_"></a>
<a id="Vercel_AI_Xai_XaiFileSearchOptions_VectorStoreIds"></a>

### VectorStoreIds


```csharp
public IReadOnlyList<string> VectorStoreIds { get; }
```


Vector store (collection) ids to search.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiTools.cs#L78)
