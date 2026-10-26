---
title: "GoogleSpeechInspection"
description: "Transcript text and whether the voice is a custom voice."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechInspection"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechInspection"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSpeechInspection
```


Transcript text and whether the voice is a custom voice.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L95)

## Constructors

<a id="Vercel_AI_Google_GoogleSpeechInspection__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechInspection__ctor_System_String_System_Boolean_"></a>

### GoogleSpeechInspection(string, bool)


```csharp
public GoogleSpeechInspection(string text, bool usesCustomVoice)
```


Creates an inspection.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usesCustomVoice` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L98)

## Properties

<a id="Vercel_AI_Google_GoogleSpeechInspection_Text_"></a>
<a id="Vercel_AI_Google_GoogleSpeechInspection_Text"></a>

### Text


```csharp
public string Text { get; }
```


Transcript that will be spoken.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L105)

<a id="Vercel_AI_Google_GoogleSpeechInspection_UsesCustomVoice_"></a>
<a id="Vercel_AI_Google_GoogleSpeechInspection_UsesCustomVoice"></a>

### UsesCustomVoice


```csharp
public bool UsesCustomVoice { get; }
```


True when the voice is a custom voice Gemini does not accept.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L108)
