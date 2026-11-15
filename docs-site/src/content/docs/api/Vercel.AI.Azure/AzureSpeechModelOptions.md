---
title: "AzureSpeechModelOptions"
description: "Azure speech model options, read from the azure provider options. They combine the OpenAI speech options and the Azure Speech options. Maps to AzureSpeechMod..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AzureSpeechModelOptions"
  hidden: true
---

<a id="Vercel_AI_Azure_AzureSpeechModelOptions"></a>

Namespace: [Vercel.AI.Azure](/api/Vercel.AI.Azure/)


```csharp
public sealed class AzureSpeechModelOptions
```


Azure speech model options, read from the `azure` provider options. They combine the OpenAI speech options and
the Azure Speech options. Maps to `AzureSpeechModelOptions` in `azure-speech-model-options.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L15)

## Properties

<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Api_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Api"></a>

### Api


```csharp
public string? Api { get; set; }
```


API to use, `openai` or `speech`. Defaults to Speech for MAI-Voice models, OpenAI otherwise.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L20)

<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Instructions_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


OpenAI API option for Azure OpenAI deployments. Maps to [Instructions](/api/Vercel.AI.OpenAI/OpenAISpeechModelOptions/#Vercel_AI_OpenAI_OpenAISpeechModelOptions_Instructions).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L23)

<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Speed_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


OpenAI API option for Azure OpenAI deployments, from 0.25 to 4.0. Maps to [Speed](/api/Vercel.AI.OpenAI/OpenAISpeechModelOptions/#Vercel_AI_OpenAI_OpenAISpeechModelOptions_Speed).

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L26)

<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Style_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModelOptions_Style"></a>

### Style


```csharp
public string? Style { get; set; }
```


Speaking style for the Azure Speech API. Maps to [Style](/api/Vercel.AI.Azure/AzureSpeechSpeechModelOptions/#Vercel_AI_Azure_AzureSpeechSpeechModelOptions_Style).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L29)

<a id="Vercel_AI_Azure_AzureSpeechModelOptions_StyleDegree_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModelOptions_StyleDegree"></a>

### StyleDegree


```csharp
public double? StyleDegree { get; set; }
```


Intensity of [Style](#Vercel_AI_Azure_AzureSpeechModelOptions_Style) for the Azure Speech API, from 0.01 to 2. Maps to [StyleDegree](/api/Vercel.AI.Azure/AzureSpeechSpeechModelOptions/#Vercel_AI_Azure_AzureSpeechSpeechModelOptions_StyleDegree).

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L32)

## Methods

<a id="Vercel_AI_Azure_AzureSpeechModelOptions_GetMAIVoiceModel_"></a>
<a id="Vercel_AI_Azure_AzureSpeechModelOptions_GetMAIVoiceModel_System_String_"></a>

### GetMAIVoiceModel(string)


```csharp
public static string? GetMAIVoiceModel(string modelId)
```


Returns the voice model name of an Azure Speech model, for example `MAI-Voice-2` for `mai-voice-2`,
or null when the model is not a MAI-Voice model. Maps to `getMAIVoiceModel`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Azure/AzureSpeechModelOptions.cs#L38)
