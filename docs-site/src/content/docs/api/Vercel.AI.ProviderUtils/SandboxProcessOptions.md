---
title: "SandboxProcessOptions"
description: "Options for executing a command in the sandbox with run or spawn. Maps to SandboxProcessOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxProcessOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public class SandboxProcessOptions
```


Options for executing a command in the sandbox with `run` or `spawn`. Maps to `SandboxProcessOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [SandboxRunOptions](/api/Vercel.AI.ProviderUtils/SandboxRunOptions/), [SandboxSpawnOptions](/api/Vercel.AI.ProviderUtils/SandboxSpawnOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L8)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_AbortSignal_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; init; }
```


Token that aborts the command. When it is cancelled, the running process is killed. Maps to `abortSignal`.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L22)

<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_Command_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_Command"></a>

### Command


```csharp
public string Command { get; init; }
```


Command to execute in the sandbox. Maps to `command`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L11)

<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_Env_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_Env"></a>

### Env


```csharp
public IReadOnlyDictionary<string, string>? Env { get; init; }
```


Environment variables to set for this command. Merged with the default environment of the sandbox, and the values here take precedence. Maps to `env`.

**Value:** [IReadOnlyDictionary&lt;string, string&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L19)

<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_WorkingDirectory_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxProcessOptions_WorkingDirectory"></a>

### WorkingDirectory


```csharp
public string? WorkingDirectory { get; init; }
```


Working directory to execute the command in. Maps to `workingDirectory`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L14)
