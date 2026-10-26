---
title: "GoogleRealtimeTool"
description: "A realtime function tool."
editUrl: false
prev: false
next: false
sidebar:
  label: "GoogleRealtimeTool"
  hidden: true
---

<a id="Vercel_AI_Google_GoogleRealtimeTool"></a>

Namespace: [Vercel.AI.Google](/api/Vercel.AI.Google/)


```csharp
public sealed class GoogleRealtimeTool
```


A realtime function tool.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L86)

## Constructors

<a id="Vercel_AI_Google_GoogleRealtimeTool__ctor_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeTool__ctor_System_String_System_String_System_Nullable_System_Text_Json_JsonElement__"></a>

### GoogleRealtimeTool(string, string?, JsonElement?)


```csharp
public GoogleRealtimeTool(string name, string? description, JsonElement? parameters)
```


Creates a tool.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `parameters` | [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L89)

## Properties

<a id="Vercel_AI_Google_GoogleRealtimeTool_Description_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeTool_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L100)

<a id="Vercel_AI_Google_GoogleRealtimeTool_Name_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeTool_Name"></a>

### Name


```csharp
public string Name { get; }
```


Tool name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L97)

<a id="Vercel_AI_Google_GoogleRealtimeTool_Parameters_"></a>
<a id="Vercel_AI_Google_GoogleRealtimeTool_Parameters"></a>

### Parameters


```csharp
public JsonElement? Parameters { get; }
```


JSON Schema parameters.

**Value:** [JsonElement?](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Google/GoogleRealtime.cs#L103)
