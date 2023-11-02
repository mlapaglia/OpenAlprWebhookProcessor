#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim-amd64 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y ca-certificates curl gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && NODE_MAJOR=20 \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update && apt-get install nodejs -y

COPY ["OpenAlprWebhookProcessor/OpenAlprWebhookProcessor.csproj", "OpenAlprWebhookProcessor/"]
RUN dotnet restore "OpenAlprWebhookProcessor/OpenAlprWebhookProcessor.csproj"
COPY . .
WORKDIR "/src/OpenAlprWebhookProcessor"
RUN dotnet build "OpenAlprWebhookProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenAlprWebhookProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV SQLITE_TMPDIR=/app/config/

ENTRYPOINT ["dotnet", "OpenAlprWebhookProcessor.dll"]