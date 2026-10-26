---
title: "SpeechModelCall"
description: "Arguments for doGenerate on a speech model."
editUrl: false
prev: false
next: false
sidebar:
  label: "SpeechModelCall"
  hidden: true
---

<a id="Vercel_AI_Operations_SpeechModelCall"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SpeechModelCall
```


Arguments for `doGenerate` on a speech model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L10)

## Constructors

<a id="Vercel_AI_Operations_SpeechModelCall__ctor_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall__ctor_System_String_System_String_System_String_System_String_System_Nullable_System_Double__System_String_System_Text_Json_JsonElement_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Threading_CancellationToken_"></a>

### `SpeechModelCall(string, string?, string?, string?, double?, string?, JsonElement, IReadOnlyDictionary<string, string>, CancellationToken)`


```csharp
public SpeechModelCall(string text, string? voice, string? outputFormat, string? instructions, double? speed, string? language, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
```


Creates a speech call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `voice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `instructions` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `speed` | [double?](https://learn.microsoft.com/dotnet/api/system.double) |  |
| `language` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerOptions` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L13)

## Properties

<a id="Vercel_AI_Operations_SpeechModelCall_CancellationToken_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Cancellation token.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L51)

<a id="Vercel_AI_Operations_SpeechModelCall_Headers_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```


Headers including the user-agent suffix.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L48)

<a id="Vercel_AI_Operations_SpeechModelCall_Instructions_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_Instructions"></a>

### Instructions


```csharp
public string? Instructions { get; }
```


Speaking instructions.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L36)

<a id="Vercel_AI_Operations_SpeechModelCall_Language_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_Language"></a>

### Language


```csharp
public string? Language { get; }
```


Language or `auto`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L42)

<a id="Vercel_AI_Operations_SpeechModelCall_OutputFormat_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_OutputFormat"></a>

### OutputFormat


```csharp
public string? OutputFormat { get; }
```


Requested output format.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L33)

<a id="Vercel_AI_Operations_SpeechModelCall_ProviderOptions_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement ProviderOptions { get; }
```


Provider options. Empty object when omitted.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L45)

<a id="Vercel_AI_Operations_SpeechModelCall_Speed_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_Speed"></a>

### Speed


```csharp
public double? Speed { get; }
```


Playback speed.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L39)

<a id="Vercel_AI_Operations_SpeechModelCall_Text_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_Text"></a>

### Text


```csharp
public string Text { get; }
```


Text to speak.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L27)

<a id="Vercel_AI_Operations_SpeechModelCall_Voice_"></a>
<a id="Vercel_AI_Operations_SpeechModelCall_Voice"></a>

### Voice


```csharp
public string? Voice { get; }
```


Voice id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/GenerateSpeech.cs#L30)
