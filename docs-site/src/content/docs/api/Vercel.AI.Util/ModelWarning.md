---
title: "ModelWarning"
description: "A warning returned by a model provider."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelWarning"
  hidden: true
---

<a id="Vercel_AI_Util_ModelWarning"></a>

Namespace: [Vercel.AI.Util](/api/Vercel.AI.Util/)


```csharp
public abstract class ModelWarning
```


A warning returned by a model provider.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [CompatibilityWarning](/api/Vercel.AI.Util/CompatibilityWarning/), [DeprecatedWarning](/api/Vercel.AI.Util/DeprecatedWarning/), [OtherWarning](/api/Vercel.AI.Util/OtherWarning/), [UnsupportedWarning](/api/Vercel.AI.Util/UnsupportedWarning/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L10)

## Properties

<a id="Vercel_AI_Util_ModelWarning_Type_"></a>
<a id="Vercel_AI_Util_ModelWarning_Type"></a>

### Type


```csharp
public abstract string Type { get; }
```


Warning kind: `unsupported`, `compatibility`, `deprecated`, or `other`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Util/LogWarnings.cs#L13)
