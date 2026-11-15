---
title: "ElevenLabsFailedResponseHandler"
description: "Creates .NET API exceptions from ElevenLabs failed JSON responses."
editUrl: false
prev: false
next: false
sidebar:
  label: "ElevenLabsFailedResponseHandler"
  hidden: true
---

<a id="Vercel_AI_ElevenLabs_ElevenLabsFailedResponseHandler"></a>

Namespace: [Vercel.AI.ElevenLabs](/api/Vercel.AI.ElevenLabs/)


```csharp
public static class ElevenLabsFailedResponseHandler
```


Creates .NET API exceptions from ElevenLabs failed JSON responses.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L398)

## Methods

<a id="Vercel_AI_ElevenLabs_ElevenLabsFailedResponseHandler_Handle_"></a>
<a id="Vercel_AI_ElevenLabs_ElevenLabsFailedResponseHandler_Handle_System_String_System_Int32_System_String_System_Object_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Handle(string, int, string, object, IReadOnlyDictionary<string, string>?)`


```csharp
public static ApiException Handle(string responseBody, int statusCode, string url, object requestBodyValues, IReadOnlyDictionary<string, string>? responseHeaders = null)
```


Parses an ElevenLabs error response and preserves the parsed body on [ErrorData](/api/Vercel.AI.Provider/ApiException/#Vercel_AI_Provider_ApiException_ErrorData).

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `responseBody` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `url` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `requestBodyValues` | [object](https://learn.microsoft.com/dotnet/api/system.object) |  |
| `responseHeaders` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [ApiException](/api/Vercel.AI.Provider/ApiException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ElevenLabs/ElevenLabsProvider.cs#L401)
