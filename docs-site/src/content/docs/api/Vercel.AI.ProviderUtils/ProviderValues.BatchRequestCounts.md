---
title: "ProviderValues.BatchRequestCounts"
description: "Complete batch request counts."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderValues.BatchRequestCounts"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ProviderValues.BatchRequestCounts
```


Complete batch request counts.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L350)

## Constructors

<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts__ctor_System_Int64_System_Int64_System_Int64_System_Int64_"></a>

### BatchRequestCounts(long, long, long, long)


```csharp
public BatchRequestCounts(long total, long pending, long completed, long failed)
```


Creates counts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `total` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `pending` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `completed` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |
| `failed` | [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L353)

## Properties

<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Completed_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Completed"></a>

### Completed


```csharp
public long Completed { get; }
```


Completed requests.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L368)

<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Failed_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Failed"></a>

### Failed


```csharp
public long Failed { get; }
```


Failed requests.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L371)

<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Pending_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Pending"></a>

### Pending


```csharp
public long Pending { get; }
```


Requests still pending.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L365)

<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Total_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_BatchRequestCounts_Total"></a>

### Total


```csharp
public long Total { get; }
```


Total requests.

**Value:** [long](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L362)
