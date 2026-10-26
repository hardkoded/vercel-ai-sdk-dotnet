---
title: "GoogleProviderTool"
description: "A Gemini provider-defined tool such as Google Search."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleProviderTool"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleProviderTool"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleProviderTool
```


A Gemini provider-defined tool such as Google Search.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L12)

## Constructors

<a id="Vercel_AI_Google_GoogleProviderTool__ctor_"></a>
<a id="Vercel_AI_Google_GoogleProviderTool__ctor_System_String_System_Nullable_System_Text_Json_JsonElement__System_String_"></a>

### GoogleProviderTool(string, JsonElement?, string?)


```csharp
public GoogleProviderTool(string id, JsonElement? args = null, string? name = null)
```


Creates a provider tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `id` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `args` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L15)

## Properties

<a id="Vercel_AI_Google_GoogleProviderTool_Args_"></a>
<a id="Vercel_AI_Google_GoogleProviderTool_Args"></a>

### Args


```csharp
public JsonElement? Args { get; }
```


Tool arguments.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L26)

<a id="Vercel_AI_Google_GoogleProviderTool_Id_"></a>
<a id="Vercel_AI_Google_GoogleProviderTool_Id"></a>

### Id


```csharp
public string Id { get; }
```


Tool id, such as `google.google_search`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L23)

<a id="Vercel_AI_Google_GoogleProviderTool_Name_"></a>
<a id="Vercel_AI_Google_GoogleProviderTool_Name"></a>

### Name


```csharp
public string? Name { get; }
```


Custom name used when the model echoes a built-in tool such as code execution.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleTools.cs#L29)
