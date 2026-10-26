---
title: "GoogleResponse"
description: "Parses a Gemini generateContent response."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleResponse"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleResponse"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleResponse
```


Parses a Gemini generateContent response.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L13)

## Methods

<a id="Vercel_AI_Google_GoogleResponse_Parse_"></a>
<a id="Vercel_AI_Google_GoogleResponse_Parse_System_Text_Json_JsonElement_Vercel_AI_Google_GoogleParseContext_"></a>

### Parse(JsonElement, GoogleParseContext)


```csharp
public static LanguageModelGenerateResult Parse(JsonElement root, GoogleParseContext context)
```


Parses one generateContent JSON document.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `root` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `context` | [GoogleParseContext](/api/Vercel.AI.Google/GoogleParseContext/) |  |

**Returns:** [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L16)

<a id="Vercel_AI_Google_GoogleResponse_Sources_"></a>
<a id="Vercel_AI_Google_GoogleResponse_Sources_System_Text_Json_JsonElement_Vercel_AI_Google_GoogleParseContext_"></a>

### Sources(JsonElement, GoogleParseContext)


```csharp
public static IReadOnlyList<GeneratedSource> Sources(JsonElement grounding, GoogleParseContext context)
```


Reads URL and document sources from grounding metadata.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `grounding` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `context` | [GoogleParseContext](/api/Vercel.AI.Google/GoogleParseContext/) |  |

**Returns:** [IReadOnlyList&lt;GeneratedSource&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L96)
