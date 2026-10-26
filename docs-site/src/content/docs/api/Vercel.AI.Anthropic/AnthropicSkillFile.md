---
title: "AnthropicSkillFile"
description: "A skill file sent as multipart files[]."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicSkillFile"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicSkillFile"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicSkillFile
```


A skill file sent as multipart `files[]`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L13)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicSkillFile__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillFile__ctor_System_String_System_Byte___"></a>

### AnthropicSkillFile(string, byte[])


```csharp
public AnthropicSkillFile(string path, byte[] data)
```


Creates a skill file.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `path` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L16)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicSkillFile_Data_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillFile_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


File bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L26)

<a id="Vercel_AI_Anthropic_AnthropicSkillFile_Path_"></a>
<a id="Vercel_AI_Anthropic_AnthropicSkillFile_Path"></a>

### Path


```csharp
public string Path { get; }
```


Relative path inside the skill.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicSkills.cs#L23)
