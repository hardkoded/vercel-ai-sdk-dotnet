// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json.Nodes;
using Vercel.AI.ProviderUtils;
using Vercel.AI.Ui;
using Vercel.AI.Util;

namespace Vercel.AI.Tests.Upstream.ProcessUiMessageStream;

/// <summary>Port of <c>process-ui-message-stream.test.ts</c> &gt; <c>processUIMessageStream</c> &gt; <c>dynamic tool errors after input streaming</c>.</summary>
public sealed class DynamicToolErrorsAfterInputStreamingTests
{
    private const string Prefix = "packages/ai/src/ui/process-ui-message-stream.test.ts::processUIMessageStream > ";

    [Theory]
    [InlineData("tool input error")]
    [InlineData("tool output error")]
    [UpstreamTest(Prefix + "dynamic tool errors after input streaming::clears raw input on $name", Coverage = UpstreamCoverage.Partial, Note = "validateUIMessages is not ported.")]
    public async Task Clears_raw_input_on_name(string name)
    {
        var terminalChunk = name == "tool input error"
            ? """{"type":"tool-input-error","toolCallId":"call-1","toolName":"cityAttractions","input":{"cities":["San Francisco"]},"errorText":"Invalid input for tool cityAttractions","dynamic":true}"""
            : """{"type":"tool-output-error","toolCallId":"call-1","errorText":"Tool execution failed","dynamic":true}""";
        var warnings = new List<LogWarningsOptions>();
        var previous = LogWarnings.Logger;
        LogWarnings.Logger = new Action<LogWarningsOptions>(warnings.Add);
        try
        {
            var harness = new ProcessUIMessageStreamHarness();
            harness.CreateState("msg-123", null);
            await harness.ProcessAsync(
                "[" + """{"type":"start"},{"type":"start-step"},{"type":"tool-input-start","toolCallId":"call-1","toolName":"cityAttractions","dynamic":true},{"type":"tool-input-delta","toolCallId":"call-1","inputTextDelta":"{ \"cities\": [\"San Francisco\"] }"},""" + terminalChunk + """,{"type":"finish-step"},{"type":"finish"}""" + "]");

            var toolPart = harness.FindToolPart("call-1")!;
            Assert.Equal("dynamic-tool", (string?)toolPart["type"]);
            Assert.Equal("output-error", (string?)toolPart["state"]);
            Assert.Null(toolPart["rawInput"]);

            // validateUIMessages is not ported yet, so the message is not validated here.
            Assert.Empty(warnings);
        }
        finally
        {
            LogWarnings.Logger = previous;
        }
    }
}
