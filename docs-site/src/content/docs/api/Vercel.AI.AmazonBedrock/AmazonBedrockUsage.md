---
title: "AmazonBedrockUsage"
description: "Converts a Converse usage object. Bedrock inputTokens excludes cache tokens; the V4 input total adds cache read and cache write."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockUsage"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockUsage"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockUsage
```


Converts a Converse `usage` object. Bedrock `inputTokens` excludes cache tokens;
the V4 input total adds cache read and cache write.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockUsage.cs#L14)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockUsage_Convert_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockUsage_Convert_System_Nullable_System_Text_Json_JsonElement__"></a>

### Convert(JsonElement?)


```csharp
public static LanguageModelUsage Convert(JsonElement? usage)
```


Converts &lt;code class="paramref"&gt;usage&lt;/code&gt;. A missing object leaves the token counts unset.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [LanguageModelUsage](/api/Vercel.AI.Provider/LanguageModelUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockUsage.cs#L17)
