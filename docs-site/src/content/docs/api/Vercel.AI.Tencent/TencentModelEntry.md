---
title: "TencentModelEntry"
description: "One catalog record of a model id, holding the operations it declares. Upstream ModelEntry in tencentModelCatalog."
editUrl: false
prev: false
next: false
sidebar:
  label: "TencentModelEntry"
  hidden: true
---

<a id="Vercel_AI_Tencent_TencentModelEntry"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class TencentModelEntry
```


One catalog record of a model id, holding the operations it declares. Upstream `ModelEntry` in `tencentModelCatalog`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L246)

## Properties

<a id="Vercel_AI_Tencent_TencentModelEntry_Language_"></a>
<a id="Vercel_AI_Tencent_TencentModelEntry_Language"></a>

### Language


```csharp
public TencentLanguageModelEntry? Language { get; }
```


The language entry, or null when the id is not a language model.

**Value:** [TencentLanguageModelEntry](/api/Vercel.AI.Tencent/TencentLanguageModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L255)

<a id="Vercel_AI_Tencent_TencentModelEntry_Video_"></a>
<a id="Vercel_AI_Tencent_TencentModelEntry_Video"></a>

### Video


```csharp
public TencentVideoModelEntry? Video { get; }
```


The video entry, or null when the id is not a video model.

**Value:** [TencentVideoModelEntry](/api/Vercel.AI.Tencent/TencentVideoModelEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L258)
