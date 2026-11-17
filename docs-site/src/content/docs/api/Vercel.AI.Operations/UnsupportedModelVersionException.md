---
title: "UnsupportedModelVersionException"
description: "The model specification version is not supported."
editUrl: false
prev: false
next: false
sidebar:
  label: "UnsupportedModelVersionException"
  hidden: true
---

<a id="Vercel_AI_Operations_UnsupportedModelVersionException"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public sealed class UnsupportedModelVersionException : AiSdkException, ISerializable
```


The model specification version is not supported.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [Exception](https://learn.microsoft.com/dotnet/api/system.exception) → [AiSdkException](/api/Vercel.AI.Provider/AiSdkException/)

**Implements:** [ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L202)

## Constructors

<a id="Vercel_AI_Operations_UnsupportedModelVersionException__ctor_"></a>
<a id="Vercel_AI_Operations_UnsupportedModelVersionException__ctor_System_String_System_String_System_String_"></a>

### UnsupportedModelVersionException(string, string, string)


```csharp
public UnsupportedModelVersionException(string version, string provider, string modelId)
```


Creates the exception.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `version` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L205)

## Properties

<a id="Vercel_AI_Operations_UnsupportedModelVersionException_ModelId_"></a>
<a id="Vercel_AI_Operations_UnsupportedModelVersionException_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L228)

<a id="Vercel_AI_Operations_UnsupportedModelVersionException_Provider_"></a>
<a id="Vercel_AI_Operations_UnsupportedModelVersionException_Provider"></a>

### Provider


```csharp
public string Provider { get; }
```


Provider id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L225)

<a id="Vercel_AI_Operations_UnsupportedModelVersionException_Version_"></a>
<a id="Vercel_AI_Operations_UnsupportedModelVersionException_Version"></a>

### Version


```csharp
public string Version { get; }
```


Rejected specification version.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/OperationSupport.cs#L222)
