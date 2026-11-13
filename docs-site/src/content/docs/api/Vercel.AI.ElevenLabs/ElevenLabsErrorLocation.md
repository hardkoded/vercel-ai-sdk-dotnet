---
title: "ElevenLabsErrorLocation"
description: "One string or number in a FastAPI validation issue location."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsErrorLocation"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsErrorLocation
```


One string or number in a FastAPI validation issue location.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L26)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation_Number_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation_Number"></a>

### Number


```csharp
public double? Number { get; }
```


Numeric location component, when the provider sent a number.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L38)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation_Text_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation_Text"></a>

### Text


```csharp
public string? Text { get; }
```


String location component, when the provider sent a string.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L35)

## Methods

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation_ToString_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorLocation_ToString"></a>

### ToString()


```csharp
public override string ToString()
```


Returns a string that represents the current object.

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string) — A string that represents the current object.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L48)
