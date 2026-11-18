---
title: "SandboxSpawnOptions"
description: "Options of SpawnAsync(SandboxSpawnOptions). Maps to SandboxSpawnOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxSpawnOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxSpawnOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxSpawnOptions : SandboxProcessOptions
```


Options of [SpawnAsync(SandboxSpawnOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_SpawnAsync_Vercel_AI_ProviderUtils_SandboxSpawnOptions_). Maps to `SandboxSpawnOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object) → [SandboxProcessOptions](/api/Vercel.AI.ProviderUtils/SandboxProcessOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L29)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxSpawnOptions_Stdin_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxSpawnOptions_Stdin"></a>

### Stdin


```csharp
public bool Stdin { get; init; }
```


Whether to keep standard input open and expose a writable stream on the process. Maps to `stdin`.

**Value:** [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L32)
