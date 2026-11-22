// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI.Prompt;
using Vercel.AI.Util;

namespace Vercel.AI.Operations;

/// <summary>
/// Hands extraction events to the telemetry integrations. Maps to <c>createRestrictedTelemetryDispatcher</c>
/// in <c>extract/restricted-telemetry-dispatcher.ts</c>. Request headers and provider options never reach integrations, file
/// locations in the state are replaced with <c>[redacted]</c>, and a failed download loses its URL. Recording flags, the
/// SDK version and the content hiding come from <see cref="TelemetryDispatcher"/>.
/// </summary>
internal sealed class RestrictedTelemetryDispatcher
{
    private const string Redacted = "[redacted]";

    private readonly TelemetryDispatcher _dispatcher;
    private readonly TelemetryOptions? _telemetry;

    private RestrictedTelemetryDispatcher(TelemetryOptions? telemetry)
    {
        _telemetry = telemetry;
        _dispatcher = TelemetryDispatcher.Create(telemetry);
    }

    /// <summary>Maps to <c>createRestrictedTelemetryDispatcher</c>.</summary>
    internal static RestrictedTelemetryDispatcher CreateRestrictedTelemetryDispatcher(
        TelemetryOptions? telemetry
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

    /// <summary>Maps to <c>onStart</c>, which calls <c>experimental_onExtractStart</c>.</summary>
    internal Task OnStartAsync(ExtractStartEvent source, CancellationToken cancellationToken)
    {
        return _dispatcher.OnExtractStart?.Invoke(Start(source), cancellationToken)
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>onEnd</c>, which calls <c>experimental_onExtractEnd</c>.</summary>
    internal Task OnEndAsync(ExtractEndEvent source, CancellationToken cancellationToken)
    {
        if (_dispatcher.OnExtractEnd == null)
        {
            return Task.CompletedTask;
        }

        return _dispatcher.OnExtractEnd(
            new ExtractEndEvent(
                Start(source),
                source.Answers,
                source.Usage,
                source.Warnings,
                source.ProviderMetadata,
                source.Response
            ),
            cancellationToken
        );
    }

    /// <summary>Maps to <c>experimental_onExtractionModelCallStart</c>.</summary>
    internal Task ExperimentalOnExtractionModelCallStartAsync(
        ExtractionModelCallStartEvent source,
        CancellationToken cancellationToken
    )
    {
        return _dispatcher.OnExtractionModelCallStart?.Invoke(
            ModelCallStart(source),
            cancellationToken
        )
            ?? Task.CompletedTask;
    }

    /// <summary>Maps to <c>experimental_onExtractionModelCallEnd</c>.</summary>
    internal Task ExperimentalOnExtractionModelCallEndAsync(
        ExtractionModelCallEndEvent source,
        CancellationToken cancellationToken
    )
    {
        if (_dispatcher.OnExtractionModelCallEnd == null)
        {
            return Task.CompletedTask;
        }

        return _dispatcher.OnExtractionModelCallEnd(
            new ExtractionModelCallEndEvent(
                ModelCallStart(source),
                source.Answers,
                source.Usage,
                source.Warnings,
                source.ProviderMetadata,
                source.Response
            ),
            cancellationToken
        );
    }

    /// <summary>Maps to <c>onError</c>. A download error reaches integrations without its URL.</summary>
    internal Task OnErrorAsync(string callId, Exception error, CancellationToken cancellationToken)
    {
        return _dispatcher.OnError?.Invoke(
            new TelemetryErrorEvent(callId, error),
            cancellationToken
        ) ?? Task.CompletedTask;
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
        return new ExtractStartEvent(
            restricted.CallId,
            FilterIncludedContextHelper.FilterIncludedContext(
                source.RuntimeContext,
                _telemetry?.IncludeRuntimeContext
            ),
            restricted.Provider,
            restricted.ModelId,
            restricted.State,
            restricted.Fields,
            restricted.ReferenceDate,
            restricted.MaxOutputTokens,
            restricted.Reasoning,
            restricted.MaxRetries,
            null,
            restricted.ProviderOptions
        );
    }

    private static ExtractionModelCallStartEvent ModelCallStart(
        ExtractionModelCallStartEvent source
    )
    {
        return new ExtractionModelCallStartEvent(
            source.CallId,
            source.Provider,
            source.ModelId,
            RedactFileLocations(source.State),
            source.Fields,
            source.ReferenceDate
        );
    }
}
