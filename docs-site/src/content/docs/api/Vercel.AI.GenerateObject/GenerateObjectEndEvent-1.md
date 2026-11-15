---
title: "GenerateObjectEndEvent<T>"
description: "Event sent after the generated object has been parsed and validated."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateObjectEndEvent<T>"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class GenerateObjectEndEvent<T> : GenerateObjectEvent
```


Event sent after the generated object has been parsed and validated.

**Type parameters**

| Name | Description |
| --- | --- |
| `T` |  |

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GenerateObjectEvent](/api/Vercel.AI.GenerateObject/GenerateObjectEvent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L247)

## Properties

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_FinishReason_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Finish reason.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L269)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Object_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Object"></a>

### Object


```csharp
public T Object { get; }
```


The parsed object.

**Value:** `T`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L263)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_ProviderMetadata_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific response metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L284)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Reasoning_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Reasoning"></a>

### Reasoning


```csharp
public string? Reasoning { get; }
```


Reasoning returned by the model.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L266)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Request_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Request"></a>

### Request


```csharp
public LanguageModelRequestMetadata Request { get; }
```


Request metadata.

**Value:** [LanguageModelRequestMetadata](/api/Vercel.AI/LanguageModelRequestMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L278)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Response_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Response"></a>

### Response


```csharp
public StepResponse Response { get; }
```


Response metadata.

**Value:** [StepResponse](/api/Vercel.AI/StepResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L281)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Usage_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Token usage.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L272)

<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Warnings_"></a>
<a id="Vercel_AI_GenerateObject_GenerateObjectEndEvent_1_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<CallWarning> Warnings { get; }
```


Provider warnings.

**Value:** [IReadOnlyList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/GenerateObject.cs#L275)
