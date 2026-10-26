---
title: "UploadSkillResult"
description: "Skill upload result."
editUrl: false
prev: false
next: false
sidebar:
  label: "UploadSkillResult"
  hidden: true
---

<a id="Vercel_AI_Operations_UploadSkillResult"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UploadSkillResult
```


Skill upload result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L403)

## Constructors

<a id="Vercel_AI_Operations_UploadSkillResult__ctor_"></a>
<a id="Vercel_AI_Operations_UploadSkillResult__ctor_Vercel_AI_Operations_ProviderFileReference_System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_OperationWarning__"></a>

### `UploadSkillResult(ProviderFileReference, IReadOnlyList<OperationWarning>?)`


```csharp
public UploadSkillResult(ProviderFileReference providerReference, IReadOnlyList<OperationWarning>? warnings = null)
```


Creates a skill result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerReference` | [ProviderFileReference](/api/Vercel.AI.Operations/ProviderFileReference/) |  |
| `warnings` | [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L406)

## Properties

<a id="Vercel_AI_Operations_UploadSkillResult_ProviderReference_"></a>
<a id="Vercel_AI_Operations_UploadSkillResult_ProviderReference"></a>

### ProviderReference


```csharp
public ProviderFileReference ProviderReference { get; }
```


Provider reference.

**Value:** [ProviderFileReference](/api/Vercel.AI.Operations/ProviderFileReference/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L413)

<a id="Vercel_AI_Operations_UploadSkillResult_Warnings_"></a>
<a id="Vercel_AI_Operations_UploadSkillResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OperationWarning> Warnings { get; }
```


Warnings.

**Value:** [IReadOnlyList&lt;OperationWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L416)
