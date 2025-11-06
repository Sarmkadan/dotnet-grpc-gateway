# =============================================================================
# dotnet-grpc-gateway Makefile
# Author: Vladyslav Zaiets | https://sarmkadan.com
# =============================================================================

.PHONY: help build test clean run docker-build docker-run docker-down \
        restore format lint docs coverage pre-commit

.DEFAULT_GOAL := help

# Variables
SOLUTION := dotnet-grpc-gateway.sln
PROJECT := src/dotnet-grpc-gateway/dotnet-grpc-gateway.csproj
CONFIG := Release
OUTPUT := bin/$(CONFIG)

# Colors for output
BLUE := \033[0;36m
GREEN := \033[0;32m
YELLOW := \033[0;33m
NC := \033[0m # No Color

help: ## Display this help screen
	@echo "$(BLUE)dotnet-grpc-gateway - Build & Development Commands$(NC)"
	@echo ""
	@grep -h -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  $(YELLOW)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

# Build targets
restore: ## Restore NuGet packages
	@echo "$(BLUE)Restoring packages...$(NC)"
	dotnet restore $(SOLUTION)

build: restore ## Build the solution
	@echo "$(BLUE)Building solution...$(NC)"
	dotnet build $(SOLUTION) -c $(CONFIG) --no-restore

build-release: ## Build release version
	@echo "$(BLUE)Building release version...$(NC)"
	dotnet build $(SOLUTION) -c Release --no-restore

clean: ## Clean build artifacts
	@echo "$(BLUE)Cleaning artifacts...$(NC)"
	dotnet clean $(SOLUTION)
	rm -rf bin obj
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true

rebuild: clean build ## Clean and rebuild

# Run targets
run: build ## Build and run the gateway
	@echo "$(GREEN)Starting gateway...$(NC)"
	dotnet run --project $(PROJECT) -c $(CONFIG)

run-dev: ## Run in development mode
	@echo "$(GREEN)Starting gateway in development mode...$(NC)"
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project $(PROJECT)

run-prod: build-release ## Run in production mode
	@echo "$(GREEN)Starting gateway in production mode...$(NC)"
	ASPNETCORE_ENVIRONMENT=Production dotnet run --project $(PROJECT) -c Release

# Testing targets
test: build ## Run unit tests
	@echo "$(BLUE)Running tests...$(NC)"
	dotnet test $(SOLUTION) -c $(CONFIG) --no-build --verbosity normal

test-verbose: build ## Run tests with verbose output
	@echo "$(BLUE)Running tests (verbose)...$(NC)"
	dotnet test $(SOLUTION) -c $(CONFIG) --no-build --verbosity detailed

coverage: ## Run tests with code coverage
	@echo "$(BLUE)Running coverage analysis...$(NC)"
	dotnet test $(SOLUTION) --collect:"XPlat Code Coverage" -c $(CONFIG)

watch: ## Watch for changes and run tests
	@echo "$(BLUE)Watching for changes...$(NC)"
	dotnet watch test $(SOLUTION) -c $(CONFIG)

# Code quality targets
format: ## Format code with dotnet-format
	@echo "$(BLUE)Formatting code...$(NC)"
	dotnet format $(SOLUTION)

format-check: ## Check code formatting
	@echo "$(BLUE)Checking code format...$(NC)"
	dotnet format $(SOLUTION) --verify-no-changes

lint: format-check ## Run linting checks
	@echo "$(BLUE)Running lint checks...$(NC)"

analyze: ## Run static code analysis
	@echo "$(BLUE)Running code analysis...$(NC)"
	dotnet build $(SOLUTION) /p:EnforceCodeStyleInBuild=true

pre-commit: format test lint ## Run all pre-commit checks
	@echo "$(GREEN)Pre-commit checks passed!$(NC)"

# Docker targets
docker-build: ## Build Docker image
	@echo "$(BLUE)Building Docker image...$(NC)"
	docker build -t dotnet-grpc-gateway:latest .

docker-run: docker-build ## Build and run Docker container
	@echo "$(GREEN)Starting Docker container...$(NC)"
	docker-compose up -d

docker-logs: ## View Docker container logs
	@echo "$(BLUE)Container logs...$(NC)"
	docker-compose logs -f gateway

docker-down: ## Stop and remove Docker containers
	@echo "$(BLUE)Stopping containers...$(NC)"
	docker-compose down

docker-clean: docker-down ## Remove Docker containers and volumes
	@echo "$(YELLOW)Removing volumes...$(NC)"
	docker-compose down -v

docker-rebuild: docker-clean docker-run ## Rebuild and run Docker containers

# Database targets
db-create: ## Create database
	@echo "$(BLUE)Creating database...$(NC)"
	createdb grpc_gateway || echo "Database already exists"

db-drop: ## Drop database
	@echo "$(YELLOW)Dropping database...$(NC)"
	dropdb grpc_gateway || echo "Database doesn't exist"

db-reset: db-drop db-create ## Reset database
	@echo "$(GREEN)Database reset complete!$(NC)"

db-migrate: ## Run database migrations
	@echo "$(BLUE)Running migrations...$(NC)"
	dotnet run --project $(PROJECT) -- migrate

# Documentation targets
docs: ## Generate documentation
	@echo "$(BLUE)Generating documentation...$(NC)"
	@echo "  - README.md"
	@echo "  - docs/GETTING-STARTED.md"
	@echo "  - docs/ARCHITECTURE.md"
	@echo "  - docs/API-REFERENCE.md"
	@echo "  - docs/DEPLOYMENT.md"
	@echo "  - docs/FAQ.md"
	@echo "$(GREEN)Documentation available in /docs directory$(NC)"

docs-serve: ## Serve documentation locally
	@echo "$(BLUE)Documentation files are in /docs directory$(NC)"
	@echo "Edit with your favorite markdown editor"

# Git targets
git-status: ## Show git status
	@git status

git-diff: ## Show uncommitted changes
	@git diff

git-log: ## Show recent commits
	@git log --oneline -10

# Utility targets
version: ## Show .NET version
	@echo "$(BLUE).NET Version:$(NC)"
	@dotnet --version

info: ## Display project information
	@echo "$(BLUE)Project Information:$(NC)"
	@echo "  Name: dotnet-grpc-gateway"
	@echo "  Framework: .NET 10.0"
	@echo "  Language: C# 14"
	@echo "  Solution: $(SOLUTION)"
	@echo "  Project: $(PROJECT)"

status: git-status ## Alias for git-status

logs: docker-logs ## Alias for docker-logs

ps: ## Show running containers
	@docker-compose ps

# Combined targets
setup: restore ## Setup development environment
	@echo "$(GREEN)Setup complete! Run 'make run' to start the gateway$(NC)"

all: clean build test lint ## Run all checks

develop: setup run-dev ## Setup and run in development mode

production: setup build-release run-prod ## Setup and run in production mode

deploy: clean build-release test docker-build ## Prepare for deployment
	@echo "$(GREEN)Ready for deployment!$(NC)"
	@echo "Docker image: dotnet-grpc-gateway:latest"
	@echo "Push with: docker push <registry>/dotnet-grpc-gateway:latest"

# Help with specific tasks
help-docker: ## Display Docker commands help
	@echo "$(BLUE)Docker Commands:$(NC)"
	@echo "  make docker-build   - Build Docker image"
	@echo "  make docker-run     - Start Docker containers"
	@echo "  make docker-logs    - View container logs"
	@echo "  make docker-down    - Stop containers"
	@echo "  make docker-clean   - Remove containers and volumes"

help-database: ## Display database commands help
	@echo "$(BLUE)Database Commands:$(NC)"
	@echo "  make db-create      - Create database"
	@echo "  make db-drop        - Drop database"
	@echo "  make db-reset       - Reset database"
	@echo "  make db-migrate     - Run migrations"

# Default
.PHONY: .DEFAULT_GOAL
