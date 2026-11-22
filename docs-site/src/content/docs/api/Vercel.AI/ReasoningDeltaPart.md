---
title: "ReasoningDeltaPart"
description: "Reasoning delta. Maps to reasoning-delta."
editUrl: false
prev: false
next: false
sidebar:
  label: "ReasoningDeltaPart"
  hidden: true
---

<a id="Vercel_AI_ReasoningDeltaPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ReasoningDeltaPart : TextStreamPart
```


Reasoning delta. Maps to `reasoning-delta`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2708)

## Constructors

<a id="Vercel_AI_ReasoningDeltaPart__ctor_"></a>
<a id="Vercel_AI_ReasoningDeltaPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ReasoningDeltaPart(string, string, JsonElement?)


```csharp
public ReasoningDeltaPart(string id, string text, JsonElement? providerMetadata = null)
```


Creates a reasoning delta.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2711)

## Properties

<a id="Vercel_AI_ReasoningDeltaPart_Id_"></a>
<a id="Vercel_AI_ReasoningDeltaPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Id of the reasoning block.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2720)

<a id="Vercel_AI_ReasoningDeltaPart_ProviderMetadata_"></a>
<a id="Vercel_AI_ReasoningDeltaPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata of the delta, such as a reasoning signature.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2726)

<a id="Vercel_AI_ReasoningDeltaPart_Text_"></a>
<a id="Vercel_AI_ReasoningDeltaPart_Text"></a>

### Text


```csharp
public string Text { get; }
```


Delta text.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2723)
