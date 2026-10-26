---
title: "OpenAIJsonSchemaResult"
description: "JSON Schema adjusted for OpenAI structured outputs."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIJsonSchemaResult"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIJsonSchemaResult"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIJsonSchemaResult
```


JSON Schema adjusted for OpenAI structured outputs.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L11)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIJsonSchemaResult_Schema_"></a>
<a id="Vercel_AI_OpenAI_OpenAIJsonSchemaResult_Schema"></a>

### Schema


```csharp
public JsonNode Schema { get; }
```


Schema with unsupported keywords removed.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L20)

<a id="Vercel_AI_OpenAI_OpenAIJsonSchemaResult_Warnings_"></a>
<a id="Vercel_AI_OpenAI_OpenAIJsonSchemaResult_Warnings"></a>

### Warnings


```csharp
public IReadOnlyList<OpenAICallWarning> Warnings { get; }
```


Warnings for keywords that were removed.

**Value:** [IReadOnlyList&lt;OpenAICallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAIJsonSchema.cs#L23)
