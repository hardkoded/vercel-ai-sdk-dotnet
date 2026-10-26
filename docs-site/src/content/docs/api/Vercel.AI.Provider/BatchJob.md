---
title: "BatchJob"
description: "A submitted batch job."
editUrl: false
prev: false
next: false
sidebar:
  label: "BatchJob"
  hidden: true
---

<a id="Vercel_AI_Provider_BatchJob"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class BatchJob
```


A submitted batch job.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L588)

## Constructors

<a id="Vercel_AI_Provider_BatchJob__ctor_"></a>
<a id="Vercel_AI_Provider_BatchJob__ctor_System_String_System_String_"></a>

### BatchJob(string, string)


```csharp
public BatchJob(string id, string status)
```


Creates a batch job.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `status` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L591)

## Properties

<a id="Vercel_AI_Provider_BatchJob_Id_"></a>
<a id="Vercel_AI_Provider_BatchJob_Id"></a>

### Id


```csharp
public string Id { get; }
```


Provider job id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L598)

<a id="Vercel_AI_Provider_BatchJob_Status_"></a>
<a id="Vercel_AI_Provider_BatchJob_Status"></a>

### Status


```csharp
public string Status { get; }
```


Provider status.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/OtherModels.cs#L601)
