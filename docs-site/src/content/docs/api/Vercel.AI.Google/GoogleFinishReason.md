---
title: "GoogleFinishReason"
description: "Maps Gemini finish reasons onto the shared finish-reason enum."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleFinishReason"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleFinishReason"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleFinishReason
```


Maps Gemini finish reasons onto the shared finish-reason enum.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleFinishReason.cs#L10)

## Methods

<a id="Vercel_AI_Google_GoogleFinishReason_IsConfirmedPromptBlock_"></a>
<a id="Vercel_AI_Google_GoogleFinishReason_IsConfirmedPromptBlock_System_String_"></a>

### IsConfirmedPromptBlock(string?)


```csharp
public static bool IsConfirmedPromptBlock(string? blockReason)
```


A prompt block is terminal when the reason is present and is not an unspecified placeholder.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `blockReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleFinishReason.cs#L43)

<a id="Vercel_AI_Google_GoogleFinishReason_Map_"></a>
<a id="Vercel_AI_Google_GoogleFinishReason_Map_System_String_System_Boolean_"></a>

### Map(string?, bool)


```csharp
public static FinishReason Map(string? finishReason, bool hasToolCalls)
```


Maps &lt;code class="paramref"&gt;finishReason&lt;/code&gt;. `STOP` becomes [ToolCalls](/api/Vercel.AI.Provider/FinishReason/#Vercel_AI_Provider_FinishReason_ToolCalls) when the response contains a client tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `finishReason` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `hasToolCalls` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleFinishReason.cs#L15)
