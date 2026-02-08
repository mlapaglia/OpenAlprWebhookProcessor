ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS dependencies
ARG BUILD_CONFIGURATION
WORKDIR /src

RUN apk add --no-cache curl nodejs npm

COPY ["OpenAlprWebhookProcessor.Server/OpenAlprWebhookProcessor.Server.csproj", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/openalprwebhookprocessor.client.esproj", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/nuget.config", "openalprwebhookprocessor.client/"]
RUN dotnet restore "./OpenAlprWebhookProcessor.Server/./OpenAlprWebhookProcessor.Server.csproj"

COPY ["openalprwebhookprocessor.client/package*.json", "openalprwebhookprocessor.client/"]
WORKDIR "/src/openalprwebhookprocessor.client"
RUN npm ci

FROM dependencies AS build
ARG BUILD_CONFIGURATION
ARG GIT_VERSION
ARG GIT_ASSEMBLY_VERSION
ARG GIT_FILE_VERSION
WORKDIR /src

COPY ["openalprwebhookprocessor.client/angular.json", \
      "openalprwebhookprocessor.client/tsconfig*.json", \
      "openalprwebhookprocessor.client/karma.conf.js", \
      "openalprwebhookprocessor.client/eslint.config.mjs", \
      "openalprwebhookprocessor.client/ngsw-config.json", \
      "openalprwebhookprocessor.client/"]

COPY ["OpenAlprWebhookProcessor.Server/", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/src/", "openalprwebhookprocessor.client/src/"]

WORKDIR "/src/OpenAlprWebhookProcessor.Server"

RUN dotnet build "./OpenAlprWebhookProcessor.Server.csproj" -c $BUILD_CONFIGURATION \
    -p:GitVersion="$GIT_VERSION" \
    -p:GitAssemblyVersion="$GIT_ASSEMBLY_VERSION" \
    -p:GitFileVersion="$GIT_FILE_VERSION"

FROM build AS publish
ARG BUILD_CONFIGURATION
ARG GIT_VERSION
ARG GIT_ASSEMBLY_VERSION
ARG GIT_FILE_VERSION
RUN dotnet publish "./OpenAlprWebhookProcessor.Server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -r linux-musl-x64 \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false \
    -p:GitVersion="$GIT_VERSION" \
    -p:GitAssemblyVersion="$GIT_ASSEMBLY_VERSION" \
    -p:GitFileVersion="$GIT_FILE_VERSION"

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "OpenAlprWebhookProcessor.Server.dll"]