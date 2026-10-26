---
title: "StreamTranslate"
description: "Streams a speech translation. Maps to streamTranslate."
editUrl: false
prev: false
next: false
sidebar:
  label: "StreamTranslate"
  hidden: true
---

<a id="Vercel_AI_Operations_StreamTranslate"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class StreamTranslate
```


Streams a speech translation. Maps to `streamTranslate`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L421)

## Fields

<a id="Vercel_AI_Operations_StreamTranslate_MissingProviderMessage"></a>

### MissingProviderMessage


```csharp
public const string MissingProviderMessage = "The default provider does not support speech translation models. Please pass a provider model instance that implements the experimental speech translation model specification."
```


Message thrown when the default provider has no speech translation models.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L424)

## Methods

<a id="Vercel_AI_Operations_StreamTranslate_Start_"></a>
<a id="Vercel_AI_Operations_StreamTranslate_Start_Vercel_AI_Operations_StreamTranslateRequest_System_Threading_CancellationToken_"></a>

### Start(StreamTranslateRequest, CancellationToken)


```csharp
public static StreamTranslateResult Start(StreamTranslateRequest request, CancellationToken cancellationToken = default)
```


Starts a translation stream. `doStream` is invoked immediately.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `request` | [StreamTranslateRequest](/api/Vercel.AI.Operations/StreamTranslateRequest/) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

**Returns:** [StreamTranslateResult](/api/Vercel.AI.Operations/StreamTranslateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Translate.cs#L427)
