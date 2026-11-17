---
title: "TakoNode"
description: "A Data Graph node of a TakoCard."
editUrl: false
prev: false
next: false
sidebar:
  label: "TakoNode"
  hidden: true
---

<a id="Vercel_AI_Gateway_TakoNode"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class TakoNode
```


A Data Graph node of a [TakoCard](/api/Vercel.AI.Gateway/TakoCard/).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L913)

## Properties

<a id="Vercel_AI_Gateway_TakoNode_Description_"></a>
<a id="Vercel_AI_Gateway_TakoNode_Description"></a>

### Description


```csharp
[JsonPropertyName("description")]
public string? Description { get; set; }
```


Description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L928)

<a id="Vercel_AI_Gateway_TakoNode_Id_"></a>
<a id="Vercel_AI_Gateway_TakoNode_Id"></a>

### Id


```csharp
[JsonPropertyName("id")]
public string Id { get; set; }
```


Id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L916)

<a id="Vercel_AI_Gateway_TakoNode_Name_"></a>
<a id="Vercel_AI_Gateway_TakoNode_Name"></a>

### Name


```csharp
[JsonPropertyName("name")]
public string Name { get; set; }
```


Name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L924)

<a id="Vercel_AI_Gateway_TakoNode_Type_"></a>
<a id="Vercel_AI_Gateway_TakoNode_Type"></a>

### Type


```csharp
[JsonPropertyName("type")]
public string Type { get; set; }
```


`entity` or `metric`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewayTools.cs#L920)
