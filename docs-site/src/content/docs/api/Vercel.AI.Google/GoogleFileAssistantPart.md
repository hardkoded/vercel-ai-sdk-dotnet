---
title: "GoogleFileAssistantPart"
description: "An assistant file or reasoning file."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleFileAssistantPart"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleFileAssistantPart"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleFileAssistantPart : GoogleAssistantPart
```


An assistant file or reasoning file.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GoogleAssistantPart](/api/Vercel.AI.Google/GoogleAssistantPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L203)

## Constructors

<a id="Vercel_AI_Google_GoogleFileAssistantPart__ctor_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart__ctor_System_String_System_String_System_Boolean_"></a>

### GoogleFileAssistantPart(string, string, bool)


```csharp
public GoogleFileAssistantPart(string mediaType, string kind, bool reasoning)
```


Creates a file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `reasoning` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L206)

## Properties

<a id="Vercel_AI_Google_GoogleFileAssistantPart_Data_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; set; }
```


Inline bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L223)

<a id="Vercel_AI_Google_GoogleFileAssistantPart_Kind_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


`url`, `data`, `text`, or `reference`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L217)

<a id="Vercel_AI_Google_GoogleFileAssistantPart_MediaType_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


Media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L214)

<a id="Vercel_AI_Google_GoogleFileAssistantPart_Reasoning_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_Reasoning"></a>

### Reasoning


```csharp
public bool Reasoning { get; }
```


True for a reasoning file.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L220)

<a id="Vercel_AI_Google_GoogleFileAssistantPart_Reference_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_Reference"></a>

### Reference


```csharp
public IReadOnlyDictionary<string, string>? Reference { get; set; }
```


Map from provider name to an uploaded file id. The `google` entry is the file URI.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L232)

<a id="Vercel_AI_Google_GoogleFileAssistantPart_Text_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Inline text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L226)

<a id="Vercel_AI_Google_GoogleFileAssistantPart_Url_"></a>
<a id="Vercel_AI_Google_GoogleFileAssistantPart_Url"></a>

### Url


```csharp
public string? Url { get; set; }
```


Remote URL.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleMessages.cs#L229)
