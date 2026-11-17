---
title: "GatewaySpeechModel"
description: "Gateway streaming speech model. Maps to GatewaySpeechModel in gateway-speech-model.ts. Only the URL helper used by experimentalspeech.getToken is ported here..."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewaySpeechModel"
  hidden: true
---

<a id="Vercel_AI_Gateway_GatewaySpeechModel"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public sealed class GatewaySpeechModel
```


Gateway streaming speech model. Maps to `GatewaySpeechModel` in `gateway-speech-model.ts`. Only the URL
helper used by `experimental_speech.getToken` is ported here; the model itself is tracked in #752.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpeechModel.cs#L11)

## Methods

<a id="Vercel_AI_Gateway_GatewaySpeechModel_ToGatewaySpeechUrl_"></a>
<a id="Vercel_AI_Gateway_GatewaySpeechModel_ToGatewaySpeechUrl_System_String_System_String_"></a>

### ToGatewaySpeechUrl(string, string)


```csharp
public static string ToGatewaySpeechUrl(string baseUrl, string modelId)
```


Gateway streaming speech WebSocket URL: the HTTP(S) base upgraded to WS(S), with the model id in
`?ai-model-id=` (slash-safe for qualified ids like `elevenlabs/eleven-flash-v2.5`). Maps to `toGatewaySpeechUrl`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/GatewaySpeechModel.cs#L19)
