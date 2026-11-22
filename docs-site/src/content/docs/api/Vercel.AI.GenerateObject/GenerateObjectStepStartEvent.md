---
title: "GenerateObjectStepStartEvent"
description: "Event sent immediately before the generate-object model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectStepStartEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectStepStartEvent : GenerateObjectEvent
```


Event sent immediately before the generate-object model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L266)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_Headers_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; init; }
```


Additional HTTP headers sent with the request. Maps to `headers`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L278)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_PromptMessages_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_PromptMessages"></a>

### PromptMessages


```csharp
public IReadOnlyList<ModelMessage>? PromptMessages { get; init; }
```


The prompt messages in provider format. Maps to `promptMessages`.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L281)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Additional provider-specific options. Maps to `providerOptions`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L275)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_StepNumber_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepStartEvent_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


The only step number made by generateObject.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L272)
