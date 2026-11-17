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

**Derived:** [CustomContentPart](/api/Vercel.AI.Provider/CustomContentPart/), [FileContentPart](/api/Vercel.AI.Provider/FileContentPart/), [FileUrlContentPart](/api/Vercel.AI.Provider/FileUrlContentPart/), [ImageUrlContentPart](/api/Vercel.AI.Provider/ImageUrlContentPart/), [TextContentPart](/api/Vercel.AI.Provider/TextContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L760)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L763)

## Properties

<a id="Vercel_AI_Provider_UserContentPart_Type_"></a>
<a id="Vercel_AI_Provider_UserContentPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L769)
