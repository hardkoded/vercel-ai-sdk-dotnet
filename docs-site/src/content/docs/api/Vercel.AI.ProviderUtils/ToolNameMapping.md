---
title: "ToolNameMapping"
description: "Maps client tool names to provider tool names. Maps to ToolNameMapping."
editUrl: false
prev: false
next: false
sidebar:
  label: "ToolNameMapping"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ToolNameMapping"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ToolNameMapping
```


Maps client tool names to provider tool names. Maps to `ToolNameMapping`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L29)

## Methods

<a id="Vercel_AI_ProviderUtils_ToolNameMapping_ToCustomToolName_"></a>
<a id="Vercel_AI_ProviderUtils_ToolNameMapping_ToCustomToolName_System_String_"></a>

### ToCustomToolName(string)


```csharp
public string ToCustomToolName(string providerToolName)
```


Maps a provider name to the client name, or returns the input when it is not mapped.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `providerToolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L48)

<a id="Vercel_AI_ProviderUtils_ToolNameMapping_ToProviderToolName_"></a>
<a id="Vercel_AI_ProviderUtils_ToolNameMapping_ToProviderToolName_System_String_"></a>

### ToProviderToolName(string)


```csharp
public string ToProviderToolName(string customToolName)
```


Maps a client name to the provider name, or returns the input when it is not mapped.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `customToolName` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

**Returns:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ToolNameMapping.cs#L41)
