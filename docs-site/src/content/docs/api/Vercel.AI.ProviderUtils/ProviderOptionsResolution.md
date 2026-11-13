---
title: "ProviderOptionsResolution"
description: "How options found under several providerOptions namespaces are combined. Maps to ProviderOptionsResolution."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderOptionsResolution"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderOptionsResolution"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public enum ProviderOptionsResolution
```


How options found under several `providerOptions` namespaces are combined. Maps to `ProviderOptionsResolution`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L12)

## Fields

<a id="Vercel_AI_ProviderUtils_ProviderOptionsResolution_First"></a>

### First


```csharp
First = 0
```


Namespaces are aliases. The first namespace that is present wins as a whole and later namespaces are
ignored (no merging).

**Value:** [ProviderOptionsResolution](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L18)

<a id="Vercel_AI_ProviderUtils_ProviderOptionsResolution_Merge"></a>

### Merge


```csharp
Merge = 1
```


Every present namespace is parsed and the results are merged in order, so later namespaces override
earlier ones key by key.

**Value:** [ProviderOptionsResolution](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderOptionsResolver.cs#L24)
