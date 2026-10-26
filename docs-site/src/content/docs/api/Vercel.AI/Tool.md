---
title: "Tool"
description: "A tool the model can call. Maps to tool()."
editUrl: false
prev: false
next: false
sidebar:
  label: "Tool"
  hidden: true
---

<a id="Vercel_AI_Tool"></a>

Namespace: [Vercel.AI](/api/Vercel.AI/)


```csharp
public sealed class Tool
```


A tool the model can call. Maps to `tool()`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L14)

## Constructors

<a id="Vercel_AI_Tool__ctor_"></a>
<a id="Vercel_AI_Tool__ctor_System_String_System_String_System_Text_Json_JsonElement_System_Func_System_Text_Json_JsonElement_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_String___System_Nullable_System_Boolean__"></a>

### `Tool(string, string?, JsonElement, Func<JsonElement, CancellationToken, Task<string>>?, bool?)`


```csharp
public Tool(string name, string? description, JsonElement inputSchema, Func<JsonElement, CancellationToken, Task<string>>? execute, bool? strict = null)
```


Creates a function tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `inputSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `execute` | [Func&lt;JsonElement, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `strict` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L17)

## Properties

<a id="Vercel_AI_Tool_Description_"></a>
<a id="Vercel_AI_Tool_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Tool description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L30)

<a id="Vercel_AI_Tool_Execute_"></a>
<a id="Vercel_AI_Tool_Execute"></a>

### Execute


```csharp
public Func<JsonElement, CancellationToken, Task<string>>? Execute { get; }
```


Executes the tool and returns a JSON string. Null means the call is not executed locally, unless [ExecuteWithOptions](#Vercel_AI_Tool_ExecuteWithOptions) is set.

**Value:** [Func&lt;JsonElement, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L45)

<a id="Vercel_AI_Tool_ExecuteWithOptions_"></a>
<a id="Vercel_AI_Tool_ExecuteWithOptions"></a>

### ExecuteWithOptions


```csharp
public Func<JsonElement, ToolExecutionOptions, Task<string>>? ExecuteWithOptions { get; set; }
```


Executes the tool with the options of the call, such as the messages of the step. When it is set, the loop calls it
instead of [Execute](#Vercel_AI_Tool_Execute). Maps to `execute(input, { toolCallId, messages, abortSignal })`.

**Value:** [Func&lt;JsonElement, ToolExecutionOptions, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L54)

<a id="Vercel_AI_Tool_InputSchema_"></a>
<a id="Vercel_AI_Tool_InputSchema"></a>

### InputSchema


```csharp
public JsonElement InputSchema { get; }
```


JSON Schema for the arguments. Undefined for a provider-defined tool declared without one.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L42)

<a id="Vercel_AI_Tool_IsDynamic_"></a>
<a id="Vercel_AI_Tool_IsDynamic"></a>

### IsDynamic


```csharp
public bool IsDynamic { get; set; }
```


True for a tool whose input type is not known at compile time, such as an MCP tool. Parsed calls of the tool are dynamic. Maps to `dynamicTool`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L39)

<a id="Vercel_AI_Tool_IsProviderDefined_"></a>
<a id="Vercel_AI_Tool_IsProviderDefined"></a>

### IsProviderDefined


```csharp
public bool IsProviderDefined { get; }
```


Whether this is a provider-defined tool, which the provider executes. Maps to `{ type: 'provider', id, args }`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L92)

<a id="Vercel_AI_Tool_Metadata_"></a>
<a id="Vercel_AI_Tool_Metadata"></a>

### Metadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? Metadata { get; set; }
```


Metadata of the tool, such as the name of the client that created it. Parsed calls of the tool carry it as tool metadata, apart from provider metadata.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L36)

<a id="Vercel_AI_Tool_Name_"></a>
<a id="Vercel_AI_Tool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L27)

<a id="Vercel_AI_Tool_NeedsApproval_"></a>
<a id="Vercel_AI_Tool_NeedsApproval"></a>

### NeedsApproval


```csharp
public bool NeedsApproval { get; set; }
```


When true, a call to this tool is not executed. The loop ends at that step and the call is left for the caller,
unless [ApproveTool](/api/Vercel.AI/GenerateTextOptions/#Vercel_AI_GenerateTextOptions_ApproveTool) is set, which then decides. Maps to `needsApproval`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L66)

<a id="Vercel_AI_Tool_ProviderOptions_"></a>
<a id="Vercel_AI_Tool_ProviderOptions"></a>

### ProviderOptions


```csharp
public JsonElement? ProviderOptions { get; set; }
```


Provider options of the tool, keyed by provider name, such as `{ "openai": { "omitFromRequestTools": true } }`.
Maps to the tool's `providerOptions`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L72)

<a id="Vercel_AI_Tool_ProviderToolArgs_"></a>
<a id="Vercel_AI_Tool_ProviderToolArgs"></a>

### ProviderToolArgs


```csharp
public JsonElement ProviderToolArgs { get; }
```


Arguments of a provider-defined tool. Undefined for a function tool.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L89)

<a id="Vercel_AI_Tool_ProviderToolId_"></a>
<a id="Vercel_AI_Tool_ProviderToolId"></a>

### ProviderToolId


```csharp
public string? ProviderToolId { get; }
```


Provider-defined tool id (`<provider>.<tool>`), or null for a function tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L86)

<a id="Vercel_AI_Tool_Strict_"></a>
<a id="Vercel_AI_Tool_Strict"></a>

### Strict


```csharp
public bool? Strict { get; }
```


Responses strict mode. Null means unset, which Responses sends as false.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L48)

<a id="Vercel_AI_Tool_Title_"></a>
<a id="Vercel_AI_Tool_Title"></a>

### Title


```csharp
public string? Title { get; set; }
```


Display title of the tool. Parsed calls of the tool carry it.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L33)

<a id="Vercel_AI_Tool_ToModelOutput_"></a>
<a id="Vercel_AI_Tool_ToModelOutput"></a>

### ToModelOutput


```csharp
public Func<ToolModelOutputCall, ToolModelOutput>? ToModelOutput { get; set; }
```


Converts the JSON output of the tool into the result sent to the model. Null sends the output as it is.
Maps to `toModelOutput`.

**Value:** [Func&lt;ToolModelOutputCall, ToolModelOutput&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L60)

## Methods

<a id="Vercel_AI_Tool_DynamicTool_"></a>
<a id="Vercel_AI_Tool_DynamicTool_System_String_System_String_System_String_System_Func_System_Text_Json_JsonElement_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_String___"></a>

### `DynamicTool(string, string?, string, Func<JsonElement, CancellationToken, Task<string>>?)`


```csharp
public static Tool DynamicTool(string name, string? description, string jsonSchema, Func<JsonElement, CancellationToken, Task<string>>? execute)
```


Creates a dynamic tool from a JSON schema string. Maps to `dynamicTool`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `jsonSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `execute` | [Func&lt;JsonElement, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |

**Returns:** [Tool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L113)

<a id="Vercel_AI_Tool_Function_"></a>
<a id="Vercel_AI_Tool_Function_System_String_System_String_System_String_System_Func_System_Text_Json_JsonElement_System_Threading_CancellationToken_System_Threading_Tasks_Task_System_String___System_Nullable_System_Boolean__"></a>

### `Function(string, string?, string, Func<JsonElement, CancellationToken, Task<string>>?, bool?)`


```csharp
public static Tool Function(string name, string? description, string jsonSchema, Func<JsonElement, CancellationToken, Task<string>>? execute, bool? strict = null)
```


Creates a tool from a JSON schema string.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `jsonSchema` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `execute` | [Func&lt;JsonElement, CancellationToken, Task&lt;string&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-3) |  |
| `strict` | [bool?](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

**Returns:** [Tool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L121)

<a id="Vercel_AI_Tool_ProviderDefined_"></a>
<a id="Vercel_AI_Tool_ProviderDefined_System_String_System_String_System_Text_Json_JsonElement_"></a>

### ProviderDefined(string, string, JsonElement)


```csharp
public static Tool ProviderDefined(string id, string name, JsonElement args)
```


Declares a provider-defined tool. The provider executes it, so it has no [Execute](#Vercel_AI_Tool_Execute).
&lt;code class="paramref"&gt;args&lt;/code&gt; must be a JSON object. Maps to `JsonElement)`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `args` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [Tool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L98)

<a id="Vercel_AI_Tool_ProviderDefined_System_String_System_String_System_Text_Json_JsonElement_System_Text_Json_JsonElement_"></a>

### ProviderDefined(string, string, JsonElement, JsonElement)


```csharp
public static Tool ProviderDefined(string id, string name, JsonElement args, JsonElement inputSchema)
```


Declares a provider-defined tool with an input schema. The input of a call the provider ran is parsed with the
schema, so a property the schema does not declare is dropped, as a Zod object does. Maps to a provider tool's `inputSchema`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `args` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `inputSchema` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

**Returns:** [Tool](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/PublicTypes.cs#L107)
