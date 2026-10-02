// Copyright 2023 Vercel, Inc.
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests;

[Collection("LogWarnings")]
public sealed class LogWarningsTests
{
    public LogWarningsTests()
    {
        LogWarnings.ResetState();
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is false::should not log any warnings (single)",
        Coverage = UpstreamCoverage.Covered)]
    public void Suppressed_logger_drops_a_single_warning()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Suppressed;
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("Test warning"), "providerX", "modelY"));

        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is false::should not log any warnings (multiple)",
        Coverage = UpstreamCoverage.Covered)]
    public void Suppressed_logger_drops_multiple_warnings()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Suppressed;
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(
            new SdkWarning[] { new OtherWarning("Test warning 1"), new OtherWarning("Test warning 2") },
            "provider",
            "model"));

        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is false::should not count empty arrays as first call",
        Coverage = UpstreamCoverage.Covered)]
    public void Suppressed_logger_ignores_empty_then_non_empty()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Suppressed;
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "prov", "mod"));
        LogWarnings.Log(Context(new OtherWarning("foo"), "p1", "m1"));

        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is a custom function::should call the custom function with warning options",
        Coverage = UpstreamCoverage.Covered)]
    public void Custom_logger_receives_the_context()
    {
        var calls = new List<LogWarningContext>();
        var emit = new List<string>();
        var console = new List<string>();
        var context = Context(new OtherWarning("Test warning"), "pp", "mm");
        LogWarnings.Mode = AiSdkLogWarningsMode.Custom;
        LogWarnings.CustomLogger = calls.Add;
        LogWarnings.ProcessEmitWarning = (message, _) => emit.Add(message);
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(context);

        Assert.Same(context, Assert.Single(calls));
        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is a custom function::should call the custom function with multiple warnings",
        Coverage = UpstreamCoverage.Covered)]
    public void Custom_logger_receives_multiple_warnings()
    {
        var calls = new List<LogWarningContext>();
        var emit = new List<string>();
        var console = new List<string>();
        var context = new LogWarningContext(
            new SdkWarning[]
            {
                new UnsupportedWarning("temperature", "Temperature not supported"),
                new OtherWarning("Another warning"),
            },
            "provider",
            "model");
        LogWarnings.Mode = AiSdkLogWarningsMode.Custom;
        LogWarnings.CustomLogger = calls.Add;
        LogWarnings.ProcessEmitWarning = (message, _) => emit.Add(message);
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(context);

        Assert.Same(context, Assert.Single(calls));
        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is a custom function::should not call the custom function with empty warnings array",
        Coverage = UpstreamCoverage.Covered)]
    public void Custom_logger_skips_an_empty_list()
    {
        var calls = new List<LogWarningContext>();
        var emit = new List<string>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Custom;
        LogWarnings.CustomLogger = calls.Add;
        LogWarnings.ProcessEmitWarning = (message, _) => emit.Add(message);
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "x", "y"));

        Assert.Empty(calls);
        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should emit the information note and warning via process.emitWarning without logging to stdout",
        Coverage = UpstreamCoverage.Covered)]
    public void Default_logger_emits_the_note_and_the_warning()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("Test warning message"), "myProvider", "myModel"));

        Assert.Empty(console);
        Assert.Equal(2, emit.Count);
        Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), emit[0]);
        Assert.Equal(("AI SDK Warning (myProvider / myModel): Test warning message", "Warning"), emit[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should only emit the information note on the first non-empty call",
        Coverage = UpstreamCoverage.Covered)]
    public void Default_logger_emits_the_note_once()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("1"), "a", "b"));
        LogWarnings.Log(Context(new OtherWarning("2"), "a", "b"));

        Assert.Empty(console);
        Assert.Equal(
            new[]
            {
                (LogWarnings.FirstWarningInfoMessage, "Warning"),
                ("AI SDK Warning (a / b): 1", "Warning"),
                ("AI SDK Warning (a / b): 2", "Warning"),
            },
            emit);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should log the information note and warnings with console.warn when process.emitWarning is unavailable",
        Coverage = UpstreamCoverage.Covered)]
    public void Default_logger_uses_console_when_emit_warning_is_absent()
    {
        var emit = new List<string>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = null;
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("Test warning"), "provider", "model"));

        Assert.Empty(emit);
        Assert.Equal(
            new[]
            {
                LogWarnings.FirstWarningInfoMessage,
                "AI SDK Warning (provider / model): Test warning",
            },
            console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should only log for non-empty warnings",
        Coverage = UpstreamCoverage.Covered)]
    public void Default_logger_ignores_empty_lists_between_warnings()
    {
        var emit = new List<(string, string)>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "err", "m"));
        Assert.Empty(emit);
        Assert.Empty(console);

        LogWarnings.Log(Context(new OtherWarning("t1"), "prov", "mod"));
        Assert.Equal(2, emit.Count);

        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "prov", "mod"));
        Assert.Equal(2, emit.Count);

        LogWarnings.Log(Context(new OtherWarning("t2"), "prov", "mod"));
        Assert.Empty(console);
        Assert.Equal(3, emit.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should handle various warning types per formatWarning",
        Coverage = UpstreamCoverage.Covered)]
    public void Default_logger_formats_each_warning_type()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(
            new SdkWarning[]
            {
                new UnsupportedWarning("mediaType", "detail"),
                new UnsupportedWarning("voice", "detail2"),
                new DeprecatedWarning("providerOptions key 'old-key'", "Use 'oldKey' instead."),
                new OtherWarning("other msg"),
            },
            "zzz",
            "MMM"));

        Assert.Empty(console);
        Assert.Equal(5, emit.Count);
        Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), emit[0]);
        Assert.Equal(("AI SDK Warning (zzz / MMM): The feature \"mediaType\" is not supported. detail", "Warning"), emit[1]);
        Assert.Equal(("AI SDK Warning (zzz / MMM): The feature \"voice\" is not supported. detail2", "Warning"), emit[2]);
        Assert.Equal(("AI SDK Warning (zzz / MMM): Deprecated: \"providerOptions key 'old-key'\". Use 'oldKey' instead.", "DeprecationWarning"), emit[3]);
        Assert.Equal(("AI SDK Warning (zzz / MMM): other msg", "Warning"), emit[4]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should include warning even with \"unknown provider\" and \"unknown model\"",
        Coverage = UpstreamCoverage.Covered)]
    public void Default_logger_includes_unknown_provider_and_model()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("messx"), "unknown provider", "unknown model"));

        Assert.Empty(console);
        Assert.Contains(("AI SDK Warning (unknown provider / unknown model): messx", "Warning"), emit);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is undefined (explicitly set)::should use default behavior and emit via process.emitWarning",
        Coverage = UpstreamCoverage.Covered)]
    public void Explicit_default_mode_emits_the_note_and_the_warning()
    {
        var emit = new List<(string Message, string Type)>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Default;
        LogWarnings.CustomLogger = null;
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("Test warning with undefined logger"), "p1", "m1"));

        Assert.Empty(console);
        Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), emit[0]);
        Assert.Equal(("AI SDK Warning (p1 / m1): Test warning with undefined logger", "Warning"), emit[1]);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should not display the info message for empty warnings",
        Coverage = UpstreamCoverage.Covered)]
    public void Empty_warnings_do_not_emit_the_note()
    {
        var emit = new List<string>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, _) => emit.Add(message);
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "a", "b"));

        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should display informational note only on first real call",
        Coverage = UpstreamCoverage.Covered)]
    public void Information_note_waits_for_the_first_real_warning()
    {
        var emit = new List<(string, string)>();
        var console = new List<string>();
        LogWarnings.ProcessEmitWarning = (message, type) => emit.Add((message, type));
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "a", "b"));
        LogWarnings.Log(new LogWarningContext(Array.Empty<SdkWarning>(), "a", "b"));
        Assert.Empty(emit);
        Assert.Empty(console);

        LogWarnings.Log(Context(new OtherWarning("foo"), "abc", "bbb"));
        Assert.Equal(2, emit.Count);

        LogWarnings.Log(Context(new OtherWarning("bar"), "abc", "bbb"));
        Assert.Empty(console);
        Assert.Equal(3, emit.Count);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should not display information note when using custom logger",
        Coverage = UpstreamCoverage.Covered)]
    public void Custom_logger_does_not_emit_the_note()
    {
        var calls = 0;
        var emit = new List<string>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Custom;
        LogWarnings.CustomLogger = _ => calls++;
        LogWarnings.ProcessEmitWarning = (message, _) => emit.Add(message);
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("Message"), "provV", "modZ"));

        Assert.Equal(1, calls);
        Assert.Empty(emit);
        Assert.Empty(console);
    }

    [Fact]
    [UpstreamTest(
        "packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should not display information note when AI_SDK_LOG_WARNINGS is false",
        Coverage = UpstreamCoverage.Covered)]
    public void Suppressed_logger_does_not_emit_the_note()
    {
        var emit = new List<string>();
        var console = new List<string>();
        LogWarnings.Mode = AiSdkLogWarningsMode.Suppressed;
        LogWarnings.ProcessEmitWarning = (message, _) => emit.Add(message);
        LogWarnings.ConsoleWarn = console.Add;

        LogWarnings.Log(Context(new OtherWarning("Suppressed"), "notProv", "notModel"));

        Assert.Empty(emit);
        Assert.Empty(console);
    }

    private static LogWarningContext Context(SdkWarning warning, string provider, string model)
    {
        return new LogWarningContext(new[] { warning }, provider, model);
    }
}

[CollectionDefinition("LogWarnings", DisableParallelization = true)]
public sealed class LogWarningsCollection
{
}
