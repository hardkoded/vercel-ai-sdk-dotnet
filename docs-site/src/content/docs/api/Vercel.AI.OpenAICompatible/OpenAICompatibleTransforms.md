---
title: "OpenAICompatibleTransforms"
description: "Request-body transforms used by thin OpenAI-compatible providers."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAICompatibleTransforms"
  hidden: true
---

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms"></a>

Namespace: [Vercel.AI.OpenAICompatible](/api/Vercel.AI.OpenAICompatible/)


```csharp
public static class OpenAICompatibleTransforms
```


Request-body transforms used by thin OpenAI-compatible providers.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L11)

## Methods

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_BasetenError_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_BasetenError_System_String_"></a>

### BasetenError(string?)


```csharp
public static string? BasetenError(string? body)
```


Reads a Baseten error. The Model APIs send `error` as a string; dedicated deployments send an
object with a `message`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L338)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Cerebras_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Cerebras_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IList_Vercel_AI_Provider_CallWarning__"></a>

### `Cerebras(JsonObject, IList<CallWarning>)`


```csharp
public static JsonObject Cerebras(JsonObject body, IList<CallWarning> warnings)
```


Maps Cerebras field names and assistant `reasoning_content` onto `reasoning`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L52)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_CerebrasError_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_CerebrasError_System_String_"></a>

### CerebrasError(string?)


```csharp
public static string? CerebrasError(string? body)
```


Reads a Cerebras error message from the top-level `message` field.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L357)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Fireworks_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Fireworks_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IList_Vercel_AI_Provider_CallWarning__"></a>

### `Fireworks(JsonObject, IList<CallWarning>)`


```csharp
public static JsonObject Fireworks(JsonObject body, IList<CallWarning> warnings)
```


Maps Fireworks thinking, cache, service tier, and reasoning effort. Fireworks supports `low`, `medium`, and `high`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L83)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_GmiCloudError_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_GmiCloudError_System_String_"></a>

### GmiCloudError(string?)


```csharp
public static string? GmiCloudError(string? body)
```


Reads a GMI Cloud error, preferring the diagnostic nested in `error.details`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L304)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Groq_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Groq_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IList_Vercel_AI_Provider_CallWarning__"></a>

### `Groq(JsonObject, IList<CallWarning>)`


```csharp
public static JsonObject Groq(JsonObject body, IList<CallWarning> warnings)
```


Maps Groq reasoning levels. `none` is kept only for `qwen/qwen3.6-27b`.
`minimal` becomes `low` and `xhigh` and `max` become `high`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L17)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Zai_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_Zai_System_Text_Json_Nodes_JsonObject_System_Collections_Generic_IList_Vercel_AI_Provider_CallWarning__"></a>

### `Zai(JsonObject, IList<CallWarning>)`


```csharp
public static JsonObject Zai(JsonObject body, IList<CallWarning> warnings)
```


Drops fields Z.AI does not accept and renames its provider options.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject) |  |
| `warnings` | [IList&lt;CallWarning&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1) |  |

**Returns:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L139)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_ZaiError_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_ZaiError_System_String_"></a>

### ZaiError(string?)


```csharp
public static string? ZaiError(string? body)
```


Reads a Z.AI error from `error.message` or a top-level `message`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L376)

<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_ZaiFinish_"></a>
<a id="Vercel_AI_OpenAICompatible_OpenAICompatibleTransforms_ZaiFinish_System_String_"></a>

### ZaiFinish(string?)


```csharp
public static FinishReason? ZaiFinish(string? raw)
```


Maps a Z.AI finish reason. Unrecognized values fall through.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `raw` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** `FinishReason?`

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAICompatible/OpenAICompatibleTransforms.cs#L288)
