---
title: "UnsupportedFunctionalityException"
description: "The provider does not implement the requested operation."
editUrl: false
prev: false
next: false
sidebar:
  label: "UnsupportedFunctionalityException"
  hidden: true
---

<a id="Vercel_AI_Operations_UnsupportedFunctionalityException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UnsupportedFunctionalityException : AiSdkException, ISerializable
```


The provider does not implement the requested operation.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L167)

## Constructors

<a id="Vercel_AI_Operations_UnsupportedFunctionalityException__ctor_"></a>
<a id="Vercel_AI_Operations_UnsupportedFunctionalityException__ctor_System_String_System_String_"></a>

### UnsupportedFunctionalityException(string, string)


```csharp
public UnsupportedFunctionalityException(string functionality, string message)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `functionality` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L170)

## Properties

<a id="Vercel_AI_Operations_UnsupportedFunctionalityException_Functionality_"></a>
<a id="Vercel_AI_Operations_UnsupportedFunctionalityException_Functionality"></a>

### Functionality


```csharp
public string Functionality { get; }
```


Short functionality name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L177)
