---
title: "SkillFile"
description: "One file inside a skill upload."
editUrl: false
prev: false
next: false
sidebar:
  label: "SkillFile"
  hidden: true
---

<a id="Vercel_AI_Operations_SkillFile"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class SkillFile
```


One file inside a skill upload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L340)

## Constructors

<a id="Vercel_AI_Operations_SkillFile__ctor_"></a>
<a id="Vercel_AI_Operations_SkillFile__ctor_System_String_System_Object_System_String_"></a>

### SkillFile(string?, object?, string?)


```csharp
public SkillFile(string? path, object? data, string? mediaType = null)
```


Creates a skill file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `path` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L343)

## Properties

<a id="Vercel_AI_Operations_SkillFile_Data_"></a>
<a id="Vercel_AI_Operations_SkillFile_Data"></a>

### Data


```csharp
public object? Data { get; }
```


Tagged data or a bytes/string shorthand.

**Value:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L354)

<a id="Vercel_AI_Operations_SkillFile_MediaType_"></a>
<a id="Vercel_AI_Operations_SkillFile_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Optional media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L357)

<a id="Vercel_AI_Operations_SkillFile_Path_"></a>
<a id="Vercel_AI_Operations_SkillFile_Path"></a>

### Path


```csharp
public string? Path { get; }
```


Path inside the skill.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/Upload.cs#L351)
