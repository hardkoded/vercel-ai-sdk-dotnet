---
title: "ReasoningFileUIPart"
description: "A file produced as reasoning. Maps to ReasoningFileUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningFileUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_ReasoningFileUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class ReasoningFileUIPart : UIMessagePart
```


A file produced as reasoning. Maps to `ReasoningFileUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L201)

## Constructors

<a id="Vercel_AI_Ui_ReasoningFileUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_ReasoningFileUIPart__ctor_System_String_System_String_"></a>

### ReasoningFileUIPart(string, string)


```csharp
public ReasoningFileUIPart(string mediaType, string url)
```


Creates a reasoning file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L204)

## Properties

<a id="Vercel_AI_Ui_ReasoningFileUIPart_MediaType_"></a>
<a id="Vercel_AI_Ui_ReasoningFileUIPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L212)

<a id="Vercel_AI_Ui_ReasoningFileUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_ReasoningFileUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L218)

<a id="Vercel_AI_Ui_ReasoningFileUIPart_Url_"></a>
<a id="Vercel_AI_Ui_ReasoningFileUIPart_Url"></a>

### Url


```csharp
public string Url { get; }
```


The URL, which can be a data URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L215)
