# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/AutismCenter.WebApi/AutismCenter.WebApi.csproj", "src/AutismCenter.WebApi/"]
COPY ["src/AutismCenter.Application/AutismCenter.Application.csproj", "src/AutismCenter.Application/"]
COPY ["src/AutismCenter.Domain/AutismCenter.Domain.csproj", "src/AutismCenter.Domain/"]
COPY ["src/AutismCenter.Infrastructure/AutismCenter.Infrastructure.csproj", "src/AutismCenter.Infrastructure/"]

RUN dotnet restore "src/AutismCenter.WebApi/AutismCenter.WebApi.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/AutismCenter.WebApi"
RUN dotnet build "AutismCenter.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AutismCenter.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --gid 1001 appuser

# Copy published app
COPY --from=publish /app/publish .

# Set ownership and permissions
RUN chown -R appuser:appgroup /app
USER appuser

# Configure environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AutismCenter.WebApi.dll"]