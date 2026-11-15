---
title: "ElevenLabsValidationIssue"
description: "One FastAPI validation issue from an ElevenLabs detail array."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsValidationIssue"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsValidationIssue
```


One FastAPI validation issue from an ElevenLabs `detail` array.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L55)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_AdditionalProperties_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_AdditionalProperties"></a>

### AdditionalProperties


```csharp
public IReadOnlyDictionary<string, JsonElement> AdditionalProperties { get; }
```


Loose fields ElevenLabs included in the issue.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L79)

<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_Location_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_Location"></a>

### Location


```csharp
public IReadOnlyList<ElevenLabsErrorLocation>? Location { get; }
```


Path components from `loc`.

**Value:** [IReadOnlyList&lt;ElevenLabsErrorLocation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L73)

<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_Message_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_Message"></a>

### Message


```csharp
public string Message { get; }
```


Validation message from `msg`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L70)

<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_Type_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsValidationIssue_Type"></a>

### Type


```csharp
public string? Type { get; }
```


Validation issue type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L76)
