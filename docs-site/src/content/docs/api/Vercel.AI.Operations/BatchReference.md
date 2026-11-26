---
title: "BatchReference"
description: "A version-2 batch reference. Upstream BatchReference."
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
public class BatchReference
```


A version-2 batch reference. Upstream `BatchReference`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [BatchStatus](/api/Vercel.AI.Operations/BatchStatus/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L693)

## Constructors

<a id="Vercel_AI_Operations_BatchReference__ctor_"></a>
<a id="Vercel_AI_Operations_BatchReference__ctor"></a>

### BatchReference()


```csharp
public BatchReference()
```


Creates an empty reference. [Batch](/api/Vercel.AI.Operations/Batch/) fills it in.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L696)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L699)

## Properties

<a id="Vercel_AI_Operations_BatchReference_Id_"></a>
<a id="Vercel_AI_Operations_BatchReference_Id"></a>

### Id


```csharp
public string Id { get; set; }
```


Batch id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L710)

<a id="Vercel_AI_Operations_BatchReference_Provider_"></a>
<a id="Vercel_AI_Operations_BatchReference_Provider"></a>

### Provider


```csharp
public string Provider { get; set; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L713)

<a id="Vercel_AI_Operations_BatchReference_Version_"></a>
<a id="Vercel_AI_Operations_BatchReference_Version"></a>

### Version


```csharp
public int Version { get; set; }
```


Reference version. Only 2 is accepted.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Batch.cs#L707)
