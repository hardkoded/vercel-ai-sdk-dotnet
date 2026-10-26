---
title: "ToolSearchState"
description: "Per-generation discovery state. Maps to createToolSearchState."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolSearchState"
  hidden: true
---

<a id="Vercel_AI_Operations_ToolSearchState"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class ToolSearchState
```


Per-generation discovery state. Maps to `createToolSearchState`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L142)

## Methods

<a id="Vercel_AI_Operations_ToolSearchState_Create_"></a>
<a id="Vercel_AI_Operations_ToolSearchState_Create_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_SearchableTool__System_Collections_Generic_IReadOnlyDictionary_System_String_System_Collections_Generic_IReadOnlyList_System_String___"></a>

### `Create(IReadOnlyDictionary<string, SearchableTool>?, IReadOnlyDictionary<string, IReadOnlyList<string>>?)`


```csharp
public static Func<IReadOnlyDictionary<string, SearchableTool>?, IReadOnlyDictionary<string, object?>?, IReadOnlyDictionary<string, SearchableTool>?> Create(IReadOnlyDictionary<string, SearchableTool>? tools, IReadOnlyDictionary<string, IReadOnlyList<string>>? toolCallers)
```


Creates a prepare function. An empty routing map does not inherit keys such as `constructor`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `tools` | [IReadOnlyDictionary&lt;string, SearchableTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `toolCallers` | [IReadOnlyDictionary&lt;string, IReadOnlyList&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [Func&lt;IReadOnlyDictionary&lt;string, SearchableTool&gt;, IReadOnlyDictionary&lt;string, object&gt;, IReadOnlyDictionary&lt;string, SearchableTool&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L145)

<a id="Vercel_AI_Operations_ToolSearchState_Tokenize_"></a>
<a id="Vercel_AI_Operations_ToolSearchState_Tokenize_System_String_"></a>

### Tokenize(string)


```csharp
public static List<string> Tokenize(string text)
```


Splits camel case, then keeps letters and numbers.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [List&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ToolSearch.cs#L271)
