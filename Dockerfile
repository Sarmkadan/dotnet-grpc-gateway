# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

COPY ["dotnet-grpc-gateway.sln", "."]
COPY ["src/dotnet-grpc-gateway/dotnet-grpc-gateway.csproj", "src/dotnet-grpc-gateway/"]

RUN dotnet restore "dotnet-grpc-gateway.sln"

COPY . .

WORKDIR "/src/src/dotnet-grpc-gateway"

RUN dotnet build "dotnet-grpc-gateway.csproj" -c Release -o /app/build

RUN dotnet publish "dotnet-grpc-gateway.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=builder /app/publish .

RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser

EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "dotnet-grpc-gateway.dll"]
