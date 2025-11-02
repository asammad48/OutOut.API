# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all projects
COPY OutOut.sln ./
COPY OutOut.Constants/ OutOut.Constants/
COPY OutOut.Core/ OutOut.Core/
COPY OutOut.DataGenerator/ OutOut.DataGenerator/
COPY OutOut.Infrastructure/ OutOut.Infrastructure/
COPY OutOut.Models/ OutOut.Models/
COPY OutOut.Persistence/ OutOut.Persistence/
COPY OutOut.ViewModels/ OutOut.ViewModels/
COPY OutOut/ OutOut/

# Restore, build, and publish
RUN dotnet restore OutOut/OutOut.csproj
RUN dotnet publish OutOut/OutOut.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OutOut.dll"]
