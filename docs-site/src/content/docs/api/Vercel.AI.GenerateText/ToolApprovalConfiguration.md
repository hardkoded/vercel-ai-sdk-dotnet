---
title: "ToolApprovalConfiguration"
description: "Configures whether individual tools require approval before they can run. Either use a generic function that is called for all tool calls, or set an approval..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolApprovalConfiguration"
  hidden: true
---

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration"></a>

Namespace: [Vercel.AI.GenerateText](/api/Vercel.AI.GenerateText/)


```csharp
public sealed class ToolApprovalConfiguration : IEnumerable<KeyValuePair<string, ToolApprovalEntry>>, IEnumerable
```


Configures whether individual tools require approval before they can run. Either use a generic function that is
called for all tool calls, or set an approval status (or a function that produces one) for each tool.
It takes precedence over the approval settings of the tools. Maps to `ToolApprovalConfiguration`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Implements:** [IEnumerable&lt;KeyValuePair&lt;string, ToolApprovalEntry&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1), [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)

**Example**

```csharp
var toolApproval = new ToolApprovalConfiguration
{
    { "deleteFile", "user-approval" },
    { "readFile", ToolApprovalStatus.Approved },
};
```

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L166)

## Constructors

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration__ctor_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration__ctor"></a>

### ToolApprovalConfiguration()


```csharp
public ToolApprovalConfiguration()
```


Creates a configuration with per-tool settings. Add them with `Add` or the indexer.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L171)

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration__ctor_Vercel_AI_GenerateText_GenericToolApprovalFunction_"></a>

### ToolApprovalConfiguration(GenericToolApprovalFunction)


```csharp
public ToolApprovalConfiguration(GenericToolApprovalFunction function)
```


Creates a configuration with a generic function that is called for all tool calls.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `function` | [GenericToolApprovalFunction](/api/Vercel.AI.GenerateText/GenericToolApprovalFunction/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L176)

## Properties

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Function_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Function"></a>

### Function


```csharp
public GenericToolApprovalFunction? Function { get; }
```


The generic function, or null for a per-tool configuration.

**Value:** [GenericToolApprovalFunction](/api/Vercel.AI.GenerateText/GenericToolApprovalFunction/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L182)

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Item_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Item_System_String_"></a>

### this[string]


```csharp
public ToolApprovalEntry this[string toolName] { get; set; }
```


Gets or sets the approval setting of a tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Value:** [ToolApprovalEntry](/api/Vercel.AI.GenerateText/ToolApprovalEntry/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L185)

## Methods

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Add_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Add_System_String_Vercel_AI_GenerateText_SingleToolApprovalFunction_"></a>

### Add(string, SingleToolApprovalFunction)


```csharp
public void Add(string toolName, SingleToolApprovalFunction function)
```


Sets the function that produces the status of a tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `function` | [SingleToolApprovalFunction](/api/Vercel.AI.GenerateText/SingleToolApprovalFunction/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L198)

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_Add_System_String_Vercel_AI_GenerateText_ToolApprovalStatus_"></a>

### Add(string, ToolApprovalStatus)


```csharp
public void Add(string toolName, ToolApprovalStatus status)
```


Sets the status of a tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `status` | [ToolApprovalStatus](/api/Vercel.AI.GenerateText/ToolApprovalStatus/) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L195)

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_GetEnumerator_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_GetEnumerator"></a>

### GetEnumerator()


```csharp
public IEnumerator<KeyValuePair<string, ToolApprovalEntry>> GetEnumerator()
```


Returns an enumerator that iterates through the collection.

**Returns:** [IEnumerator&lt;KeyValuePair&lt;string, ToolApprovalEntry&gt;&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerator-1) — An enumerator that can be used to iterate through the collection.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L207)

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_TryGetValue_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_TryGetValue_System_String_Vercel_AI_GenerateText_ToolApprovalEntry__"></a>

### TryGetValue(string, out ToolApprovalEntry?)


```csharp
public bool TryGetValue(string toolName, out ToolApprovalEntry? entry)
```


Returns the setting of a tool. It is not found for a tool that has none, whatever its name.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `entry` | [ToolApprovalEntry](/api/Vercel.AI.GenerateText/ToolApprovalEntry/) |  |

**Returns:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L201)

## Operators

<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_op_Implicit_"></a>
<a id="Vercel_AI_GenerateText_ToolApprovalConfiguration_op_Implicit_Vercel_AI_GenerateText_GenericToolApprovalFunction__Vercel_AI_GenerateText_ToolApprovalConfiguration"></a>

### implicit operator ToolApprovalConfiguration(GenericToolApprovalFunction)


```csharp
public static implicit operator ToolApprovalConfiguration(GenericToolApprovalFunction function)
```


Converts a generic function to a configuration.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `function` | [GenericToolApprovalFunction](/api/Vercel.AI.GenerateText/GenericToolApprovalFunction/) |  |

**Returns:** [ToolApprovalConfiguration](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/GenerateText/ToolApprovalConfiguration.cs#L192)
