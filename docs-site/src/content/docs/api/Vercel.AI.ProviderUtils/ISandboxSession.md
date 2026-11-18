---
title: "ISandboxSession"
description: "Sandbox session that can execute commands and read and write files. Relative paths in every method resolve against the same base directory: the directory tha..."
editUrl: false
prev: false
next: false
sidebar:
  label: "ISandboxSession"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_ISandboxSession"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public interface ISandboxSession
```


Sandbox session that can execute commands and read and write files.
Relative paths in every method resolve against the same base directory: the directory that [RunAsync(SandboxRunOptions)](#Vercel_AI_ProviderUtils_ISandboxSession_RunAsync_Vercel_AI_ProviderUtils_SandboxRunOptions_) and [SpawnAsync(SandboxSpawnOptions)](#Vercel_AI_ProviderUtils_ISandboxSession_SpawnAsync_Vercel_AI_ProviderUtils_SandboxSpawnOptions_) start in when no
working directory is given. Absolute paths are used as is. Maps to `SandboxSession` (`Experimental_SandboxSession`).

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L186)
