---
title: "GoogleInteractionsFinish"
description: "Maps an Interactions API status onto a finish reason."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleInteractionsFinish"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleInteractionsFinish"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleInteractionsFinish
```


Maps an Interactions API status onto a finish reason.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L12)

## Methods

<a id="Vercel_AI_Google_GoogleInteractionsFinish_Map_"></a>
<a id="Vercel_AI_Google_GoogleInteractionsFinish_Map_System_String_System_Boolean_"></a>

### Map(string?, bool)


```csharp
public static FinishReason Map(string? status, bool hasFunctionCall)
```


`completed` is a tool call when the response contains a client function call.
`requires_action` is always a tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `status` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `hasFunctionCall` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [FinishReason](/api/Vercel.AI.Provider/FinishReason/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleInteractions.cs#L18)
