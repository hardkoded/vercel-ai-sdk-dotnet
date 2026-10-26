---
title: "RevAIErrorData"
description: "Parsed Rev.ai error object."
editUrl: false
prev: false
next: false
sidebar:
  label: "RevAIErrorData"
  hidden: true
---

<a id="Vercel_AI_RevAI_RevAIErrorData"></a>

Namespace: [Vercel.AI.RevAI](/api/Vercel.AI.RevAI/)


```csharp
public sealed class RevAIErrorData
```


Parsed Rev.ai `error` object.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L27)

## Constructors

<a id="Vercel_AI_RevAI_RevAIErrorData__ctor_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorData__ctor_System_String_System_Int32_"></a>

### RevAIErrorData(string, int)


```csharp
public RevAIErrorData(string message, int code)
```


Creates parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L30)

## Properties

<a id="Vercel_AI_RevAI_RevAIErrorData_Code_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorData_Code"></a>

### Code


```csharp
public int Code { get; }
```


Provider error code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L40)

<a id="Vercel_AI_RevAI_RevAIErrorData_Message_"></a>
<a id="Vercel_AI_RevAI_RevAIErrorData_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider message. Nested JSON is left as text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/RevAI/RevAIProvider.cs#L37)
