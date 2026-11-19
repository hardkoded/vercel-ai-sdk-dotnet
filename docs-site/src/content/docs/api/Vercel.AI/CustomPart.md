---
title: "CustomPart"
description: "Content that only the provider understands, such as a compaction. Maps to the custom stream part."
editUrl: false
prev: false
next: false
sidebar:
  label: "CustomPart"
  hidden: true
---

<a id="Vercel_AI_CustomPart"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class CustomPart : TextStreamPart
```


Content that only the provider understands, such as a compaction. Maps to the `custom` stream part.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [TextStreamPart](/api/Vercel.AI/TextStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2203)

## Constructors

<a id="Vercel_AI_CustomPart__ctor_"></a>
<a id="Vercel_AI_CustomPart__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### CustomPart(string, JsonElement?)


```csharp
public CustomPart(string kind, JsonElement? providerMetadata = null)
```


Creates a custom part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2206)

## Properties

<a id="Vercel_AI_CustomPart_Kind_"></a>
<a id="Vercel_AI_CustomPart_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


The kind of the content, in the form `{provider}.{provider-type}`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2214)

<a id="Vercel_AI_CustomPart_ProviderMetadata_"></a>
<a id="Vercel_AI_CustomPart_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L2217)
