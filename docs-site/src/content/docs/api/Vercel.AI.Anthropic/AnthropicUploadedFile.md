---
title: "AnthropicUploadedFile"
description: "Files API upload or metadata result."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicUploadedFile"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicUploadedFile
```


Files API upload or metadata result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L323)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### AnthropicUploadedFile(string, string?, JsonElement)


```csharp
public AnthropicUploadedFile(string id, string? fileName, JsonElement raw)
```


Creates a result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L328)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile__ctor_System_String_System_String_System_Text_Json_JsonElement_System_String_"></a>

### AnthropicUploadedFile(string, string?, JsonElement, string?)


```csharp
public AnthropicUploadedFile(string id, string? fileName, JsonElement raw, string? uploadedMediaType)
```


Creates a result. The uploaded media type is the fallback for an upload response.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `uploadedMediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L332)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ByteSize_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ByteSize"></a>

### ByteSize


```csharp
public long? ByteSize { get; }
```


`size_bytes`.

**Value:** [long?](https://learn.microsoft.com/dotnet/api/system.int64)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L362)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_CreatedAt_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_CreatedAt"></a>

### CreatedAt


```csharp
public DateTimeOffset? CreatedAt { get; }
```


`created_at`.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L370)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ExpiresAt_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ExpiresAt"></a>

### ExpiresAt


```csharp
public DateTimeOffset? ExpiresAt { get; }
```


`expires_at`.

**Value:** [DateTimeOffset?](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L373)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_FileName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


File name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L349)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Id_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Id"></a>

### Id


```csharp
public string Id { get; }
```


File id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L346)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_MediaType_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_MediaType"></a>

### MediaType


```csharp
public string? MediaType { get; }
```


`mime_type`, or the uploaded media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L359)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ProviderMetadata_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> ProviderMetadata { get; }
```


Metadata stored under `providerMetadata.anthropic`. Fields the response left out or set to null are omitted.

**Value:** [IReadOnlyDictionary&lt;string, IReadOnlyDictionary&lt;string, object&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L379)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ProviderReference_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_ProviderReference"></a>

### ProviderReference


```csharp
public IReadOnlyDictionary<string, string> ProviderReference { get; }
```


Reference containing the `anthropic` file id.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L355)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Raw_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Raw"></a>

### Raw


```csharp
public JsonElement Raw { get; }
```


Raw response.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L352)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Warnings_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<ModelWarning> Warnings { get; }
```


Always empty.

**Value:** [IReadOnlyList&lt;ModelWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L376)
