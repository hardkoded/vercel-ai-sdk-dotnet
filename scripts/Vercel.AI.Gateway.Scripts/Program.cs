// Copyright 2023 Vercel, Inc.
// Copyright 2026 Darío Kondratiuk
// SPDX-License-Identifier: Apache-2.0

using Vercel.AI.Gateway.Scripts;

// Run from the repo root: dotnet run --project scripts/Vercel.AI.Gateway.Scripts
var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "src", "Vercel.AI", "Gateway");
using var http = new HttpClient();
await GenerateModelSettings.MainAsync(new GenerateModelSettingsHost(http, outputDir));
