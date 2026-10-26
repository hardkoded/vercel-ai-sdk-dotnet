---
title: "SkillFilePayload"
description: "Normalized skill file passed to the provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "SkillFilePayload"
  hidden: true
---

<a id="Vercel_AI_Operations_SkillFilePayload"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SkillFilePayload
```


Normalized skill file passed to the provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L361)

## Constructors

<a id="Vercel_AI_Operations_SkillFilePayload__ctor_"></a>
<a id="Vercel_AI_Operations_SkillFilePayload__ctor_System_String_Vercel_AI_Operations_UploadData_System_String_"></a>

### SkillFilePayload(string?, UploadData, string?)


```csharp
public SkillFilePayload(string? path, UploadData data, string? mediaType)
```


Creates a payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `path` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [UploadData](/api/Vercel.AI.Operations/UploadData/) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L364)

## Properties

<a id="Vercel_AI_Operations_SkillFilePayload_Data_"></a>
<a id="Vercel_AI_Operations_SkillFilePayload_Data"></a>

### Data


```csharp
public UploadData Data { get; }
```


Tagged data.

**Value:** [UploadData](/api/Vercel.AI.Operations/UploadData/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L375)

<a id="Vercel_AI_Operations_SkillFilePayload_MediaType_"></a>
<a id="Vercel_AI_Operations_SkillFilePayload_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L378)

<a id="Vercel_AI_Operations_SkillFilePayload_Path_"></a>
<a id="Vercel_AI_Operations_SkillFilePayload_Path"></a>

### Path


```csharp
public string? Path { get; }
```


Path inside the skill.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L372)
