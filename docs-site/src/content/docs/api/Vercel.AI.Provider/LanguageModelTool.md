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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L662)

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

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L665)

## Properties

<a id="Vercel_AI_Provider_LanguageModelTool_Args_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Args"></a>

### Args


```csharp
public JsonElement Args { get; }
```


Arguments of a provider-defined tool. Undefined for a function tool.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L710)

<a id="Vercel_AI_Provider_LanguageModelTool_Description_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Tool description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L741)

<a id="Vercel_AI_Provider_LanguageModelTool_InputSchema_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement InputSchema { get; }
```


JSON Schema for the arguments.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L744)

<a id="Vercel_AI_Provider_LanguageModelTool_IsProviderDefined_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_IsProviderDefined"></a>

### IsProviderDefined


```csharp
public bool IsProviderDefined { get; }
```


Whether this is a provider-defined tool rather than a function tool.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L707)

<a id="Vercel_AI_Provider_LanguageModelTool_Name_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L701)

<a id="Vercel_AI_Provider_LanguageModelTool_OutputSchema_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_OutputSchema"></a>

### OutputSchema


```csharp
public JsonElement? OutputSchema { get; init; }
```


JSON Schema for the tool result. OpenAI Responses parses the result as JSON, so text results of this tool are sent as JSON string literals.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L752)

<a id="Vercel_AI_Provider_LanguageModelTool_ProviderOptions_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; init; }
```


Provider options of the tool, keyed by provider name, such as `{ "openai": { "omitFromRequestTools": true } }`.
Maps to the tool's `providerOptions`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L758)

<a id="Vercel_AI_Provider_LanguageModelTool_ProviderToolId_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_ProviderToolId"></a>

### ProviderToolId


```csharp
public string? ProviderToolId { get; }
```


Provider-defined tool id, or null for a function tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L704)

<a id="Vercel_AI_Provider_LanguageModelTool_RedactArgs_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_RedactArgs"></a>

### RedactArgs


```csharp
public Func<JsonElement, JsonElement>? RedactArgs { get; init; }
```


Provider-owned telemetry view of [Args](#Vercel_AI_Provider_LanguageModelTool_Args), which hides credentials. Maps to the `redactArgs` option of a provider tool factory.

**Value:** [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L729)

<a id="Vercel_AI_Provider_LanguageModelTool_Strict_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_Strict"></a>

### Strict


```csharp
public bool? Strict { get; }
```


Responses strict mode. Null means unset, which Responses sends as false.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L747)

## Methods

<a id="Vercel_AI_Provider_LanguageModelTool_ProviderDefined_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_ProviderDefined_System_String_System_String_System_Text_Json_JsonElement_System_Func_System_Text_Json_JsonElement_System_Text_Json_JsonElement__"></a>

### `ProviderDefined(string, string, JsonElement, Func<JsonElement, JsonElement>?)`


```csharp
public static LanguageModelTool ProviderDefined(string id, string name, JsonElement args, Func<JsonElement, JsonElement>? redactArgs = null)
```


Creates a provider-defined tool. Maps to `{ type: 'provider', id, name, args }`.
&lt;code class="paramref"&gt;id&lt;/code&gt; is `<provider>.<tool>`. &lt;code class="paramref"&gt;args&lt;/code&gt; must be a JSON object.
&lt;code class="paramref"&gt;redactArgs&lt;/code&gt; is the provider-owned telemetry view of the arguments. Maps to the `redactArgs` option of a provider tool factory.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `args` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `redactArgs` | [Func&lt;JsonElement, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.func-2) |  |

**Returns:** [LanguageModelTool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L690)

<a id="Vercel_AI_Provider_LanguageModelTool_RedactTelemetryArgs_"></a>
<a id="Vercel_AI_Provider_LanguageModelTool_RedactTelemetryArgs"></a>

### RedactTelemetryArgs()


```csharp
public JsonElement? RedactTelemetryArgs()
```


Applies [RedactArgs](#Vercel_AI_Provider_LanguageModelTool_RedactArgs) to [Args](#Vercel_AI_Provider_LanguageModelTool_Args) for telemetry, leaving [Args](#Vercel_AI_Provider_LanguageModelTool_Args) unchanged. Returns [Args](#Vercel_AI_Provider_LanguageModelTool_Args) when the tool has no
redactor, and null when the redactor throws, so a broken redactor neither exposes credentials nor fails the call. Maps to `redactTelemetryValue`.

**Returns:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L735)
