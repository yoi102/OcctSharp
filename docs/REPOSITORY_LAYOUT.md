# Repository Layout

## Decision

The Git repository root contains documentation and repository-level metadata.
All code-related projects and artifacts live under the inner `OcctSharp/` directory.

```text
OcctSharp/                       # Git repository root
├── .git/
├── .gitignore
├── .gitattributes
├── AGENTS.md
├── README.md
├── LICENSE
├── NOTICE
├── docs/
│   ├── DOCUMENTATION_INDEX.md
│   ├── ARCHITECTURE.md
│   ├── ROADMAP.md
│   ├── STATUS.md
│   └── adr/
└── OcctSharp/                   # Code and build workspace
    ├── OcctSharp.slnx
    ├── Directory.Build.props
    ├── Directory.Packages.props
    ├── CMakePresets.json
    ├── src/
    │   ├── OcctSharp.Generator/
    │   ├── OcctSharp.Native/
    │   │   └── generated/
    │   └── OcctSharp/
    │       └── Generated/
    ├── tests/
    │   ├── OcctSharp.Generator.Tests/
    │   ├── OcctSharp.Native.Tests/
    │   ├── OcctSharp.Runtime.Tests/
    │   ├── OcctSharp.Integration.Tests/
    │   └── TestData/
    ├── benchmarks/
    ├── config/
    ├── baselines/
    ├── reports/
    └── packaging/
        └── OcctSharp.targets
```

This is a target layout, not a statement that those files already exist.

## Ownership of directories

| Area | Purpose | Committed |
|---|---|---|
| `docs/` | Human-maintained architecture and project records | Yes |
| `OcctSharp/src/` | Generator, native bridge, raw bindings, friendly SDK | Yes |
| `OcctSharp/tests/` | Unit, compile, runtime, integration, lifetime tests | Yes |
| `OcctSharp/config/` | Versioned generator scope and generation settings | Yes |
| `OcctSharp/packaging/` | NuGet build assets and package layout rules | Yes |
| `OcctSharp/baselines/` | Canonical API manifests used for upgrade diffs | Yes |
| `OcctSharp/generated/manifest.json` | Generated-file ownership and content hashes | Yes |
| `OcctSharp/reports/` | Selected reviewable generation and upgrade reports | Only when explicitly selected |
| `OcctSharp/artifacts/generator-reports/` | Transient coverage and detailed diagnostics from generation | No |
| `OcctSharp/src/OcctSharp.Native/generated/` | Deterministic generated C ABI source | Yes |
| `OcctSharp/src/OcctSharp/Generated/` | Deterministic generated managed raw source | Yes |
| Build output and downloaded dependencies | Rebuildable local artifacts | No |

## Git rules

- Initialize Git only at the repository root.
- Do not place a nested `.git` directory under the inner `OcctSharp/` directory.
- Git does not track empty directories; the inner directory appears in a clone only
  after it contains committed files.
- Large CAD test assets require an explicit size and licensing policy before commit.
- Generated sources must never contain machine-specific absolute paths.
- Stale generated files are removed only when the previous generated manifest owns
  their paths; generated directories are never cleared indiscriminately.
