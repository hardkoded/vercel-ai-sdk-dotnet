---
title: "GoogleUsage"
description: "Converts Gemini usageMetadata."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleUsage"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleUsage"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public static class GoogleUsage
```


Converts Gemini `usageMetadata`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L61)

## Methods

<a id="Vercel_AI_Google_GoogleUsage_Add_"></a>
<a id="Vercel_AI_Google_GoogleUsage_Add_System_Nullable_System_Text_Json_JsonElement__System_Nullable_System_Text_Json_JsonElement__"></a>

### Add(JsonElement?, JsonElement?)


```csharp
public static JsonElement? Add(JsonElement? a, JsonElement? b)
```


Sums the usage metadata of two requests, such as the hops of a long decode response that was continued automatically.
Numeric counts add, a missing count is the other side, and other fields come from &lt;code class="paramref"&gt;b&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `a` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `b` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L87)

<a id="Vercel_AI_Google_GoogleUsage_Convert_"></a>
<a id="Vercel_AI_Google_GoogleUsage_Convert_System_Nullable_System_Text_Json_JsonElement__"></a>

### Convert(JsonElement?)


```csharp
public static GoogleTokenUsage? Convert(JsonElement? usage)
```


Adds tool-use prompt tokens to the input total and thought tokens to the output total.
Returns null when &lt;code class="paramref"&gt;usage&lt;/code&gt; is missing.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `usage` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [GoogleTokenUsage](/api/Vercel.AI.Google/GoogleTokenUsage/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleUsage.cs#L67)
