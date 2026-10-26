---
title: "AmazonBedrockToolIds"
description: "Mistral tool-call ids on Bedrock must be exactly nine alphanumeric characters."
editUrl: false
prev: false
next: false
sidebar:
  label: "AmazonBedrockToolIds"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockToolIds"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class AmazonBedrockToolIds
```


Mistral tool-call ids on Bedrock must be exactly nine alphanumeric characters.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockToolIds.cs#L8)

## Methods

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockToolIds_IsMistralModel_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockToolIds_IsMistralModel_System_String_"></a>

### IsMistralModel(string)


```csharp
public static bool IsMistralModel(string modelId)
```


True when &lt;code class="paramref"&gt;modelId&lt;/code&gt; is a Mistral model, including region-prefixed ids.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockToolIds.cs#L14)

<a id="Vercel_AI_AmazonBedrock_AmazonBedrockToolIds_Normalize_"></a>
<a id="Vercel_AI_AmazonBedrock_AmazonBedrockToolIds_Normalize_System_String_System_Boolean_"></a>

### Normalize(string, bool)


```csharp
public static string Normalize(string toolCallId, bool isMistral)
```


Returns &lt;code class="paramref"&gt;toolCallId&lt;/code&gt; unchanged for other models.
For Mistral, keeps a valid nine-character alphanumeric id and hashes any other id into one.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `isMistral` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/AmazonBedrockToolIds.cs#L23)
