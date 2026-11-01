#!/usr/bin/env node
import { createInterface } from 'node:readline';

const tinyPng =
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==';

const lines = createInterface({ input: process.stdin });

lines.on('line', line => {
  const message = JSON.parse(line);

  if (message.method === 'initialize') {
    respond(message.id, {
      protocolVersion: '2025-06-18',
      serverInfo: { name: 'image-test-server', version: '1.0.0' },
      capabilities: { tools: {} },
    });
  } else if (message.method === 'tools/list') {
    respond(message.id, {
      tools: [
        {
          name: 'get-image',
          description: 'Returns a test image',
          inputSchema: { type: 'object', properties: {} },
        },
      ],
    });
  } else if (message.method === 'tools/call') {
    respond(message.id, {
      content: [
        {
          type: 'image',
          data: tinyPng,
          mimeType: 'image/png',
        },
      ],
      isError: false,
    });
  }
});

function respond(id, result) {
  console.log(JSON.stringify({ jsonrpc: '2.0', id, result }));
}
