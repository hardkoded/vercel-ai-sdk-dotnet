---
title: "RealtimeModelV4ToolDefinition"
description: "A tool definition in a realtime session config. Maps to RealtimeModelV4ToolDefinition."
editUrl: false
prev: false
next: false
sidebar:
  label: "RealtimeModelV4ToolDefinition"
  hidden: true
---

<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition"></a>

Namespace: [Vercel.AI.Provider](/api/Vercel.AI.Provider/)


```csharp
public sealed class RealtimeModelV4ToolDefinition
```


A tool definition in a realtime session config. Maps to `RealtimeModelV4ToolDefinition`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L112)

## Constructors

<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition__ctor_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition__ctor_System_String_System_String_System_Text_Json_JsonElement_"></a>

### RealtimeModelV4ToolDefinition(string, string?, JsonElement)


```csharp
public RealtimeModelV4ToolDefinition(string name, string? description, JsonElement parameters)
```


Creates a function tool definition.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `name` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `description` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `parameters` | [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L115)

## Properties

<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition_Description_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition_Description"></a>

### Description


```csharp
public string? Description { get; }
```


Function description.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L126)

<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition_Name_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition_Name"></a>

### Name


```csharp
public string Name { get; }
```


Function name.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L123)

<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition_Parameters_"></a>
<a id="Vercel_AI_Provider_RealtimeModelV4ToolDefinition_Parameters"></a>

### Parameters


```csharp
public JsonElement Parameters { get; }
```


JSON Schema of the parameters.

**Value:** [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Provider/RealtimeModelV4Events.cs#L129)
