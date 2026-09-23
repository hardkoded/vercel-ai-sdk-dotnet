# Compatibility

The repo root `COMPATIBILITY.md` is the source of truth: upstream commit, package map, UI chunks that are not emitted, and the Node-only packages that this port does not include.

This library does not ship React, Vue, Svelte, Angular, or RSC bindings. Server code that needs the UI protocol should return `ToUIMessageStreamResult`.
