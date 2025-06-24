FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "dotnet9-jwt-concept.dll"]
