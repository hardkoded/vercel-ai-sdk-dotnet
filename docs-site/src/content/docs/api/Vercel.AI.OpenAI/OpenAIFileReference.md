---
title: "OpenAIFileReference"
description: "A file reference that carries an OpenAI file id."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIFileReference"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIFileReference"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIFileReference
```


A file reference that carries an OpenAI file id.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L14)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIFileReference__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileReference__ctor_System_String_"></a>

### OpenAIFileReference(string?)


```csharp
public OpenAIFileReference(string? openAI)
```


Creates a reference.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `openAI` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L17)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIFileReference_OpenAI_"></a>
<a id="Vercel_AI_OpenAI_OpenAIFileReference_OpenAI"></a>

### OpenAI


```csharp
public string? OpenAI { get; }
```


OpenAI file id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Files/OpenAIFileStore.cs#L23)
