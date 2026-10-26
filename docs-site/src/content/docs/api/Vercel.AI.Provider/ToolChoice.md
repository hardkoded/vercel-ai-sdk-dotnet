---
title: "ToolChoice"
description: "How the model should choose tools. Maps to toolChoice."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolChoice"
  hidden: true
---

<a id="Vercel_AI_Provider_ToolChoice"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public abstract class ToolChoice
```


How the model should choose tools. Maps to `toolChoice`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [ToolChoice.NamedChoice](/api/Vercel.AI.Provider/ToolChoice.NamedChoice/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L474)

## Properties

<a id="Vercel_AI_Provider_ToolChoice_Auto_"></a>
<a id="Vercel_AI_Provider_ToolChoice_Auto"></a>

### Auto


```csharp
public static ToolChoice Auto { get; }
```


The model may call tools.

**Value:** [ToolChoice](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L485)

<a id="Vercel_AI_Provider_ToolChoice_None_"></a>
<a id="Vercel_AI_Provider_ToolChoice_None"></a>

### None


```csharp
public static ToolChoice None { get; }
```


The model must not call tools.

**Value:** [ToolChoice](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L488)

<a id="Vercel_AI_Provider_ToolChoice_Required_"></a>
<a id="Vercel_AI_Provider_ToolChoice_Required"></a>

### Required


```csharp
public static ToolChoice Required { get; }
```


The model must call a tool.

**Value:** [ToolChoice](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L491)

<a id="Vercel_AI_Provider_ToolChoice_Type_"></a>
<a id="Vercel_AI_Provider_ToolChoice_Type"></a>

### Type


```csharp
public string Type { get; }
```


Choice type: auto, none, required, or tool.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L482)

## Methods

<a id="Vercel_AI_Provider_ToolChoice_Tool_"></a>
<a id="Vercel_AI_Provider_ToolChoice_Tool_System_String_"></a>

### Tool(string)


```csharp
public static ToolChoice Tool(string toolName)
```


The model must call &lt;code class="paramref"&gt;toolName&lt;/code&gt;.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `toolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [ToolChoice](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/LanguageModel.cs#L494)
