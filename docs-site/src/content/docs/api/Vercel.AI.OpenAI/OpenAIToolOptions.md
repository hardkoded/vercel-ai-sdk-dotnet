---
title: "OpenAIToolOptions"
description: "The openai options of a tool. Maps to OpenAIToolOptions in openai-responses-prepare-tools.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIToolOptions"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIToolOptions"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIToolOptions
```


The `openai` options of a tool. Maps to `OpenAIToolOptions` in `openai-responses-prepare-tools.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L11)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_AllowedCallers_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_AllowedCallers"></a>

### AllowedCallers


```csharp
public IReadOnlyList<string>? AllowedCallers { get; set; }
```


Who may call the tool: `direct` and/or `programmatic`.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L14)

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_Async_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_Async"></a>

### Async


```csharp
public bool? Async { get; set; }
```


Whether the model can continue generating after calling this tool without waiting for its result.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L17)

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_DeferLoading_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_DeferLoading"></a>

### DeferLoading


```csharp
public bool? DeferLoading { get; set; }
```


Defers loading the tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L20)

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_Namespace_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_Namespace"></a>

### Namespace


```csharp
public OpenAIToolNamespace? Namespace { get; set; }
```


The namespace the tool belongs to.

**Value:** [OpenAIToolNamespace](/api/Vercel.AI.OpenAI/OpenAIToolNamespace/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L32)

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_OmitFromRequestTools_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_OmitFromRequestTools"></a>

### OmitFromRequestTools


```csharp
public bool? OmitFromRequestTools { get; set; }
```


Omits this function from the Responses request's top-level tools array. Use for tools supplied in history
through `additionalTools`. The tool stays active in the SDK so its calls can still be validated and executed.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L26)

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_OutputSchema_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_OutputSchema"></a>

### OutputSchema


```csharp
public JsonObject? OutputSchema { get; set; }
```


The JSON Schema of the tool output, an object.

**Value:** [JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L29)

## Methods

<a id="Vercel_AI_OpenAI_OpenAIToolOptions_ToProviderOptions_"></a>
<a id="Vercel_AI_OpenAI_OpenAIToolOptions_ToProviderOptions_System_String_"></a>

### ToProviderOptions(string)


```csharp
public JsonElement ToProviderOptions(string providerOptionsName = "openai")
```


Returns the options as tool provider options under &lt;code class="paramref"&gt;providerOptionsName&lt;/code&gt; (`openai` by
default), ready for `Tool.ProviderOptions`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerOptionsName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/Responses/OpenAIToolOptions.cs#L38)
