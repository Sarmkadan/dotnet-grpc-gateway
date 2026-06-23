# Contributing to dotnet-grpc-gateway

Thank you for your interest in contributing to dotnet-grpc-gateway! We welcome contributions from the community.

## Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally
3. **Create** a feature branch: `git checkout -b feature/my-feature`
4. **Commit** your changes following our commit conventions
5. **Push** to your fork
6. **Submit** a pull request

## Code Standards

- Follow **C# naming conventions** (PascalCase for classes/methods, camelCase for parameters)
- Add **XML documentation** to all public members
- Keep methods under **50 lines** when possible
- Write **unit tests** for new functionality (xUnit + Moq)
- Use **dependency injection** for testability
- Follow the existing code structure and patterns

## Testing

Run the test suite before submitting changes:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/dotnet-grpc-gateway.Tests/
```

## Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/) format:

- `feat:` for new features
- `fix:` for bug fixes
- `docs:` for documentation changes
- `test:` for test additions/updates
- `refactor:` for code improvements without functional changes
- `chore:` for build/config changes
- `perf:` for performance improvements

Example: `feat: add circuit breaker per service`

## Pull Request Process

1. Update any relevant documentation
2. Include tests for new functionality
3. Ensure all tests pass
4. Add descriptive PR title and description
5. Link related issues if applicable
6. Request review from maintainers

## Development Setup

### Prerequisites
- .NET 10 SDK or later
- PostgreSQL 14+ (for configuration storage)
- Git

### Running Locally

```bash
# Clone repository
git clone https://github.com/sarmkadan/dotnet-grpc-gateway.git
cd dotnet-grpc-gateway

# Restore dependencies
dotnet restore

# Build solution
dotnet build -c Release

# Run tests
dotnet test
```

## Reporting Issues

- Use GitHub Issues for bug reports and feature requests
- Include reproduction steps for bugs
- Provide environment details (OS, .NET version, etc.)
- Be clear and concise in your description


## Community Guidelines

- Be respectful and inclusive
- Follow the [Code of Conduct](CODE_OF_CONDUCT.md)
- Ask questions in GitHub Discussions if unsure
- Help review other contributors' PRs


## License
By contributing, you agree that your contributions will be licensed under the MIT License.

---
**Questions?** Open an issue or start a discussion on GitHub.
