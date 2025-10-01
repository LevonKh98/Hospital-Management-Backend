# Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY BackendApi/BackendApi.csproj BackendApi/
RUN dotnet restore BackendApi/BackendApi.csproj
COPY . .
WORKDIR /src/BackendApi
RUN dotnet publish -c Release -o /out

# Final
FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet","BackendApi.dll"]
