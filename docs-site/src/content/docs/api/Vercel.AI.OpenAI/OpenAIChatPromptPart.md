---
title: "OpenAIChatPromptPart"
description: "One user or assistant content part."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIChatPromptPart"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIChatPromptPart
```


One user or assistant content part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L51)

## Constructors

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart__ctor_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart__ctor_System_String_"></a>

### OpenAIChatPromptPart(string)


```csharp
public OpenAIChatPromptPart(string type)
```


Creates a part. &lt;code class="paramref"&gt;type&lt;/code&gt; is `text` or `file`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L54)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Base64_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Base64"></a>

### Base64


```csharp
public string? Base64 { get; set; }
```


Base64 payload used as-is when [Data](#Vercel_AI_OpenAI_OpenAIChatPromptPart_Data) is null.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L75)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Data_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Data"></a>

### Data


```csharp
public byte[]? Data { get; set; }
```


Inline file bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L72)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_FileId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_FileId"></a>

### FileId


```csharp
public string? FileId { get; set; }
```


OpenAI file id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L78)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_FileName_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_FileName"></a>

### FileName


```csharp
public string? FileName { get; set; }
```


File name for PDF data.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L84)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_MediaType_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; set; }
```


IANA media type for a file part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L66)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_ProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options for this part.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L87)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_ProviderReference_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string>? ProviderReference { get; set; }
```


File ids by provider. The `openai` entry is sent as the file id; a missing entry throws.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L81)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Text_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Text"></a>

### Text


```csharp
public string? Text { get; set; }
```


Text, when [Type](#Vercel_AI_OpenAI_OpenAIChatPromptPart_Type) is `text`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L63)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Type"></a>

### Type


```csharp
public string Type { get; }
```


Part type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L60)

<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Url_"></a>
<a id="Vercel_AI_OpenAI_OpenAIChatPromptPart_Url"></a>

### Url


```csharp
public string? Url { get; set; }
```


Remote URL for a file part.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Chat/OpenAIChatMessages.cs#L69)
