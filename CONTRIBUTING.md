# Contributing to dotnet-grpc-gateway

Thank you for considering contributing to dotnet-grpc-gateway! Every contribution helps make the project better.

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Docker (optional, for container testing)
- Git

## Building Locally

```bash
# Clone the repository
git clone https://github.com/your-username/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build -c Release

# Or use the Makefile shortcut
make build
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run tests with TRX output (for CI parity)
dotnet test --verbosity normal --logger "trx"

# Run a specific test project
dotnet test tests/dotnet-grpc-gateway.Tests/dotnet-grpc-gateway.Tests.csproj
```

## Running Locally with Docker

```bash
# Build the Docker image
docker build -t dotnet-grpc-gateway .

# Start the full stack
docker compose up
```

## How to Contribute

### 1. Fork and Clone
Fork the repository on GitHub, then clone your fork:

```bash
git clone https://github.com/your-username/dotnet-grpc-gateway.git
```

### 2. Create a Branch

```bash
git checkout -b feature/my-feature
# or
git checkout -b fix/my-bug-fix
```

### 3. Make Your Changes

- Write clear, focused commits.
- Keep changes scoped — one logical change per PR.
- Add or update tests for any new behaviour.

### 4. Submit a Pull Request
Push your branch and open a PR against `main`. Describe what your change does and why.

## Code Style

The project enforces consistent formatting via `.editorconfig`. Your editor should pick it up automatically.

Key conventions:
- 4 spaces for indentation in C# files.
- Allman brace style (`{` on its own line).
- `var` preferred when the type is apparent.
- File-scoped namespaces.
- XML documentation comments on all public APIs.

Run the formatter to check compliance:

```bash
dotnet format --verify-no-changes
```

## Reporting Issues

Use [GitHub Issues](https://github.com/sarmkadan/dotnet-grpc-gateway/issues) to report bugs or request features. Please include:

- A clear, descriptive title.
- Steps to reproduce the issue.
- Expected vs. actual behaviour.
- Relevant logs or error messages.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).