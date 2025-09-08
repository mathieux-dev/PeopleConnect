# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY PeopleConnect.sln ./

# Copy project files
COPY Domain/PeopleConnect.Domain/*.csproj ./Domain/PeopleConnect.Domain/
COPY Domain/PeopleConnect.Domain.Tests/*.csproj ./Domain/PeopleConnect.Domain.Tests/
COPY Application/PeopleConnect.Application/*.csproj ./Application/PeopleConnect.Application/
COPY Application/PeopleConnect.Application.Tests/*.csproj ./Application/PeopleConnect.Application.Tests/
COPY Infrastructure/PeopleConnect.Infrastructure/*.csproj ./Infrastructure/PeopleConnect.Infrastructure/
COPY Infrastructure/PeopleConnect.Infrastructure.Tests/*.csproj ./Infrastructure/PeopleConnect.Infrastructure.Tests/
COPY Presentation/PeopleConnect.Api/*.csproj ./Presentation/PeopleConnect.Api/
COPY Tests/PeopleConnect.IntegrationTests/*.csproj ./Tests/PeopleConnect.IntegrationTests/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build the application
RUN dotnet publish Presentation/PeopleConnect.Api/PeopleConnect.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
#ENTRYPOINT ["dotnet", "PeopleConnect.Api.dll"]

# ADICIONE ESTAS DUAS LINHAS PARA DEBUG
CMD printenv > /tmp/env_vars.txt && cat /tmp/env_vars.txt && dotnet PeopleConnect.Api.dll
