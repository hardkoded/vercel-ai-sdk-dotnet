---
title: "InvalidPromptException"
description: "The prompt input is invalid. Maps to InvalidPromptError."
editUrl: false
prev: false
next: false
sidebar:
  label: "InvalidPromptException"
  hidden: true
---

<a id="Vercel_AI_Prompt_InvalidPromptException"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class InvalidPromptException : AiSdkException, ISerializable
```


The prompt input is invalid. Maps to `InvalidPromptError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L132)

## Constructors

<a id="Vercel_AI_Prompt_InvalidPromptException__ctor_"></a>
<a id="Vercel_AI_Prompt_InvalidPromptException__ctor_System_String_"></a>

### InvalidPromptException(string)


```csharp
public InvalidPromptException(string message)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L135)

## Properties

<a id="Vercel_AI_Prompt_InvalidPromptException_Name_"></a>
<a id="Vercel_AI_Prompt_InvalidPromptException_Name"></a>

### Name


```csharp
public string Name { get; }
```


Stable error name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/StandardizePrompt.cs#L141)
