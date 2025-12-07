# ================================
# Build Stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy API Web project
COPY CETS.API/CETS.API.Web/*.csproj CETS.API/CETS.API.Web/

# Copy Core projects
COPY CETS.Core/Application/*.csproj CETS.Core/Application/
COPY CETS.Core/Domain/*.csproj CETS.Core/Domain/
COPY CETS.Core/DTOs/*.csproj CETS.Core/DTOs/
COPY CETS.Core/Infrastructure/*.csproj CETS.Core/Infrastructure/
COPY CETS.Core/Utils/*.csproj CETS.Core/Utils/

# Restore dependencies
RUN dotnet restore CETS.API/CETS.API.Web/CETS.API.Web.csproj

# Copy full solution
COPY . .

# Publish
RUN dotnet publish CETS.API/CETS.API.Web/CETS.API.Web.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Runtime Stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CETS.API.Web.dll"]
