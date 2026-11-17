---
title: "OpenResponsesExtension"
description: "One Open Responses extension. Tool, item, and event capabilities are registered independently."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenResponsesExtension"
  hidden: true
---

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension"></a>

Namespace: [Vercel.AI.OpenResponses](/api/Vercel.AI.OpenResponses/)


```csharp
public sealed class OpenResponsesExtension
```


One Open Responses extension. Tool, item, and event capabilities are registered independently.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L93)

## Properties

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_AllowBareTypes_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_AllowBareTypes"></a>

### AllowBareTypes


```csharp
public bool AllowBareTypes { get; set; }
```


Explicitly enables non-portable bare discriminator registration for this extension. It must be true exactly when
[BareToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_BareToolType), [BareItemTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_BareItemTypes), or [BareEventTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_BareEventTypes) is set.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L105)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_BareEventTypes_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_BareEventTypes"></a>

### BareEventTypes


```csharp
public IReadOnlyList<string>? BareEventTypes { get; set; }
```


Exact non-namespaced streaming event types decoded by this extension. Must be provided together with [DecodeEvent](#Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeEvent).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L117)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_BareItemTypes_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_BareItemTypes"></a>

### BareItemTypes


```csharp
public IReadOnlyList<string>? BareItemTypes { get; set; }
```


Exact non-namespaced item types decoded by a documented implementation extension. Must be provided together with [DecodeItem](#Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeItem).

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L114)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_BareToolType_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_BareToolType"></a>

### BareToolType


```csharp
public string? BareToolType { get; set; }
```


Exact non-namespaced tool type used by a documented implementation extension. Must be provided together with
[EncodeTool](#Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeTool) and cannot be used together with [ToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType).

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L111)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeEvent_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeEvent"></a>

### DecodeEvent


```csharp
public Func<OpenResponsesDecodeEventContext, IReadOnlyList<LanguageModelStreamPart>?>? DecodeEvent { get; set; }
```


Decodes a streaming event into stream parts. Present or absent together with [EventTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_EventTypes).

**Value:** [Func&lt;OpenResponsesDecodeEventContext, IReadOnlyList&lt;LanguageModelStreamPart&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L144)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeItem_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeItem"></a>

### DecodeItem


```csharp
public Func<OpenResponsesDecodeItemContext, IReadOnlyList<GeneratedContent>?>? DecodeItem { get; set; }
```


Decodes a completed item into content parts. The model adds a `open-responses.extension-replay` custom part holding the original item. Present or absent together with [ItemTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_ItemTypes).

**Value:** [Func&lt;OpenResponsesDecodeItemContext, IReadOnlyList&lt;GeneratedContent&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L132)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeInputItem_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeInputItem"></a>

### EncodeInputItem


```csharp
public Func<OpenResponsesEncodeInputItemContext, IReadOnlyList<JsonObject>?>? EncodeInputItem { get; set; }
```


Encodes a history part when no original wire item is available. Every returned item must use one of [ItemTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_ItemTypes). Requires an item decoder.

**Value:** [Func&lt;OpenResponsesEncodeInputItemContext, IReadOnlyList&lt;JsonObject&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L138)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeTool_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeTool"></a>

### EncodeTool


```csharp
public Func<OpenResponsesToolContext, JsonObject?>? EncodeTool { get; set; }
```


Encodes a provider tool. Return null when its arguments cannot be encoded. The model adds [ToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType) and omits a tool choice that selects the omitted tool. Present or absent together with [ToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType).

**Value:** [Func&lt;OpenResponsesToolContext, JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L126)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeToolChoice_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeToolChoice"></a>

### EncodeToolChoice


```csharp
public Func<OpenResponsesToolContext, JsonObject?>? EncodeToolChoice { get; set; }
```


Encodes a specific tool choice. The model adds [ToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType). The default is an object holding only [ToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType). Requires a tool encoder.

**Value:** [Func&lt;OpenResponsesToolContext, JsonObject&gt;](https://learn.microsoft.com/dotnet/api/system.func-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L129)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EventTypes_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_EventTypes"></a>

### EventTypes


```csharp
public IReadOnlyList<string>? EventTypes { get; set; }
```


Namespaced event types. Required together with [DecodeEvent](#Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeEvent), unless [BareEventTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_BareEventTypes) is used.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L123)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_Id_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_Id"></a>

### Id


```csharp
public string Id { get; set; }
```


Extension id in `implementor.extension` form.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L96)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_ItemTypes_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_ItemTypes"></a>

### ItemTypes


```csharp
public IReadOnlyList<string>? ItemTypes { get; set; }
```


Namespaced item types. Required together with [DecodeItem](#Vercel_AI_OpenResponses_OpenResponsesExtension_DecodeItem), unless [BareItemTypes](#Vercel_AI_OpenResponses_OpenResponsesExtension_BareItemTypes) is used.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L120)

<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType_"></a>
<a id="Vercel_AI_OpenResponses_OpenResponsesExtension_ToolType"></a>

### ToolType


```csharp
public string? ToolType { get; set; }
```


Namespaced tool type. Required together with [EncodeTool](#Vercel_AI_OpenResponses_OpenResponsesExtension_EncodeTool), unless [BareToolType](#Vercel_AI_OpenResponses_OpenResponsesExtension_BareToolType) is used.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenResponses/OpenResponsesExtension.cs#L99)
