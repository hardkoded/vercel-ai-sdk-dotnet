---
title: "AnthropicReasoningContent"
description: "Reasoning content with a thinking signature or redacted payload."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicReasoningContent"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicReasoningContent"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicReasoningContent : GeneratedContent
```


Reasoning content with a thinking signature or redacted payload.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L21)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicReasoningContent__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningContent__ctor_System_String_System_Text_Json_JsonElement_"></a>

### AnthropicReasoningContent(string, JsonElement)


```csharp
public AnthropicReasoningContent(string text, JsonElement providerMetadata)
```


Creates reasoning content.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `text` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L24)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicReasoningContent_ProviderMetadata_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningContent_ProviderMetadata"></a>

### ProviderMetadata


```csharp
public JsonElement ProviderMetadata { get; }
```


Signature or redacted data.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L35)

<a id="Vercel_AI_Anthropic_AnthropicReasoningContent_Text_"></a>
<a id="Vercel_AI_Anthropic_AnthropicReasoningContent_Text"></a>

### Text


```csharp
public string Text { get; }
```


Thinking text. Empty for redacted thinking.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicContent.cs#L32)
