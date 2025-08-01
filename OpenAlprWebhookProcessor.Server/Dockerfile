#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache jq
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION
ARG GIT_VERSION
ARG GIT_ASSEMBLY_VERSION
ARG GIT_FILE_VERSION
WORKDIR /src

RUN apk add --no-cache curl nodejs npm && \
    dotnet tool install --global dotnet-ef

COPY ["openalprwebhookprocessor.client/nuget.config", "openalprwebhookprocessor.client/"]
COPY ["OpenAlprWebhookProcessor.Server/OpenAlprWebhookProcessor.Server.csproj", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/openalprwebhookprocessor.client.esproj", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/package.json", "openalprwebhookprocessor.client/"]
COPY ["openalprwebhookprocessor.client/package-lock.json", "openalprwebhookprocessor.client/"]

RUN dotnet restore "./OpenAlprWebhookProcessor.Server/./OpenAlprWebhookProcessor.Server.csproj"

COPY ["openalprwebhookprocessor.client/angular.json", \
      "openalprwebhookprocessor.client/tsconfig*.json", \
      "openalprwebhookprocessor.client/karma.conf.js", \
      "openalprwebhookprocessor.client/eslint.config.mjs", \
      "openalprwebhookprocessor.client/ngsw-config.json", \
      "openalprwebhookprocessor.client/"]

COPY ["OpenAlprWebhookProcessor.Server/", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/src/", "openalprwebhookprocessor.client/src/"]
COPY ["openalprwebhookprocessor.client/e2e/", "openalprwebhookprocessor.client/e2e/"]

WORKDIR "/src/OpenAlprWebhookProcessor.Server"

RUN dotnet build "./OpenAlprWebhookProcessor.Server.csproj" -c $BUILD_CONFIGURATION \
    -p:GitVersion="$GIT_VERSION" \
    -p:GitAssemblyVersion="$GIT_ASSEMBLY_VERSION" \
    -p:GitFileVersion="$GIT_FILE_VERSION" && \
    mkdir -p /app/build && \
    /root/.dotnet/tools/dotnet-ef migrations bundle \
        --project "./OpenAlprWebhookProcessor.Server.csproj" \
        --startup-project "./OpenAlprWebhookProcessor.Server.csproj" \
        --context ProcessorContext \
        --output /app/build/processor-migrator \
        --configuration $BUILD_CONFIGURATION \
        --verbose \
        --force && \
    /root/.dotnet/tools/dotnet-ef migrations bundle \
        --project "./OpenAlprWebhookProcessor.Server.csproj" \
        --startup-project "./OpenAlprWebhookProcessor.Server.csproj" \
        --context UsersContext \
        --output /app/build/users-migrator \
        --configuration $BUILD_CONFIGURATION \
        --verbose \
        --force

FROM build AS publish
ARG BUILD_CONFIGURATION
ARG GIT_VERSION
ARG GIT_ASSEMBLY_VERSION
ARG GIT_FILE_VERSION
RUN dotnet publish "./OpenAlprWebhookProcessor.Server.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false \
    -p:GitVersion="$GIT_VERSION" \
    -p:GitAssemblyVersion="$GIT_ASSEMBLY_VERSION" \
    -p:GitFileVersion="$GIT_FILE_VERSION"

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