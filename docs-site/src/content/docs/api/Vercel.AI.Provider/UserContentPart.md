---
title: "UserContentPart"
description: "A user content part."
editUrl: false
prev: false
next: false
sidebar:
  label: "UserContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_UserContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class UserContentPart
```


A user content part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [CustomContentPart](/api/Vercel.AI.Provider/CustomContentPart/), [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/), [FileDataContentPart](/api/Vercel.AI.Provider/FileDataContentPart/), [FileIdContentPart](/api/Vercel.AI.Provider/FileIdContentPart/), [FileReferenceContentPart](/api/Vercel.AI.Provider/FileReferenceContentPart/), [FileUrlContentPart](/api/Vercel.AI.Provider/FileUrlContentPart/), [ImageDataContentPart](/api/Vercel.AI.Provider/ImageDataContentPart/), [ImageFileIdContentPart](/api/Vercel.AI.Provider/ImageFileIdContentPart/), [ImageFileReferenceContentPart](/api/Vercel.AI.Provider/ImageFileReferenceContentPart/), [ImageUrlContentPart](/api/Vercel.AI.Provider/ImageUrlContentPart/), [TextContentPart](/api/Vercel.AI.Provider/TextContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L800)

## Constructors

<a id="Vercel_AI_Provider_UserContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_UserContentPart__ctor_System_String_"></a>

### UserContentPart(string)


```csharp
protected UserContentPart(string type)
```


Creates a part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L803)

## Properties

<a id="Vercel_AI_Provider_UserContentPart_Type_"></a>
<a id="Vercel_AI_Provider_UserContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L809)
