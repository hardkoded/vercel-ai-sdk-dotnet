---
title: "SandboxPathOptions"
description: "Options for reading, inspecting, or deleting one path in the sandbox. Maps to PathOptions."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxPathOptions"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxPathOptions"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public class SandboxPathOptions
```


Options for reading, inspecting, or deleting one path in the sandbox. Maps to `PathOptions`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

**Derived:** [SandboxDeleteFileOptions](/api/Vercel.AI.ProviderUtils/SandboxDeleteFileOptions/), [SandboxListDirectoryOptions](/api/Vercel.AI.ProviderUtils/SandboxListDirectoryOptions/), [SandboxReadBinaryFileOptions](/api/Vercel.AI.ProviderUtils/SandboxReadBinaryFileOptions/), [SandboxReadFileOptions](/api/Vercel.AI.ProviderUtils/SandboxReadFileOptions/), [SandboxReadTextFileOptions](/api/Vercel.AI.ProviderUtils/SandboxReadTextFileOptions/), [SandboxStatOptions](/api/Vercel.AI.ProviderUtils/SandboxStatOptions/), [SandboxWriteBinaryFileOptions](/api/Vercel.AI.ProviderUtils/SandboxWriteBinaryFileOptions/), [SandboxWriteFileOptions](/api/Vercel.AI.ProviderUtils/SandboxWriteFileOptions/), [SandboxWriteTextFileOptions](/api/Vercel.AI.ProviderUtils/SandboxWriteTextFileOptions/)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L36)

## Properties

<a id="Vercel_AI_ProviderUtils_SandboxPathOptions_AbortSignal_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxPathOptions_AbortSignal"></a>

### AbortSignal


```csharp
public CancellationToken AbortSignal { get; init; }
```


Token that aborts the operation. Maps to `abortSignal`.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L42)

<a id="Vercel_AI_ProviderUtils_SandboxPathOptions_Path_"></a>
<a id="Vercel_AI_ProviderUtils_SandboxPathOptions_Path"></a>

### Path


```csharp
public string Path { get; init; }
```


Path to operate on. Maps to `path`.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L39)
