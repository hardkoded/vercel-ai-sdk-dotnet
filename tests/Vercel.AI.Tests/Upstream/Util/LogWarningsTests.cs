// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;

namespace Vercel.AI.Tests;

[Collection("LogWarnings")]
public sealed class LogWarningsTests
{
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is false::should not log any warnings (single)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void False_logger_suppresses_one_warning()
    {
        var log = new Recorder();
        try
        {
            log.Install(false);
            LogWarnings.Log(Options(new OtherWarning("Test warning"), "providerX", "modelY"));
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is false::should not log any warnings (multiple)", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void False_logger_suppresses_several_warnings()
    {
        var log = new Recorder();
        try
        {
            log.Install(false);
            LogWarnings.Log(Options(new ModelWarning[] { new OtherWarning("Test warning 1"), new OtherWarning("Test warning 2") }, "provider", "model"));
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is false::should not count empty arrays as first call", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void False_logger_ignores_an_empty_call()
    {
        var log = new Recorder();
        try
        {
            log.Install(false);
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "prov", "mod"));
            LogWarnings.Log(Options(new OtherWarning("foo"), "p1", "m1"));
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is a custom function::should call the custom function with warning options", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Custom_logger_receives_the_options()
    {
        var log = new Recorder();
        LogWarningsOptions? seen = null;
        try
        {
            log.Install((Action<LogWarningsOptions>)delegate (LogWarningsOptions options) { seen = options; });
            var options = Options(new OtherWarning("Test warning"), "pp", "mm");
            LogWarnings.Log(options);
            Assert.Same(options, seen);
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is a custom function::should call the custom function with multiple warnings", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Custom_logger_receives_multiple_warnings()
    {
        var log = new Recorder();
        var calls = 0;
        try
        {
            log.Install((Action<LogWarningsOptions>)delegate { calls++; });
            LogWarnings.Log(Options(new ModelWarning[]
            {
                new UnsupportedWarning("temperature", "Temperature not supported"),
                new OtherWarning("Another warning"),
            }, "provider", "model"));
            Assert.Equal(1, calls);
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is a custom function::should not call the custom function with empty warnings array", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Custom_logger_skips_an_empty_list()
    {
        var log = new Recorder();
        var calls = 0;
        try
        {
            log.Install((Action<LogWarningsOptions>)delegate { calls++; });
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "x", "y"));
            Assert.Equal(0, calls);
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should emit the information note and warning via process.emitWarning without logging to stdout", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Default_logger_emits_the_note_and_warning()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(new OtherWarning("Test warning message"), "myProvider", "myModel"));
            Assert.Empty(log.Console);
            Assert.Equal(2, log.Process.Count);
            Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), log.Process[0]);
            Assert.Equal(("AI SDK Warning (myProvider / myModel): Test warning message", "Warning"), log.Process[1]);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should only emit the information note on the first non-empty call", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Default_logger_emits_the_note_once()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(new OtherWarning("1"), "a", "b"));
            LogWarnings.Log(Options(new OtherWarning("2"), "a", "b"));
            Assert.Empty(log.Console);
            Assert.Equal(3, log.Process.Count);
            Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), log.Process[0]);
            Assert.Equal(("AI SDK Warning (a / b): 1", "Warning"), log.Process[1]);
            Assert.Equal(("AI SDK Warning (a / b): 2", "Warning"), log.Process[2]);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should log the information note and warnings with console.warn when process.emitWarning is unavailable", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Default_logger_falls_back_to_console_warn()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.ProcessEmitWarning = null;
            LogWarnings.Log(Options(new OtherWarning("Test warning"), "provider", "model"));
            Assert.Equal(2, log.Console.Count);
            Assert.Equal(LogWarnings.FirstWarningInfoMessage, log.Console[0]);
            Assert.Equal("AI SDK Warning (provider / model): Test warning", log.Console[1]);
            Assert.Empty(log.Process);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should only log for non-empty warnings", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Default_logger_skips_empty_lists()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "err", "m"));
            Assert.Empty(log.Process);
            LogWarnings.Log(Options(new OtherWarning("t1"), "prov", "mod"));
            Assert.Equal(2, log.Process.Count);
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "prov", "mod"));
            Assert.Equal(2, log.Process.Count);
            LogWarnings.Log(Options(new OtherWarning("t2"), "prov", "mod"));
            Assert.Equal(3, log.Process.Count);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should handle various warning types per formatWarning", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Default_logger_formats_each_warning_type()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(new ModelWarning[]
            {
                new UnsupportedWarning("mediaType", "detail"),
                new UnsupportedWarning("voice", "detail2"),
                new DeprecatedWarning("providerOptions key 'old-key'", "Use 'oldKey' instead."),
                new OtherWarning("other msg"),
            }, "zzz", "MMM"));
            Assert.Empty(log.Console);
            Assert.Equal(5, log.Process.Count);
            Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), log.Process[0]);
            Assert.Equal(("AI SDK Warning (zzz / MMM): The feature \"mediaType\" is not supported. detail", "Warning"), log.Process[1]);
            Assert.Equal(("AI SDK Warning (zzz / MMM): The feature \"voice\" is not supported. detail2", "Warning"), log.Process[2]);
            Assert.Equal(("AI SDK Warning (zzz / MMM): Deprecated: \"providerOptions key 'old-key'\". Use 'oldKey' instead.", "DeprecationWarning"), log.Process[3]);
            Assert.Equal(("AI SDK Warning (zzz / MMM): other msg", "Warning"), log.Process[4]);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is unset/undefined (default behavior)::should include warning even with \"unknown provider\" and \"unknown model\"", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Default_logger_keeps_unknown_provider_and_model()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(new OtherWarning("messx"), "unknown provider", "unknown model"));
            Assert.Empty(log.Console);
            Assert.Contains(("AI SDK Warning (unknown provider / unknown model): messx", "Warning"), log.Process);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > when AI_SDK_LOG_WARNINGS is undefined (explicitly set)::should use default behavior and emit via process.emitWarning", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Explicit_null_logger_uses_the_default()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(new OtherWarning("Test warning with undefined logger"), "p1", "m1"));
            Assert.Empty(log.Console);
            Assert.Equal(2, log.Process.Count);
            Assert.Equal((LogWarnings.FirstWarningInfoMessage, "Warning"), log.Process[0]);
            Assert.Equal(("AI SDK Warning (p1 / m1): Test warning with undefined logger", "Warning"), log.Process[1]);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should not display the info message for empty warnings", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Empty_warnings_do_not_emit_the_note()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "a", "b"));
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should display informational note only on first real call", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Note_appears_on_the_first_real_call_only()
    {
        var log = new Recorder();
        try
        {
            log.Install(null);
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "a", "b"));
            LogWarnings.Log(Options(Array.Empty<ModelWarning>(), "a", "b"));
            Assert.Empty(log.Process);
            LogWarnings.Log(Options(new OtherWarning("foo"), "abc", "bbb"));
            Assert.Equal(2, log.Process.Count);
            LogWarnings.Log(Options(new OtherWarning("bar"), "abc", "bbb"));
            Assert.Equal(3, log.Process.Count);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should not display information note when using custom logger", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void Custom_logger_does_not_emit_the_note()
    {
        var log = new Recorder();
        var calls = 0;
        try
        {
            log.Install((Action<LogWarningsOptions>)delegate { calls++; });
            LogWarnings.Log(Options(new OtherWarning("Message"), "provV", "modZ"));
            Assert.Equal(1, calls);
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > first-time information note::should not display information note when AI_SDK_LOG_WARNINGS is false", Coverage = UpstreamCoverage.Covered)]
    [Fact]
    public void False_logger_does_not_emit_the_note()
    {
        var log = new Recorder();
        try
        {
            log.Install(false);
            LogWarnings.Log(Options(new OtherWarning("Suppressed"), "notProv", "notModel"));
            Assert.Empty(log.Process);
            Assert.Empty(log.Console);
        }
        finally
        {
            log.Restore();
        }
    }

    private static LogWarningsOptions Options(ModelWarning warning, string provider, string model)
    {
        return Options(new ModelWarning[] { warning }, provider, model);
    }

    private static LogWarningsOptions Options(IReadOnlyList<ModelWarning> warnings, string provider, string model)
    {
        return new LogWarningsOptions(warnings, provider, model);
    }

    private sealed class Recorder
    {
        public List<(string Message, string Type)> Process { get; } = new List<(string, string)>();

        public List<string> Console { get; } = new List<string>();

        public void Install(object? logger)
        {
            LogWarnings.ResetState();
            LogWarnings.Logger = logger;
            LogWarnings.ProcessEmitWarning = delegate (string message, string type) { Process.Add((message, type)); };
            LogWarnings.ConsoleWarn = delegate (string message) { Console.Add(message); };
        }

        public void Restore()
        {
            LogWarnings.Logger = null;
            LogWarnings.ProcessEmitWarning = delegate { };
            LogWarnings.ConsoleWarn = delegate { };
            LogWarnings.ResetState();
        }
    }
}
