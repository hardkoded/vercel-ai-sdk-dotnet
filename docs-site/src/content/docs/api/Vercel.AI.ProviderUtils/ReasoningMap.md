---
title: "ReasoningMap"
description: "Maps reasoning levels onto provider effort and token budgets."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningMap"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ReasoningMap"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ReasoningMap
```


Maps reasoning levels onto provider effort and token budgets.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ReasoningMap.cs#L11)

## Methods

<a id="Vercel_AI_ProviderUtils_ReasoningMap_IsCustomReasoning_"></a>
<a id="Vercel_AI_ProviderUtils_ReasoningMap_IsCustomReasoning_System_String_"></a>

### IsCustomReasoning(string?)


```csharp
public static bool IsCustomReasoning(string? reasoning)
```


True for every reasoning value except omitted and `provider-default`. Maps to `isCustomReasoning`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ReasoningMap.cs#L56)

<a id="Vercel_AI_ProviderUtils_ReasoningMap_MapReasoningToProviderBudget_"></a>
<a id="Vercel_AI_ProviderUtils_ReasoningMap_MapReasoningToProviderBudget_System_String_System_Int32_System_Int32_System_Collections_Generic_IList_Vercel_AI_Util_ModelWarning__System_Int32_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Double__"></a>

### `MapReasoningToProviderBudget(string, int, int, IList<ModelWarning>, int, IReadOnlyDictionary<string, double>?)`


```csharp
public static int MapReasoningToProviderBudget(string reasoning, int maxOutputTokens, int maxReasoningBudget, IList<ModelWarning> warnings, int minReasoningBudget = 1024, IReadOnlyDictionary<string, double>? budgetPercentages = null)
```


Maps a reasoning value to a token budget. Levels missing from &lt;code class="paramref"&gt;budgetPercentages&lt;/code&gt; use the default
percentages. Handle `none` before calling when the model can disable reasoning. Otherwise `none` maps to
&lt;code class="paramref"&gt;minReasoningBudget&lt;/code&gt; with a compatibility warning. Maps to `mapReasoningToProviderBudget`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `maxOutputTokens` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `maxReasoningBudget` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `warnings` | [IList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |
| `minReasoningBudget` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `budgetPercentages` | [IReadOnlyDictionary&lt;string, double&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ReasoningMap.cs#L150)

<a id="Vercel_AI_ProviderUtils_ReasoningMap_MapReasoningToProviderEffort_"></a>
<a id="Vercel_AI_ProviderUtils_ReasoningMap_MapReasoningToProviderEffort_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IList_Vercel_AI_Util_ModelWarning__System_String_"></a>

### `MapReasoningToProviderEffort(string?, IReadOnlyDictionary<string, string>, IList<ModelWarning>, string?)`


```csharp
public static string? MapReasoningToProviderEffort(string? reasoning, IReadOnlyDictionary<string, string> effortMap, IList<ModelWarning> warnings, string? providerOptionsEffort = null)
```


Maps a reasoning value to a provider effort string. A &lt;code class="paramref"&gt;providerOptionsEffort&lt;/code&gt; is returned
unchanged. Otherwise an omitted or `provider-default` reasoning returns `null`. An unsupported value maps
to the next lower supported level, or the next higher one if there is no lower level. `none` is never a
fallback, so an unsupported `none` maps to the lowest supported level. Maps to `mapReasoningToProviderEffort`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `reasoning` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `effortMap` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `warnings` | [IList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |
| `providerOptionsEffort` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ReasoningMap.cs#L67)
