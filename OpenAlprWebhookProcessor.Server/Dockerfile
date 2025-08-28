ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache jq
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS dependencies
ARG BUILD_CONFIGURATION
WORKDIR /src

RUN apk add --no-cache curl nodejs npm && \
    dotnet tool install --global dotnet-ef

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
    -p:GitFileVersion="$GIT_FILE_VERSION" && \
    mkdir -p /app/build && \
    /root/.dotnet/tools/dotnet-ef migrations script \
        --project "./OpenAlprWebhookProcessor.Server.csproj" \
        --startup-project "./OpenAlprWebhookProcessor.Server.csproj" \
        --context ProcessorContext \
        --idempotent \
        --output /app/build/processor-migrations.sql \
        --configuration $BUILD_CONFIGURATION && \
    /root/.dotnet/tools/dotnet-ef migrations script \
        --project "./OpenAlprWebhookProcessor.Server.csproj" \
        --startup-project "./OpenAlprWebhookProcessor.Server.csproj" \
        --context UsersContext \
        --idempotent \
        --output /app/build/users-migrations.sql \
        --configuration $BUILD_CONFIGURATION

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

USER root
RUN apk add --no-cache sqlite

COPY --from=publish /app/publish .
COPY --from=build /app/build/processor-migrations.sql ./processor-migrations.sql
COPY --from=build /app/build/users-migrations.sql ./users-migrations.sql
COPY --from=build /src/OpenAlprWebhookProcessor.Server/scripts/entrypoint.sh ./

USER root
RUN chmod +x ./entrypoint.sh
USER app

ENTRYPOINT ["./entrypoint.sh"]