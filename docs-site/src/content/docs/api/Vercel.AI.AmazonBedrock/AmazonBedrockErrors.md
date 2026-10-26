---
title: "AmazonBedrockErrors"
description: "Maps a Bedrock JSON error body onto the SDK exception hierarchy."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockErrors"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockErrors"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockErrors
```


Maps a Bedrock JSON error body onto the SDK exception hierarchy.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockErrors.cs#L12)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockErrors_Create_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockErrors_Create_System_Int32_System_String_"></a>

### Create(int, string?)


```csharp
public static ApiException Create(int statusCode, string? body)
```


Creates the status exception, using the Bedrock `type` prefix when the body has one.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `statusCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ApiException](/api/Vercel.AI.Provider/ApiException/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockErrors.cs#L57)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockErrors_FormatMessage_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockErrors_FormatMessage_System_String_"></a>

### FormatMessage(string?)


```csharp
public static string? FormatMessage(string? body)
```


Returns the provider message. When `type` is present the message is `{type}: {message}`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `body` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockErrors.cs#L17)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockErrors_FormatMessage_System_Text_Json_JsonElement_"></a>

### FormatMessage(JsonElement)


```csharp
public static string? FormatMessage(JsonElement root)
```


Formats a parsed Bedrock error object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `root` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockErrors.cs#L36)
