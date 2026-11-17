---
title: "AssemblyAIErrorData"
description: "Parsed AssemblyAI error. AssemblyAI documents a flat { \"error\": \"Invalid API key\" } body for 4xx and 5xx responses. The nested { \"error\": { \"message\": \"...\",..."
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


Parsed AssemblyAI error. AssemblyAI documents a flat `{ "error": "Invalid API key" }` body for 4xx and 5xx responses.
The nested `{ "error": { "message": "...", "code": 429 } }` body is still read.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L31)

## Constructors

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData__ctor_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData__ctor_System_String_"></a>

### AssemblyAIErrorData(string)


```csharp
public AssemblyAIErrorData(string message)
```


Creates parsed error data from the flat shape. [Code](#Vercel_AI_AssemblyAI_AssemblyAIErrorData_Code) is null.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L34)

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData__ctor_System_String_System_Int32_"></a>

### AssemblyAIErrorData(string, int)


```csharp
public AssemblyAIErrorData(string message, int code)
```


Creates parsed error data from the nested shape.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L40)

## Properties

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Code_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Code"></a>

### Code


```csharp
public int? Code { get; }
```


Provider error code. Null for the flat shape, which has none.

**Value:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L50)

<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Message_"></a>
<a id="Vercel_AI_AssemblyAI_AssemblyAIErrorData_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider message. Nested JSON is left as text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AssemblyAI/AssemblyAIProvider.cs#L47)
