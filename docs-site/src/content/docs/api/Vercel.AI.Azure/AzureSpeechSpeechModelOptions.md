---
title: "AzureSpeechSpeechModelOptions"
description: "Azure Speech text to speech options, read from the azure provider options. Maps to AzureSpeechModelSpeechOptions in azure-speech-speech-model-options.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureSpeechSpeechModelOptions"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureSpeechSpeechModelOptions
```


Azure Speech text to speech options, read from the `azure` provider options. Maps to
`AzureSpeechModelSpeechOptions` in `azure-speech-speech-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModelOptions.cs#L11)

## Properties

<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions_Style_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions_Style"></a>

### Style


```csharp
public string? Style { get; set; }
```


Speaking style applied with `mstts:express-as`, for example `excited` or `whispering`.
Supported styles vary by voice.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModelOptions.cs#L19)

<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions_StyleDegree_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions_StyleDegree"></a>

### StyleDegree


```csharp
public double? StyleDegree { get; set; }
```


Intensity of [Style](#Vercel_AI_Azure_AzureSpeechSpeechModelOptions_Style), from 0.01 to 2. Azure defaults to 1.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModelOptions.cs#L22)

## Methods

<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions_Shape_"></a>
<a id="Vercel_AI_Azure_AzureSpeechSpeechModelOptions_Shape"></a>

### Shape()


```csharp
public static IReadOnlyList<string> Shape()
```


The option keys. Maps to `azureSpeechSpeechModelOptionsShape`.

**Returns:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechSpeechModelOptions.cs#L25)
