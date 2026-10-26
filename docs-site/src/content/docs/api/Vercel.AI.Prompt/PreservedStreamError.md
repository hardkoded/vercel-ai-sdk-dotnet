---
title: "PreservedStreamError"
description: "An error-shaped value that must be returned unchanged."
editUrl: false
prev: false
next: false
sidebar:
  label: "PreservedStreamError"
  hidden: true
---

<a id="Vercel_AI_Prompt_PreservedStreamError"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class PreservedStreamError
```


An error-shaped value that must be returned unchanged.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L71)

## Constructors

<a id="Vercel_AI_Prompt_PreservedStreamError__ctor_"></a>
<a id="Vercel_AI_Prompt_PreservedStreamError__ctor_System_String_System_Boolean_"></a>

### PreservedStreamError(string, bool)


```csharp
public PreservedStreamError(string message, bool isAiSdkError)
```


Creates a preserved value.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isAiSdkError` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L74)

## Properties

<a id="Vercel_AI_Prompt_PreservedStreamError_IsAiSdkError_"></a>
<a id="Vercel_AI_Prompt_PreservedStreamError_IsAiSdkError"></a>

### IsAiSdkError


```csharp
public bool IsAiSdkError { get; }
```


True when the value stands in for a cross-realm AI SDK error.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L84)

<a id="Vercel_AI_Prompt_PreservedStreamError_Message_"></a>
<a id="Vercel_AI_Prompt_PreservedStreamError_Message"></a>

### Message


```csharp
public string Message { get; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/NormalizeStreamProviderError.cs#L81)
