---
title: "ProviderValues.RuntimeProbe"
description: "Inputs for GetRuntimeEnvironmentUserAgent(RuntimeProbe)."
editUrl: false
prev: false
next: false
sidebar:
  label: "ProviderValues.RuntimeProbe"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class ProviderValues.RuntimeProbe
```


Inputs for [GetRuntimeEnvironmentUserAgent(RuntimeProbe)](/api/Vercel.AI.ProviderUtils/ProviderValues/#Vercel_AI_ProviderUtils_ProviderValues_GetRuntimeEnvironmentUserAgent_Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_).

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L331)

## Properties

<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_EdgeRuntime_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_EdgeRuntime"></a>

### EdgeRuntime


```csharp
public bool EdgeRuntime { get; set; }
```


True for the Vercel Edge runtime.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L346)

<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_NavigatorUserAgent_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_NavigatorUserAgent"></a>

### NavigatorUserAgent


```csharp
public string? NavigatorUserAgent { get; set; }
```


Navigator user agent, when the host exposes one.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L337)

<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_NodeVersion_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_NodeVersion"></a>

### NodeVersion


```csharp
public string? NodeVersion { get; set; }
```


Node version marker. Any non-empty value selects the Node branch.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L340)

<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_ProcessVersion_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_ProcessVersion"></a>

### ProcessVersion


```csharp
public string? ProcessVersion { get; set; }
```


`process.version` text appended after `runtime/node.js/`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L343)

<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_Window_"></a>
<a id="Vercel_AI_ProviderUtils_ProviderValues_RuntimeProbe_Window"></a>

### Window


```csharp
public bool Window { get; set; }
```


True when a browser `window` is present.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/ProviderValues.cs#L334)
