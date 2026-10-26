---
title: "DeepgramErrorData"
description: "Parsed Deepgram error body."
editUrl: false
prev: false
next: false
sidebar:
  label: "DeepgramErrorData"
  hidden: true
---

<a id="Vercel_AI_Deepgram_DeepgramErrorData"></a>

Namespace: [Vercel.AI.Deepgram](/api/Vercel.AI.Deepgram/)


```csharp
public sealed class DeepgramErrorData
```


Parsed Deepgram error body.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L26)

## Constructors

<a id="Vercel_AI_Deepgram_DeepgramErrorData__ctor_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorData__ctor_System_String_System_String_System_String_"></a>

### DeepgramErrorData(string, string, string?)


```csharp
public DeepgramErrorData(string errCode, string errMsg, string? requestId)
```


Creates parsed error data.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `errCode` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `errMsg` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L29)

## Properties

<a id="Vercel_AI_Deepgram_DeepgramErrorData_ErrCode_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorData_ErrCode"></a>

### ErrCode


```csharp
public string ErrCode { get; }
```


Error code, such as `INVALID_AUTH`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L37)

<a id="Vercel_AI_Deepgram_DeepgramErrorData_ErrMsg_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorData_ErrMsg"></a>

### ErrMsg


```csharp
public string ErrMsg { get; }
```


Error message.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L40)

<a id="Vercel_AI_Deepgram_DeepgramErrorData_RequestId_"></a>
<a id="Vercel_AI_Deepgram_DeepgramErrorData_RequestId"></a>

### RequestId


```csharp
public string? RequestId { get; }
```


Request id, when Deepgram sends one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Deepgram/DeepgramProvider.cs#L43)
