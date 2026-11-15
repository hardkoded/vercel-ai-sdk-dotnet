---
title: "GenerateObjectStepEndEvent"
description: "The raw result of the one model step made by generateObject."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectStepEndEvent"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectStepEndEvent : GenerateObjectEvent
```


The raw result of the one model step made by `generateObject`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L203)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_FinishReason_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L228)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_ObjectText_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_ObjectText"></a>

### ObjectText


```csharp
public string ObjectText { get; }
```


All text returned by the model, before provider-selected text is parsed.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L222)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_ProviderMetadata_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific response metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L243)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Reasoning_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning returned by the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L225)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Request_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Request"></a>

### Request


```csharp
public LanguageModelRequestMetadata Request { get; }
```


Request metadata.

**Value:** [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L237)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Response_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Response"></a>

### Response


```csharp
public StepResponse Response { get; }
```


Response metadata, including the raw response body.

**Value:** [StepResponse](/api/Vercel.AI/StepResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L240)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_StepNumber_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_StepNumber"></a>

### StepNumber


```csharp
public int StepNumber { get; }
```


The only step number made by generateObject.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L219)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Usage_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Token usage.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L231)

<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Warnings_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectStepEndEvent_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Provider warnings.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L234)
