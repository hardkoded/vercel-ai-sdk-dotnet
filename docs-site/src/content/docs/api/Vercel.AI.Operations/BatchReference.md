---
title: "BatchReference"
description: "A version-2 batch reference."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchReference"
  hidden: true
---

<a id="Vercel_AI_Operations_BatchReference"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class BatchReference
```


A version-2 batch reference.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L425)

## Constructors

<a id="Vercel_AI_Operations_BatchReference__ctor_"></a>
<a id="Vercel_AI_Operations_BatchReference__ctor_System_Int32_System_String_System_String_"></a>

### BatchReference(int, string, string)


```csharp
public BatchReference(int version, string id, string provider)
```


Creates a reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `version` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L428)

## Properties

<a id="Vercel_AI_Operations_BatchReference_Id_"></a>
<a id="Vercel_AI_Operations_BatchReference_Id"></a>

### Id


```csharp
public string Id { get; }
```


Batch id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L439)

<a id="Vercel_AI_Operations_BatchReference_Provider_"></a>
<a id="Vercel_AI_Operations_BatchReference_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L442)

<a id="Vercel_AI_Operations_BatchReference_Version_"></a>
<a id="Vercel_AI_Operations_BatchReference_Version"></a>

### Version


```csharp
public int Version { get; }
```


Reference version. Only 2 is accepted.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L436)
