---
title: "FileParts"
description: "File-part conversion. Maps to convertToLanguageModelV4FilePart."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileParts"
  hidden: true
---

<a id="Vercel_AI_Prompt_FileParts"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class FileParts
```


File-part conversion. Maps to `convertToLanguageModelV4FilePart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L153)

## Methods

<a id="Vercel_AI_Prompt_FileParts_ConvertToLanguageModelV4FilePart_"></a>
<a id="Vercel_AI_Prompt_FileParts_ConvertToLanguageModelV4FilePart_Vercel_AI_Prompt_FilePartInput_"></a>

### ConvertToLanguageModelV4FilePart(FilePartInput)


```csharp
public static LanguageModelFilePart ConvertToLanguageModelV4FilePart(FilePartInput content)
```


Converts a legacy or tagged file value into the v4 provider shape.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [FilePartInput](/api/Vercel.AI.Prompt/FilePartInput/) |  |

**Returns:** [LanguageModelFilePart](/api/Vercel.AI.Prompt/LanguageModelFilePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/FilePartData.cs#L156)
