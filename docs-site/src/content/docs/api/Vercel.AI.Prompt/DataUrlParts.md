---
title: "DataUrlParts"
description: "Media type and base64 payload from a data URL. Maps to splitDataUrl."
editUrl: false
prev: false
next: false
sidebar:
  label: "DataUrlParts"
  hidden: true
---

<a id="Vercel_AI_Prompt_DataUrlParts"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class DataUrlParts
```


Media type and base64 payload from a data URL. Maps to `splitDataUrl`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/SplitDataUrl.cs#L8)

## Constructors

<a id="Vercel_AI_Prompt_DataUrlParts__ctor_"></a>
<a id="Vercel_AI_Prompt_DataUrlParts__ctor_System_String_System_String_"></a>

### DataUrlParts(string?, string?)


```csharp
public DataUrlParts(string? mediaType, string? base64Content)
```


Creates parts.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `base64Content` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/SplitDataUrl.cs#L11)

## Properties

<a id="Vercel_AI_Prompt_DataUrlParts_Base64Content_"></a>
<a id="Vercel_AI_Prompt_DataUrlParts_Base64Content"></a>

### Base64Content


```csharp
public string? Base64Content { get; }
```


Base64 payload, when a comma was present.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/SplitDataUrl.cs#L21)

<a id="Vercel_AI_Prompt_DataUrlParts_MediaType_"></a>
<a id="Vercel_AI_Prompt_DataUrlParts_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Media type, when the header contained one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/SplitDataUrl.cs#L18)
