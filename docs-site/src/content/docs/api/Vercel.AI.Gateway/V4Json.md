---
title: "V4Json"
description: "Serializes call options and parses results in the Language Model V4 JSON shape the Gateway expects."
editUrl: false
prev: false
next: false
sidebar:
  label: "V4Json"
  hidden: true
---

<a id="Vercel_AI_Gateway_V4Json"></a>

Namespace: [Vercel.AI.Gateway](/api/Vercel.AI.Gateway/)


```csharp
public static class V4Json
```


Serializes call options and parses results in the Language Model V4 JSON shape the Gateway expects.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/V4Json.cs#L15)

## Methods

<a id="Vercel_AI_Gateway_V4Json_CallOptions_"></a>
<a id="Vercel_AI_Gateway_V4Json_CallOptions_Vercel_AI_Provider_LanguageModelCallOptions_"></a>

### CallOptions(LanguageModelCallOptions)


```csharp
public static string CallOptions(LanguageModelCallOptions options)
```


Serializes &lt;code class="paramref"&gt;options&lt;/code&gt; as a V4 call body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `options` | [LanguageModelCallOptions](/api/Vercel.AI.Provider/LanguageModelCallOptions/) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/V4Json.cs#L18)

<a id="Vercel_AI_Gateway_V4Json_ParseGenerate_"></a>
<a id="Vercel_AI_Gateway_V4Json_ParseGenerate_System_Text_Json_JsonElement_"></a>

### ParseGenerate(JsonElement)


```csharp
public static LanguageModelGenerateResult ParseGenerate(JsonElement root)
```


Parses a V4 generate result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `root` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/V4Json.cs#L134)

<a id="Vercel_AI_Gateway_V4Json_ParseStreamPart_"></a>
<a id="Vercel_AI_Gateway_V4Json_ParseStreamPart_System_String_"></a>

### ParseStreamPart(string)


```csharp
public static LanguageModelStreamPart? ParseStreamPart(string json)
```


Parses one SSE JSON object into a stream part.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [LanguageModelStreamPart](/api/Vercel.AI.Provider/LanguageModelStreamPart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Gateway/V4Json.cs#L158)
