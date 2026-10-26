---
title: "ToolInputDeltaPart"
description: "A streamed tool-input fragment."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolInputDeltaPart"
  hidden: true
---

<a id="Vercel_AI_ToolInputDeltaPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ToolInputDeltaPart : TextStreamPart
```


A streamed tool-input fragment.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1248)

## Constructors

<a id="Vercel_AI_ToolInputDeltaPart__ctor_"></a>
<a id="Vercel_AI_ToolInputDeltaPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### ToolInputDeltaPart(string, string, JsonElement?)


```csharp
public ToolInputDeltaPart(string id, string delta, JsonElement? providerMetadata)
```


Creates a tool-input-delta part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `delta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1251)

## Properties

<a id="Vercel_AI_ToolInputDeltaPart_Delta_"></a>
<a id="Vercel_AI_ToolInputDeltaPart_Delta"></a>

### Delta


```csharp
public string Delta { get; }
```


JSON fragment.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1263)

<a id="Vercel_AI_ToolInputDeltaPart_Id_"></a>
<a id="Vercel_AI_ToolInputDeltaPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1260)

<a id="Vercel_AI_ToolInputDeltaPart_ProviderMetadata_"></a>
<a id="Vercel_AI_ToolInputDeltaPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this fragment.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1266)
