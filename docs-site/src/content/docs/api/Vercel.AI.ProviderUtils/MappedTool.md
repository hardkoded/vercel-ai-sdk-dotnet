---
title: "MappedTool"
description: "A tool passed to a language model when building a name map."
editUrl: false
prev: false
next: false
sidebar:
  label: "MappedTool"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_MappedTool"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class MappedTool
```


A tool passed to a language model when building a name map.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L8)

## Constructors

<a id="Vercel_AI_ProviderUtils_MappedTool__ctor_"></a>
<a id="Vercel_AI_ProviderUtils_MappedTool__ctor_System_String_System_String_System_String_"></a>

### MappedTool(string, string, string?)


```csharp
public MappedTool(string type, string name, string? id = null)
```


Creates a tool description.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `type` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L11)

## Properties

<a id="Vercel_AI_ProviderUtils_MappedTool_Id_"></a>
<a id="Vercel_AI_ProviderUtils_MappedTool_Id"></a>

### Id


```csharp
public string? Id { get; }
```


Provider tool id, such as `anthropic.computer-use`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L25)

<a id="Vercel_AI_ProviderUtils_MappedTool_Name_"></a>
<a id="Vercel_AI_ProviderUtils_MappedTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Client tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L22)

<a id="Vercel_AI_ProviderUtils_MappedTool_Type_"></a>
<a id="Vercel_AI_ProviderUtils_MappedTool_Type"></a>

### Type


```csharp
public string Type { get; }
```


`provider` or `function`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L19)
