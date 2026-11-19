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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L741)

## Properties

<a id="Vercel_AI_GenerateSpeechOptions_Headers_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L768)

<a id="Vercel_AI_GenerateSpeechOptions_Instructions_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Instructions for the speech generation.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L756)

<a id="Vercel_AI_GenerateSpeechOptions_Language_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


ISO 639-1 language code, or `auto`. Provider support varies.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L762)

<a id="Vercel_AI_GenerateSpeechOptions_Model_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Model"></a>

### Model


```csharp
public ISpeechModel? Model { get; set; }
```


Speech model.

**Value:** [ISpeechModel](/api/Vercel.AI.Provider/ISpeechModel/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L744)

<a id="Vercel_AI_GenerateSpeechOptions_OutputFormat_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Desired output format such as `mp3` or `wav`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L753)

<a id="Vercel_AI_GenerateSpeechOptions_ProviderOptions_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Additional provider-specific options, keyed by provider name.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L765)

<a id="Vercel_AI_GenerateSpeechOptions_Speed_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speed of the speech generation.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L759)

<a id="Vercel_AI_GenerateSpeechOptions_Text_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Text"></a>

### Text


```csharp
public string Text { get; set; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L747)

<a id="Vercel_AI_GenerateSpeechOptions_Voice_"></a>
<a id="Vercel_AI_GenerateSpeechOptions_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AiClient.cs#L750)
