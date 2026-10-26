---
title: "ElevenLabsErrorData"
description: "Parsed ElevenLabs error object."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsErrorData"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public sealed class ElevenLabsErrorData
```


Parsed ElevenLabs `error` object.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L26)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData__ctor_System_String_System_Int32_"></a>

### ElevenLabsErrorData(string, int)


```csharp
public ElevenLabsErrorData(string message, int code)
```


Creates parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L29)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Code_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Code"></a>

### Code


```csharp
public int Code { get; }
```


Provider error code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L39)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Message_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Message"></a>

### Message


```csharp
public string Message { get; }
```


Provider message. Nested JSON is left as text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L36)
