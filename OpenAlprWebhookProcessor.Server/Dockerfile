#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
RUN apk add --no-cache jq
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION
WORKDIR /src

RUN apk add --no-cache curl nodejs npm

RUN dotnet tool install --global dotnet-ef

COPY ["openalprwebhookprocessor.client/nuget.config", "openalprwebhookprocessor.client/"]
COPY ["OpenAlprWebhookProcessor.Server/OpenAlprWebhookProcessor.Server.csproj", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/openalprwebhookprocessor.client.esproj", "openalprwebhookprocessor.client/"]
RUN dotnet restore "./OpenAlprWebhookProcessor.Server/./OpenAlprWebhookProcessor.Server.csproj"
COPY . .
WORKDIR "/src/OpenAlprWebhookProcessor.Server"
RUN dotnet build "./OpenAlprWebhookProcessor.Server.csproj" -c $BUILD_CONFIGURATION

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