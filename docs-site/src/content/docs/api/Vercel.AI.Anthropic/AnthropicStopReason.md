---
title: "AnthropicStopReason"
description: "Maps an Anthropic stopreason onto the SDK finish reason."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicStopReason"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicStopReason"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicStopReason
```


Maps an Anthropic `stop_reason` onto the SDK finish reason.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStopReason.cs#L10)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicStopReason_Map_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStopReason_Map_System_String_System_Boolean_"></a>

### Map(string?, bool)


```csharp
public static FinishReason Map(string? finishReason, bool isJsonResponseFromTool = false)
```


Maps a stop reason. `tool_use` is [Stop](/api/Vercel.AI.Provider/FinishReason/#Vercel_AI_Provider_FinishReason_Stop) when the JSON response tool produced the payload.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isJsonResponseFromTool` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicStopReason.cs#L15)
