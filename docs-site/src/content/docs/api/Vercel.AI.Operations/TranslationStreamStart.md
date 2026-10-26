---
title: "TranslationStreamStart"
description: "A started translation stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranslationStreamStart"
  hidden: true
---

<a id="Vercel_AI_Operations_TranslationStreamStart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class TranslationStreamStart
```


A started translation stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L55)

## Constructors

<a id="Vercel_AI_Operations_TranslationStreamStart__ctor_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart__ctor_Vercel_AI_Operations_ModelPartStream_Vercel_AI_Operations_ProviderResponse_"></a>

### TranslationStreamStart(ModelPartStream, ProviderResponse?)


```csharp
public TranslationStreamStart(ModelPartStream stream, ProviderResponse? response = null)
```


Creates a start payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L58)

## Properties

<a id="Vercel_AI_Operations_TranslationStreamStart_Response_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Initial response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L68)

<a id="Vercel_AI_Operations_TranslationStreamStart_Stream_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart_Stream"></a>

### Stream


```csharp
public ModelPartStream Stream { get; }
```


Model parts.

**Value:** [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L65)
