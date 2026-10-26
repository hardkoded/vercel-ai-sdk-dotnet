---
title: "GoogleSpeechCall"
description: "Speech request settings beyond the shared text and voice fields."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechCall"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechCall"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSpeechCall
```


Speech request settings beyond the shared text and voice fields.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L13)

## Constructors

<a id="Vercel_AI_Google_GoogleSpeechCall__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall__ctor_System_String_"></a>

### GoogleSpeechCall(string)


```csharp
public GoogleSpeechCall(string text)
```


Creates a speech call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L16)

## Properties

<a id="Vercel_AI_Google_GoogleSpeechCall_Instructions_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Style directions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L28)

<a id="Vercel_AI_Google_GoogleSpeechCall_Language_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Language. Gemini TTS ignores it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L34)

<a id="Vercel_AI_Google_GoogleSpeechCall_OutputFormat_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Output format such as `wav` or `pcm`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L37)

<a id="Vercel_AI_Google_GoogleSpeechCall_ProviderOptions_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; set; }
```


Provider options under `google`, `googleVertex`, or `vertex`.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L40)

<a id="Vercel_AI_Google_GoogleSpeechCall_Speed_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Speaking rate. Gemini TTS ignores it.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L31)

<a id="Vercel_AI_Google_GoogleSpeechCall_Text_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_Text"></a>

### Text


```csharp
public string Text { get; }
```


Transcript.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L22)

<a id="Vercel_AI_Google_GoogleSpeechCall_Voice_"></a>
<a id="Vercel_AI_Google_GoogleSpeechCall_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Prebuilt voice. Defaults to Kore.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L25)
