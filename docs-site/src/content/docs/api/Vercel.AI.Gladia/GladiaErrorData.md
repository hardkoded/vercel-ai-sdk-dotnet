---
title: "GladiaErrorData"
description: "Parsed Gladia error payload."
editUrl: false
prev: false
next: false
sidebar:
  label: "GladiaErrorData"
  hidden: true
---

<a id="Vercel_AI_Gladia_GladiaErrorData"></a>

Namespace: [Vercel.AI.Gladia](/api/Vercel.AI.Gladia/)


```csharp
public sealed class GladiaErrorData
```


Parsed Gladia error payload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L26)

## Constructors

<a id="Vercel_AI_Gladia_GladiaErrorData__ctor_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorData__ctor_System_String_System_Int32_"></a>

### GladiaErrorData(string, int)


```csharp
public GladiaErrorData(string message, int code)
```


Creates parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L29)

## Properties

<a id="Vercel_AI_Gladia_GladiaErrorData_Code_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorData_Code"></a>

### Code


```csharp
public int Code { get; }
```


Provider code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L39)

<a id="Vercel_AI_Gladia_GladiaErrorData_Message_"></a>
<a id="Vercel_AI_Gladia_GladiaErrorData_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gladia/GladiaProvider.cs#L36)
