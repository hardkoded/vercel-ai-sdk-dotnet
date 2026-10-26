---
title: "FinishReasons"
description: "Maps provider finish-reason strings onto FinishReason."
editUrl: false
prev: false
next: false
sidebar:
  label: "FinishReasons"
  hidden: true
---

<a id="Vercel_AI_Provider_FinishReasons"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public static class FinishReasons
```


Maps provider finish-reason strings onto [FinishReason](/api/Vercel.AI.Provider/FinishReason/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2265)

## Methods

<a id="Vercel_AI_Provider_FinishReasons_Parse_"></a>
<a id="Vercel_AI_Provider_FinishReasons_Parse_System_String_"></a>

### Parse(string?)


```csharp
public static FinishReason Parse(string? raw)
```


Maps a provider finish reason.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2268)
