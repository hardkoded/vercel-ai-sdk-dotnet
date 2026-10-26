---
title: "LanguageModelTool"
description: "A function tool definition sent to the model."
editUrl: false
prev: false
next: false
sidebar:
  label: "LanguageModelTool"
  hidden: true
---

<a id="Vercel_AI_Provider_LanguageModelTool"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class LanguageModelTool
```


A function tool definition sent to the model.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L543)

## Constructors

<a id="Vercel_AI_Provider_LanguageModelTool__ctor_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool__ctor_System_String_System_String_System_Text_Json_JsonElement_System_Nullable_System_Boolean__"></a>

### LanguageModelTool(string, string?, JsonElement, bool?)


```csharp
public LanguageModelTool(string name, string? description, JsonElement inputSchema, bool? strict = null)
```


Creates a function tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `inputSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `strict` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L546)

## Properties

<a id="Vercel_AI_Provider_LanguageModelTool_Args_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Args"></a>

### Args


```csharp
public JsonElement Args { get; }
```


Arguments of a provider-defined tool. Undefined for a function tool.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L580)

<a id="Vercel_AI_Provider_LanguageModelTool_Description_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Tool description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L583)

<a id="Vercel_AI_Provider_LanguageModelTool_InputSchema_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement InputSchema { get; }
```


JSON Schema for the arguments.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L586)

<a id="Vercel_AI_Provider_LanguageModelTool_IsProviderDefined_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_IsProviderDefined"></a>

### IsProviderDefined


```csharp
public bool IsProviderDefined { get; }
```


Whether this is a provider-defined tool rather than a function tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L577)

<a id="Vercel_AI_Provider_LanguageModelTool_Name_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L571)

<a id="Vercel_AI_Provider_LanguageModelTool_OutputSchema_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_OutputSchema"></a>

### OutputSchema


```csharp
public JsonElement? OutputSchema { get; init; }
```


JSON Schema for the tool result. OpenAI Responses parses the result as JSON, so text results of this tool are sent as JSON string literals.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L594)

<a id="Vercel_AI_Provider_LanguageModelTool_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; init; }
```


Provider options of the tool, keyed by provider name, such as `{ "openai": { "omitFromRequestTools": true } }`.
Maps to the tool's `providerOptions`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L600)

<a id="Vercel_AI_Provider_LanguageModelTool_ProviderToolId_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_ProviderToolId"></a>

### ProviderToolId


```csharp
public string? ProviderToolId { get; }
```


Provider-defined tool id, or null for a function tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L574)

<a id="Vercel_AI_Provider_LanguageModelTool_Strict_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Strict"></a>

### Strict


```csharp
public bool? Strict { get; }
```


Responses strict mode. Null means unset, which Responses sends as false.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L589)

## Methods

<a id="Vercel_AI_Provider_LanguageModelTool_ProviderDefined_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_ProviderDefined_System_String_System_String_System_Text_Json_JsonElement_"></a>

### ProviderDefined(string, string, JsonElement)


```csharp
public static LanguageModelTool ProviderDefined(string id, string name, JsonElement args)
```


Creates a provider-defined tool. Maps to `{ type: 'provider', id, name, args }`.
&lt;code class="paramref"&gt;id&lt;/code&gt; is `<provider>.<tool>`. &lt;code class="paramref"&gt;args&lt;/code&gt; must be a JSON object.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `args` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [LanguageModelTool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L565)
