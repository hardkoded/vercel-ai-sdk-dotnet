---
title: "NoOutputGeneratedException"
description: "No LLM output was generated, for example because a model stream ended without output. Maps to NoOutputGeneratedError."
editUrl: false
prev: false
next: false
sidebar:
  label: "NoOutputGeneratedException"
  hidden: true
---

<a id="Vercel_AI_GenerateText_NoOutputGeneratedException"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class NoOutputGeneratedException : AiSdkException, ISerializable
```


No LLM output was generated, for example because a model stream ended without output. Maps to `NoOutputGeneratedError`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/NoOutputGeneratedException.cs#L10)

## Constructors

<a id="Vercel_AI_GenerateText_NoOutputGeneratedException__ctor_"></a>
<a id="Vercel_AI_GenerateText_NoOutputGeneratedException__ctor"></a>

### NoOutputGeneratedException()


```csharp
public NoOutputGeneratedException()
```


Creates the exception with the default message.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/NoOutputGeneratedException.cs#L13)

<a id="Vercel_AI_GenerateText_NoOutputGeneratedException__ctor_System_String_"></a>

### NoOutputGeneratedException(string)


```csharp
public NoOutputGeneratedException(string message)
```


Creates the exception with a message.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/NoOutputGeneratedException.cs#L19)

<a id="Vercel_AI_GenerateText_NoOutputGeneratedException__ctor_System_String_System_Exception_"></a>

### NoOutputGeneratedException(string, Exception)


```csharp
public NoOutputGeneratedException(string message, Exception innerException)
```


Creates the exception with a message and the underlying error.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `innerException` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/NoOutputGeneratedException.cs#L25)

## Properties

<a id="Vercel_AI_GenerateText_NoOutputGeneratedException_Name_"></a>
<a id="Vercel_AI_GenerateText_NoOutputGeneratedException_Name"></a>

### Name


```csharp
public string Name { get; }
```


Stable error name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/NoOutputGeneratedException.cs#L31)
