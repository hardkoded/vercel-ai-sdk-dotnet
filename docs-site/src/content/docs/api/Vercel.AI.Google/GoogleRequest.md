---
title: "GoogleRequest"
description: "Builds a Gemini generateContent body."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRequest"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRequest"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleRequest
```


Builds a Gemini `generateContent` body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L43)

## Methods

<a id="Vercel_AI_Google_GoogleRequest_Prepare_"></a>
<a id="Vercel_AI_Google_GoogleRequest_Prepare_System_String_System_String_Vercel_AI_Provider_LanguageModelCallOptions_System_Boolean_"></a>

### Prepare(string, string, LanguageModelCallOptions, bool)


```csharp
public static GooglePreparedRequest Prepare(string modelId, string provider, LanguageModelCallOptions options, bool streaming)
```


Builds the request for a language-model call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |
| `streaming` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [GooglePreparedRequest](/api/Vercel.AI.Google/GooglePreparedRequest/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRequest.cs#L58)
