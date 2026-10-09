// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Util;
using WarningLog = Vercel.AI.Util.LogWarnings;

namespace Vercel.AI.Tests.Upstream.Logger.LogWarningsNode;

/// <summary>Port of <c>log-warnings.node.test.ts</c> &gt; <c>Node deprecation warning behavior</c>.</summary>
public sealed class NodeDeprecationWarningBehaviorTests
{
    [Fact]
    [UpstreamTest("packages/ai/src/logger/log-warnings.node.test.ts::Node deprecation warning behavior::emits a coded warning event and prints the deprecation only once", Coverage = UpstreamCoverage.Covered)]
    public void Emits_a_coded_warning_event_and_prints_the_deprecation_only_once()
    {
        using var log = new LogWarningsRecorder();
        var options = new LogWarningsOptions(new ModelWarning[]
        {
            new DeprecatedWarning("generateObject", "Use generateText with an output setting instead."),
        });

        WarningLog.Log(options);
        WarningLog.Log(options);

        Assert.Equal(2, log.Process.Count);
        Assert.Equal("Warning", log.Process[0].Type);
        Assert.Null(log.Process[0].Code);
        Assert.Equal("DeprecationWarning", log.Process[1].Type);
        Assert.Equal("AISDK_DEP_GENERATE_OBJECT", log.Process[1].Code);
    }
}
