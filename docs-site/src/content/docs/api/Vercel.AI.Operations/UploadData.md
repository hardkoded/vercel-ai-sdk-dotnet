---
title: "UploadData"
description: "Tagged file bytes, text, or a stream."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadData"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadData"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadData
```


Tagged file bytes, text, or a stream.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L10)

## Constructors

<a id="Vercel_AI_Operations_UploadData__ctor_"></a>
<a id="Vercel_AI_Operations_UploadData__ctor_System_String_System_Object_"></a>

### UploadData(string, object?)


```csharp
public UploadData(string type, object? data)
```


Creates tagged upload data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L13)

## Properties

<a id="Vercel_AI_Operations_UploadData_Data_"></a>
<a id="Vercel_AI_Operations_UploadData_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Bytes, base64 text, or a [UploadStream](/api/Vercel.AI.Operations/UploadStream/).

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L23)

<a id="Vercel_AI_Operations_UploadData_Type_"></a>
<a id="Vercel_AI_Operations_UploadData_Type"></a>

### Type


```csharp
public string Type { get; }
```


`data`, `text`, or `stream`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L20)
