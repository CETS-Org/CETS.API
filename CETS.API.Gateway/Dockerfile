# ================================
# Build Stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Gateway project file
COPY CETS.API/CETS.API.Gateway/*.csproj CETS.API/CETS.API.Gateway/

# Copy Core project files
COPY CETS.Core/Application/*.csproj CETS.Core/Application/
COPY CETS.Core/Domain/*.csproj CETS.Core/Domain/
COPY CETS.Core/DTOs/*.csproj CETS.Core/DTOs/
COPY CETS.Core/Infrastructure/*.csproj CETS.Core/Infrastructure/
COPY CETS.Core/Utils/*.csproj CETS.Core/Utils/

# Restore dependencies
RUN dotnet restore CETS.API/CETS.API.Gateway/CETS.API.Gateway.csproj

# Copy full source
COPY . .

# Build & Publish
RUN dotnet publish CETS.API/CETS.API.Gateway/CETS.API.Gateway.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Runtime Stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CETS.API.Gateway.dll"]
