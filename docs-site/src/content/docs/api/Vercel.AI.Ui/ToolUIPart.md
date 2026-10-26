---
title: "ToolUIPart"
description: "A tool invocation part. Its type is tool-{name}, or dynamic-tool for a tool whose name is not known up front. Maps to ToolUIPart and DynamicToolUIPart."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolUIPart"
  hidden: true
---

<a id="Vercel_AI_Ui_ToolUIPart"></a>

Namespace: [Vercel.AI.Ui](/api/Vercel.AI.Ui/)


```csharp
public sealed class ToolUIPart : UIMessagePart
```


A tool invocation part. Its type is `tool-{name}`, or `dynamic-tool` for a tool whose name is not known up front.
Maps to `ToolUIPart` and `DynamicToolUIPart`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [UIMessagePart](/api/Vercel.AI.Ui/UIMessagePart/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L306)

## Constructors

<a id="Vercel_AI_Ui_ToolUIPart__ctor_"></a>
<a id="Vercel_AI_Ui_ToolUIPart__ctor_System_String_System_String_Vercel_AI_Ui_ToolUIPartState_System_Boolean_"></a>

### ToolUIPart(string, string, ToolUIPartState, bool)


```csharp
public ToolUIPart(string toolName, string toolCallId, ToolUIPartState state, bool isDynamic = false)
```


Creates a tool part for the tool &lt;code class="paramref"&gt;toolName&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `toolCallId` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `state` | [ToolUIPartState](/api/Vercel.AI.Ui/ToolUIPartState/) |  |
| `isDynamic` | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L309)

## Properties

<a id="Vercel_AI_Ui_ToolUIPart_Approval_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_Approval"></a>

### Approval


```csharp
public ToolUIApproval? Approval { get; init; }
```


The approval of the call.

**Value:** [ToolUIApproval](/api/Vercel.AI.Ui/ToolUIApproval/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L364)

<a id="Vercel_AI_Ui_ToolUIPart_CallProviderMetadata_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_CallProviderMetadata"></a>

### CallProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? CallProviderMetadata { get; init; }
```


The provider metadata of the call.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L358)

<a id="Vercel_AI_Ui_ToolUIPart_Dynamic_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_Dynamic"></a>

### Dynamic


```csharp
public bool? Dynamic { get; init; }
```


For a `dynamic-tool` part, false when the part came from a static tool that is no longer available.
Maps to `dynamic: false`.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L334)

<a id="Vercel_AI_Ui_ToolUIPart_ErrorText_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_ErrorText"></a>

### ErrorText


```csharp
public string? ErrorText { get; init; }
```


The error text of a failed call.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L352)

<a id="Vercel_AI_Ui_ToolUIPart_Input_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_Input"></a>

### Input


```csharp
public JsonElement? Input { get; init; }
```


The tool input. Null when it is undefined.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L343)

<a id="Vercel_AI_Ui_ToolUIPart_IsDynamic_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_IsDynamic"></a>

### IsDynamic


```csharp
public bool IsDynamic { get; }
```


True for a `dynamic-tool` part.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L328)

<a id="Vercel_AI_Ui_ToolUIPart_Output_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_Output"></a>

### Output


```csharp
public JsonElement? Output { get; init; }
```


The tool output. Null when it is undefined.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L349)

<a id="Vercel_AI_Ui_ToolUIPart_Preliminary_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_Preliminary"></a>

### Preliminary


```csharp
public bool? Preliminary { get; init; }
```


True when the output is a preliminary result.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L355)

<a id="Vercel_AI_Ui_ToolUIPart_ProviderExecuted_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_ProviderExecuted"></a>

### ProviderExecuted


```csharp
public bool? ProviderExecuted { get; init; }
```


Whether the provider executed the tool.

**Value:** [bool?](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L340)

<a id="Vercel_AI_Ui_ToolUIPart_RawInput_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_RawInput"></a>

### RawInput


```csharp
public JsonElement? RawInput { get; init; }
```


The raw input of a failed or streaming call.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L346)

<a id="Vercel_AI_Ui_ToolUIPart_ResultProviderMetadata_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_ResultProviderMetadata"></a>

### ResultProviderMetadata


```csharp
public IReadOnlyDictionary<string, JsonElement>? ResultProviderMetadata { get; init; }
```


The provider metadata of the result.

**Value:** [IReadOnlyDictionary&lt;string, JsonElement&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L361)

<a id="Vercel_AI_Ui_ToolUIPart_State_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_State"></a>

### State


```csharp
public ToolUIPartState State { get; }
```


The state of the invocation.

**Value:** [ToolUIPartState](/api/Vercel.AI.Ui/ToolUIPartState/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L325)

<a id="Vercel_AI_Ui_ToolUIPart_Title_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_Title"></a>

### Title


```csharp
public string? Title { get; init; }
```


The title of the tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L337)

<a id="Vercel_AI_Ui_ToolUIPart_ToolCallId_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_ToolCallId"></a>

### ToolCallId


```csharp
public string ToolCallId { get; }
```


The tool call id.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L322)

<a id="Vercel_AI_Ui_ToolUIPart_ToolName_"></a>
<a id="Vercel_AI_Ui_ToolUIPart_ToolName"></a>

### ToolName


```csharp
public string ToolName { get; }
```


The tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Ui/UIMessages.cs#L319)
