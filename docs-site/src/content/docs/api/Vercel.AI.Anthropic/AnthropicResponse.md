---
title: "AnthropicResponse"
description: "Maps an Anthropic Messages JSON body onto a generate result."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicResponse"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicResponse"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public static class AnthropicResponse
```


Maps an Anthropic Messages JSON body onto a generate result.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L45)

## Methods

<a id="Vercel_AI_Anthropic_AnthropicResponse_Parse_"></a>
<a id="Vercel_AI_Anthropic_AnthropicResponse_Parse_System_String_Vercel_AI_Anthropic_AnthropicParseContext_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Parse(string, AnthropicParseContext, IReadOnlyDictionary<string, string>?)`


```csharp
public static LanguageModelGenerateResult Parse(string json, AnthropicParseContext context, IReadOnlyDictionary<string, string>? headers = null)
```


Parses a Messages response body.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `json` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `context` | [AnthropicParseContext](/api/Vercel.AI.Anthropic/AnthropicParseContext/) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L48)

<a id="Vercel_AI_Anthropic_AnthropicResponse_Parse_System_Text_Json_JsonElement_Vercel_AI_Anthropic_AnthropicParseContext_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__"></a>

### `Parse(JsonElement, AnthropicParseContext, string?, IReadOnlyDictionary<string, string>?)`


```csharp
public static LanguageModelGenerateResult Parse(JsonElement root, AnthropicParseContext context, string? raw = null, IReadOnlyDictionary<string, string>? headers = null)
```


Parses a Messages response object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `root` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `context` | [AnthropicParseContext](/api/Vercel.AI.Anthropic/AnthropicParseContext/) |  |
| `raw` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `headers` | [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

**Returns:** [LanguageModelGenerateResult](/api/Vercel.AI.Provider/LanguageModelGenerateResult/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicResponse.cs#L55)
