#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

RUN curl -sL https://deb.nodesource.com/setup_15.x |  bash -
RUN apt-get install -y nodejs

COPY ["OpenAlprWebhookProcessor.csproj", ""]
RUN dotnet restore "./OpenAlprWebhookProcessor.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "OpenAlprWebhookProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenAlprWebhookProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenAlprWebhookProcessor.dll"]