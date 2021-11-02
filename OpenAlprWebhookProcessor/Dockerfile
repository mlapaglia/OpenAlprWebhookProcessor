#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.0-rc.2-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-rc.2-bullseye-slim-amd64 AS build
WORKDIR /src

RUN curl -sL https://deb.nodesource.com/setup_15.x |  bash -
RUN apt-get install -y nodejs

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
ENTRYPOINT ["dotnet", "OpenAlprWebhookProcessor.dll"]