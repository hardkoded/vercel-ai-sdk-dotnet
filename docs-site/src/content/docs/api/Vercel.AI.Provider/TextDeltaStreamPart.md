---
title: "TextDeltaStreamPart"
description: "A text delta."
editUrl: false
prev: false
next: false
sidebar:
  label: "TextDeltaStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_TextDeltaStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class TextDeltaStreamPart : LanguageModelStreamPart
```


A text delta.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1660)

## Constructors

<a id="Vercel_AI_Provider_TextDeltaStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_TextDeltaStreamPart__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### TextDeltaStreamPart(string, string, JsonElement?)


```csharp
public TextDeltaStreamPart(string id, string delta, JsonElement? providerMetadata = null)
```


Creates a text delta.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `delta` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1663)

## Properties

<a id="Vercel_AI_Provider_TextDeltaStreamPart_Delta_"></a>
<a id="Vercel_AI_Provider_TextDeltaStreamPart_Delta"></a>

### Delta


```csharp
public string Delta { get; }
```


Text fragment.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1675)

<a id="Vercel_AI_Provider_TextDeltaStreamPart_Id_"></a>
<a id="Vercel_AI_Provider_TextDeltaStreamPart_Id"></a>

### Id


```csharp
public string Id { get; }
```


Text block id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1672)

<a id="Vercel_AI_Provider_TextDeltaStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_TextDeltaStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this delta.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L1678)
