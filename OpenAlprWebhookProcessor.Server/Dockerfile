# Optimized Dockerfile for ASP.NET + Angular integrated build
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache jq
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION
WORKDIR /src

# Install system dependencies once (rarely changes)
RUN apk add --no-cache curl nodejs npm
RUN dotnet tool install --global dotnet-ef

# Copy .NET project files FIRST for dependency caching
COPY ["openalprwebhookprocessor.client/nuget.config", "openalprwebhookprocessor.client/"]
COPY ["OpenAlprWebhookProcessor.Server/OpenAlprWebhookProcessor.Server.csproj", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/openalprwebhookprocessor.client.esproj", "openalprwebhookprocessor.client/"]

# Copy npm dependency files for frontend dependency caching
COPY ["openalprwebhookprocessor.client/package.json", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/package-lock.json", "openalprwebhookprocessor.client/"]

# Restore .NET dependencies (this will also handle npm dependencies via the esproj)
RUN dotnet restore "./OpenAlprWebhookProcessor.Server/./OpenAlprWebhookProcessor.Server.csproj"

# Copy configuration files that change less frequently
COPY ["openalprwebhookprocessor.client/angular.json", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/tsconfig*.json", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/karma.conf.js", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/eslint.config.mjs", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/ngsw-config.json", "openalprwebhookprocessor.client/"]

# Copy source code LAST (changes most frequently)
COPY ["OpenAlprWebhookProcessor.Server/", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/src/", "openalprwebhookprocessor.client/src/"]
COPY ["openalprwebhookprocessor.client/e2e/", "openalprwebhookprocessor.client/e2e/"]

# Copy remaining project files
COPY ["Tests/", "Tests/"]
COPY ["*.sln", "./"]

# Build the application (includes integrated Angular build)
WORKDIR "/src/OpenAlprWebhookProcessor.Server"
RUN dotnet build "./OpenAlprWebhookProcessor.Server.csproj" -c $BUILD_CONFIGURATION

# Create migration bundles (only rebuilds if EF contexts change)
RUN /root/.dotnet/tools/dotnet-ef migrations bundle --context ProcessorContext --output /app/build/processor-migrator --no-build --configuration $BUILD_CONFIGURATION
RUN /root/.dotnet/tools/dotnet-ef migrations bundle --context UsersContext --output /app/build/users-migrator --no-build --configuration $BUILD_CONFIGURATION

FROM build AS publish
ARG BUILD_CONFIGURATION

RUN dotnet publish "./OpenAlprWebhookProcessor.Server.csproj" \
	-c $BUILD_CONFIGURATION \
	-o /app/publish \
	/p:UseAppHost=false \
	/p:PublishSingleFile=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
COPY --from=build /app/build/processor-migrator ./processor-migrator
COPY --from=build /app/build/users-migrator ./users-migrator
COPY --from=build /src/OpenAlprWebhookProcessor.Server/scripts/entrypoint.sh ./

USER root
RUN chmod +x ./entrypoint.sh
USER app

ENTRYPOINT ["./entrypoint.sh"] 