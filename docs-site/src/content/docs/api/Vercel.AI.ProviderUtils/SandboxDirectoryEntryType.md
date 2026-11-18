---
title: "SandboxDirectoryEntryType"
description: "Type of an entry returned by ListDirectoryAsync(SandboxListDirectoryOptions). Maps to SandboxDirectoryEntry['type']."
editUrl: false
prev: false
next: false
sidebar:
  label: "SandboxDirectoryEntryType"
  hidden: true
---

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntryType"></a>

Namespace: [Vercel.AI.ProviderUtils](/api/Vercel.AI.ProviderUtils/)


```csharp
public enum SandboxDirectoryEntryType
```


Type of an entry returned by [ListDirectoryAsync(SandboxListDirectoryOptions)](/api/Vercel.AI.ProviderUtils/ISandboxSession/#Vercel_AI_ProviderUtils_ISandboxSession_ListDirectoryAsync_Vercel_AI_ProviderUtils_SandboxListDirectoryOptions_). Maps to `SandboxDirectoryEntry['type']`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L121)

## Fields

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntryType_Directory"></a>

### Directory


```csharp
Directory = 1
```


A directory.

**Value:** [SandboxDirectoryEntryType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L127)

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntryType_File"></a>

### File


```csharp
File = 0
```


A file.

**Value:** [SandboxDirectoryEntryType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L124)

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntryType_Other"></a>

### Other


```csharp
Other = 3
```


A device, socket, or named pipe.

**Value:** [SandboxDirectoryEntryType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L133)

<a id="Vercel_AI_ProviderUtils_SandboxDirectoryEntryType_Symlink"></a>

### Symlink


```csharp
Symlink = 2
```


A symbolic link. It is not followed.

**Value:** [SandboxDirectoryEntryType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/ProviderUtils/Sandbox.cs#L130)
