---
title: "SandboxFileType"
description: "Type of a path returned by StatAsync(SandboxStatOptions). Maps to SandboxFileStat['type']."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxFileType"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxFileType"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public enum SandboxFileType
```


Type of a path returned by [StatAsync(SandboxStatOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_StatAsync_Vercel_AI_ProviderUtils_SandboxStatOptions_). Maps to `SandboxFileStat['type']`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L98)

## Fields

<a id="Vercel_AI_ProviderUtils_SandboxFileType_Directory"></a>

### Directory


```csharp
Directory = 1
```


A directory.

**Value:** [SandboxFileType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L104)

<a id="Vercel_AI_ProviderUtils_SandboxFileType_File"></a>

### File


```csharp
File = 0
```


A file.

**Value:** [SandboxFileType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L101)

<a id="Vercel_AI_ProviderUtils_SandboxFileType_Other"></a>

### Other


```csharp
Other = 2
```


A device, socket, or named pipe.

**Value:** [SandboxFileType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L107)
