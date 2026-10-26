---
title: "XaiFileUpload"
description: "Bytes, base64, or a stream uploaded to xAI."
editUrl: false
prev: false
next: false
sidebar:
  label: "XaiFileUpload"
  hidden: true
---

<a id="Vercel_AI_Xai_XaiFileUpload"></a>

Namespace: [Vercel.AI.Xai](/api/Vercel.AI.Xai/)


```csharp
public sealed class XaiFileUpload
```


Bytes, base64, or a stream uploaded to xAI.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L15)

## Properties

<a id="Vercel_AI_Xai_XaiFileUpload_Base64_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_Base64"></a>

### Base64


```csharp
public string? Base64 { get; set; }
```


Base64 file contents. Decoded before they are sent.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L21)

<a id="Vercel_AI_Xai_XaiFileUpload_Bytes_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_Bytes"></a>

### Bytes


```csharp
public byte[]? Bytes { get; set; }
```


Inline file bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L18)

<a id="Vercel_AI_Xai_XaiFileUpload_CancelStream_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_CancelStream"></a>

### CancelStream


```csharp
public Action<Exception>? CancelStream { get; set; }
```


Called when [ExpiresAfter](#Vercel_AI_Xai_XaiFileUpload_ExpiresAfter) is rejected before any request.

**Value:** [Action&lt;Exception&gt;](https://learn.microsoft.com/dotnet/api/system.action-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L27)

<a id="Vercel_AI_Xai_XaiFileUpload_ExpiresAfter_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_ExpiresAfter"></a>

### ExpiresAfter


```csharp
public double? ExpiresAfter { get; set; }
```


TTL in seconds. Integers from 3600 through 2592000 are accepted.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L39)

<a id="Vercel_AI_Xai_XaiFileUpload_FileName_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_FileName"></a>

### FileName


```csharp
public string? FileName { get; set; }
```


File name. Omitted names are sent as `blob`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L33)

<a id="Vercel_AI_Xai_XaiFileUpload_Headers_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_Headers"></a>

### Headers


```csharp
public IDictionary<string, string>? Headers { get; set; }
```


Headers merged over the provider headers.

**Value:** [IDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L42)

<a id="Vercel_AI_Xai_XaiFileUpload_MediaType_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; set; }
```


File media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L30)

<a id="Vercel_AI_Xai_XaiFileUpload_Stream_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_Stream"></a>

### Stream


```csharp
public Stream? Stream { get; set; }
```


Streamed file contents. Fields are written before these bytes.

**Value:** [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L24)

<a id="Vercel_AI_Xai_XaiFileUpload_TeamId_"></a>
<a id="Vercel_AI_Xai_XaiFileUpload_TeamId"></a>

### TeamId


```csharp
public string? TeamId { get; set; }
```


`team_id`, when set.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Xai/XaiFiles.cs#L36)
