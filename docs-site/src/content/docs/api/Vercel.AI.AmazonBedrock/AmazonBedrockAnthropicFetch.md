---
title: "AmazonBedrockAnthropicFetch"
description: "Wraps a handler so the Anthropic Messages model can read Bedrock responses. Failed responses become Anthropic error bodies, and application/vnd.amazon.events..."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockAnthropicFetch"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicFetch"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockAnthropicFetch
```


Wraps a handler so the Anthropic Messages model can read Bedrock responses. Failed responses become
Anthropic error bodies, and `application/vnd.amazon.eventstream` bodies become server-sent events.
Maps to `createAmazonBedrockAnthropicFetch`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockHandlers.cs#L125)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicFetch_Create_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockAnthropicFetch_Create_System_Net_Http_HttpMessageHandler_"></a>

### Create(HttpMessageHandler)


```csharp
public static DelegatingHandler Create(HttpMessageHandler baseHandler)
```


Wraps &lt;code class="paramref"&gt;baseHandler&lt;/code&gt;. Disposing the result disposes &lt;code class="paramref"&gt;baseHandler&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `baseHandler` | [HttpMessageHandler](https://learn.microsoft.com/dotnet/api/system.net.http.httpmessagehandler) |  |

**Returns:** [DelegatingHandler](https://learn.microsoft.com/dotnet/api/system.net.http.delegatinghandler)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockHandlers.cs#L128)
