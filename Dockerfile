# GigRadarApi — ASP.NET Core 8 (.NET)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dulu (layer caching)
COPY GigRadarApi/*.csproj ./
RUN dotnet restore

# Build + publish
COPY GigRadarApi/ ./
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render menyetel PORT saat runtime — diekspor lewat shell karena ENV Docker
# tidak meng-expand variabel runtime. Default 8080 untuk lokal.
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "export ASPNETCORE_URLS=http://+:${PORT:-8080} && exec dotnet GigRadarApi.dll"]
