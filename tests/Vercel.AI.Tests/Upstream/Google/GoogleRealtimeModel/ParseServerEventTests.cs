// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

namespace Vercel.AI.Tests.Upstream.Google.GoogleRealtimeModel;

/// <summary>Port of <c>google-realtime-model.test.ts</c> &gt; <c>GoogleRealtimeModel &gt; parseServerEvent</c>.</summary>
public sealed class ParseServerEventTests
{
    private const string Id =
        "packages/google/src/realtime/google-realtime-model.test.ts::GoogleRealtimeModel > parseServerEvent::";

    [Fact]
    [UpstreamTest(Id + "delegates to mapper", Coverage = UpstreamCoverage.Covered)]
    public void Delegates_to_mapper()
    {
        var model = GoogleRealtimeTestSupport.CreateModel(
            "gemini-2.0-flash-live-001",
            GoogleRealtimeTestSupport.ApiKeyHeaders()
        );

        var result = Assert.Single(
            model.ParseServerEvent(GoogleUpstream.Element("""{"setupComplete":true}"""))
        );

        GoogleRealtimeTestSupport.AssertEvent(
            result,
            """{"setupComplete":true}""",
            "session-created"
        );
    }
}
