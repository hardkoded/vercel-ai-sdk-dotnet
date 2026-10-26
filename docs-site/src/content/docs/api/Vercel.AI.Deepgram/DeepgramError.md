---
title: "DeepgramError"
description: "Parses Deepgram error JSON."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramError"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramError"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public static class DeepgramError
```


Parses Deepgram error JSON.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L68)

## Methods

<a id="Vercel_AI_Deepgram_DeepgramError_Parse_"></a>
<a id="Vercel_AI_Deepgram_DeepgramError_Parse_System_String_"></a>

### Parse(string?)


```csharp
public static DeepgramErrorParseResult Parse(string? json)
```


Parses an `err_code`/`err_msg` body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DeepgramErrorParseResult](/api/Vercel.AI.Deepgram/DeepgramErrorParseResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L71)
