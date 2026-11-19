---
title: "ReasoningEndPart"
description: "A reasoning block ended. Maps to reasoning-end."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningEndPart"
  hidden: true
---

<a id="Vercel_AI_ReasoningEndPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ReasoningEndPart : TextStreamPart
```


A reasoning block ended. Maps to `reasoning-end`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2087)

## Constructors

<a id="Vercel_AI_ReasoningEndPart__ctor_"></a>
<a id="Vercel_AI_ReasoningEndPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningEndPart(string, JsonElement?)


```csharp
public ReasoningEndPart(string id, JsonElement? providerMetadata = null)
```


Creates a reasoning end.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2090)

## Properties

<a id="Vercel_AI_ReasoningEndPart_Id_"></a>
<a id="Vercel_AI_ReasoningEndPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Id of the reasoning block.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2098)

<a id="Vercel_AI_ReasoningEndPart_ProviderMetadata_"></a>
<a id="Vercel_AI_ReasoningEndPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the reasoning block, such as encrypted reasoning content.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2101)
