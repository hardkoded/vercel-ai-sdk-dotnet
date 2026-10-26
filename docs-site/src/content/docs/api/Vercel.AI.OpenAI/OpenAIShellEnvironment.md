---
title: "OpenAIShellEnvironment"
description: "Environment of Shell(OpenAIShellToolOptions): containerAuto, containerReference, or local."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenAIShellEnvironment"
  hidden: true
---

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment"></a>

Namespace: [Vercel.AI.OpenAI](/api/Vercel.AI.OpenAI/)


```csharp
public sealed class OpenAIShellEnvironment
```


Environment of [Shell(OpenAIShellToolOptions)](/api/Vercel.AI.OpenAI/OpenAITools/#Vercel_AI_OpenAI_OpenAITools_Shell_Vercel_AI_OpenAI_OpenAIShellToolOptions_): `containerAuto`, `containerReference`, or `local`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L169)

## Properties

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_AllowedDomains_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_AllowedDomains"></a>

### AllowedDomains


```csharp
public IReadOnlyList<string>? AllowedDomains { get; set; }
```


Allowed domains of a `containerAuto` environment network allowlist.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L187)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_ContainerId_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_ContainerId"></a>

### ContainerId


```csharp
public string? ContainerId { get; set; }
```


Container id of a `containerReference` environment.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L175)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_DomainSecrets_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_DomainSecrets"></a>

### DomainSecrets


```csharp
public IReadOnlyList<OpenAIShellDomainSecret>? DomainSecrets { get; set; }
```


Domain secrets of a network allowlist.

**Value:** [IReadOnlyList&lt;OpenAIShellDomainSecret&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L190)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_FileIds_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_FileIds"></a>

### FileIds


```csharp
public IReadOnlyList<string>? FileIds { get; set; }
```


File ids of a `containerAuto` environment.

**Value:** [IReadOnlyList&lt;string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L178)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_MemoryLimit_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_MemoryLimit"></a>

### MemoryLimit


```csharp
public string? MemoryLimit { get; set; }
```


Memory limit of a `containerAuto` environment: `1g`, `4g`, `16g`, or `64g`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L181)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_NetworkDisabled_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_NetworkDisabled"></a>

### NetworkDisabled


```csharp
public bool NetworkDisabled { get; set; }
```


Disable network access of a `containerAuto` environment.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L184)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_Skills_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_Skills"></a>

### Skills


```csharp
public IReadOnlyList<OpenAIShellLocalSkill>? Skills { get; set; }
```


Skills of a `local` environment.

**Value:** [IReadOnlyList&lt;OpenAIShellLocalSkill&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L193)

<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_Type_"></a>
<a id="Vercel_AI_OpenAI_OpenAIShellEnvironment_Type"></a>

### Type


```csharp
public string Type { get; set; }
```


`containerAuto`, `containerReference`, or `local`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenAI/OpenAITools.cs#L172)
