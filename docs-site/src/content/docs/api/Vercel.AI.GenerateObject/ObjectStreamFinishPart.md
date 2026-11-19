---
title: "ObjectStreamFinishPart"
description: "The end of the stream. Maps to { type: 'finish' }."
editUrl: false
prev: false
next: false
sidebar:
  label: "ObjectStreamFinishPart"
  hidden: true
---

<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart"></a>

Namespace: [Vercel.AI.GenerateObject](/api/Vercel.AI.GenerateObject/)


```csharp
public sealed class ObjectStreamFinishPart : ObjectStreamPart
```


The end of the stream. Maps to `{ type: 'finish' }`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ObjectStreamPart](/api/Vercel.AI.GenerateObject/ObjectStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L213)

## Properties

<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_FinishReason_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_FinishReason"></a>

### FinishReason


```csharp
public FinishReason FinishReason { get; }
```


Why the model finished.

**Value:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L230)

<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_ProviderMetadata_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider-specific metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L239)

<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_Response_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_Response"></a>

### Response


```csharp
public LanguageModelResponseMetadata Response { get; }
```


Response metadata.

**Value:** [LanguageModelResponseMetadata](/api/Vercel.AI/LanguageModelResponseMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L236)

<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_Usage_"></a>
<a id="Vercel_AI_GenerateObject_ObjectStreamFinishPart_Usage"></a>

### Usage


```csharp
public LanguageModelUsage Usage { get; }
```


Token usage.

**Value:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateObject/StreamObject.cs#L233)
