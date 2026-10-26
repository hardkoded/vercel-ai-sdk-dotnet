---
title: "GoogleSpeechTranslation"
description: "Speech translation configuration for the Gemini Live API. The socket itself is not opened here."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechTranslation"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechTranslation"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleSpeechTranslation
```


Speech translation configuration for the Gemini Live API. The socket itself is not opened here.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L226)

## Fields

<a id="Vercel_AI_Google_GoogleSpeechTranslation_LivePath"></a>

### LivePath


```csharp
public const string LivePath = "google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent"
```


Live translation WebSocket path.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L229)

## Methods

<a id="Vercel_AI_Google_GoogleSpeechTranslation_WarningsFor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechTranslation_WarningsFor_System_String_System_String_"></a>

### WarningsFor(string?, string?)


```csharp
public static IReadOnlyList<GoogleWarning> WarningsFor(string? sourceLanguage, string? outputAudioFormat)
```


Warnings for options the Live translation API does not accept.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sourceLanguage` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `outputAudioFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L266)

<a id="Vercel_AI_Google_GoogleSpeechTranslation_WebSocketUrl_"></a>
<a id="Vercel_AI_Google_GoogleSpeechTranslation_WebSocketUrl_System_String_System_String_"></a>

### WebSocketUrl(string, string?)


```csharp
public static Uri WebSocketUrl(string baseUrl, string? apiKey)
```


Builds a `wss` URL and strips a trailing `/v1beta` or `/v1alpha` segment.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `apiKey` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [Uri](https://learn.microsoft.com/dotnet/api/system.uri)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTranscriptionModel.cs#L232)
