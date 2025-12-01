# MCP (Model Context Protocol) configuration for this workspace

This workspace includes an `mcp.json` (which configures MCP servers for GitHub Copilot and Microsoft Learn documentation) and recommends a few extensions to enable MCP features in VS Code.

## Recommended setup

1. Install the recommended extensions (copilot, copilot-labs, C#):
   - `github.copilot`
   - `github.copilot-labs`
   - `ms-dotnettools.csharp`

2. Sign into GitHub Copilot and (optionally) Copilot Labs in VS Code.

3. When prompted by the MCP client or the Copilot extension, provide a GitHub PAT or App token. This repository has an `mcp.json` that defines an input named `Authorization` that the extension will ask for.

4. After installing extensions and signing in, open `Command Palette (Ctrl+Shift+P)` and look for MCP or Copilot commands to connect to the ``io.github.github/github-mcp-server`` server and the ``microsoftdocs/mcp`` server.

## Privacy & security

- Do not commit secrets (PATs, tokens or credentials). The `mcp.json` file expects the token to be provided via the editor prompt and does not store it in the repository.

## Troubleshooting

- If you don't see the MCP options in Copilot, update Copilot and Copilot Labs to the latest version.
- Ensure your GitHub token has appropriate scopes (usually `repo`, `read:org`, or other scopes Copilot/extension documents require).
