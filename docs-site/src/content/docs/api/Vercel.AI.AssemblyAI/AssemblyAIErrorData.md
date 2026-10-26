---
title: "AssemblyAIErrorData"
description: "Parsed AssemblyAI error object."
editUrl: false
prev: false
next: false
sidebar:
  label: "AssemblyAIErrorData"
  hidden: true
---

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData"></a>

Namespace: [Vercel.AI.AssemblyAI](/api/Vercel.AI.AssemblyAI/)


```csharp
public sealed class AssemblyAIErrorData
```


Parsed AssemblyAI `error` object.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L28)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData__ctor_System_String_System_Int32_"></a>

### AssemblyAIErrorData(string, int)


```csharp
public AssemblyAIErrorData(string message, int code)
```


Creates parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L31)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Code_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Code"></a>

### Code


```csharp
public int Code { get; }
```


Provider error code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L41)

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Message_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider message. Nested JSON is left as text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L38)
