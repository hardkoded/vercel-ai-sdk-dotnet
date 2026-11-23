---
title: "ReasoningStartPart"
description: "A reasoning block started. Maps to reasoning-start."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningStartPart"
  hidden: true
---

<a id="Vercel_AI_ReasoningStartPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ReasoningStartPart : TextStreamPart
```


A reasoning block started. Maps to `reasoning-start`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2722)

## Constructors

<a id="Vercel_AI_ReasoningStartPart__ctor_"></a>
<a id="Vercel_AI_ReasoningStartPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningStartPart(string, JsonElement?)


```csharp
public ReasoningStartPart(string id, JsonElement? providerMetadata = null)
```


Creates a reasoning start.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2725)

## Properties

<a id="Vercel_AI_ReasoningStartPart_Id_"></a>
<a id="Vercel_AI_ReasoningStartPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Id of the reasoning block.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2733)

<a id="Vercel_AI_ReasoningStartPart_ProviderMetadata_"></a>
<a id="Vercel_AI_ReasoningStartPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the reasoning block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2736)
