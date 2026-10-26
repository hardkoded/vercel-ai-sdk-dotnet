---
title: "AnthropicStreamError"
description: "HTTP status implied by an Anthropic stream error type."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicStreamError"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicStreamError"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicStreamError
```


HTTP status implied by an Anthropic stream error type.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L84)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicStreamError_IsRetryable_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStreamError_IsRetryable_System_String_"></a>

### IsRetryable(string?)


```csharp
public static bool IsRetryable(string? type)
```


True for api, overloaded, and rate-limit errors.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L114)

<a id="Vercel_AI_Anthropic_AnthropicStreamError_StatusCode_"></a>
<a id="Vercel_AI_Anthropic_AnthropicStreamError_StatusCode_System_String_"></a>

### StatusCode(string?)


```csharp
public static int? StatusCode(string? type)
```


Maps an error type onto a status code. Unknown types have no status.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [int?](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicErrorData.cs#L87)
