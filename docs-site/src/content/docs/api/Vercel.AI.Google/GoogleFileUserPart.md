---
title: "GoogleFileUserPart"
description: "A user file. Kind is url, data, text, or reference."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleFileUserPart"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleFileUserPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleFileUserPart : GoogleUserPart
```


A user file. [Kind](#Vercel_AI_Google_GoogleFileUserPart_Kind) is `url`, `data`, `text`, or `reference`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleUserPart](/api/Vercel.AI.Google/GoogleUserPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L105)

## Constructors

<a id="Vercel_AI_Google_GoogleFileUserPart__ctor_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart__ctor_System_String_System_String_"></a>

### GoogleFileUserPart(string, string)


```csharp
public GoogleFileUserPart(string mediaType, string kind)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L108)

## Properties

<a id="Vercel_AI_Google_GoogleFileUserPart_Data_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; set; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L127)

<a id="Vercel_AI_Google_GoogleFileUserPart_Kind_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


`url`, `data`, `text`, or `reference`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L118)

<a id="Vercel_AI_Google_GoogleFileUserPart_MediaType_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Declared media type. A top-level type such as `image` is completed from the bytes.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L115)

<a id="Vercel_AI_Google_GoogleFileUserPart_OriginalUrl_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_OriginalUrl"></a>

### OriginalUrl


```csharp
public string? OriginalUrl { get; set; }
```


Original URI preserved when a `gs:` URL was normalized.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L124)

<a id="Vercel_AI_Google_GoogleFileUserPart_Reference_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_Reference"></a>

### Reference


```csharp
public IReadOnlyDictionary<string, string>? Reference { get; set; }
```


Map from provider name to an uploaded file id. The `google` entry is the file URI.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L133)

<a id="Vercel_AI_Google_GoogleFileUserPart_Text_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Inline text source.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L130)

<a id="Vercel_AI_Google_GoogleFileUserPart_Url_"></a>
<a id="Vercel_AI_Google_GoogleFileUserPart_Url"></a>

### Url


```csharp
public string? Url { get; set; }
```


Remote URL, including `gs://` and `data:` URLs.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L121)
