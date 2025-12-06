# Base image NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Because Dockerfile is inside Fusion.API folder, use ../ to reach root repo
COPY ../Fusion.API/Fusion.API.csproj Fusion.API/
COPY ../Fusion.Service/Fusion.Service.csproj Fusion.Service/
COPY ../Fusion.Repository/Fusion.Repository.csproj Fusion.Repository/

RUN dotnet restore "Fusion.API/Fusion.API.csproj"

# Copy entire repo
COPY .. .

WORKDIR "/src/Fusion.API"
RUN dotnet build "Fusion.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Fusion.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fusion.API.dll"]
