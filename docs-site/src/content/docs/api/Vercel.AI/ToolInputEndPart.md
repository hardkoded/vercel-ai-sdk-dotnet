---
title: "ToolInputEndPart"
description: "The model finished streaming tool input."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputEndPart"
  hidden: true
---

<a id="Vercel_AI_ToolInputEndPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolInputEndPart : TextStreamPart
```


The model finished streaming tool input.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1270)

## Constructors

<a id="Vercel_AI_ToolInputEndPart__ctor_"></a>
<a id="Vercel_AI_ToolInputEndPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolInputEndPart(string, JsonElement?)


```csharp
public ToolInputEndPart(string id, JsonElement? providerMetadata)
```


Creates a tool-input-end part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1273)

## Properties

<a id="Vercel_AI_ToolInputEndPart_Id_"></a>
<a id="Vercel_AI_ToolInputEndPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1281)

<a id="Vercel_AI_ToolInputEndPart_ProviderMetadata_"></a>
<a id="Vercel_AI_ToolInputEndPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this tool input.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1284)
