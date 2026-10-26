---
title: "GoogleSpeechInput"
description: "Inspects a speech transcript before a model call."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechInput"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechInput"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleSpeechInput
```


Inspects a speech transcript before a model call.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L44)

## Methods

<a id="Vercel_AI_Google_GoogleSpeechInput_Inspect_"></a>
<a id="Vercel_AI_Google_GoogleSpeechInput_Inspect_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### Inspect(string, string?, JsonElement?)


```csharp
public static GoogleSpeechInspection Inspect(string text, string? voice, JsonElement? google)
```


Joins structured turns and detects custom voices, which Gemini rejects.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `voice` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `google` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [GoogleSpeechInspection](/api/Vercel.AI.Google/GoogleSpeechInspection/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L47)
