---
title: "GeneratedReasoningFile"
description: "A file the model produced as part of its reasoning. Maps to the reasoning-file content part."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedReasoningFile"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedReasoningFile"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedReasoningFile : GeneratedContent
```


A file the model produced as part of its reasoning. Maps to the `reasoning-file` content part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L567)

## Constructors

<a id="Vercel_AI_Provider_GeneratedReasoningFile__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoningFile__ctor_System_Byte___System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedReasoningFile(byte[], string, JsonElement?)


```csharp
public GeneratedReasoningFile(byte[] data, string mediaType, JsonElement? providerMetadata = null)
```


Creates reasoning-file content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `data` | [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte) |  |
| `mediaType` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L570)

## Properties

<a id="Vercel_AI_Provider_GeneratedReasoningFile_Data_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoningFile_Data"></a>

### Data


```csharp
public byte[] Data { get; }
```


File bytes.

**Value:** [byte\[\]](https://learn.microsoft.com/dotnet/api/system.byte)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L583)

<a id="Vercel_AI_Provider_GeneratedReasoningFile_MediaType_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoningFile_MediaType"></a>

### MediaType


```csharp
public string MediaType { get; }
```


IANA media type.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L586)

<a id="Vercel_AI_Provider_GeneratedReasoningFile_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedReasoningFile_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this part, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L589)
