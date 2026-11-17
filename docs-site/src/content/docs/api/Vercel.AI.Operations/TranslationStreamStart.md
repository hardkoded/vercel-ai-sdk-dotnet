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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L70)

## Constructors

<a id="Vercel_AI_Operations_TranslationStreamStart__ctor_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart__ctor_Vercel_AI_Operations_ModelPartStream_Vercel_AI_Operations_ProviderResponse_Vercel_AI_Operations_ProviderRequest_"></a>

### TranslationStreamStart(ModelPartStream, ProviderResponse?, ProviderRequest?)


```csharp
public TranslationStreamStart(ModelPartStream stream, ProviderResponse? response = null, ProviderRequest? request = null)
```


Creates a start payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `stream` | [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/) |  |
| `response` | [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/) |  |
| `request` | [ProviderRequest](/api/Vercel.AI.Operations/ProviderRequest/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L73)

## Properties

<a id="Vercel_AI_Operations_TranslationStreamStart_Request_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart_Request"></a>

### Request


```csharp
public ProviderRequest? Request { get; }
```


Request information: the setup payload sent to the provider.

**Value:** [ProviderRequest](/api/Vercel.AI.Operations/ProviderRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L85)

<a id="Vercel_AI_Operations_TranslationStreamStart_Response_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart_Response"></a>

### Response


```csharp
public ProviderResponse? Response { get; }
```


Initial response metadata.

**Value:** [ProviderResponse](/api/Vercel.AI.Operations/ProviderResponse/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L91)

<a id="Vercel_AI_Operations_TranslationStreamStart_Stream_"></a>
<a id="Vercel_AI_Operations_TranslationStreamStart_Stream"></a>

### Stream


```csharp
public ModelPartStream Stream { get; }
```


Model parts.

**Value:** [ModelPartStream](/api/Vercel.AI.Operations/ModelPartStream/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L88)
