---
title: "ModelOptionsSerialization"
description: "Serializes model options for a workflow boundary. Maps to serializeModelOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ModelOptionsSerialization"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ModelOptionsSerialization"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public static class ModelOptionsSerialization
```


Serializes model options for a workflow boundary. Maps to `serializeModelOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L28)

## Methods

<a id="Vercel_AI_ProviderUtils_ModelOptionsSerialization_SerializeModelOptions_"></a>
<a id="Vercel_AI_ProviderUtils_ModelOptionsSerialization_SerializeModelOptions_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_Object__"></a>

### `SerializeModelOptions(string, IReadOnlyDictionary<string, object?>)`


```csharp
public static SerializedModelOptions SerializeModelOptions(string modelId, IReadOnlyDictionary<string, object?> config)
```


Copies JSON-serializable config values. A function-valued `headers` entry is invoked.
Asynchronous headers throw [SerializationError](/api/Vercel.AI.ProviderUtils/SerializationError/).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `config` | [IReadOnlyDictionary&lt;string, object&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [SerializedModelOptions](/api/Vercel.AI.ProviderUtils/SerializedModelOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/SerializeModelOptions.cs#L34)
