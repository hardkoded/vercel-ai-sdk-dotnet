---
title: "FileToolModelContentPart"
description: "An inline base64 file part inside a content tool output."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileToolModelContentPart"
  hidden: true
---

<a id="Vercel_AI_Prompt_FileToolModelContentPart"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public sealed class FileToolModelContentPart : ToolModelContentPart
```


An inline base64 file part inside a content tool output.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [ToolModelContentPart](/api/Vercel.AI.Prompt/ToolModelContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L126)

## Constructors

<a id="Vercel_AI_Prompt_FileToolModelContentPart__ctor_"></a>
<a id="Vercel_AI_Prompt_FileToolModelContentPart__ctor_System_String_System_String_System_String_"></a>

### FileToolModelContentPart(string, string, string?)


```csharp
public FileToolModelContentPart(string mediaType, string data, string? fileName = null)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `data` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L129)

## Properties

<a id="Vercel_AI_Prompt_FileToolModelContentPart_Data_"></a>
<a id="Vercel_AI_Prompt_FileToolModelContentPart_Data"></a>

### Data


```csharp
public FilePartInput Data { get; }
```


File data as a tagged `data` value.

**Value:** [FilePartInput](/api/Vercel.AI.Prompt/FilePartInput/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L138)

<a id="Vercel_AI_Prompt_FileToolModelContentPart_FileName_"></a>
<a id="Vercel_AI_Prompt_FileToolModelContentPart_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


Optional file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L144)

<a id="Vercel_AI_Prompt_FileToolModelContentPart_MediaType_"></a>
<a id="Vercel_AI_Prompt_FileToolModelContentPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type or top-level media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/CreateToolModelOutput.cs#L141)
