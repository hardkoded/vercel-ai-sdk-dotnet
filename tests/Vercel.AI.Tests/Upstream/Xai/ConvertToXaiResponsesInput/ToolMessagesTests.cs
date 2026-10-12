// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Provider;
using static Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput.ConvertToXaiResponsesInputSupport;

namespace Vercel.AI.Tests.Upstream.Xai.ConvertToXaiResponsesInput;

/// <summary>Port of <c>convert-to-xai-responses-input.test.ts</c> &gt; <c>convertToXaiResponsesInput &gt; tool messages</c>.</summary>
public sealed class ToolMessagesTests
{
    private const string Prefix = TestFile + " > tool messages::";

    [Fact]
    [UpstreamTest(Prefix + "should convert tool-result to function_call_output with json", Coverage = UpstreamCoverage.Covered)]
    public void Should_convert_tool_result_to_function_call_output_with_json()
    {
        var input = Convert(new ToolModelMessage("call_123", "weather", "{\"temp\":72}", isError: false));

        JsonAssert.Equal(input, """[{"call_id":"call_123","output":"{\"temp\":72}","type":"function_call_output"}]""");
    }

    [Fact]
    [UpstreamTest(Prefix + "should handle text output", Coverage = UpstreamCoverage.Covered)]
    public void Should_handle_text_output()
    {
        var input = Convert(new ToolModelMessage("call_123", "weather", "sunny, 72 degrees", isError: false));

        JsonAssert.Equal(input, """[{"call_id":"call_123","output":"sunny, 72 degrees","type":"function_call_output"}]""");
    }
}
