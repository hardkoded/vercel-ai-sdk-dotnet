---
title: "GoogleSpeechPreparation"
description: "A prepared Gemini speech request."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleSpeechPreparation"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleSpeechPreparation"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleSpeechPreparation
```


A prepared Gemini speech request.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L475)

## Constructors

<a id="Vercel_AI_Google_GoogleSpeechPreparation__ctor_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation__ctor_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IReadOnlyList_Vercel_AI_Google_GoogleWarning__System_String_System_Boolean_"></a>

### `GoogleSpeechPreparation(JsonObject, IReadOnlyList<GoogleWarning>, string, bool)`


```csharp
public GoogleSpeechPreparation(JsonObject body, IReadOnlyList<GoogleWarning> warnings, string outputFormat = "AUDIO_WAV", bool usesStructuredSpeech = false)
```


Creates a preparation.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `outputFormat` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `usesStructuredSpeech` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L478)

## Properties

<a id="Vercel_AI_Google_GoogleSpeechPreparation_Body_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation_Body"></a>

### Body


```csharp
public JsonObject Body { get; }
```


Request JSON.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L487)

<a id="Vercel_AI_Google_GoogleSpeechPreparation_OutputFormat_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation_OutputFormat"></a>

### OutputFormat


```csharp
public string OutputFormat { get; }
```


Resolved Gemini audio format, such as `AUDIO_WAV`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L493)

<a id="Vercel_AI_Google_GoogleSpeechPreparation_UsesStructuredSpeech_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation_UsesStructuredSpeech"></a>

### UsesStructuredSpeech


```csharp
public bool UsesStructuredSpeech { get; }
```


True for Gemini 3.8 style models, which take structured speech metadata.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L496)

<a id="Vercel_AI_Google_GoogleSpeechPreparation_Warnings_"></a>
<a id="Vercel_AI_Google_GoogleSpeechPreparation_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<GoogleWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;GoogleWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleSpeechModel.cs#L490)
