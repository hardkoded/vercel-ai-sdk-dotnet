---
title: "ElevenLabsErrorDetail"
description: "ElevenLabs API error details."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsErrorDetail"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsErrorDetail
```


ElevenLabs API error details.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L83)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_AdditionalProperties_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; }
```


Loose fields ElevenLabs included in the detail object.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L122)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Code_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Code"></a>

### Code


```csharp
public string? Code { get; }
```


Provider error code.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L107)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Message_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L104)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Parameter_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Parameter"></a>

### Parameter


```csharp
public string? Parameter { get; }
```


Invalid parameter name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L119)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_RequestId_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_RequestId"></a>

### RequestId


```csharp
public string? RequestId { get; }
```


ElevenLabs request id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L116)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Status_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Status"></a>

### Status


```csharp
public string? Status { get; }
```


Provider error status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L110)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Type_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorDetail_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Provider error type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L113)
