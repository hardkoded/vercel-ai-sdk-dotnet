---
title: "ElevenLabsErrorData"
description: "Parsed ElevenLabs error body."
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


Parsed ElevenLabs error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L144)

## Constructors

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData__ctor_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData__ctor_System_String_System_Int32_"></a>

### ElevenLabsErrorData(string, int)


```csharp
public ElevenLabsErrorData(string message, int code)
```


Creates legacy parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `message` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `code` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L147)

## Properties

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Code_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Code"></a>

### Code


```csharp
[Obsolete("Use Error.Code.")]
public int Code { get; }
```


Legacy integral provider code. Use [Error](#Vercel_AI_ElevenLabs_ElevenLabsErrorData_Error) for the upstream numeric code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L180)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Detail_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Detail"></a>

### Detail


```csharp
public ElevenLabsErrorDetail? Detail { get; }
```


Object-valued `detail`, when present.

**Value:** [ElevenLabsErrorDetail](/api/Vercel.AI.ElevenLabs/ElevenLabsErrorDetail/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L164)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_DetailMessage_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_DetailMessage"></a>

### DetailMessage


```csharp
public string? DetailMessage { get; }
```


String-valued `detail`, when present.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L170)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Error_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Error"></a>

### Error


```csharp
public ElevenLabsLegacyError? Error { get; }
```


Legacy `error` object, when present.

**Value:** [ElevenLabsLegacyError](/api/Vercel.AI.ElevenLabs/ElevenLabsLegacyError/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L173)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Issues_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Issues"></a>

### Issues


```csharp
public IReadOnlyList<ElevenLabsValidationIssue>? Issues { get; }
```


Array-valued `detail`, when present.

**Value:** [IReadOnlyList&lt;ElevenLabsValidationIssue&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L167)

<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Message_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsErrorData_Message"></a>

### Message


```csharp
[Obsolete("Use Error.Message.")]
public string Message { get; }
```


Legacy provider message. Use [Error](#Vercel_AI_ElevenLabs_ElevenLabsErrorData_Error) for parsed legacy bodies.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

:::caution[Obsolete]
This API is obsolete.
:::

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L176)
