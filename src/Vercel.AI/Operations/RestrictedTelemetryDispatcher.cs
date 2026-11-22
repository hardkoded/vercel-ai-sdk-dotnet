// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>
/// Hands extraction events to <see cref="ExtractTelemetry"/> callbacks. Maps to <c>createRestrictedTelemetryDispatcher</c>
/// in <c>extract/restricted-telemetry-dispatcher.ts</c>. Request headers and provider options never reach telemetry, file
/// locations in the state are replaced with <c>[redacted]</c>, and a failed download loses its URL.
/// </summary>
internal sealed class RestrictedTelemetryDispatcher
{
    private const string Redacted = "[redacted]";

    private readonly ExtractTelemetry? _telemetry;

    private RestrictedTelemetryDispatcher(ExtractTelemetry? telemetry)
    {
        _telemetry = telemetry is { IsEnabled: true } ? telemetry : null;
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static RestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        ExtractTelemetry? telemetry
    )
    {
        return new RestrictedTelemetryDispatcher(telemetry);
    }

    /// <summary>
    /// Maps to <c>toTelemetryEvent</c>: drops headers and provider options and redacts file locations from the state.
    /// </summary>
    internal static ExtractStartEvent ToTelemetryEvent(ExtractStartEvent source)
    {
        return new ExtractStartEvent(
            source.CallId,
            source.RuntimeContext,
            source.Provider,
            source.ModelId,
            RedactFileLocations(source.State),
            source.Fields,
            source.ReferenceDate,
            source.MaxOutputTokens,
            source.Reasoning,
            source.MaxRetries,
            null,
            OperationJson.Parse("{}"),
            source.RecordInputs,
            source.RecordOutputs,
            source.FunctionId
        );
    }

    /// <summary>Maps to <c>experimental_onExtractStart</c> through the restricted dispatcher's <c>onStart</c>.</summary>
    internal Task OnStartAsync(ExtractStartEvent source)
    {
        if (_telemetry?.OnExtractStart is not { } callback)
        {
            return Task.CompletedTask;
        }

        return OperationCallbacks.NotifyAsync(Start(source), callback);
    }

    /// <summary>Maps to <c>experimental_onExtractEnd</c> through the restricted dispatcher's <c>onEnd</c>.</summary>
    internal Task OnEndAsync(ExtractEndEvent source)
    {
        if (_telemetry?.OnExtractEnd is not { } callback)
        {
            return Task.CompletedTask;
        }

        var start = Start(source);
        return OperationCallbacks.NotifyAsync(
            new ExtractEndEvent(
                start,
                HideOutputs ? new Dictionary<string, ExtractionAnswer>() : source.Answers,
                source.Usage,
                source.Warnings,
                source.ProviderMetadata,
                source.Response
            ),
            callback
        );
    }

    /// <summary>Maps to <c>experimental_onExtractionModelCallStart</c>.</summary>
    internal Task ExperimentalOnExtractionModelCallStartAsync(ExtractionModelCallStartEvent source)
    {
        if (_telemetry?.OnExtractionModelCallStart is not { } callback)
        {
            return Task.CompletedTask;
        }

        return OperationCallbacks.NotifyAsync(ModelCallStart(source), callback);
    }

    /// <summary>Maps to <c>experimental_onExtractionModelCallEnd</c>.</summary>
    internal Task ExperimentalOnExtractionModelCallEndAsync(ExtractionModelCallEndEvent source)
    {
        if (_telemetry?.OnExtractionModelCallEnd is not { } callback)
        {
            return Task.CompletedTask;
        }

        return OperationCallbacks.NotifyAsync(
            new ExtractionModelCallEndEvent(
                ModelCallStart(source),
                HideOutputs ? new Dictionary<string, ExtractionAnswer>() : source.Answers,
                source.Usage,
                source.Warnings,
                source.ProviderMetadata,
                source.Response
            ),
            callback
        );
    }

    /// <summary>Maps to <c>onError</c>. A download error reaches telemetry without its URL.</summary>
    internal async Task OnErrorAsync(string callId, Exception error)
    {
        if (_telemetry?.OnError is not { } callback)
        {
            return;
        }

        try
        {
            await callback(callId, RedactTelemetryCredentials(error)).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Callback failures are isolated.
        }
    }

    private bool HideInputs => _telemetry?.RecordInputs == false;

    private bool HideOutputs => _telemetry?.RecordOutputs == false;

    private static Exception RedactTelemetryCredentials(Exception error)
    {
        return error is DownloadError download
            ? new DownloadError(
                Redacted,
                "Failed to download a file.",
                download.StatusCode,
                download.StatusText
            )
            : error;
    }

    private static DecisionState? RedactFileLocations(DecisionState? state)
    {
        if (state?.Parts is not { } parts)
        {
            return state;
        }

        var redacted = new DecisionStatePart[parts.Count];
        for (var index = 0; index < parts.Count; index++)
        {
            redacted[index] =
                parts[index] is DecisionFilePart file && IsFileLocation(file.Data)
                    ? new DecisionFilePart(file.MediaType, FilePartInput.FromString(Redacted))
                    {
                        FileName = file.FileName,
                        ProviderOptions = file.ProviderOptions,
                    }
                    : parts[index];
        }

        return DecisionState.From(redacted);
    }

    private static IReadOnlyList<DecisionModelStatePart> RedactFileLocations(
        IReadOnlyList<DecisionModelStatePart> state
    )
    {
        var redacted = new DecisionModelStatePart[state.Count];
        for (var index = 0; index < state.Count; index++)
        {
            redacted[index] =
                state[index] is DecisionModelFilePart file
                && file.Data.DataType is "url" or "reference"
                    ? new DecisionModelFilePart(
                        file.MediaType,
                        new LanguageModelFilePart("text", null) { Text = Redacted }
                    )
                    {
                        FileName = file.FileName,
                        ProviderOptions = file.ProviderOptions,
                    }
                    : state[index];
        }

        return redacted;
    }

    private static bool IsFileLocation(FilePartInput data)
    {
        if (data.Url != null || data.UrlText != null || data.Reference != null)
        {
            return true;
        }

        if (data.Type is "url" or "reference")
        {
            return true;
        }

        return data.Type == null && data.TextData is { } text && HasNonDataScheme(text);
    }

    // A string is a location when it parses as an absolute URL whose scheme is not data:.
    private static bool HasNonDataScheme(string text)
    {
        var colon = text.IndexOf(':');
        if (colon < 1 || !char.IsLetter(text[0]))
        {
            return false;
        }

        for (var index = 1; index < colon; index++)
        {
            var character = text[index];
            if (
                !char.IsLetterOrDigit(character)
                && character != '+'
                && character != '-'
                && character != '.'
            )
            {
                return false;
            }
        }

        return !text.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
    }

    private ExtractStartEvent Start(ExtractStartEvent source)
    {
        var restricted = ToTelemetryEvent(source);
        var hide = HideInputs;
        return new ExtractStartEvent(
            restricted.CallId,
            FilterContext(source.RuntimeContext, _telemetry?.IncludeRuntimeContext),
            restricted.Provider,
            restricted.ModelId,
            hide ? DecisionState.From(string.Empty) : restricted.State,
            hide ? new Dictionary<string, ExtractionField>() : restricted.Fields,
            hide ? null : restricted.ReferenceDate,
            restricted.MaxOutputTokens,
            restricted.Reasoning,
            restricted.MaxRetries,
            null,
            restricted.ProviderOptions,
            _telemetry?.RecordInputs,
            _telemetry?.RecordOutputs,
            _telemetry?.FunctionId
        );
    }

    private ExtractionModelCallStartEvent ModelCallStart(ExtractionModelCallStartEvent source)
    {
        var hide = HideInputs;
        return new ExtractionModelCallStartEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            hide ? Array.Empty<DecisionModelStatePart>() : RedactFileLocations(source.State),
            hide ? new Dictionary<string, ExtractionField>() : source.Fields,
            hide ? null : source.ReferenceDate,
            _telemetry?.RecordInputs,
            _telemetry?.RecordOutputs,
            _telemetry?.FunctionId
        );
    }

    private static IReadOnlyDictionary<string, object?> FilterContext(
        IReadOnlyDictionary<string, object?> context,
        IReadOnlyDictionary<string, bool>? include
    )
    {
        var filtered = new Dictionary<string, object?>();
        if (include == null)
        {
            return filtered;
        }

        foreach (var pair in include)
        {
            if (pair.Value && context.TryGetValue(pair.Key, out var value))
            {
                filtered[pair.Key] = value;
            }
        }

        return filtered;
    }
}
