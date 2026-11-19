---
title: "LanguageModelCallStartEvent"
description: "Passed to OnLanguageModelCallStart before the model is called."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelCallStartEvent"
  hidden: true
---

<a id="Vercel_AI_LanguageModelCallStartEvent"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class LanguageModelCallStartEvent
```


Passed to [OnLanguageModelCallStart](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_OnLanguageModelCallStart) before the model is called.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1486)

## Properties

<a id="Vercel_AI_LanguageModelCallStartEvent_CallId_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_CallId"></a>

### CallId


```csharp
public string CallId { get; init; }
```


Id of the model call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1489)

<a id="Vercel_AI_LanguageModelCallStartEvent_CallOptions_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_CallOptions"></a>

### CallOptions


```csharp
public LanguageModelCallOptions CallOptions { get; init; }
```


Call settings.

**Value:** [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1507)

<a id="Vercel_AI_LanguageModelCallStartEvent_Instructions_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Instructions"></a>

### Instructions


```csharp
public Instructions? Instructions { get; init; }
```


Instructions of the call.

**Value:** [Instructions](/api/Vercel.AI.Prompt/Instructions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1498)

<a id="Vercel_AI_LanguageModelCallStartEvent_Messages_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; init; }
```


Messages of the call, without the instructions.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1501)

<a id="Vercel_AI_LanguageModelCallStartEvent_ModelId_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; init; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1495)

<a id="Vercel_AI_LanguageModelCallStartEvent_Provider_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Provider"></a>

### Provider


```csharp
public string Provider { get; init; }
```


Provider id of the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1492)

<a id="Vercel_AI_LanguageModelCallStartEvent_Tools_"></a>
<a id="Vercel_AI_LanguageModelCallStartEvent_Tools"></a>

### Tools


```csharp
public IReadOnlyList<LanguageModelTool>? Tools { get; init; }
```


Tools offered to the model.

**Value:** [IReadOnlyList&lt;LanguageModelTool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1504)
