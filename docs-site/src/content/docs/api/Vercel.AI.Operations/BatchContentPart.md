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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L226)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L229)

## Properties

<a id="Vercel_AI_Operations_BatchContentPart_Dynamic_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Dynamic"></a>

### Dynamic


```csharp
public bool? Dynamic { get; set; }
```


True for a dynamic tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L256)

<a id="Vercel_AI_Operations_BatchContentPart_FileBytes_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_FileBytes"></a>

### FileBytes


```csharp
public byte[]? FileBytes { get; set; }
```


Downloaded file bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L268)

<a id="Vercel_AI_Operations_BatchContentPart_FileUrl_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_FileUrl"></a>

### FileUrl


```csharp
public string? FileUrl { get; set; }
```


File URL before download.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L271)

<a id="Vercel_AI_Operations_BatchContentPart_Input_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Input"></a>

### Input


```csharp
public JsonElement? Input { get; set; }
```


Parsed tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L247)

<a id="Vercel_AI_Operations_BatchContentPart_IsError_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_IsError"></a>

### IsError


```csharp
public bool? IsError { get; set; }
```


True when a tool result is an error.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L259)

<a id="Vercel_AI_Operations_BatchContentPart_MediaType_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; set; }
```


File media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L265)

<a id="Vercel_AI_Operations_BatchContentPart_Output_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Output"></a>

### Output


```csharp
public JsonElement? Output { get; set; }
```


Tool output.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L250)

<a id="Vercel_AI_Operations_BatchContentPart_ProviderExecuted_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool? ProviderExecuted { get; set; }
```


True when the provider executed the tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L253)

<a id="Vercel_AI_Operations_BatchContentPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; set; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L262)

<a id="Vercel_AI_Operations_BatchContentPart_Text_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L238)

<a id="Vercel_AI_Operations_BatchContentPart_ToolCallId_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ToolCallId"></a>

### ToolCallId


```csharp
public string? ToolCallId { get; set; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L241)

<a id="Vercel_AI_Operations_BatchContentPart_ToolName_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_ToolName"></a>

### ToolName


```csharp
public string? ToolName { get; set; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L244)

<a id="Vercel_AI_Operations_BatchContentPart_Type_"></a>
<a id="Vercel_AI_Operations_BatchContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L235)
