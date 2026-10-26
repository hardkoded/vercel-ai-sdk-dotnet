---
title: "AnthropicBashResult"
description: "Output of a bash sandbox command."
editUrl: false
prev: false
next: false
sidebar:
  label: "AnthropicBashResult"
  hidden: true
---

<a id="Vercel_AI_Anthropic_AnthropicBashResult"></a>

Namespace: [Vercel.AI.Anthropic](/api/Vercel.AI.Anthropic/)


```csharp
public sealed class AnthropicBashResult
```


Output of a bash sandbox command.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L527)

## Constructors

<a id="Vercel_AI_Anthropic_AnthropicBashResult__ctor_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBashResult__ctor_System_Int32_System_String_System_String_System_Threading_CancellationToken_"></a>

### AnthropicBashResult(int, string, string, CancellationToken)


```csharp
public AnthropicBashResult(int exitCode, string stdout, string stderr, CancellationToken cancellationToken)
```


Creates a bash result.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `exitCode` | [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |
| `stdout` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `stderr` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |
| `cancellationToken` | [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L530)

## Properties

<a id="Vercel_AI_Anthropic_AnthropicBashResult_CancellationToken_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBashResult_CancellationToken"></a>

### CancellationToken


```csharp
public CancellationToken CancellationToken { get; }
```


Token the sandbox observed.

**Value:** [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L548)

<a id="Vercel_AI_Anthropic_AnthropicBashResult_ExitCode_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBashResult_ExitCode"></a>

### ExitCode


```csharp
public int ExitCode { get; }
```


Process exit code.

**Value:** [int](https://learn.microsoft.com/dotnet/api/system.int32)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L539)

<a id="Vercel_AI_Anthropic_AnthropicBashResult_Stderr_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBashResult_Stderr"></a>

### Stderr


```csharp
public string Stderr { get; }
```


Standard error.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L545)

<a id="Vercel_AI_Anthropic_AnthropicBashResult_Stdout_"></a>
<a id="Vercel_AI_Anthropic_AnthropicBashResult_Stdout"></a>

### Stdout


```csharp
public string Stdout { get; }
```


Standard output.

**Value:** [string](https://learn.microsoft.com/dotnet/api/system.string)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Anthropic/AnthropicTools.cs#L542)
