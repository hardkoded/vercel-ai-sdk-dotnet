---
title: "SourceUrlUIPart"
description: "A source that is a URL. Maps to SourceUrlUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "SourceUrlUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_SourceUrlUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class SourceUrlUIPart : UIMessagePart
```


A source that is a URL. Maps to `SourceUrlUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L122)

## Constructors

<a id="Vercel_AI_Ui_SourceUrlUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_SourceUrlUIPart__ctor_System_String_System_String_"></a>

### SourceUrlUIPart(string, string)


```csharp
public SourceUrlUIPart(string sourceId, string url)
```


Creates a source URL part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `sourceId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L125)

## Properties

<a id="Vercel_AI_Ui_SourceUrlUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_SourceUrlUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L142)

<a id="Vercel_AI_Ui_SourceUrlUIPart_SourceId_"></a>
<a id="Vercel_AI_Ui_SourceUrlUIPart_SourceId"></a>

### SourceId


```csharp
public string SourceId { get; }
```


The source id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L133)

<a id="Vercel_AI_Ui_SourceUrlUIPart_Title_"></a>
<a id="Vercel_AI_Ui_SourceUrlUIPart_Title"></a>

### Title


```csharp
public string? Title { get; init; }
```


The title.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L139)

<a id="Vercel_AI_Ui_SourceUrlUIPart_Url_"></a>
<a id="Vercel_AI_Ui_SourceUrlUIPart_Url"></a>

### Url


```csharp
public string Url { get; }
```


The URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L136)
