---
title: "DataUrls"
description: "Data URL parsing. Maps to splitDataUrl."
editUrl: false
prev: false
next: false
sidebar:
  label: "DataUrls"
  hidden: true
---

<a id="Vercel_AI_Prompt_DataUrls"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class DataUrls
```


Data URL parsing. Maps to `splitDataUrl`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/SplitDataUrl.cs#L25)

## Methods

<a id="Vercel_AI_Prompt_DataUrls_SplitDataUrl_"></a>
<a id="Vercel_AI_Prompt_DataUrls_SplitDataUrl_System_String_"></a>

### SplitDataUrl(string)


```csharp
public static DataUrlParts SplitDataUrl(string dataUrl)
```


Splits a data URL. Returns empty parts when the value cannot be split.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `dataUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [DataUrlParts](/api/Vercel.AI.Prompt/DataUrlParts/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/SplitDataUrl.cs#L28)
