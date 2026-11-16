---
title: "TranscriptionStreamClientFrame"
description: "Server-side classification of a client text frame. Maps to TranscriptionStreamClientFrame."
editUrl: false
prev: false
next: false
sidebar:
  label: "TranscriptionStreamClientFrame"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class TranscriptionStreamClientFrame
```


Server-side classification of a client text frame. Maps to `TranscriptionStreamClientFrame`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L67)

## Properties

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Frame_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Frame"></a>

### Frame


```csharp
public TranscriptionStreamStartFrame? Frame { get; }
```


The start frame, when [Type](#Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Type) is `start`.

**Value:** [TranscriptionStreamStartFrame](/api/Vercel.AI.ProviderUtils/TranscriptionStreamStartFrame/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L80)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Message_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Message"></a>

### Message


```csharp
public string? Message { get; }
```


Why the frame is invalid, when [Type](#Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Type) is `invalid`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L83)

<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Type_"></a>
<a id="Vercel_AI_ProviderUtils_TranscriptionStreamClientFrame_Type"></a>

### Type


```csharp
public string Type { get; }
```


`start`, `audio-done`, `invalid` (malformed JSON or a recognized frame with an invalid shape), or `unknown` (ignore for forward compatibility).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/TranscriptionStreamEnvelope.cs#L77)
