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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3297)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3300)

## Properties

<a id="Vercel_AI_Provider_LanguageModelStreamRequest_Body_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


Request body sent to the provider, for debugging.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3313)

<a id="Vercel_AI_Provider_LanguageModelStreamRequest_RedactBody_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest_RedactBody"></a>

### RedactBody


```csharp
public Func<JsonElement, JsonElement>? RedactBody { get; init; }
```


Provider-owned telemetry view of [Body](#Vercel_AI_Provider_LanguageModelStreamRequest_Body), which hides credentials. Maps to the annotation of `withTelemetryRedaction` on the request body.

**Value:** [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3319)

<a id="Vercel_AI_Provider_LanguageModelStreamRequest_Url_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest_Url"></a>

### Url


```csharp
public string? Url { get; init; }
```


URL the request was sent to. Telemetry events carry only its origin. Maps to `request.url`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3316)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3306)

<a id="Vercel_AI_Provider_LanguageModelStreamRequest_RedactTelemetryBody_"></a>
<a id="Vercel_AI_Provider_LanguageModelStreamRequest_RedactTelemetryBody"></a>

### RedactTelemetryBody()


```csharp
public JsonElement? RedactTelemetryBody()
```


Applies [RedactBody](#Vercel_AI_Provider_LanguageModelStreamRequest_RedactBody) to [Body](#Vercel_AI_Provider_LanguageModelStreamRequest_Body) for telemetry, leaving [Body](#Vercel_AI_Provider_LanguageModelStreamRequest_Body) unchanged. Returns null when there is no body or the redactor throws.
Maps to `redactTelemetryValue`.

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L3325)
