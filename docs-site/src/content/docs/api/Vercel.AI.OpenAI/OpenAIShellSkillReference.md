---
title: "OpenAIShellSkillReference"
description: "A skillReference skill of a containerAuto shell environment."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellSkillReference"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellSkillReference"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellSkillReference : OpenAIShellContainerSkill
```


A `skillReference` skill of a `containerAuto` shell environment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [OpenAIShellContainerSkill](/api/Vercel.AI.OpenAI/OpenAIShellContainerSkill/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L161)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellSkillReference_ProviderReference_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellSkillReference_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; set; }
```


Provider reference of the skill, keyed by provider name. The `openai` entry is the skill id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L164)

<a id="Vercel_AI_OpenAI_OpenAIShellSkillReference_Version_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellSkillReference_Version"></a>

### Version


```csharp
public string? Version { get; set; }
```


Skill version. Defaults to `latest`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L168)
