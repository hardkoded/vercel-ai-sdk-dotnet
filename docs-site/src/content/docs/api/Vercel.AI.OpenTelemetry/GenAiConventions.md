---
title: "GenAiConventions"
description: "GenAI semantic-convention names shared by spans and the UI message protocol."
editUrl: false
prev: false
next: false
sidebar:
  label: "GenAiConventions"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_GenAiConventions"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public static class GenAiConventions
```


GenAI semantic-convention names shared by spans and the UI message protocol.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/GenAiConventions.cs#L10)

## Methods

<a id="Vercel_AI_OpenTelemetry_GenAiConventions_FormatFinishReason_"></a>
<a id="Vercel_AI_OpenTelemetry_GenAiConventions_FormatFinishReason_Vercel_AI_Provider_FinishReason_"></a>

### FormatFinishReason(FinishReason)


```csharp
public static string FormatFinishReason(FinishReason finishReason)
```


Protocol finish reason used by UI message `finish` chunks and `gen_ai.response.finish_reasons`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finishReason` | [FinishReason](/api/Vercel.AI.Provider/FinishReason/) | Normalized finish reason. |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/GenAiConventions.cs#L85)

<a id="Vercel_AI_OpenTelemetry_GenAiConventions_MapOperationName_"></a>
<a id="Vercel_AI_OpenTelemetry_GenAiConventions_MapOperationName_System_String_"></a>

### MapOperationName(string)


```csharp
public static string MapOperationName(string operationId)
```


Maps an AI SDK operation id such as `ai.generateText` to `gen_ai.operation.name`.
An unrecognized id is returned unchanged, which keeps `generateText` and `streamText`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `operationId` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Operation id passed to telemetry. |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/GenAiConventions.cs#L63)

<a id="Vercel_AI_OpenTelemetry_GenAiConventions_MapProviderName_"></a>
<a id="Vercel_AI_OpenTelemetry_GenAiConventions_MapProviderName_System_String_"></a>

### MapProviderName(string)


```csharp
public static string MapProviderName(string provider)
```


Maps a provider id such as `openai.chat` to the GenAI well-known provider name.
An unrecognized id is returned unchanged.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `provider` | [string](https://learn.microsoft.com/dotnet/api/system.string) | Provider id from the model. |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/GenAiConventions.cs#L37)
