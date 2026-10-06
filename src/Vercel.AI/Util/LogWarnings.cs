// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Vercel.AI.Util;

/// <summary>A warning returned by a model provider.</summary>
public abstract class ModelWarning
{
    /// <summary>Warning kind: <c>unsupported</c>, <c>compatibility</c>, <c>deprecated</c>, or <c>other</c>.</summary>
    public abstract string Type { get; }
}

/// <summary>A feature the provider does not support.</summary>
public sealed class UnsupportedWarning : ModelWarning
{
    /// <summary>Creates the warning.</summary>
    public UnsupportedWarning(string feature, string? details = null)
    {
        Feature = feature;
        Details = details;
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "unsupported"; }
    }

    /// <summary>Feature name.</summary>
    public string Feature { get; }

    /// <summary>Optional extra detail.</summary>
    public string? Details { get; }
}

/// <summary>A feature running in compatibility mode.</summary>
public sealed class CompatibilityWarning : ModelWarning
{
    /// <summary>Creates the warning.</summary>
    public CompatibilityWarning(string feature, string? details = null)
    {
        Feature = feature;
        Details = details;
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "compatibility"; }
    }

    /// <summary>Feature name.</summary>
    public string Feature { get; }

    /// <summary>Optional extra detail.</summary>
    public string? Details { get; }
}

/// <summary>A deprecated setting.</summary>
public sealed class DeprecatedWarning : ModelWarning
{
    /// <summary>Creates the warning.</summary>
    public DeprecatedWarning(string setting, string message)
    {
        Setting = setting;
        Message = message;
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "deprecated"; }
    }

    /// <summary>Deprecated setting name.</summary>
    public string Setting { get; }

    /// <summary>Replacement guidance.</summary>
    public string Message { get; }
}

/// <summary>A provider warning that is not one of the structured kinds.</summary>
public sealed class OtherWarning : ModelWarning
{
    /// <summary>Creates the warning.</summary>
    public OtherWarning(string message)
    {
        Message = message;
    }

    /// <inheritdoc />
    public override string Type
    {
        get { return "other"; }
    }

    /// <summary>Warning text.</summary>
    public string Message { get; }
}

/// <summary>Arguments passed to <see cref="LogWarnings.Log"/>.</summary>
public sealed class LogWarningsOptions
{
    /// <summary>Creates the options.</summary>
    public LogWarningsOptions(IReadOnlyList<ModelWarning>? warnings, string? provider = null, string? model = null)
    {
        Warnings = warnings ?? Array.Empty<ModelWarning>();
        Provider = provider;
        Model = model;
    }

    /// <summary>Warnings returned by the provider.</summary>
    public IReadOnlyList<ModelWarning> Warnings { get; }

    /// <summary>Provider id, when the call was scoped to one.</summary>
    public string? Provider { get; }

    /// <summary>Model id, when the call was scoped to one.</summary>
    public string? Model { get; }
}

/// <summary>
/// Logs provider warnings. Set <see cref="Logger"/> to <c>false</c> to suppress them,
/// or to a delegate to replace the default emitter.
/// </summary>
public static class LogWarnings
{
    /// <summary>Note emitted once before the first default warning.</summary>
    public const string FirstWarningInfoMessage = "AI SDK Warning System: To turn off warning logging, set the AI_SDK_LOG_WARNINGS global to false.";

    private static readonly object Gate = new object();
    private static bool _hasLoggedBefore;
    private static object? _logger;
    private static Action<string, string>? _processEmitWarning = delegate (string message, string type)
    {
        System.Diagnostics.Trace.TraceWarning(type + ": " + message);
    };

    private static Action<string>? _consoleWarn = delegate (string message)
    {
        System.Diagnostics.Trace.TraceWarning(message);
    };

    /// <summary>
    /// <c>false</c> suppresses warnings. A <see cref="Action{LogWarningsOptions}"/> receives them.
    /// <c>null</c> uses the default emitter.
    /// </summary>
    public static object? Logger
    {
        get
        {
            lock (Gate)
            {
                return _logger;
            }
        }

        set
        {
            lock (Gate)
            {
                _logger = value;
            }
        }
    }

    /// <summary>
    /// Node <c>process.emitWarning</c> stand-in. Arguments are the message and the warning type.
    /// <c>null</c> falls back to <see cref="ConsoleWarn"/>.
    /// </summary>
    public static Action<string, string>? ProcessEmitWarning
    {
        get
        {
            lock (Gate)
            {
                return _processEmitWarning;
            }
        }

        set
        {
            lock (Gate)
            {
                _processEmitWarning = value;
            }
        }
    }

    /// <summary><c>console.warn</c> stand-in used when <see cref="ProcessEmitWarning"/> is null.</summary>
    public static Action<string>? ConsoleWarn
    {
        get
        {
            lock (Gate)
            {
                return _consoleWarn;
            }
        }

        set
        {
            lock (Gate)
            {
                _consoleWarn = value;
            }
        }
    }

    /// <summary>Clears the first-call note so the next non-empty log emits it again.</summary>
    public static void ResetState()
    {
        lock (Gate)
        {
            _hasLoggedBefore = false;
        }
    }

    /// <summary>Logs <paramref name="options"/> using the current logger settings.</summary>
    public static void Log(LogWarningsOptions options)
    {
        if (options == null || options.Warnings.Count == 0)
        {
            return;
        }

        object? logger;
        lock (Gate)
        {
            logger = _logger;
        }

        if (logger is bool disabled && disabled == false)
        {
            return;
        }

        var custom = logger as Action<LogWarningsOptions>;
        if (custom != null)
        {
            custom(options);
            return;
        }

        var first = false;
        lock (Gate)
        {
            if (!_hasLoggedBefore)
            {
                _hasLoggedBefore = true;
                first = true;
            }
        }

        if (first)
        {
            Emit(FirstWarningInfoMessage, "Warning");
        }

        for (var i = 0; i < options.Warnings.Count; i++)
        {
            var warning = options.Warnings[i];
            Emit(FormatWarning(warning, options.Provider, options.Model), warning.Type == "deprecated" ? "DeprecationWarning" : "Warning");
        }
    }

    private static void Emit(string message, string type)
    {
        Action<string, string>? processEmit;
        Action<string>? consoleWarn;
        lock (Gate)
        {
            processEmit = _processEmitWarning;
            consoleWarn = _consoleWarn;
        }

        if (processEmit != null)
        {
            processEmit(message, type);
            return;
        }

        if (consoleWarn != null)
        {
            consoleWarn(message);
        }
    }

    private static string FormatWarning(ModelWarning warning, string? provider, string? model)
    {
        var scope = provider != null && model != null ? " (" + provider + " / " + model + ")" : string.Empty;
        var prefix = "AI SDK Warning" + scope + ":";
        var unsupported = warning as UnsupportedWarning;
        if (unsupported != null)
        {
            var message = prefix + " The feature \"" + unsupported.Feature + "\" is not supported.";
            if (!string.IsNullOrEmpty(unsupported.Details))
            {
                message += " " + unsupported.Details;
            }

            return message;
        }

        var compatibility = warning as CompatibilityWarning;
        if (compatibility != null)
        {
            var message = prefix + " The feature \"" + compatibility.Feature + "\" is used in a compatibility mode.";
            if (!string.IsNullOrEmpty(compatibility.Details))
            {
                message += " " + compatibility.Details;
            }

            return message;
        }

        var deprecated = warning as DeprecatedWarning;
        if (deprecated != null)
        {
            return prefix + " Deprecated: \"" + deprecated.Setting + "\". " + deprecated.Message;
        }

        var other = warning as OtherWarning;
        if (other != null)
        {
            return prefix + " " + other.Message;
        }

        return prefix + " " + JsonSerializer.Serialize(warning, new JsonSerializerOptions { WriteIndented = true });
    }
}
