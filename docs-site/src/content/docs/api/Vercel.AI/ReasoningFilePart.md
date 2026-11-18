---
title: "ReasoningFilePart"
description: "A file the model generated as part of its reasoning. A URL has been downloaded by the time this part is emitted. Maps to the reasoning-file stream part."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningFilePart"
  hidden: true
---

<a id="Vercel_AI_ReasoningFilePart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ReasoningFilePart : TextStreamPart
```


A file the model generated as part of its reasoning. A URL has been downloaded by the time this part is emitted. Maps to the `reasoning-file` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2146)

## Constructors

<a id="Vercel_AI_ReasoningFilePart__ctor_"></a>
<a id="Vercel_AI_ReasoningFilePart__ctor_Vercel_AI_Provider_GeneratedFile_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningFilePart(GeneratedFile, JsonElement?)


```csharp
public ReasoningFilePart(GeneratedFile file, JsonElement? providerMetadata = null)
```


Creates a reasoning-file part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `file` | [GeneratedFile](/api/Vercel.AI.Provider/GeneratedFile/) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2149)

## Properties

<a id="Vercel_AI_ReasoningFilePart_File_"></a>
<a id="Vercel_AI_ReasoningFilePart_File"></a>

### File


```csharp
public GeneratedFile File { get; }
```


The file, with its bytes.

**Value:** [GeneratedFile](/api/Vercel.AI.Provider/GeneratedFile/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2157)

<a id="Vercel_AI_ReasoningFilePart_ProviderMetadata_"></a>
<a id="Vercel_AI_ReasoningFilePart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2160)
