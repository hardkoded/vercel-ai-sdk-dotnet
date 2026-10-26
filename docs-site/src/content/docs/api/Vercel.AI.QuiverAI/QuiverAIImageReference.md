---
title: "QuiverAIImageReference"
description: "A reference image accepted by providerOptions.quiverai.referenceImages: { \"url\" } or { \"base64\" }. Exactly one of Url and Base64 is set. Serializes to that J..."
editUrl: false
prev: false
next: false
sidebar:
  label: "QuiverAIImageReference"
  hidden: true
---

<a id="Vercel_AI_QuiverAI_QuiverAIImageReference"></a>

Namespace: [Vercel.AI.QuiverAI](/api/Vercel.AI.QuiverAI/)


```csharp
public sealed class QuiverAIImageReference
```


A reference image accepted by `providerOptions.quiverai.referenceImages`: `{ "url" }` or `{ "base64" }`.
Exactly one of [Url](#Vercel_AI_QuiverAI_QuiverAIImageReference_Url) and [Base64](#Vercel_AI_QuiverAI_QuiverAIImageReference_Base64) is set. Serializes to that JSON shape.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageReference.cs#L17)

## Properties

<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Base64_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Base64"></a>

### Base64


```csharp
[JsonPropertyName("base64")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? Base64 { get; }
```


Base64-encoded image data without a data URL prefix, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for a URL reference.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageReference.cs#L49)

<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Url_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Url"></a>

### Url


```csharp
[JsonPropertyName("url")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? Url { get; }
```


HTTP or HTTPS image URL, or &lt;a href="https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null"&gt;null&lt;/a&gt; for a base64 reference.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageReference.cs#L44)

## Methods

<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Prepare_"></a>
<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Prepare_System_Byte___"></a>

### Prepare(byte[])


```csharp
public static QuiverAIImageReference Prepare(byte[] input)
```


Returns a base64 reference for PNG, JPEG, WebP, GIF, or SVG bytes.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `input` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

**Returns:** [QuiverAIImageReference](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageReference.cs#L87)

<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Prepare_System_String_"></a>

### Prepare(string)


```csharp
public static QuiverAIImageReference Prepare(string input)
```


Returns a URL reference for a URL, or a base64 reference for a base64 string or a base64 data URL.
Throws [InvalidArgumentException](/api/Vercel.AI.Operations/InvalidArgumentException/) for other schemes, invalid base64, or unsupported images.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `input` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [QuiverAIImageReference](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageReference.cs#L63)

<a id="Vercel_AI_QuiverAI_QuiverAIImageReference_Prepare_System_Uri_"></a>

### Prepare(Uri)


```csharp
public static QuiverAIImageReference Prepare(Uri input)
```


Returns a URL reference for an HTTP or HTTPS URL.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `input` | [Uri](https://learn.microsoft.com/dotnet/api/system.uri) |  |

**Returns:** [QuiverAIImageReference](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/QuiverAI/QuiverAIImageReference.cs#L54)
