---
title: "LanguageModelStreamRequest"
description: "Request info for a stream call. Maps to the request field of doStream."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelStreamRequest"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelStreamRequest"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelStreamRequest
```


Request info for a stream call. Maps to the `request` field of `doStream`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2166)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelStreamRequest__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest__ctor_System_Nullable_System_Text_Json_JsonElement__"></a>

### LanguageModelStreamRequest(JsonElement?)


```csharp
public LanguageModelStreamRequest(JsonElement? body = null)
```


Creates request info.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2169)

## Properties

<a id="Vercel_AI_Provider_LanguageModelStreamRequest_Body_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


Request body sent to the provider, for debugging.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2182)

## Methods

<a id="Vercel_AI_Provider_LanguageModelStreamRequest_FromJson_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest_FromJson_System_String_"></a>

### FromJson(string)


```csharp
public static LanguageModelStreamRequest FromJson(string json)
```


Creates request info from the JSON text that was sent.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [LanguageModelStreamRequest](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L2175)
