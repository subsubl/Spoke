# Spec-kit / specify-cli — how-to for Spoke contributors

This repository includes a short `.specify` skeleton so contributors can start spec-driven work without needing the CLI installed locally.

Quick usage notes

- The project CLI (`specify`) is a Python package located in `tools/spec-kit/src/specify_cli` of the cloned repo. The recommended ways to run it locally are:

  1. Install the `uv` tooling (astral/uv) and run:

```powershell
uv tool install specify-cli --from git+https://github.com/github/spec-kit.git
specify init .
```

  2. One-time run with `uvx` (no persistent install):

```powershell
uvx --from git+https://github.com/github/spec-kit.git specify init .
```

  3. If you have Python 3.11+ and `pip`, you can install the CLI locally for development:

```powershell
py -3 -m pip install -e tools/spec-kit/src/specify_cli
specify init .
```

Notes
- If you do not want to install anything locally, use the provided `.specify` files to coordinate work. The `.specify` files are human-editable markdown and are the canonical starting point for discussions and PRs.
- Consider adding a GitHub Action to run `specify` in CI for automatic plan/task validation; we can add that in a follow-up.
