---
title: "RevAIError"
description: "Parses Rev.ai error JSON."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAIError"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAIError"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public static class RevAIError
```


Parses Rev.ai error JSON.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L65)

## Methods

<a id="Vercel_AI_RevAI_RevAIError_Parse_"></a>
<a id="Vercel_AI_RevAI_RevAIError_Parse_System_String_"></a>

### Parse(string)


```csharp
public static RevAIErrorParseResult Parse(string json)
```


Parses a resource error body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [RevAIErrorParseResult](/api/Vercel.AI.RevAI/RevAIErrorParseResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L68)
