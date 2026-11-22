---
title: "GatewayErrorWrappers"
description: "Replaces a Gateway authentication error with a message that says how to authenticate. Maps to prompt/wrap-gateway-error.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "GatewayErrorWrappers"
  hidden: true
---

<a id="Vercel_AI_Prompt_GatewayErrorWrappers"></a>

Namespace: [Vercel.AI.Prompt](/api/Vercel.AI.Prompt/)


```csharp
public static class GatewayErrorWrappers
```


Replaces a Gateway authentication error with a message that says how to authenticate. Maps to `prompt/wrap-gateway-error.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/WrapGatewayError.cs#L11)

## Methods

<a id="Vercel_AI_Prompt_GatewayErrorWrappers_WrapGatewayError_"></a>
<a id="Vercel_AI_Prompt_GatewayErrorWrappers_WrapGatewayError_System_Exception_"></a>

### WrapGatewayError(Exception)


```csharp
public static Exception WrapGatewayError(Exception error)
```


Returns &lt;code class="paramref"&gt;error&lt;/code&gt; unchanged unless it is a [GatewayAuthenticationError](/api/Vercel.AI.Gateway/GatewayAuthenticationError/).
Maps to `wrapGatewayError`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `error` | [Exception](https://learn.microsoft.com/dotnet/api/system.exception) |  |

**Returns:** [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Prompt/WrapGatewayError.cs#L19)
