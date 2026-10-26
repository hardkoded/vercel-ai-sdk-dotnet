---
title: "GoogleToolCall"
description: "A tool call that records whether the provider executed it."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleToolCall"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleToolCall"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleToolCall : GeneratedToolCall
```


A tool call that records whether the provider executed it.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [GeneratedContent](/api/Vercel.AI.Provider/GeneratedContent/) → [GeneratedToolCall](/api/Vercel.AI.Provider/GeneratedToolCall/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L465)

## Constructors

<a id="Vercel_AI_Google_GoogleToolCall__ctor_"></a>
<a id="Vercel_AI_Google_GoogleToolCall__ctor_System_String_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__System_Boolean_"></a>

### GoogleToolCall(string, string, string, JsonElement?, bool)


```csharp
public GoogleToolCall(string toolCallId, string toolName, string argumentsJson, JsonElement? providerMetadata, bool providerExecuted)
```


Creates a tool call.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `argumentsJson` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `providerMetadata` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `providerExecuted` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleResponse.cs#L468)
