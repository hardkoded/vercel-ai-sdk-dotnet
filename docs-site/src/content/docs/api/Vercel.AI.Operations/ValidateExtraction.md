---
title: "ValidateExtraction"
description: "Validates extraction input and answers. Maps to validate-extraction.ts."
editUrl: false
prev: false
next: false
sidebar:
  label: "ValidateExtraction"
  hidden: true
---

<a id="Vercel_AI_Operations_ValidateExtraction"></a>

Namespace: [Vercel.AI.Operations](/api/Vercel.AI.Operations/)


```csharp
public static class ValidateExtraction
```


Validates extraction input and answers. Maps to `validate-extraction.ts`.

**Inheritance:** [object](https://learn.microsoft.com/dotnet/api/system.object)

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ValidateExtraction.cs#L12)

## Methods

<a id="Vercel_AI_Operations_ValidateExtraction_ValidateExtractionAnswers_"></a>
<a id="Vercel_AI_Operations_ValidateExtraction_ValidateExtractionAnswers_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionField__System_Collections_Generic_IReadOnlyList_Vercel_AI_Operations_DecisionModelStatePart__System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionAnswer__"></a>

### `ValidateExtractionAnswers(IReadOnlyDictionary<string, ExtractionField>, IReadOnlyList<DecisionModelStatePart>, IReadOnlyDictionary<string, ExtractionAnswer>?)`


```csharp
public static void ValidateExtractionAnswers(IReadOnlyDictionary<string, ExtractionField> fields, IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, ExtractionAnswer>? answers)
```


Validates answers strictly. No partial results. Maps to `validateExtractionAnswers`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `fields` | [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `state` | [IReadOnlyList&lt;DecisionModelStatePart&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1) |  |
| `answers` | [IReadOnlyDictionary&lt;string, ExtractionAnswer&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ValidateExtraction.cs#L66)

<a id="Vercel_AI_Operations_ValidateExtraction_ValidateExtractionInput_"></a>
<a id="Vercel_AI_Operations_ValidateExtraction_ValidateExtractionInput_Vercel_AI_Operations_DecisionState_System_Collections_Generic_IReadOnlyDictionary_System_String_Vercel_AI_Operations_ExtractionField__System_String_"></a>

### `ValidateExtractionInput(DecisionState?, IReadOnlyDictionary<string, ExtractionField>?, string?)`


```csharp
public static void ValidateExtractionInput(DecisionState? state, IReadOnlyDictionary<string, ExtractionField>? fields, string? referenceDate = null)
```


Validates state, reference date, and fields before any model call. Maps to `validateExtractionInput`.

**Parameters**

| Name | Type | Description |
| --- | --- | --- |
| `state` | [DecisionState](/api/Vercel.AI.Operations/DecisionState/) |  |
| `fields` | [IReadOnlyDictionary&lt;string, ExtractionField&gt;](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2) |  |
| `referenceDate` | [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

[View source](https://github.com/hardkoded/vercel-ai-sdk-dotnet/blob/main/src/Vercel.AI/Operations/ValidateExtraction.cs#L23)
