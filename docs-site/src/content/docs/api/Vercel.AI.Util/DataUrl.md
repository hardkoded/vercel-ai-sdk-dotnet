---
title: "DataUrl"
description: "Reads text from text/ data URLs."
editUrl: false
prev: false
next: false
sidebar:
  label: "DataUrl"
  hidden: true
---

<a id="Vercel_AI_Util_DataUrl"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public static class DataUrl
```


Reads text from `text/*` data URLs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DataUrl.cs#L11)

## Methods

<a id="Vercel_AI_Util_DataUrl_GetTextFromDataUrl_"></a>
<a id="Vercel_AI_Util_DataUrl_GetTextFromDataUrl_System_String_"></a>

### GetTextFromDataUrl(string?)


```csharp
public static string GetTextFromDataUrl(string? dataUrl)
```


Decodes a `text/*` data URL. Without a charset the decoded bytes are returned as Latin-1 text.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `dataUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/DataUrl.cs#L20)
