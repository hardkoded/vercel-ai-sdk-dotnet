---
title: "FileUIPart"
description: "A file part. Maps to FileUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "FileUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_FileUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class FileUIPart : UIMessagePart
```


A file part. Maps to `FileUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L174)

## Constructors

<a id="Vercel_AI_Ui_FileUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_FileUIPart__ctor_System_String_System_String_"></a>

### FileUIPart(string, string)


```csharp
public FileUIPart(string mediaType, string url)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L177)

## Properties

<a id="Vercel_AI_Ui_FileUIPart_Filename_"></a>
<a id="Vercel_AI_Ui_FileUIPart_Filename"></a>

### Filename


```csharp
public string? Filename { get; init; }
```


The file name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L188)

<a id="Vercel_AI_Ui_FileUIPart_MediaType_"></a>
<a id="Vercel_AI_Ui_FileUIPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L185)

<a id="Vercel_AI_Ui_FileUIPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Ui_FileUIPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderMetadata { get; init; }
```


The provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L197)

<a id="Vercel_AI_Ui_FileUIPart_ProviderReference_"></a>
<a id="Vercel_AI_Ui_FileUIPart_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string>? ProviderReference { get; init; }
```


Map from provider name to an uploaded file id. It takes precedence over [Url](#Vercel_AI_Ui_FileUIPart_Url).

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L194)

<a id="Vercel_AI_Ui_FileUIPart_Url_"></a>
<a id="Vercel_AI_Ui_FileUIPart_Url"></a>

### Url


```csharp
public string Url { get; }
```


The URL, which can be a data URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L191)
