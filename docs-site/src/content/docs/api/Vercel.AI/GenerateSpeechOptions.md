---
title: "GenerateSpeechOptions"
description: "Speech options."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenerateSpeechOptions"
  hidden: true
---

<a id="Vercel_AI_GenerateSpeechOptions"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class GenerateSpeechOptions
```


Speech options.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L387)

## Properties

<a id="Vercel_AI_GenerateSpeechOptions_Model_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Model"></a>

### Model


```csharp
public ISpeechModel? Model { get; set; }
```


Speech model.

**Value:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L390)

<a id="Vercel_AI_GenerateSpeechOptions_Text_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Text"></a>

### Text


```csharp
public string Text { get; set; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L393)

<a id="Vercel_AI_GenerateSpeechOptions_Voice_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L396)
