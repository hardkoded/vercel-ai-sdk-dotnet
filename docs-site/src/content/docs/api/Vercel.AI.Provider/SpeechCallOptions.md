---
title: "SpeechCallOptions"
description: "Speech call settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechCallOptions"
  hidden: true
---

<a id="Vercel_AI_Provider_SpeechCallOptions"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class SpeechCallOptions
```


Speech call settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L290)

## Constructors

<a id="Vercel_AI_Provider_SpeechCallOptions__ctor_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions__ctor_System_String_"></a>

### SpeechCallOptions(string)


```csharp
public SpeechCallOptions(string text)
```


Creates speech options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L293)

## Properties

<a id="Vercel_AI_Provider_SpeechCallOptions_Text_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L299)

<a id="Vercel_AI_Provider_SpeechCallOptions_Voice_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id, when the provider uses one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L302)
