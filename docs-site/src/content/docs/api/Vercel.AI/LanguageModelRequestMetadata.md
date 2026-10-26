---
title: "LanguageModelRequestMetadata"
description: "Metadata for a language model request. Maps to LanguageModelRequestMetadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelRequestMetadata"
  hidden: true
---

<a id="Vercel_AI_LanguageModelRequestMetadata"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class LanguageModelRequestMetadata
```


Metadata for a language model request. Maps to `LanguageModelRequestMetadata`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1023)

## Properties

<a id="Vercel_AI_LanguageModelRequestMetadata_Body_"></a>
<a id="Vercel_AI_LanguageModelRequestMetadata_Body"></a>

### Body


```csharp
public JsonElement? Body { get; init; }
```


Request HTTP body that was sent to the provider API.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1032)

<a id="Vercel_AI_LanguageModelRequestMetadata_Empty_"></a>
<a id="Vercel_AI_LanguageModelRequestMetadata_Empty"></a>

### Empty


```csharp
public static LanguageModelRequestMetadata Empty { get; }
```


Request metadata with no messages and no body.

**Value:** [LanguageModelRequestMetadata](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1026)

<a id="Vercel_AI_LanguageModelRequestMetadata_Messages_"></a>
<a id="Vercel_AI_LanguageModelRequestMetadata_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage>? Messages { get; init; }
```


The input messages that were sent to the model for this step.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1029)
