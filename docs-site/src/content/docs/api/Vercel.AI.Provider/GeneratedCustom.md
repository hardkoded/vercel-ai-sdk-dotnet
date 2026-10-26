---
title: "GeneratedCustom"
description: "A provider-specific content part, identified by Kind. It carries no data of its own; the provider keeps it in ProviderMetadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "GeneratedCustom"
  hidden: true
---

<a id="Vercel_AI_Provider_GeneratedCustom"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class GeneratedCustom : GeneratedContent
```


A provider-specific content part, identified by [Kind](#Vercel_AI_Provider_GeneratedCustom_Kind). It carries no data of its own; the provider keeps it in
[ProviderMetadata](#Vercel_AI_Provider_GeneratedCustom_ProviderMetadata).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L438)

## Constructors

<a id="Vercel_AI_Provider_GeneratedCustom__ctor_"></a>
<a id="Vercel_AI_Provider_GeneratedCustom__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GeneratedCustom(string, JsonElement?)


```csharp
public GeneratedCustom(string kind, JsonElement? providerMetadata = null)
```


Creates a custom content part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L441)

## Properties

<a id="Vercel_AI_Provider_GeneratedCustom_Kind_"></a>
<a id="Vercel_AI_Provider_GeneratedCustom_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


Provider-scoped kind, such as `open-responses.extension-replay`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L449)

<a id="Vercel_AI_Provider_GeneratedCustom_ProviderMetadata_"></a>
<a id="Vercel_AI_Provider_GeneratedCustom_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for this part, keyed by provider name.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L452)
