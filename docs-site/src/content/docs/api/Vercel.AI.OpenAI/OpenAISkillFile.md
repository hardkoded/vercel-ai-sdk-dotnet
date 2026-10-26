---
title: "OpenAISkillFile"
description: "One file uploaded as part of a skill."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAISkillFile"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAISkillFile"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAISkillFile
```


One file uploaded as part of a skill.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L14)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAISkillFile__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillFile__ctor_System_String_System_Byte___System_String_"></a>

### OpenAISkillFile(string, byte[], string)


```csharp
public OpenAISkillFile(string path, byte[] data, string mediaType)
```


Creates a skill file. &lt;code class="paramref"&gt;path&lt;/code&gt; is the multipart filename.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `path` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L17)

## Properties

<a id="Vercel_AI_OpenAI_OpenAISkillFile_Data_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillFile_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


File bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L28)

<a id="Vercel_AI_OpenAI_OpenAISkillFile_MediaType_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillFile_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L31)

<a id="Vercel_AI_OpenAI_OpenAISkillFile_Path_"></a>
<a id="Vercel_AI_OpenAI_OpenAISkillFile_Path"></a>

### Path


```csharp
public string Path { get; }
```


Relative path sent as the filename.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Skills/OpenAISkillStore.cs#L25)
