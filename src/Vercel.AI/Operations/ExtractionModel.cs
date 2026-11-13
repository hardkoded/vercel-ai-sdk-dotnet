// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>A typed value to extract from the shared state. Maps to <c>ExtractionModelV4Field</c>.</summary>
public sealed class ExtractionField
{
    /// <summary>Field types an extraction model can support.</summary>
    public static readonly IReadOnlyList<string> Types = new[] { "text", "number", "date", "time", "datetime", "duration", "list" };

    /// <summary>Creates a field.</summary>
    /// <param name="type"><c>text</c>, <c>number</c>, <c>date</c>, <c>time</c>, <c>datetime</c>, <c>duration</c>, or <c>list</c>.</param>
    /// <param name="instructions">Instructions as a string, JSON object, or JSON array. Optional.</param>
    public ExtractionField(string type, DecisionInput? instructions = null)
    {
        Type = type ?? string.Empty;
        Instructions = instructions;
    }

    /// <summary><c>text</c>, <c>number</c>, <c>date</c>, <c>time</c>, <c>datetime</c>, <c>duration</c>, or <c>list</c>.</summary>
    public string Type { get; }

    /// <summary>Instructions. A string, JSON object, or JSON array.</summary>
    public DecisionInput? Instructions { get; }

    /// <summary>Unit the value should be expressed in. A free string for <c>number</c>. For <c>duration</c> it is one of <see cref="DurationUnits"/>, default <c>seconds</c>.</summary>
    public string? Unit { get; init; }

    /// <summary>Require an integer value. Applies to <c>number</c>, <c>duration</c>, and <c>list</c> of numbers.</summary>
    public bool? Integer { get; init; }

    /// <summary>Maximum decimal places, an integer from 0 to 6. Applies to <c>number</c>, <c>duration</c>, and <c>list</c> of numbers.</summary>
    public int? Decimals { get; init; }

    /// <summary>Item type of a <c>list</c> field: <c>text</c>, <c>number</c>, or <c>date</c>.</summary>
    public string? Items { get; init; }

    /// <summary>Valid duration units. Maps to <c>ExtractionModelV4DurationUnit</c>.</summary>
    public static IReadOnlyList<string> DurationUnits { get; } = new[] { "seconds", "minutes", "hours", "days", "weeks", "months", "years" };

    /// <summary>Valid item types of a list field.</summary>
    public static IReadOnlyList<string> ListItems { get; } = new[] { "text", "number", "date" };
}

/// <summary>Where in the state a value was found. Maps to <c>ExtractionModelV4Source</c>.</summary>
public sealed class ExtractionSource
{
    /// <summary>Creates a source.</summary>
    /// <param name="type"><c>text</c> or <c>file</c>.</param>
    /// <param name="part">Index of the part in the state.</param>
    /// <param name="start">Start offset in UTF-16 code units, inclusive. Text sources only.</param>
    /// <param name="end">End offset in UTF-16 code units, exclusive. Text sources only.</param>
    public ExtractionSource(string type, int part, int? start = null, int? end = null)
    {
        Type = type;
        Part = part;
        Start = start;
        End = end;
    }

    /// <summary><c>text</c> or <c>file</c>.</summary>
    public string Type { get; }

    /// <summary>Index of the text or file part in the state.</summary>
    public int Part { get; }

    /// <summary>Start offset in UTF-16 code units, inclusive. Set for a text source.</summary>
    public int? Start { get; }

    /// <summary>End offset in UTF-16 code units, exclusive. Set for a text source.</summary>
    public int? End { get; }
}

/// <summary>One entry of a list answer. Maps to the <c>items</c> member of a list <c>ExtractionModelV4Answer</c>.</summary>
public sealed class ExtractionItem
{
    /// <summary>Creates an item entry.</summary>
    public ExtractionItem(double? confidence = null, ExtractionSource? source = null)
    {
        Confidence = confidence;
        Source = source;
    }

    /// <summary>Confidence in [0, 1] that the item is correct.</summary>
    public double? Confidence { get; }

    /// <summary>Where the item was found.</summary>
    public ExtractionSource? Source { get; }
}

/// <summary>
/// One model answer. Maps to <c>ExtractionModelV4Answer</c>. The <c>value</c> member is split by type: <see cref="Text"/> for
/// <c>text</c>, <c>date</c>, <c>time</c>, and <c>datetime</c>, <see cref="Number"/> for <c>number</c> and <c>duration</c>, and
/// <see cref="TextItems"/> or <see cref="NumberItems"/> for <c>list</c>. All are null when the state has no value for the field.
/// </summary>
public sealed class ExtractionAnswer
{
    /// <summary>Creates an answer.</summary>
    public ExtractionAnswer(
        string type,
        string? text = null,
        double? number = null,
        IReadOnlyList<string>? textItems = null,
        IReadOnlyList<double>? numberItems = null,
        string? guessText = null,
        double? guessNumber = null,
        double? pNull = null,
        double? confidence = null,
        ExtractionSource? source = null,
        IReadOnlyList<ExtractionItem>? items = null)
    {
        Type = type;
        Text = text;
        Number = number;
        TextItems = textItems;
        NumberItems = numberItems;
        GuessText = guessText;
        GuessNumber = guessNumber;
        PNull = pNull;
        Confidence = confidence;
        Source = source;
        Items = items;
    }

    /// <summary>Answer type. It is the field type, or <c>refusal</c> when the model declined the field.</summary>
    public string Type { get; }

    /// <summary>Value of a <c>text</c>, <c>date</c>, <c>time</c>, or <c>datetime</c> field. Null when the state has none.</summary>
    public string? Text { get; }

    /// <summary>Value of a <c>number</c> or <c>duration</c> field. Null when the state has none.</summary>
    public double? Number { get; }

    /// <summary>Value of a <c>list</c> field of text or date items. Null when the state has none.</summary>
    public IReadOnlyList<string>? TextItems { get; }

    /// <summary>Value of a <c>list</c> field of number items. Null when the state has none.</summary>
    public IReadOnlyList<double>? NumberItems { get; }

    /// <summary>Most likely value for a text-like field, also when the value is null.</summary>
    public string? GuessText { get; }

    /// <summary>Most likely value for a <c>number</c> or <c>duration</c> field, also when the value is null.</summary>
    public double? GuessNumber { get; }

    /// <summary>Probability in [0, 1] that the state does not contain a value.</summary>
    public double? PNull { get; }

    /// <summary>Confidence in [0, 1] that the value is correct.</summary>
    public double? Confidence { get; }

    /// <summary>Where the value was found. Not used by list answers.</summary>
    public ExtractionSource? Source { get; }

    /// <summary>One entry per list item, when available. Only list answers use it.</summary>
    public IReadOnlyList<ExtractionItem>? Items { get; }
}

/// <summary>Model response for <c>doExtract</c>. Maps to <c>ExtractionModelV4Result</c>.</summary>
public sealed class ExtractionModelResult
{
    /// <summary>Creates a model result.</summary>
    public ExtractionModelResult(IReadOnlyDictionary<string, ExtractionAnswer> answers, IReadOnlyList<OperationWarning>? warnings = null, OperationUsage? usage = null, JsonElement? providerMetadata = null, ProviderResponse? response = null)
    {
        Answers = answers ?? new Dictionary<string, ExtractionAnswer>();
        Warnings = warnings ?? Array.Empty<OperationWarning>();
        Usage = usage;
        ProviderMetadata = providerMetadata;
        Response = response;
    }

    /// <summary>Exactly one answer per field, under the original field ids.</summary>
    public IReadOnlyDictionary<string, ExtractionAnswer> Answers { get; }

    /// <summary>Warnings.</summary>
    public IReadOnlyList<OperationWarning> Warnings { get; }

    /// <summary>Token usage.</summary>
    public OperationUsage? Usage { get; }

    /// <summary>Provider metadata.</summary>
    public JsonElement? ProviderMetadata { get; }

    /// <summary>Response metadata. Timestamp and model id are filled when omitted.</summary>
    public ProviderResponse? Response { get; }
}

/// <summary>Arguments for <c>doExtract</c>. Maps to <c>ExtractionModelV4CallOptions</c>.</summary>
public sealed class ExtractionModelCall
{
    /// <summary>Creates a call.</summary>
    public ExtractionModelCall(IReadOnlyList<DecisionModelStatePart> state, IReadOnlyDictionary<string, ExtractionField> fields, JsonElement providerOptions, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken, string? referenceDate = null, double? maxOutputTokens = null, string? reasoning = null)
    {
        State = state;
        Fields = fields;
        ProviderOptions = providerOptions;
        Headers = headers;
        CancellationToken = cancellationToken;
        ReferenceDate = referenceDate;
        MaxOutputTokens = maxOutputTokens;
        Reasoning = reasoning;
    }

    /// <summary>Ordered text, file, and JSON parts. Answer sources index into this list.</summary>
    public IReadOnlyList<DecisionModelStatePart> State { get; }

    /// <summary>Fields to extract.</summary>
    public IReadOnlyDictionary<string, ExtractionField> Fields { get; }

    /// <summary><c>YYYY-MM-DD</c> date that anchors relative dates and dates without a year.</summary>
    public string? ReferenceDate { get; }

    /// <summary>Provider options.</summary>
    public JsonElement ProviderOptions { get; }

    /// <summary>Headers including the user-agent suffix.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>Cancellation token.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Maximum output tokens, an integer of at least 1. Null when the caller set none.</summary>
    public double? MaxOutputTokens { get; }

    /// <summary>Reasoning effort. Language-model adapters default to <c>none</c> when it is null.</summary>
    public string? Reasoning { get; }
}

/// <summary>Experimental extraction model used by <see cref="Extract.ExtractAsync"/>. Maps to <c>ExtractionModelV4</c>.</summary>
public interface IExtractionCaller
{
    /// <summary>Provider id.</summary>
    string Provider { get; }

    /// <summary>Model id.</summary>
    string ModelId { get; }

    /// <summary>Specification version. Only <c>v4</c> is accepted.</summary>
    string SpecificationVersion { get; }

    /// <summary>Field types this model can extract. Unsupported types are rejected before any provider I/O.</summary>
    IReadOnlyList<string> SupportedFieldTypes { get; }

    /// <summary>Extracts every field from the same state. No partial results.</summary>
    Task<ExtractionModelResult> DoExtractAsync(ExtractionModelCall call, CancellationToken cancellationToken);
}
