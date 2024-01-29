#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN apt-get update && apt-get install -y ca-certificates curl gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && NODE_MAJOR=20 \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update && apt-get install nodejs -y

COPY ["openalprwebhookprocessor.client/nuget.config", "openalprwebhookprocessor.client/"]
COPY ["OpenAlprWebhookProcessor.Server/OpenAlprWebhookProcessor.Server.csproj", "OpenAlprWebhookProcessor.Server/"]
COPY ["openalprwebhookprocessor.client/openalprwebhookprocessor.client.esproj", "openalprwebhookprocessor.client/"]
RUN dotnet restore "./OpenAlprWebhookProcessor.Server/./OpenAlprWebhookProcessor.Server.csproj"
COPY . .
WORKDIR "/src/OpenAlprWebhookProcessor.Server"
RUN dotnet build "./OpenAlprWebhookProcessor.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OpenAlprWebhookProcessor.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenAlprWebhookProcessor.Server.dll"]