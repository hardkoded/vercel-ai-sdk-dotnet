// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Diagnostics;
using Vercel.AI.Mcp;

namespace Vercel.AI.Tests.Upstream.Mcp.McpStdioTransport;

public sealed class CloseTests
{
    [Fact]
    [UpstreamTest(
        "packages/mcp/src/tool/mcp-stdio/mcp-stdio-transport.test.ts::StdioMCPTransport > close::should close the transport successfully",
        Coverage = UpstreamCoverage.Partial,
        Note = "Disposing kills the child. There is no onclose callback."
    )]
    public void ShouldCloseTheTransportSuccessfully()
    {
        var windows = OperatingSystem.IsWindows();
        var options = new StdioMcpOptions(windows ? "ping" : "sleep")
        {
            Arguments = windows ? new[] { "-n", "60", "127.0.0.1" } : new[] { "60" },
        };
        var transport = StdioMcpTransport.Start(options);
        Process process;
        try
        {
            process = Process.GetProcessById(transport.ProcessId);
        }
        catch
        {
            transport.Dispose();
            throw;
        }

        using (process)
        {
            Assert.False(process.HasExited);

            transport.Dispose();

            Assert.True(process.WaitForExit(10000));
        }
    }
}
