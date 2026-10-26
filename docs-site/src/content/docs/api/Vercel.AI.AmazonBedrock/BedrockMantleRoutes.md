---
title: "BedrockMantleRoutes"
description: "Mantle URL rules. Some model ids are served under /openai/v1."
editUrl: false
prev: false
next: false
sidebar:
  label: "BedrockMantleRoutes"
  hidden: true
---

<a id="Vercel_AI_AmazonBedrock_BedrockMantleRoutes"></a>

Namespace: [Vercel.AI.AmazonBedrock](/api/Vercel.AI.AmazonBedrock/)


```csharp
public static class BedrockMantleRoutes
```


Mantle URL rules. Some model ids are served under `/openai/v1`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L60)

## Methods

<a id="Vercel_AI_AmazonBedrock_BedrockMantleRoutes_Url_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleRoutes_Url_System_String_System_String_System_String_System_String_"></a>

### Url(string, string?, string, string)


```csharp
public static string Url(string region, string? baseUrl, string modelId, string path)
```


Joins the Mantle origin and &lt;code class="paramref"&gt;path&lt;/code&gt; for &lt;code class="paramref"&gt;modelId&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `region` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `baseUrl` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `path` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L84)

<a id="Vercel_AI_AmazonBedrock_BedrockMantleRoutes_UsesOpenAiRoute_"></a>
<a id="Vercel_AI_AmazonBedrock_BedrockMantleRoutes_UsesOpenAiRoute_System_String_"></a>

### UsesOpenAiRoute(string)


```csharp
public static bool UsesOpenAiRoute(string modelId)
```


True for Mantle models served on the OpenAI route, excluding `openai.gpt-oss-`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `modelId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/AmazonBedrock/BedrockMantleProvider.cs#L63)
