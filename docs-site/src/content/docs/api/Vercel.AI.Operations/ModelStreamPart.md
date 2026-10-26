---
title: "ModelStreamPart"
description: "A part emitted by a transcription or translation model stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelStreamPart"
  hidden: true
---

<a id="Vercel_AI_Operations_ModelStreamPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ModelStreamPart
```


A part emitted by a transcription or translation model stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L31)

## Properties

<a id="Vercel_AI_Operations_ModelStreamPart_Audio_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Audio"></a>

### Audio


```csharp
public byte[]? Audio { get; set; }
```


Translated audio bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L76)

<a id="Vercel_AI_Operations_ModelStreamPart_Delta_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Delta"></a>

### Delta


```csharp
public string? Delta { get; set; }
```


Text delta.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L43)

<a id="Vercel_AI_Operations_ModelStreamPart_DurationInSeconds_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_DurationInSeconds"></a>

### DurationInSeconds


```csharp
public double? DurationInSeconds { get; set; }
```


Duration in seconds.

**Value:** [double?](https://learn.microsoft.com/dotnet/api/system.double)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L52)

<a id="Vercel_AI_Operations_ModelStreamPart_Error_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Error"></a>

### Error


```csharp
public Exception? Error { get; set; }
```


Error carried by an `error` part.

**Value:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L73)

<a id="Vercel_AI_Operations_ModelStreamPart_Headers_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Headers"></a>

### Headers


```csharp
public IReadOnlyDictionary<string, string>? Headers { get; set; }
```


Response headers.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L64)

<a id="Vercel_AI_Operations_ModelStreamPart_Id_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Id"></a>

### Id


```csharp
public string? Id { get; set; }
```


Part id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L37)

<a id="Vercel_AI_Operations_ModelStreamPart_Language_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Language"></a>

### Language


```csharp
public string? Language { get; set; }
```


Detected language.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L49)

<a id="Vercel_AI_Operations_ModelStreamPart_ModelId_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_ModelId"></a>

### ModelId


```csharp
public string? ModelId { get; set; }
```


Response model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L61)

<a id="Vercel_AI_Operations_ModelStreamPart_OutputText_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_OutputText"></a>

### OutputText


```csharp
public string? OutputText { get; set; }
```


Translated text on `finish`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L82)

<a id="Vercel_AI_Operations_ModelStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L67)

<a id="Vercel_AI_Operations_ModelStreamPart_Raw_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Raw"></a>

### Raw


```csharp
public JsonElement? Raw { get; set; }
```


Raw provider chunk.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L70)

<a id="Vercel_AI_Operations_ModelStreamPart_Segments_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Segments"></a>

### Segments


```csharp
public IReadOnlyList<TranscriptSegment>? Segments { get; set; }
```


Transcript segments.

**Value:** [IReadOnlyList&lt;TranscriptSegment&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L46)

<a id="Vercel_AI_Operations_ModelStreamPart_SourceText_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_SourceText"></a>

### SourceText


```csharp
public string? SourceText { get; set; }
```


Source transcript on `finish`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L79)

<a id="Vercel_AI_Operations_ModelStreamPart_Text_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Final text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L40)

<a id="Vercel_AI_Operations_ModelStreamPart_Timestamp_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Timestamp"></a>

### Timestamp


```csharp
public DateTime? Timestamp { get; set; }
```


Response timestamp.

**Value:** [DateTime?](https://learn.microsoft.com/dotnet/api/system.datetime)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L58)

<a id="Vercel_AI_Operations_ModelStreamPart_Type_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L34)

<a id="Vercel_AI_Operations_ModelStreamPart_Usage_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Usage"></a>

### Usage


```csharp
public OperationUsage? Usage { get; set; }
```


Usage on `finish`.

**Value:** [OperationUsage](/api/Vercel.AI.Operations/OperationUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L85)

<a id="Vercel_AI_Operations_ModelStreamPart_Warnings_"></a>
<a id="Vercel_AI_Operations_ModelStreamPart_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning>? Warnings { get; set; }
```


Warnings carried by `stream-start`.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/MediaStream.cs#L55)
