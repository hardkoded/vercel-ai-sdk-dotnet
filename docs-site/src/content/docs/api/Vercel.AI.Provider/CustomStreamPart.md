---
title: "CustomStreamPart"
description: "A provider-specific part, identified by Kind."
editUrl: false
prev: false
next: false
sidebar:
  label: "CustomStreamPart"
  hidden: true
---

<a id="Vercel_AI_Provider_CustomStreamPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class CustomStreamPart : LanguageModelStreamPart
```


A provider-specific part, identified by [Kind](#Vercel_AI_Provider_CustomStreamPart_Kind).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2126)

## Constructors

<a id="Vercel_AI_Provider_CustomStreamPart__ctor_"></a>
<a id="Vercel_AI_Provider_CustomStreamPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### CustomStreamPart(string, JsonElement?)


```csharp
public CustomStreamPart(string kind, JsonElement? providerMetadata = null)
```


Creates a custom part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2129)

## Properties

<a id="Vercel_AI_Provider_CustomStreamPart_Kind_"></a>
<a id="Vercel_AI_Provider_CustomStreamPart_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


Provider-scoped kind, such as `anthropic.message_start`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2137)

<a id="Vercel_AI_Provider_CustomStreamPart_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_CustomStreamPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this part, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2140)
