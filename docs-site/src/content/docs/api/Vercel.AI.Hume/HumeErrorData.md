---
title: "HumeErrorData"
description: "Parsed Hume error payload."
editUrl: false
prev: false
next: false
sidebar:
  label: "HumeErrorData"
  hidden: true
---

<a id="Vercel_AI_Hume_HumeErrorData"></a>

Namespace: [Vercel.AI.Hume](/api/Vercel.AI.Hume/)


```csharp
public sealed class HumeErrorData
```


Parsed Hume error payload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L21)

## Constructors

<a id="Vercel_AI_Hume_HumeErrorData__ctor_"></a>
<a id="Vercel_AI_Hume_HumeErrorData__ctor_System_String_System_Int32_"></a>

### HumeErrorData(string, int)


```csharp
public HumeErrorData(string message, int code)
```


Creates parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L24)

## Properties

<a id="Vercel_AI_Hume_HumeErrorData_Code_"></a>
<a id="Vercel_AI_Hume_HumeErrorData_Code"></a>

### Code


```csharp
public int Code { get; }
```


Provider code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L34)

<a id="Vercel_AI_Hume_HumeErrorData_Message_"></a>
<a id="Vercel_AI_Hume_HumeErrorData_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Hume/HumeProvider.cs#L31)
