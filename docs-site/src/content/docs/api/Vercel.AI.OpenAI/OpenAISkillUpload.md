---
title: "OpenAISkillUpload"
description: "Skill upload result."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISkillUpload"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISkillUpload"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISkillUpload
```


Skill upload result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L35)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_CreatedAt_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_CreatedAt"></a>

### CreatedAt


```csharp
public long? CreatedAt { get; }
```


Creation time as unix seconds.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L64)

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_DefaultVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_DefaultVersion"></a>

### DefaultVersion


```csharp
public string? DefaultVersion { get; }
```


Default version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L61)

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Skill description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L55)

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Id_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Id"></a>

### Id


```csharp
public string Id { get; }
```


Skill id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L49)

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_LatestVersion_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_LatestVersion"></a>

### LatestVersion


```csharp
public string? LatestVersion { get; }
```


Latest version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L58)

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Name_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Name"></a>

### Name


```csharp
public string? Name { get; }
```


Skill name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L52)

<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillUpload_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings produced while preparing the upload.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L67)
