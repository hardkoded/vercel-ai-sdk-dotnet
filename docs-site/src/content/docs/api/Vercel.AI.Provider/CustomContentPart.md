---
title: "CustomContentPart"
description: "A custom content part inside a tool result, used to carry provider-specific content. Maps to { type: 'custom', providerOptions } in the tool result content o..."
editUrl: false
prev: false
next: false
sidebar:
  label: "CustomContentPart"
  hidden: true
---

<a id="Vercel_AI_Provider_CustomContentPart"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class CustomContentPart : UserContentPart
```


A custom content part inside a tool result, used to carry provider-specific content.
Maps to `{ type: 'custom', providerOptions }` in the tool result content of `LanguageModelV4Prompt`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UserContentPart](/api/Vercel.AI.Provider/UserContentPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L928)

## Constructors

<a id="Vercel_AI_Provider_CustomContentPart__ctor_"></a>
<a id="Vercel_AI_Provider_CustomContentPart__ctor"></a>

### CustomContentPart()


```csharp
public CustomContentPart()
```


Creates a custom part.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L931)

## Properties

<a id="Vercel_AI_Provider_CustomContentPart_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_CustomContentPart_ProviderOptions"></a>

### ProviderOptions


```csharp
public IReadOnlyDictionary<string, JsonElement>? ProviderOptions { get; init; }
```


Provider-specific options for this part.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L935)
