---
title: "AnthropicFileDownload"
description: "Downloaded Anthropic file bytes."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicFileDownload"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicFileDownload"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicFileDownload
```


Downloaded Anthropic file bytes.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L438)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicFileDownload__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDownload__ctor_System_IO_Stream_System_String_"></a>

### AnthropicFileDownload(Stream, string?)


```csharp
public AnthropicFileDownload(Stream content, string? mediaType)
```


Creates a download.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `content` | [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L441)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicFileDownload_Content_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDownload_Content"></a>

### Content


```csharp
public Stream Content { get; }
```


File contents.

**Value:** [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L451)

<a id="Vercel_AI_Anthropic_AnthropicFileDownload_MediaType_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDownload_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


Content type with parameters removed. Null when the response has none.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L454)

<a id="Vercel_AI_Anthropic_AnthropicFileDownload_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicFileDownload_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Always empty.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L448)
