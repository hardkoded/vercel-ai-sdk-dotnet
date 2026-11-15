---
title: "ParsedTencentVideoModelId"
description: "A parsed video model id. Upstream ParsedTencentVideoModelId."
editUrl: false
prev: false
next: false
sidebar:
  label: "ParsedTencentVideoModelId"
  hidden: true
---

<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId"></a>

Namespace: [Vercel.AI.Tencent](/api/Vercel.AI.Tencent/)


```csharp
public sealed class ParsedTencentVideoModelId
```


A parsed video model id. Upstream `ParsedTencentVideoModelId`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L21)

## Properties

<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_IsVod_"></a>
<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_IsVod"></a>

### IsVod


```csharp
public bool IsVod { get; }
```


True for a VOD AIGC model (`vod/<ModelName>/<ModelVersion>`), false for a TokenHub Kling model.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L32)

<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_Kind_"></a>
<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_Kind"></a>

### Kind


```csharp
public TencentKlingVideoKind Kind { get; }
```


The Kling kind. Only meaningful when [IsVod](#Vercel_AI_Tencent_ParsedTencentVideoModelId_IsVod) is false.

**Value:** [TencentKlingVideoKind](/api/Vercel.AI.Tencent/TencentKlingVideoKind/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L35)

<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_ModelName_"></a>
<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_ModelName"></a>

### ModelName


```csharp
public string? ModelName { get; }
```


The VOD model name. Null for TokenHub.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L38)

<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_ModelVersion_"></a>
<a id="Vercel_AI_Tencent_ParsedTencentVideoModelId_ModelVersion"></a>

### ModelVersion


```csharp
public string? ModelVersion { get; }
```


The VOD model version. Null for TokenHub.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Tencent/TencentModelCatalog.cs#L41)
