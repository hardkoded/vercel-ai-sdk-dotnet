---
title: "OpenAIShellInlineSkill"
description: "An inline skill of a containerAuto shell environment: a base64 encoded zip archive."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellInlineSkill"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellInlineSkill : OpenAIShellContainerSkill
```


An `inline` skill of a `containerAuto` shell environment: a base64 encoded zip archive.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAIShellContainerSkill](/api/Vercel.AI.OpenAI/OpenAIShellContainerSkill/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L184)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill_Data_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill_Data"></a>

### Data


```csharp
public string Data { get; set; }
```


Base64 encoded content of the `application/zip` archive that holds the skill.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L193)

<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill_Description_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill_Description"></a>

### Description


```csharp
public string Description { get; set; }
```


Skill description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L190)

<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill_Name_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellInlineSkill_Name"></a>

### Name


```csharp
public string Name { get; set; }
```


Skill name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L187)
