---
title: "StepResponse"
description: "Response information of one step. Maps to LanguageModelResponseMetadata & { messages, body? } on StepResult.response."
editUrl: false
prev: false
next: false
sidebar:
  label: "StepResponse"
  hidden: true
---

<a id="Vercel_AI_StepResponse"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class StepResponse : LanguageModelResponseMetadata
```


Response information of one step. Maps to `LanguageModelResponseMetadata & { messages, body? }` on `StepResult.response`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [LanguageModelResponseMetadata](/api/Vercel.AI/LanguageModelResponseMetadata/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1066)

## Constructors

<a id="Vercel_AI_StepResponse__ctor_"></a>
<a id="Vercel_AI_StepResponse__ctor_System_String_System_DateTimeOffset_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyList_Vercel_AI_Provider_ModelMessage__System_Nullable_System_Text_Json_JsonElement__"></a>

### `StepResponse(string, DateTimeOffset, string, IReadOnlyDictionary<string, string>?, IReadOnlyList<ModelMessage>?, JsonElement?)`


```csharp
public StepResponse(string id, DateTimeOffset timestamp, string modelId, IReadOnlyDictionary<string, string>? headers = null, IReadOnlyList<ModelMessage>? messages = null, JsonElement? body = null)
```


Creates step response information.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `timestamp` | [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `messages` | [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `body` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1069)

## Properties

<a id="Vercel_AI_StepResponse_Body_"></a>
<a id="Vercel_AI_StepResponse_Body"></a>

### Body


```csharp
public JsonElement? Body { get; }
```


Response body, for providers that use HTTP and report it. It is not set for streams. Maps to `body`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1089)

<a id="Vercel_AI_StepResponse_Messages_"></a>
<a id="Vercel_AI_StepResponse_Messages"></a>

### Messages


```csharp
public IReadOnlyList<ModelMessage> Messages { get; }
```


The response messages that were generated during the call: the assistant message of this step, then the tool message with
the results the client ran. Maps to `messages`.

**Value:** [IReadOnlyList&lt;ModelMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L1086)
