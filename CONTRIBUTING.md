# Contributing to dotnet-grpc-gateway

Thanks for contributing. This file covers the workflow. For project layout and code patterns, see [CLAUDE.md](CLAUDE.md). For community rules, see [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Prerequisites

- **.NET 10 SDK.** All projects target `net10.0`, and `build.yml` uses `10.0.x`. The matrix in `ci.yml` also lists `8.0.x`, but these projects will not build against the .NET 8 SDK.
- Git
- PostgreSQL, only if you run the gateway locally. Set `ConnectionStrings:DefaultConnection` (see `src/dotnet-grpc-gateway/appsettings.example.json`).
- Docker, optional, for `make docker-run`.

## Build

```bash
dotnet restore dotnet-grpc-gateway.sln
dotnet build dotnet-grpc-gateway.sln -c Release --no-restore
```

Or run `make build`. To start the gateway, use `make run-dev`.

## Tests

```bash
dotnet test dotnet-grpc-gateway.sln -c Release
```

Tests live in `tests/dotnet-grpc-gateway.Tests/` and use xUnit, FluentAssertions and Moq. To run a single class:

```bash
dotnet test --filter "FullyQualifiedName~CircuitBreakerTests"
```

Coverage: `make coverage`.

## Coding conventions

- Follow `.editorconfig`. It is checked in, and `dotnet format` applies it.
- Use 4-space indentation and spaces, not tabs.
- Put the opening brace of types, members and blocks on its own line (Allman style).
- Nullable reference types and implicit usings are enabled. Do not disable them or suppress warnings without a reason.
- Use file-scoped namespaces. New `.cs` files should start with `#nullable enable` and the author header block (most existing files do).
- Naming: `I*` for interfaces, `_camelCase` for private fields, `*Controller`, `*BackgroundService`.
- Tests: class `FooTests`, method names `Method_Scenario_Expected`, a `CreateSut()` factory, and FluentAssertions `.Should()`.
- Before you push, run `make format` and then `make lint` (`dotnet format --verify-no-changes`). `make analyze` runs the analyzers with code-style enforcement.

## Commits

Use [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `perf:`, `chore:`. For example: `fix: keep route cache when service health changes`.

## Branch naming

Create a branch from `main` and do not commit to `main` directly. Use `<type>/<short-kebab-case-description>`, where the type matches the commit type:

- `feat/per-service-circuit-breaker`
- `fix/precompile-route-regex`
- `docs/improve-api-documentation`

## Pull requests

1. Target `main`. `ci.yml` runs on every PR to `main`.
2. Keep PRs small and focused: one logical change per PR. Put refactors in a separate PR from behaviour changes.
3. Add or update tests for new behaviour and bug fixes.
4. Update `docs/` and `README.md` when you change public behaviour.
5. CI must be green before merge. Run the format check and the tests locally first.
6. Describe the change in the PR body: what changed, why, how you tested it, and any breaking changes. Link related issues.

## Reporting issues

Use GitHub Issues. For bugs, include steps to reproduce, the .NET version, and the OS.

## License

The project is MIT licensed (see [LICENSE](LICENSE)). By contributing, you agree that your contributions are licensed under the same terms.
