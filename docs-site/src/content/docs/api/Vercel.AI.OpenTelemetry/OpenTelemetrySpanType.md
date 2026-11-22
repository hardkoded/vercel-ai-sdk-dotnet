---
title: "OpenTelemetrySpanType"
description: "The kind of span an EnrichSpan callback is asked about. Maps to OpenTelemetrySpanType."
editUrl: false
prev: false
next: false
sidebar:
  label: "OpenTelemetrySpanType"
  hidden: true
---

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType"></a>

Namespace: [Vercel.AI.OpenTelemetry](/api/Vercel.AI.OpenTelemetry/)


```csharp
public enum OpenTelemetrySpanType
```


The kind of span an [EnrichSpan](/api/Vercel.AI.OpenTelemetry/EnrichSpan/) callback is asked about. Maps to `OpenTelemetrySpanType`.

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L10)

## Fields

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType_ExperimentalDecision"></a>

### ExperimentalDecision


```csharp
ExperimentalDecision = 4
```


A decision model call span. Maps to `'experimental_decision'`.

**Value:** [OpenTelemetrySpanType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L25)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType_ExperimentalExtraction"></a>

### ExperimentalExtraction


```csharp
ExperimentalExtraction = 5
```


An extraction model call span. Maps to `'experimental_extraction'`.

**Value:** [OpenTelemetrySpanType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L28)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType_LanguageModel"></a>

### LanguageModel


```csharp
LanguageModel = 2
```


A model call span. Maps to `'languageModel'`.

**Value:** [OpenTelemetrySpanType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L19)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType_Operation"></a>

### Operation


```csharp
Operation = 0
```


The root span of an operation. Maps to `'operation'`.

**Value:** [OpenTelemetrySpanType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L13)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType_Step"></a>

### Step


```csharp
Step = 1
```


A step span. Maps to `'step'`.

**Value:** [OpenTelemetrySpanType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L16)

<a id="Vercel_AI_OpenTelemetry_OpenTelemetrySpanType_Tool"></a>

### Tool


```csharp
Tool = 3
```


A tool execution span. Maps to `'tool'`.

**Value:** [OpenTelemetrySpanType](./)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/OpenTelemetry/SupplementalAttributes.cs#L22)
