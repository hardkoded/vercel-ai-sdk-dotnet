---
title: "AnthropicCustomContent"
description: "A provider block the V4 content list has no dedicated type for."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicCustomContent"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicCustomContent"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicCustomContent : GeneratedContent
```


A provider block the V4 content list has no dedicated type for.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L39)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicCustomContent__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCustomContent__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### AnthropicCustomContent(string, JsonElement?)


```csharp
public AnthropicCustomContent(string kind, JsonElement? providerMetadata)
```


Creates custom content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `kind` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L42)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicCustomContent_Kind_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCustomContent_Kind"></a>

### Kind


```csharp
public string Kind { get; }
```


Custom kind, such as `anthropic.container_upload`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L50)

<a id="Vercel_AI_Anthropic_AnthropicCustomContent_ProviderMetadata_"></a>
<a id="Vercel_AI_Anthropic_AnthropicCustomContent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement? ProviderMetadata { get; }
```


Provider metadata for the custom block.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L53)
