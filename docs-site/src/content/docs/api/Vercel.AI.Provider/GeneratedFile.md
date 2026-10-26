---
title: "GeneratedFile"
description: "A generated file."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedFile"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedFile"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public class GeneratedFile : GeneratedContent
```


A generated file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

**Derived:** [GoogleGeneratedFile](/api/Vercel.AI.Google/GoogleGeneratedFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L456)

## Constructors

<a id="Vercel_AI_Provider_GeneratedFile__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedFile__ctor_System_Byte___System_String_"></a>

### GeneratedFile(byte[], string)


```csharp
public GeneratedFile(byte[] data, string mediaType)
```


Creates file content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L459)

## Properties

<a id="Vercel_AI_Provider_GeneratedFile_Data_"></a>
<a id="Vercel_AI_Provider_GeneratedFile_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


File bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L467)

<a id="Vercel_AI_Provider_GeneratedFile_MediaType_"></a>
<a id="Vercel_AI_Provider_GeneratedFile_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L470)
