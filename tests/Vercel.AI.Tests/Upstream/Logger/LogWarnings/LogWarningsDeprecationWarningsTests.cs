// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using WarningLog = Vercel.AI.Util.LogWarnings;

namespace Vercel.AI.Tests.Upstream.Logger.LogWarnings;

/// <summary>Port of <c>log-warnings.test.ts</c> &gt; <c>logWarnings &gt; deprecation warnings</c>.</summary>
public sealed class LogWarningsDeprecationWarningsTests
{
    private const string Message = "AI SDK Warning: Deprecated: \"generateObject\". Use generateText with an output setting instead.";

    private static readonly DeprecatedWarning Deprecated = new DeprecatedWarning("generateObject", "Use generateText with an output setting instead.");

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::emits each code once across batches, calls, and message changes", Coverage = UpstreamCoverage.Covered)]
    public void Emits_each_code_once_across_batches_calls_and_message_changes()
    {
        using var log = new LogWarningsRecorder();

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated, Deprecated }));
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { new DeprecatedWarning("generateObject", "Updated wording.") }));
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { new DeprecatedWarning("streamObject", Deprecated.Message) }));

        Assert.Equal(3, log.Process.Count);
        Assert.Equal((Message, "DeprecationWarning", "AISDK_DEP_GENERATE_OBJECT"), log.Process[1]);
        Assert.Equal("DeprecationWarning", log.Process[2].Type);
        Assert.Equal("AISDK_DEP_STREAM_OBJECT", log.Process[2].Code);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::deduplicates across models while keeping providers separate", Coverage = UpstreamCoverage.Covered)]
    public void Deduplicates_across_models_while_keeping_providers_separate()
    {
        using var log = new LogWarningsRecorder();

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }, "a", "first"));
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }, "a", "second"));
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }, "b", "first"));

        Assert.Equal(3, log.Process.Count);
        Assert.Equal("DeprecationWarning", log.Process[2].Type);
        Assert.Equal("AISDK_DEP_PROVIDER_b__generateObject", log.Process[2].Code);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::continues emitting ordinary warnings on every call", Coverage = UpstreamCoverage.Covered)]
    public void Continues_emitting_ordinary_warnings_on_every_call()
    {
        using var log = new LogWarningsRecorder();
        var other = new OtherWarning("Repeated warning.");

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated, other }));
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated, other }));

        Assert.Equal(4, log.Process.Count);
        Assert.Equal(("AI SDK Warning: Repeated warning.", "Warning", (string?)null), log.Process[3]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::does not consume codes while warnings are disabled", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_consume_codes_while_warnings_are_disabled()
    {
        using var log = new LogWarningsRecorder(false);

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));
        Assert.Empty(log.Process);

        WarningLog.Logger = null;
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));
        Assert.Equal(2, log.Process.Count);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::passes every original warning to custom loggers, including after default emission", Coverage = UpstreamCoverage.Covered)]
    public void Passes_every_original_warning_to_custom_loggers_including_after_default_emission()
    {
        using var log = new LogWarningsRecorder();
        var options = new LogWarningsOptions(new ModelWarning[] { Deprecated, Deprecated });
        var received = new List<LogWarningsOptions>();

        WarningLog.Log(options);
        WarningLog.Logger = (Action<LogWarningsOptions>)received.Add;
        WarningLog.Log(options);
        WarningLog.Log(options);

        Assert.Equal(2, received.Count);
        Assert.All(received, call => Assert.Same(options, call));
        Assert.Equal(2, log.Process.Count);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::does not consume codes while using a custom logger", Coverage = UpstreamCoverage.Covered)]
    public void Does_not_consume_codes_while_using_a_custom_logger()
    {
        using var log = new LogWarningsRecorder((Action<LogWarningsOptions>)(_ => { }));

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));
        Assert.Empty(log.Process);

        WarningLog.Logger = null;
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));
        Assert.Equal(2, log.Process.Count);
    }

    [Theory]
    [InlineData("undefined")]
    [InlineData("an object without emitWarning")]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::deduplicates and includes codes when process is %s", Coverage = UpstreamCoverage.Covered)]
    public void Deduplicates_and_includes_codes_when_process_is(string processValue)
    {
        using var log = new LogWarningsRecorder();
        WarningLog.ProcessEmitWarning = null;

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated, Deprecated }));
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));

        Assert.Empty(log.Process);
        Assert.True(log.Console.Count == 2, "process is " + processValue);
        Assert.Equal("[AISDK_DEP_GENERATE_OBJECT] " + Message, log.Console[^1]);
    }

    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.test.ts::logWarnings > deprecation warnings::clears deduplication state when resetting the logger for tests", Coverage = UpstreamCoverage.Covered)]
    public void Clears_deduplication_state_when_resetting_the_logger_for_tests()
    {
        using var log = new LogWarningsRecorder();

        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));
        WarningLog.ResetState();
        WarningLog.Log(new LogWarningsOptions(new ModelWarning[] { Deprecated }));

        Assert.Equal(4, log.Process.Count);
    }
}
