---
title: "ValidateUIMessagesOptions"
description: "Options of ValidateUIMessagesAsync(ValidateUIMessagesOptions). Maps to ValidateUIMessagesOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "ValidateUIMessagesOptions"
  hidden: true
---

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class ValidateUIMessagesOptions
```


Options of [ValidateUIMessagesAsync(ValidateUIMessagesOptions)](/api/Vercel.AI/Ai/#Vercel_AI_Ai_ValidateUIMessagesAsync_Vercel_AI_Ui_ValidateUIMessagesOptions_). Maps to `ValidateUIMessagesOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L13)

## Properties

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_DataSchemas_"></a>
<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_DataSchemas"></a>

### DataSchemas


```csharp
public IReadOnlyDictionary<string, JsonNode>? DataSchemas { get; set; }
```


JSON Schemas of the data parts, by data name. Without them the data parts are not validated. Maps to `dataSchemas`.

**Value:** [IReadOnlyDictionary&lt;string, JsonNode&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L28)

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_ExperimentalRefineToolInput_"></a>
<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_ExperimentalRefineToolInput"></a>

### ExperimentalRefineToolInput


```csharp
public IReadOnlyDictionary<string, Func<JsonElement, Task<JsonElement>>>? ExperimentalRefineToolInput { get; set; }
```


Rebuilds the input of a tool call from its `inputSchemaInput`, by tool name, for an input schema whose parsed value
differs from its input. Maps to `experimental_refineToolInput`.

**Value:** [IReadOnlyDictionary&lt;string, Func&lt;JsonElement, Task&lt;JsonElement&gt;&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L39)

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_Messages_"></a>
<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_Messages"></a>

### Messages


```csharp
public IReadOnlyList<UIMessage>? Messages { get; set; }
```


The messages to validate.

**Value:** [IReadOnlyList&lt;UIMessage&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L16)

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_MessagesJson_"></a>
<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_MessagesJson"></a>

### MessagesJson


```csharp
public JsonElement? MessagesJson { get; set; }
```


The messages as untyped JSON, such as a request body. They are validated structurally, and the result holds the typed
messages. Set either this or [Messages](#Vercel_AI_Ui_ValidateUIMessagesOptions_Messages). Maps to `messages: unknown`.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L22)

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_MetadataSchema_"></a>
<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_MetadataSchema"></a>

### MetadataSchema


```csharp
public JsonNode? MetadataSchema { get; set; }
```


JSON Schema of the message metadata. Without it the metadata is not validated. Maps to `metadataSchema`.

**Value:** [JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L25)

<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_Tools_"></a>
<a id="Vercel_AI_Ui_ValidateUIMessagesOptions_Tools"></a>

### Tools


```csharp
public IReadOnlyDictionary<string, Tool>? Tools { get; set; }
```


The tools of the tool parts, by name. Only the input and output schemas are used. Without them the tool parts are not validated.

**Value:** [IReadOnlyDictionary&lt;string, Tool&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/ValidateUIMessages.cs#L33)
