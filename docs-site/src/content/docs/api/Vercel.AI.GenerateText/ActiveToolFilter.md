---
title: "ActiveToolFilter"
description: "Active-tool filtering. Maps to filterActiveTools."
editUrl: false
prev: false
next: false
sidebar:
  label: "ActiveToolFilter"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ActiveToolFilter"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ActiveToolFilter
```


Active-tool filtering. Maps to `filterActiveTools`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/FilterActiveTools.cs#L8)

## Methods

<a id="Vercel_AI_GenerateText_ActiveToolFilter_FilterActiveTools_"></a>
<a id="Vercel_AI_GenerateText_ActiveToolFilter_FilterActiveTools__1_System_Collections_Generic_IReadOnlyDictionary_System_String___0__System_Collections_Generic_IReadOnlyList_System_String__"></a>

### `FilterActiveTools<T>(IReadOnlyDictionary<string, T>?, IReadOnlyList<string>?)`


```csharp
public static IReadOnlyDictionary<string, T>? FilterActiveTools<T>(IReadOnlyDictionary<string, T>? tools, IReadOnlyList<string>? activeTools)
```


Keeps tools whose names are in &lt;code class="paramref"&gt;activeTools&lt;/code&gt;.
A null tool set stays null. A null active-tool list returns &lt;code class="paramref"&gt;tools&lt;/code&gt; unchanged.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyDictionary&lt;string, T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `activeTools` | [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [IReadOnlyDictionary&lt;string, T&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/FilterActiveTools.cs#L14)
