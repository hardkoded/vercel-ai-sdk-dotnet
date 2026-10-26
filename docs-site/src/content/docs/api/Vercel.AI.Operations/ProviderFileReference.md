---
title: "ProviderFileReference"
description: "A provider file reference."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderFileReference"
  hidden: true
---

<a id="Vercel_AI_Operations_ProviderFileReference"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class ProviderFileReference
```


A provider file reference.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L44)

## Constructors

<a id="Vercel_AI_Operations_ProviderFileReference__ctor_"></a>
<a id="Vercel_AI_Operations_ProviderFileReference__ctor_System_String_System_String_"></a>

### ProviderFileReference(string, string?)


```csharp
public ProviderFileReference(string id, string? provider = null)
```


Creates a reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L47)

## Properties

<a id="Vercel_AI_Operations_ProviderFileReference_Id_"></a>
<a id="Vercel_AI_Operations_ProviderFileReference_Id"></a>

### Id


```csharp
public string Id { get; }
```


Provider file id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L54)

<a id="Vercel_AI_Operations_ProviderFileReference_Provider_"></a>
<a id="Vercel_AI_Operations_ProviderFileReference_Provider"></a>

### Provider


```csharp
public string? Provider { get; }
```


Provider name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L57)
