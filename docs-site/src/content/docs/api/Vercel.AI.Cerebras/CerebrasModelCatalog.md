---
title: "CerebrasModelCatalog"
description: "Cerebras model catalog. Upstream cerebras-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "CerebrasModelCatalog"
  hidden: true
---

<a id="Vercel_AI_Cerebras_CerebrasModelCatalog"></a>

Namespace: [Vercel.AI.Cerebras](/api/Vercel.AI.Cerebras/)


```csharp
public static class CerebrasModelCatalog
```


Cerebras model catalog. Upstream `cerebras-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L73)

## Properties

<a id="Vercel_AI_Cerebras_CerebrasModelCatalog_Entries_"></a>
<a id="Vercel_AI_Cerebras_CerebrasModelCatalog_Entries"></a>

### Entries


```csharp
public static IReadOnlyDictionary<string, CerebrasLanguageModelEntry> Entries { get; }
```


Entries of every known model id. Upstream `cerebrasModelCatalog`; renamed because C# does not allow a member named like its class.

**Value:** [IReadOnlyDictionary&lt;string, CerebrasLanguageModelEntry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L87)

<a id="Vercel_AI_Cerebras_CerebrasModelCatalog_LanguageModelIds_"></a>
<a id="Vercel_AI_Cerebras_CerebrasModelCatalog_LanguageModelIds"></a>

### LanguageModelIds


```csharp
public static IReadOnlyList<string> LanguageModelIds { get; }
```


Known language model ids. Upstream `CerebrasModelIds['language']`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L83)

## Methods

<a id="Vercel_AI_Cerebras_CerebrasModelCatalog_GetCerebrasLanguageModelEntry_"></a>
<a id="Vercel_AI_Cerebras_CerebrasModelCatalog_GetCerebrasLanguageModelEntry_System_String_"></a>

### GetCerebrasLanguageModelEntry(string)


```csharp
public static CerebrasLanguageModelEntry GetCerebrasLanguageModelEntry(string modelId)
```


Returns the entry of a language model. Any other model id resolves to an entry with unknown support. Upstream `getCerebrasLanguageModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [CerebrasLanguageModelEntry](/api/Vercel.AI.Cerebras/CerebrasLanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Cerebras/CerebrasModelCatalog.cs#L95)
