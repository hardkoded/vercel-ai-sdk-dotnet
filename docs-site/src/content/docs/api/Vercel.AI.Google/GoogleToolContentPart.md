---
title: "GoogleToolContentPart"
description: "One multimodal tool-result part."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolContentPart"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolContentPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolContentPart
```


One multimodal tool-result part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L280)

## Constructors

<a id="Vercel_AI_Google_GoogleToolContentPart__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart__ctor_System_String_"></a>

### GoogleToolContentPart(string)


```csharp
public GoogleToolContentPart(string type)
```


Creates a content part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L283)

## Properties

<a id="Vercel_AI_Google_GoogleToolContentPart_Data_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; set; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L301)

<a id="Vercel_AI_Google_GoogleToolContentPart_DataKind_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart_DataKind"></a>

### DataKind


```csharp
public string? DataKind { get; set; }
```


`data` or `url`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L298)

<a id="Vercel_AI_Google_GoogleToolContentPart_MediaType_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; set; }
```


Media type of a file.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L295)

<a id="Vercel_AI_Google_GoogleToolContentPart_Text_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text, when [Type](#Vercel_AI_Google_GoogleToolContentPart_Type) is `text`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L292)

<a id="Vercel_AI_Google_GoogleToolContentPart_Type_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


`text` or `file`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L289)

<a id="Vercel_AI_Google_GoogleToolContentPart_Url_"></a>
<a id="Vercel_AI_Google_GoogleToolContentPart_Url"></a>

### Url


```csharp
public string? Url { get; set; }
```


URL or data URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L304)
