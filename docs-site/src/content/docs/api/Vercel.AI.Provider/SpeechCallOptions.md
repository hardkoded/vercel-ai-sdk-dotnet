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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L334)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L337)

## Properties

<a id="Vercel_AI_Provider_SpeechCallOptions_Headers_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Headers for this call. They override the provider headers. Maps to `headers`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L364)

<a id="Vercel_AI_Provider_SpeechCallOptions_Instructions_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Instructions for the speech generation, such as "Speak in a slow and steady tone". Maps to `instructions`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L352)

<a id="Vercel_AI_Provider_SpeechCallOptions_Language_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


ISO 639-1 language code such as `en`, or `auto` for detection. Provider support varies. Maps to `language`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L358)

<a id="Vercel_AI_Provider_SpeechCallOptions_OutputFormat_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Desired output format such as `mp3` or `wav`. Maps to `outputFormat`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L349)

<a id="Vercel_AI_Provider_SpeechCallOptions_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Additional provider-specific options, keyed by provider name. Maps to `providerOptions`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L361)

<a id="Vercel_AI_Provider_SpeechCallOptions_Speed_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speed of the speech generation. Maps to `speed`.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L355)

<a id="Vercel_AI_Provider_SpeechCallOptions_Text_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L343)

<a id="Vercel_AI_Provider_SpeechCallOptions_Voice_"></a>
<a id="Vercel_AI_Provider_SpeechCallOptions_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice id, when the provider uses one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L346)
