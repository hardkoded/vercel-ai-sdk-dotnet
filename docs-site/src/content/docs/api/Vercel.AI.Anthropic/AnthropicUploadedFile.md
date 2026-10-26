---
title: "AnthropicUploadedFile"
description: "Files API upload result."
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


Files API upload result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L71)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### AnthropicUploadedFile(string, string?, JsonElement)


```csharp
public AnthropicUploadedFile(string id, string? fileName, JsonElement raw)
```


Creates an upload result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `fileName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `raw` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L74)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_FileName_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_FileName"></a>

### FileName


```csharp
public string? FileName { get; }
```


File name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L85)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Id_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Id"></a>

### Id


```csharp
public string Id { get; }
```


File id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L82)

<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Raw_"></a>
<a id="Vercel_AI_Anthropic_AnthropicUploadedFile_Raw"></a>

### Raw


```csharp
public JsonElement Raw { get; }
```


Raw response.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicFiles.cs#L88)
