// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using Vercel.AI;
using Vercel.AI.Mcp;
using Vercel.AI.Prompt;

using var transport = StdioMcpTransport.Start("node", new[] { Path.Combine(AppContext.BaseDirectory, "server.mjs") });
var tools = await McpClient.ConnectAsync(transport);
var tool = AssertSingle(tools);
using var input = JsonDocument.Parse("{}");

var output = await tool.Execute!(input.RootElement, CancellationToken.None);
Console.WriteLine("Raw execute() result (MCP format):");
Console.WriteLine(output);

var modelOutput = (ContentToolModelOutput)tool.ToModelOutput!(
    new ToolModelOutputCall("1", input.RootElement, output));
var file = (FileToolModelContentPart)modelOutput.Value[0];

Console.WriteLine();
Console.WriteLine("Image content converted:");
Console.WriteLine("  - MCP: type=\"image\", mimeType=\"image/png\"");
Console.WriteLine($"  - SDK: type=\"{file.Type}\", mediaType=\"{file.MediaType}\", data.type=\"{file.Data.Type}\"");

static Tool AssertSingle(IReadOnlyList<Tool> tools)
{
    return tools.Count == 1 ? tools[0] : throw new InvalidOperationException("Expected one MCP tool.");
}
