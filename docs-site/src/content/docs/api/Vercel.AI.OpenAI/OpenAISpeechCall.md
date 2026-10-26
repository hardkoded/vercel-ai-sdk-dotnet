---
title: "OpenAISpeechCall"
description: "Speech synthesis settings."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISpeechCall"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISpeechCall"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISpeechCall
```


Speech synthesis settings.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L13)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAISpeechCall__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall__ctor_System_String_"></a>

### OpenAISpeechCall(string)


```csharp
public OpenAISpeechCall(string text)
```


Creates a speech call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L16)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Headers_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string?>? Headers { get; set; }
```


Extra HTTP headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L43)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Instructions_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; set; }
```


Speaking instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L31)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Language_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Language. OpenAI speech models ignore it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L37)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_OutputFormat_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; set; }
```


Audio format. Unsupported values fall back to `mp3`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L34)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_ProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options. `openai.speed` and `openai.instructions` override the call.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L40)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Speed_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Speed"></a>

### Speed


```csharp
public double? Speed { get; set; }
```


Playback speed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L28)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Text_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L22)

<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Voice_"></a>
<a id="Vercel_AI_OpenAI_OpenAISpeechCall_Voice"></a>

### Voice


```csharp
public string? Voice { get; set; }
```


Voice. Defaults to `alloy`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Speech/OpenAISpeechModel.cs#L25)
