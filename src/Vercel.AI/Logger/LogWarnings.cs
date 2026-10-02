// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI;

/// <summary>How <see cref="LogWarnings.Log"/> treats <c>AI_SDK_LOG_WARNINGS</c>.</summary>
public enum AiSdkLogWarningsMode
{
    /// <summary>Format warnings and send them to the process emitter, or to the console when that emitter is absent.</summary>
    Default = 0,

    /// <summary>Drop every warning. Maps to <c>AI_SDK_LOG_WARNINGS = false</c>.</summary>
    Suppressed = 1,

    /// <summary>Call <see cref="LogWarnings.CustomLogger"/> and skip the default formatter.</summary>
    Custom = 2,
}

/// <summary>A warning returned by a model provider. Maps to the upstream <c>Warning</c> union.</summary>
public abstract class SdkWarning
{
    /// <summary>Creates a warning of the given type.</summary>
    protected SdkWarning(string type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>Warning type name: <c>unsupported</c>, <c>compatibility</c>, <c>deprecated</c>, or <c>other</c>.</summary>
    public string Type { get; }
}

/// <summary>A feature the provider does not support.</summary>
public sealed class UnsupportedWarning : SdkWarning
{
    /// <summary>Creates an unsupported-feature warning.</summary>
    public UnsupportedWarning(string feature, string? details = null)
        : base("unsupported")
    {
        Feature = feature ?? throw new ArgumentNullException(nameof(feature));
        Details = details;
    }

    /// <summary>Feature name.</summary>
    public string Feature { get; }

    /// <summary>Optional extra detail.</summary>
    public string? Details { get; }
}

/// <summary>A feature that ran in a compatibility mode.</summary>
public sealed class CompatibilityWarning : SdkWarning
{
    /// <summary>Creates a compatibility warning.</summary>
    public CompatibilityWarning(string feature, string? details = null)
        : base("compatibility")
    {
        Feature = feature ?? throw new ArgumentNullException(nameof(feature));
        Details = details;
    }

    /// <summary>Feature name.</summary>
    public string Feature { get; }

    /// <summary>Optional extra detail.</summary>
    public string? Details { get; }
}

/// <summary>A deprecated setting.</summary>
public sealed class DeprecatedWarning : SdkWarning
{
    /// <summary>Creates a deprecation warning.</summary>
    public DeprecatedWarning(string setting, string message)
        : base("deprecated")
    {
        Setting = setting ?? throw new ArgumentNullException(nameof(setting));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>Setting that is deprecated.</summary>
    public string Setting { get; }

    /// <summary>Replacement guidance.</summary>
    public string Message { get; }
}

/// <summary>A warning that does not fit the other categories.</summary>
public sealed class OtherWarning : SdkWarning
{
    /// <summary>Creates an other warning.</summary>
    public OtherWarning(string message)
        : base("other")
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>Warning text.</summary>
    public string Message { get; }
}

/// <summary>Arguments passed to <see cref="LogWarnings.Log"/> and to a custom logger.</summary>
public sealed class LogWarningContext
{
    /// <summary>Creates a log context.</summary>
    public LogWarningContext(IReadOnlyList<SdkWarning> warnings, string? provider = null, string? model = null)
    {
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
        Provider = provider;
        Model = model;
    }

    /// <summary>Warnings returned by the provider.</summary>
    public IReadOnlyList<SdkWarning> Warnings { get; }

    /// <summary>Provider id, when the call was scoped to one.</summary>
    public string? Provider { get; }

    /// <summary>Model id, when the call was scoped to one.</summary>
    public string? Model { get; }
}

/// <summary>
/// Logs provider warnings. Maps to <c>logWarnings</c>.
/// Assign <see cref="Mode"/> and <see cref="CustomLogger"/> the way JavaScript assigns <c>globalThis.AI_SDK_LOG_WARNINGS</c>.
/// </summary>
public static class LogWarnings
{
    /// <summary>Shown once before the first default-logged warning.</summary>
    public const string FirstWarningInfoMessage =
        "AI SDK Warning System: To turn off warning logging, set the AI_SDK_LOG_WARNINGS global to false.";

    private static bool _hasLoggedBefore;

    /// <summary>Current logger mode. <see cref="AiSdkLogWarningsMode.Default"/> matches an unset global.</summary>
    public static AiSdkLogWarningsMode Mode { get; set; } = AiSdkLogWarningsMode.Default;

    /// <summary>Called when <see cref="Mode"/> is <see cref="AiSdkLogWarningsMode.Custom"/>.</summary>
    public static Action<LogWarningContext>? CustomLogger { get; set; }

    /// <summary>
    /// Node <c>process.emitWarning</c> stand-in. When set, default logging uses it and does not write to <see cref="ConsoleWarn"/>.
    /// The message is the first argument and the warning type (<c>Warning</c> or <c>DeprecationWarning</c>) is the second.
    /// </summary>
    public static Action<string, string>? ProcessEmitWarning { get; set; }

    /// <summary>
    /// <c>console.warn</c> stand-in used when <see cref="ProcessEmitWarning"/> is null.
    /// Null writes the message with <see cref="Console.Error"/>.
    /// </summary>
    public static Action<string>? ConsoleWarn { get; set; }

    /// <summary>Clears the first-call flag and the logger configuration. Tests use this between cases.</summary>
    public static void ResetState()
    {
        _hasLoggedBefore = false;
        Mode = AiSdkLogWarningsMode.Default;
        CustomLogger = null;
        ProcessEmitWarning = null;
        ConsoleWarn = null;
    }

    /// <summary>Logs <paramref name="context"/> according to <see cref="Mode"/>.</summary>
    public static void Log(LogWarningContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Warnings.Count == 0)
        {
            return;
        }

        if (Mode == AiSdkLogWarningsMode.Suppressed)
        {
            return;
        }

        if (Mode == AiSdkLogWarningsMode.Custom)
        {
            CustomLogger?.Invoke(context);
            return;
        }

        if (!_hasLoggedBefore)
        {
            _hasLoggedBefore = true;
            Emit(FirstWarningInfoMessage, "Warning");
        }

        for (var i = 0; i < context.Warnings.Count; i++)
        {
            var warning = context.Warnings[i];
            Emit(Format(warning, context.Provider, context.Model), warning.Type == "deprecated" ? "DeprecationWarning" : "Warning");
        }
    }

    /// <summary>Formats one warning the way the default logger does.</summary>
    public static string Format(SdkWarning warning, string? provider, string? model)
    {
        if (warning is null)
        {
            throw new ArgumentNullException(nameof(warning));
        }

        var scope = provider != null && model != null ? " (" + provider + " / " + model + ")" : string.Empty;
        var prefix = "AI SDK Warning" + scope + ":";
        switch (warning)
        {
            case UnsupportedWarning unsupported:
                return AppendDetails(prefix + " The feature \"" + unsupported.Feature + "\" is not supported.", unsupported.Details);
            case CompatibilityWarning compatibility:
                return AppendDetails(prefix + " The feature \"" + compatibility.Feature + "\" is used in a compatibility mode.", compatibility.Details);
            case DeprecatedWarning deprecated:
                return prefix + " Deprecated: \"" + deprecated.Setting + "\". " + deprecated.Message;
            case OtherWarning other:
                return prefix + " " + other.Message;
            default:
                return prefix + " {\"type\":\"" + warning.Type + "\"}";
        }
    }

    private static string AppendDetails(string message, string? details)
    {
        if (string.IsNullOrEmpty(details))
        {
            return message;
        }

        return message + " " + details;
    }

    private static void Emit(string message, string type)
    {
        if (ProcessEmitWarning != null)
        {
            ProcessEmitWarning(message, type);
            return;
        }

        if (ConsoleWarn != null)
        {
            ConsoleWarn(message);
            return;
        }

        Console.Error.WriteLine(message);
    }
}
