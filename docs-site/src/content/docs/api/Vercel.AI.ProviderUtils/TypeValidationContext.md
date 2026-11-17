---
title: "TypeValidationContext"
description: "Where a value failed type validation. Maps to TypeValidationContext."
editUrl: false
prev: false
next: false
sidebar:
  label: "TypeValidationContext"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_TypeValidationContext"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class TypeValidationContext
```


Where a value failed type validation. Maps to `TypeValidationContext`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L98)

## Properties

<a id="Vercel_AI_ProviderUtils_TypeValidationContext_EntityId_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationContext_EntityId"></a>

### EntityId


```csharp
public string? EntityId { get; init; }
```


Entity identifier, for example a message id or a tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L107)

<a id="Vercel_AI_ProviderUtils_TypeValidationContext_EntityName_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationContext_EntityName"></a>

### EntityName


```csharp
public string? EntityName { get; init; }
```


Entity name, for example a tool name or a data type name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L104)

<a id="Vercel_AI_ProviderUtils_TypeValidationContext_Field_"></a>
<a id="Vercel_AI_ProviderUtils_TypeValidationContext_Field"></a>

### Field


```csharp
public string? Field { get; init; }
```


Field path in dot notation, for example `message.parts[3].data`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/JsonParsing.cs#L101)
