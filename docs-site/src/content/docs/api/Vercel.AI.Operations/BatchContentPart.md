---
title: "BatchContentPart"
description: "One content part inside a batch text result."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchContentPart"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchContentPart"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchContentPart
```


One content part inside a batch text result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L279)

## Constructors

<a id="Vercel_AI_Operations_BatchContentPart__ctor_"></a>
<a id="Vercel_AI_Operations_BatchContentPart__ctor_System_String_"></a>

### BatchContentPart(string)


```csharp
public BatchContentPart(string type)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L282)

## Properties

<a id="Vercel_AI_Operations_BatchContentPart_Citations_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Citations"></a>

### Citations


```csharp
public IReadOnlyList<Citation>? Citations { get; set; }
```


Citations of a text part.

**Value:** [IReadOnlyList&lt;Citation&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L327)

<a id="Vercel_AI_Operations_BatchContentPart_Dynamic_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Dynamic"></a>

### Dynamic


```csharp
public bool? Dynamic { get; set; }
```


True for a dynamic tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L309)

<a id="Vercel_AI_Operations_BatchContentPart_FileBytes_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_FileBytes"></a>

### FileBytes


```csharp
public byte[]? FileBytes { get; set; }
```


Downloaded file bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L321)

<a id="Vercel_AI_Operations_BatchContentPart_FileUrl_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_FileUrl"></a>

### FileUrl


```csharp
public string? FileUrl { get; set; }
```


File URL before download.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L324)

<a id="Vercel_AI_Operations_BatchContentPart_Input_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Input"></a>

### Input


```csharp
public JsonElement? Input { get; set; }
```


Parsed tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L300)

<a id="Vercel_AI_Operations_BatchContentPart_IsError_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_IsError"></a>

### IsError


```csharp
public bool? IsError { get; set; }
```


True when a tool result is an error.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L312)

<a id="Vercel_AI_Operations_BatchContentPart_Kind_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Kind"></a>

### Kind


```csharp
public string? Kind { get; set; }
```


Kind of a `custom` part, such as `anthropic.fallback`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L342)

<a id="Vercel_AI_Operations_BatchContentPart_MediaType_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; set; }
```


File media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L318)

<a id="Vercel_AI_Operations_BatchContentPart_Output_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Output"></a>

### Output


```csharp
public JsonElement? Output { get; set; }
```


Tool output.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L303)

<a id="Vercel_AI_Operations_BatchContentPart_ProviderExecuted_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool? ProviderExecuted { get; set; }
```


True when the provider executed the tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L306)

<a id="Vercel_AI_Operations_BatchContentPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L315)

<a id="Vercel_AI_Operations_BatchContentPart_SourceId_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_SourceId"></a>

### SourceId


```csharp
public string? SourceId { get; set; }
```


Source id of a `source` part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L330)

<a id="Vercel_AI_Operations_BatchContentPart_SourceType_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_SourceType"></a>

### SourceType


```csharp
public string? SourceType { get; set; }
```


`url` or `document` for a `source` part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L333)

<a id="Vercel_AI_Operations_BatchContentPart_Text_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L291)

<a id="Vercel_AI_Operations_BatchContentPart_Title_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Title"></a>

### Title


```csharp
public string? Title { get; set; }
```


Title of a `source` part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L339)

<a id="Vercel_AI_Operations_BatchContentPart_ToolCallId_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ToolCallId"></a>

### ToolCallId


```csharp
public string? ToolCallId { get; set; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L294)

<a id="Vercel_AI_Operations_BatchContentPart_ToolName_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ToolName"></a>

### ToolName


```csharp
public string? ToolName { get; set; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L297)

<a id="Vercel_AI_Operations_BatchContentPart_Type_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L288)

<a id="Vercel_AI_Operations_BatchContentPart_Url_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Url"></a>

### Url


```csharp
public string? Url { get; set; }
```


URL of a `url` source.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L336)
