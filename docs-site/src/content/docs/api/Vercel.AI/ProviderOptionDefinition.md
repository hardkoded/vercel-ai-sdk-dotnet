---
title: "ProviderOptionDefinition"
description: "A provider option, declared once at the provider level. Upstream ProviderOptionDefinition."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderOptionDefinition"
  hidden: true
---

<a id="Vercel_AI_ProviderOptionDefinition"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class ProviderOptionDefinition
```


A provider option, declared once at the provider level. Upstream `ProviderOptionDefinition`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L47)

## Constructors

<a id="Vercel_AI_ProviderOptionDefinition__ctor_"></a>
<a id="Vercel_AI_ProviderOptionDefinition__ctor_System_String_"></a>

### ProviderOptionDefinition(string)


```csharp
public ProviderOptionDefinition(string description)
```


Creates a definition.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L50)

## Properties

<a id="Vercel_AI_ProviderOptionDefinition_Description_"></a>
<a id="Vercel_AI_ProviderOptionDefinition_Description"></a>

### Description


```csharp
public string Description { get; }
```


What the option does.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/ModelCapability.cs#L56)
