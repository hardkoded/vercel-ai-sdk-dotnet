---
title: "DeepSeekModelCatalog"
description: "DeepSeek model catalog. Upstream deepseek-model-catalog.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepSeekModelCatalog"
  hidden: true
---

<a id="Vercel_AI_DeepSeek_DeepSeekModelCatalog"></a>

Namespace: [Vercel.AI.DeepSeek](/api/Vercel.AI.DeepSeek/)


```csharp
public static class DeepSeekModelCatalog
```


DeepSeek model catalog. Upstream `deepseek-model-catalog.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L174)

## Properties

<a id="Vercel_AI_DeepSeek_DeepSeekModelCatalog_Entries_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekModelCatalog_Entries"></a>

### Entries


```csharp
public static IReadOnlyDictionary<string, DeepSeekLanguageModelEntry> Entries { get; }
```


Entries of every known model id. Upstream `deepseekModelCatalog`; renamed because C# does not allow a member
named like its class.

**Value:** [IReadOnlyDictionary&lt;string, DeepSeekLanguageModelEntry&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L201)

## Methods

<a id="Vercel_AI_DeepSeek_DeepSeekModelCatalog_GetDeepSeekLanguageModelEntry_"></a>
<a id="Vercel_AI_DeepSeek_DeepSeekModelCatalog_GetDeepSeekLanguageModelEntry_System_String_Vercel_AI_DeepSeek_DeepSeekEndpointOptions_"></a>

### GetDeepSeekLanguageModelEntry(string, DeepSeekEndpointOptions?)


```csharp
public static DeepSeekLanguageModelEntry GetDeepSeekLanguageModelEntry(string modelId, DeepSeekEndpointOptions? endpoint = null)
```


Returns the entry of a model on an endpoint. Upstream `getDeepSeekLanguageModelEntry`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `endpoint` | [DeepSeekEndpointOptions](/api/Vercel.AI.DeepSeek/DeepSeekEndpointOptions/) |  |

**Returns:** [DeepSeekLanguageModelEntry](/api/Vercel.AI.DeepSeek/DeepSeekLanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/DeepSeek/DeepSeekModelCatalog.cs#L209)
