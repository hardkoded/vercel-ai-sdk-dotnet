---
title: "SandboxRunResult"
description: "The result of RunAsync(SandboxRunOptions). Maps to the result of run."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxRunResult"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxRunResult"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public sealed class SandboxRunResult
```


The result of [RunAsync(SandboxRunOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_RunAsync_Vercel_AI_ProviderUtils_SandboxRunOptions_). Maps to the result of `run`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L147)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxRunResult_ExitCode_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxRunResult_ExitCode"></a>

### ExitCode


```csharp
public int ExitCode { get; init; }
```


Exit code returned by the command. Maps to `exitCode`.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L150)

<a id="Vercel_AI_ProviderUtils_SandboxRunResult_Stderr_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxRunResult_Stderr"></a>

### Stderr


```csharp
public string Stderr { get; init; }
```


Standard error produced by the command. Maps to `stderr`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L156)

<a id="Vercel_AI_ProviderUtils_SandboxRunResult_Stdout_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxRunResult_Stdout"></a>

### Stdout


```csharp
public string Stdout { get; init; }
```


Standard output produced by the command. Maps to `stdout`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L153)
