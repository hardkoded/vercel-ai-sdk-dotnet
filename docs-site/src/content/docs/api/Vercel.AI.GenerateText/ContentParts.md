---
title: "ContentParts"
description: "Reads text and reasoning out of generated content. Maps to extractTextContent and extractReasoningContent."
editUrl: false
prev: false
next: false
sidebar:
  label: "ContentParts"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ContentParts"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public static class ContentParts
```


Reads text and reasoning out of generated content. Maps to `extractTextContent` and `extractReasoningContent`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ExtractContent.cs#L10)

## Methods

<a id="Vercel_AI_GenerateText_ContentParts_ExtractReasoningContent_"></a>
<a id="Vercel_AI_GenerateText_ContentParts_ExtractReasoningContent_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__"></a>

### `ExtractReasoningContent(IReadOnlyList<GeneratedContent>?)`


```csharp
public static string? ExtractReasoningContent(IReadOnlyList<GeneratedContent>? content)
```


Joins reasoning parts with newlines. Returns null when there are none.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ExtractContent.cs#L33)

<a id="Vercel_AI_GenerateText_ContentParts_ExtractTextContent_"></a>
<a id="Vercel_AI_GenerateText_ContentParts_ExtractTextContent_System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_GeneratedContent__"></a>

### `ExtractTextContent(IReadOnlyList<GeneratedContent>?)`


```csharp
public static string? ExtractTextContent(IReadOnlyList<GeneratedContent>? content)
```


Joins text parts. Returns null when there are none.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [IReadOnlyList&lt;GeneratedContent&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ExtractContent.cs#L13)
