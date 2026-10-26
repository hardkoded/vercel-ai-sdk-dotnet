---
title: "SerializedModelOptions"
description: "A serialized model id and config. Maps to the result of serializeModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SerializedModelOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SerializedModelOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SerializedModelOptions
```


A serialized model id and config. Maps to the result of `serializeModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L11)

## Constructors

<a id="Vercel_AI_ProviderUtils_SerializedModelOptions__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_SerializedModelOptions__ctor_System_String_System_Collections_Generic_Dictionary_System_String_System_Object__"></a>

### `SerializedModelOptions(string, Dictionary<string, object?>)`


```csharp
public SerializedModelOptions(string modelId, Dictionary<string, object?> config)
```


Creates the serialized form.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [Dictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L14)

## Properties

<a id="Vercel_AI_ProviderUtils_SerializedModelOptions_Config_"></a>
<a id="Vercel_AI_ProviderUtils_SerializedModelOptions_Config"></a>

### Config


```csharp
public Dictionary<string, object?> Config { get; }
```


JSON-serializable configuration.

**Value:** [Dictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L24)

<a id="Vercel_AI_ProviderUtils_SerializedModelOptions_ModelId_"></a>
<a id="Vercel_AI_ProviderUtils_SerializedModelOptions_ModelId"></a>

### ModelId


```csharp
public string ModelId { get; }
```


Model id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L21)
