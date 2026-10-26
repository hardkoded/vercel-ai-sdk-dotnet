---
title: "GoogleFileUrls"
description: "URLs Gemini can read without downloading the bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleFileUrls"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleFileUrls"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleFileUrls
```


URLs Gemini can read without downloading the bytes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleFileUrls.cs#L10)

## Methods

<a id="Vercel_AI_Google_GoogleFileUrls_IsSupported_"></a>
<a id="Vercel_AI_Google_GoogleFileUrls_IsSupported_System_String_"></a>

### IsSupported(string?)


```csharp
public static bool IsSupported(string? url)
```


True for Generative Language file URLs and public YouTube watch URLs.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleFileUrls.cs#L19)
